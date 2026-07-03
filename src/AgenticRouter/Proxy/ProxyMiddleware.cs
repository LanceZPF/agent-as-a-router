using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

    private static readonly string[] AlwaysSkippedRequestHeaders = ["Host", "Content-Type", "Content-Length"];

    // The OpenAI-compatible model discovery path. Answered locally from configuration (mirroring LiteLLM's
    // /v1/models behavior) since it has no request body to resolve a single upstream provider from, and no
    // single upstream to forward it to anyway when ModelList spans multiple providers.
    private const string ModelsListPath = "/v1/models";

    private readonly ILogger<ProxyMiddleware> _logger;
    private readonly HttpClient _httpClient;
    private readonly RequestInterceptor _interceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyMiddleware"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="interceptor">Request/response interceptor.</param>
    /// <param name="httpClient">Optional HTTP client used for forwarding requests.</param>
    public ProxyMiddleware(ILogger<ProxyMiddleware> logger, RequestInterceptor interceptor, HttpClient? httpClient = null)
    {
        _logger = logger;
        _interceptor = interceptor;
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });
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

        using var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

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

        await responseMessage.Content.CopyToAsync(context.Response.Body);

        await _interceptor.InterceptResponseAsync(context);
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
