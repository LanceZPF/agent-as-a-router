using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgenticRouter.Proxy
{
    /// <summary>
    /// Intercepts and processes requests and responses.
    /// This class will contain the logic for inspecting, modifying, and routing requests
    /// based on the agent's decisions.
    /// </summary>
    public class RequestInterceptor
    {
        private readonly ILogger<RequestInterceptor> _logger;
        private readonly IModelRouteResolver _modelRouteResolver;
        public int InterceptedRequestCount { get; private set; }

        public RequestInterceptor(ILogger<RequestInterceptor> logger, IModelRouteResolver modelRouteResolver)
        {
            _logger = logger;
            _modelRouteResolver = modelRouteResolver;
        }

        /// <summary>
        /// Intercepts an incoming HTTP request before it is forwarded.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InterceptRequestAsync(HttpContext context)
        {
            _logger.LogInformation("[INTERCEPTOR] Intercepting request for {Method} {Scheme}://{Host}{Path}", context.Request.Method, context.Request.Scheme, context.Request.Host, context.Request.Path);
            InterceptedRequestCount++;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Intercepts the response from the target server before it is sent to the client.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InterceptResponseAsync(HttpContext context)
        {
            _logger.LogInformation("[INTERCEPTOR] Intercepting response for {Path} with status {StatusCode}", context.Request.Path, context.Response.StatusCode);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Lists the client-facing models this proxy is configured to route. Used to answer the
        /// OpenAI-compatible model discovery endpoint (<c>GET /v1/models</c>).
        /// </summary>
        public IReadOnlyList<AvailableModel> ListAvailableModels() => _modelRouteResolver.ListModels();

        /// <summary>
        /// Reads the request body, resolves the requested model against the known-model allowlist, and
        /// rewrites <c>model</c> to the upstream provider's model id. The proxy only ever forwards to
        /// upstreams present in this allowlist, so a request can never be routed back to the proxy itself.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A <see cref="ModelRouteResolutionResult"/> describing the outcome.</returns>
        public async Task<ModelRouteResolutionResult> ResolveModelRouteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            string body;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return ModelRouteResolutionResult.Failure("Request body must be a JSON object containing a 'model' field.");
            }

            JsonNode? json;
            try
            {
                json = JsonNode.Parse(body);
            }
            catch (JsonException)
            {
                return ModelRouteResolutionResult.Failure("Request body is not valid JSON.");
            }

            if (json is not JsonObject jsonObject)
            {
                return ModelRouteResolutionResult.Failure("Request body must be a JSON object containing a 'model' field.");
            }

            var modelName = jsonObject["model"] is JsonValue modelValue && modelValue.TryGetValue<string>(out var value)
                ? value
                : null;

            if (string.IsNullOrWhiteSpace(modelName))
            {
                return ModelRouteResolutionResult.Failure("Request body must include a non-empty 'model' field.");
            }

            if (!_modelRouteResolver.TryResolve(modelName, out var route))
            {
                _logger.LogWarning("[INTERCEPTOR] Rejected request for unknown model '{ModelName}'.", modelName);
                return ModelRouteResolutionResult.Failure($"model '{modelName}' is not in the known model list.");
            }

            jsonObject["model"] = route.ProviderModelId;
            var rewrittenBody = Encoding.UTF8.GetBytes(jsonObject.ToJsonString());

            return ModelRouteResolutionResult.Success(route, rewrittenBody);
        }
    }
}
