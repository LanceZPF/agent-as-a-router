using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgenticRouter.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Collections.Generic;

namespace AgenticRouter.Proxy;

/// <summary>
/// Middleware for handling and forwarding proxy requests.
/// </summary>
public class ProxyMiddleware : IMiddleware
{
    // RFC 7230 Section 6.1 hop-by-hop headers: meaningful only for a single transport-level connection,
    // so they must never be blindly forwarded between the client, this proxy, and the upstream.
    private static readonly string[] HopByHopHeaders =
    [
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    ];

    // "Authorization" carries the client's inbound credential to the proxy itself (e.g. a placeholder
    // token an IDE/BYOK client requires but never validates), not a credential for the upstream provider.
    // It must never be forwarded as-is: for providers whose AuthHeaderName is something else (e.g.
    // Anthropic's "x-api-key"), forwarding it would send the client's bogus token to the upstream
    // alongside the real injected credential, and some providers reject the request outright when both
    // are present.
    private static readonly string[] AlwaysSkippedRequestHeaders = ["Host", "Content-Type", "Content-Length", "Authorization"];

    // The OpenAI-compatible model discovery path. Answered locally from configuration (mirroring LiteLLM's
    // /v1/models behavior) since it has no request body to resolve a single upstream provider from, and no
    // single upstream to forward it to anyway when ModelList spans multiple providers.
    private const string ModelsListPath = "/v1/models";

    // Cap on how much of the response body telemetry captures for usage parsing (see CopyAndCaptureAsync).
    // Real chat/completion responses are almost always well under this; a response that exceeds it just
    // means usage parsing has less to work with (a truncated/partial buffer that the usage parsers already
    // handle gracefully by finding nothing), never a failure of the actual client-facing forward, which is
    // unaffected by this cap - every byte is still copied to the client regardless.
    private const int MaxCapturedResponseBytes = 4 * 1024 * 1024;

    private readonly ILogger<ProxyMiddleware> _logger;
    private readonly HttpClient _httpClient;
    private readonly RequestInterceptor _interceptor;
    private readonly ISessionIdResolver _sessionIdResolver;
    private readonly IConversationTurnTracker _turnTracker;
    private readonly IUsageExtractor _usageExtractor;
    private readonly ITelemetryPublisher _telemetryPublisher;
    private readonly PricingOptions _pricingOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyMiddleware"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="interceptor">Request/response interceptor.</param>
    /// <param name="httpClient">Optional HTTP client used for forwarding requests.</param>
    /// <param name="sessionIdResolver">Optional session-id resolver; defaults to <see cref="SessionIdResolver"/>.</param>
    /// <param name="turnTracker">Optional turn tracker; defaults to a fresh <see cref="ConversationTurnTracker"/> private to this instance.</param>
    /// <param name="usageExtractor">Optional usage extractor; defaults to <see cref="UsageExtractor"/>.</param>
    /// <param name="telemetryPublisher">Optional telemetry publisher; defaults to a fresh, unattached <see cref="TelemetryPublisher"/> (a safe no-op until attached).</param>
    /// <param name="pricingOptions">Optional pricing configuration; defaults to an empty <see cref="PricingOptions"/> (cost is then always reported as unknown).</param>
    public ProxyMiddleware(
        ILogger<ProxyMiddleware> logger,
        RequestInterceptor interceptor,
        HttpClient? httpClient = null,
        ISessionIdResolver? sessionIdResolver = null,
        IConversationTurnTracker? turnTracker = null,
        IUsageExtractor? usageExtractor = null,
        ITelemetryPublisher? telemetryPublisher = null,
        IOptions<PricingOptions>? pricingOptions = null)
    {
        _logger = logger;
        _interceptor = interceptor;
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });
        _sessionIdResolver = sessionIdResolver ?? new SessionIdResolver();
        _turnTracker = turnTracker ?? new ConversationTurnTracker();
        _usageExtractor = usageExtractor ?? new UsageExtractor();
        _telemetryPublisher = telemetryPublisher ?? new TelemetryPublisher();
        _pricingOptions = pricingOptions?.Value ?? new PricingOptions();
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Proxy middleware caught request to {Path}", context.Request.Path);

        if (IsModelsListRequest(context.Request))
        {
            await WriteModelsListResponseAsync(context);
            return;
        }

        await _interceptor.InterceptRequestAsync(context);

        var resolution = await _interceptor.ResolveModelRouteAsync(context, context.RequestAborted);

        if (!resolution.IsSuccess)
        {
            await WriteModelNotFoundResponseAsync(context, resolution.ErrorMessage!);
            return;
        }

        var route = resolution.Route!;
        var targetUri = new Uri(route.UpstreamBaseUrl, $"{context.Request.Path}{context.Request.QueryString}");

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = targetUri,
            Method = new HttpMethod(context.Request.Method)
        };

        var requestHopByHopHeaders = GetHopByHopHeaderNames(
            context.Request.Headers.TryGetValue("Connection", out var requestConnectionValues) ? requestConnectionValues : default);

        foreach (var header in context.Request.Headers)
        {
            if (AlwaysSkippedRequestHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase) ||
                requestHopByHopHeaders.Contains(header.Key) ||
                string.Equals(header.Key, route.AuthHeaderName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        requestMessage.Content = new ByteArrayContent(resolution.RewrittenBody!);
        requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");

        if (route.AuthHeaderValue is not null)
        {
            requestMessage.Headers.TryAddWithoutValidation(route.AuthHeaderName, route.AuthHeaderValue);
        }

        var stopwatch = Stopwatch.StartNew();
        using var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        var latencyToHeadersMs = stopwatch.ElapsedMilliseconds;

        var responseHopByHopHeaders = GetHopByHopHeaderNames(responseMessage.Headers.Connection);

        context.Response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var header in responseMessage.Headers)
        {
            if (responseHopByHopHeaders.Contains(header.Key))
            {
                continue;
            }

            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            if (responseHopByHopHeaders.Contains(header.Key))
            {
                continue;
            }

            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        var isStreaming = string.Equals(responseMessage.Content.Headers.ContentType?.MediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase);

        byte[] capturedResponseBytes;
        using (var upstreamBody = await responseMessage.Content.ReadAsStreamAsync(context.RequestAborted))
        {
            capturedResponseBytes = await CopyAndCaptureAsync(upstreamBody, context.Response.Body, MaxCapturedResponseBytes, context.RequestAborted);
        }

        var totalDurationMs = stopwatch.ElapsedMilliseconds;

        await _interceptor.InterceptResponseAsync(context);

        // Telemetry is best-effort observability layered on top of an already-completed forward: every
        // byte of the response has already reached the client by this point, and any failure here
        // (malformed JSON, an extractor throwing, a disconnected dashboard) must never surface as a
        // proxy error.
        try
        {
            await PublishTelemetryAsync(context, route, resolution.RewrittenBody!, capturedResponseBytes, isStreaming, latencyToHeadersMs, totalDurationMs, (int)responseMessage.StatusCode, context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to publish routing telemetry; the forwarded response was unaffected.");
        }
    }

    private async Task PublishTelemetryAsync(
        HttpContext context,
        ResolvedModelRoute route,
        byte[] rewrittenRequestBody,
        byte[] capturedResponseBytes,
        bool isStreaming,
        long latencyToHeadersMs,
        long totalDurationMs,
        int statusCode,
        CancellationToken cancellationToken)
    {
        var requestBody = TryParseJsonObject(rewrittenRequestBody);
        var resolvedSessionId = _sessionIdResolver.Resolve(context.Request.Headers, requestBody);

        var isSynthesized = resolvedSessionId is null;
        var sessionId = resolvedSessionId ?? Guid.NewGuid().ToString("N");
        var turnNumber = _turnTracker.NextTurn(sessionId);

        var requestedModel = route.ModelName;
        var isFallback = false; // No fallback-routing concept exists in ModelRouteResolver today; reserved for when one does.

        int? promptTokens = null;
        int? completionTokens = null;
        decimal? estimatedCostUsd = null;

        if (_usageExtractor.TryExtractUsage(route.Provider, isStreaming, capturedResponseBytes, out var usage))
        {
            promptTokens = usage.PromptTokens;
            completionTokens = usage.CompletionTokens;

            if (_pricingOptions.Models.TryGetValue(requestedModel, out var price))
            {
                estimatedCostUsd = price.EstimateCost(usage.PromptTokens, usage.CompletionTokens);
            }
        }

        var telemetryEvent = new RoutingTelemetryEvent(
            SessionId: sessionId,
            TurnNumber: turnNumber,
            IsSessionSynthesized: isSynthesized,
            RequestedModel: requestedModel,
            ResolvedModel: route.ProviderModelId,
            Provider: route.Provider,
            IsFallback: isFallback,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            EstimatedCostUsd: estimatedCostUsd,
            IsStreaming: isStreaming,
            LatencyToHeadersMs: latencyToHeadersMs,
            TotalDurationMs: totalDurationMs,
            StatusCode: statusCode,
            TimestampUtc: DateTimeOffset.UtcNow);

        await _telemetryPublisher.PublishAsync(telemetryEvent, cancellationToken);
    }

    private static JsonObject? TryParseJsonObject(byte[] bytes)
    {
        try
        {
            return JsonNode.Parse(bytes) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Copies <paramref name="source"/> to <paramref name="destination"/> unchanged (the client-facing
    /// forward), while also capturing up to <paramref name="captureCap"/> bytes for telemetry usage
    /// parsing. The capture never delays or alters what reaches <paramref name="destination"/> - it's an
    /// in-memory side copy of each chunk immediately after (not instead of) writing it downstream.
    /// </summary>
    private static async Task<byte[]> CopyAndCaptureAsync(Stream source, Stream destination, int captureCap, CancellationToken cancellationToken)
    {
        using var capture = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                var remainingCapacity = captureCap - (int)capture.Length;
                if (remainingCapacity > 0)
                {
                    await capture.WriteAsync(buffer.AsMemory(0, Math.Min(bytesRead, remainingCapacity)), cancellationToken);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return capture.ToArray();
    }

    /// <summary>
    /// Builds the set of hop-by-hop header names to strip: the fixed RFC 7230 set, plus any additional header
    /// names nominated by a <c>Connection</c> header value (e.g. <c>Connection: Foo</c> makes <c>Foo</c> hop-by-hop).
    /// </summary>
    private static HashSet<string> GetHopByHopHeaderNames(IEnumerable<string>? connectionHeaderValues)
    {
        var names = new HashSet<string>(HopByHopHeaders, StringComparer.OrdinalIgnoreCase);

        if (connectionHeaderValues is null)
        {
            return names;
        }

        foreach (var value in connectionHeaderValues)
        {
            foreach (var token in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                names.Add(token);
            }
        }

        return names;
    }

    /// <summary>
    /// Determines whether a request targets the OpenAI-compatible model discovery endpoint
    /// (<c>GET /v1/models</c>), matched case-insensitively and with an optional trailing slash tolerated,
    /// since both conventions vary by client.
    /// </summary>
    private static bool IsModelsListRequest(HttpRequest request) =>
        HttpMethods.IsGet(request.Method) &&
        string.Equals(request.Path.Value?.TrimEnd('/'), ModelsListPath, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Writes the configured model list as an OpenAI-compatible <c>/v1/models</c> response, mirroring
    /// LiteLLM's behavior of answering this endpoint from local configuration rather than forwarding it
    /// upstream.
    /// </summary>
    private async Task WriteModelsListResponseAsync(HttpContext context)
    {
        var entries = _interceptor.ListAvailableModels()
            .Select(model => new ModelListEntry(model.ModelName, "model", 0, model.Provider))
            .ToList();

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ModelsListResponse("list", entries)),
            context.RequestAborted);
    }

    /// <summary>
    /// A single entry in the <c>/v1/models</c> response, shaped to match OpenAI's model list schema.
    /// </summary>
    private sealed record ModelListEntry(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("object")] string Object,
        [property: JsonPropertyName("created")] long Created,
        [property: JsonPropertyName("owned_by")] string OwnedBy);

    /// <summary>
    /// The top-level <c>/v1/models</c> response envelope, shaped to match OpenAI's model list schema.
    /// </summary>
    private sealed record ModelsListResponse(
        [property: JsonPropertyName("object")] string Object,
        [property: JsonPropertyName("data")] IReadOnlyList<ModelListEntry> Data);

    private static async Task WriteModelNotFoundResponseAsync(HttpContext context, string errorMessage)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = new
            {
                message = errorMessage,
                type = "invalid_request_error",
                param = "model",
                code = "400"
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload), context.RequestAborted);
    }
}
