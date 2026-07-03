using System.Diagnostics.CodeAnalysis;
using AgenticRouter.Models;
using Microsoft.Extensions.Options;

namespace AgenticRouter.Proxy;

/// <summary>
/// Resolves a client-facing model name to its configured upstream provider, mirroring LiteLLM's
/// <c>model_list</c> allowlist: only models present in configuration are routable.
/// </summary>
public interface IModelRouteResolver
{
    /// <summary>
    /// Attempts to resolve a client-facing model name to its upstream route.
    /// </summary>
    /// <param name="modelName">The model name from the request body.</param>
    /// <param name="route">The resolved route, when successful.</param>
    /// <returns><see langword="true"/> if the model is known and resolved; otherwise <see langword="false"/>.</returns>
    bool TryResolve(string? modelName, [NotNullWhen(true)] out ResolvedModelRoute? route);

    /// <summary>
    /// Lists the client-facing models this proxy is configured to route, in the order they appear in
    /// configuration. Used to answer the OpenAI-compatible model discovery endpoint (<c>GET /v1/models</c>)
    /// without forwarding the request to any single upstream provider.
    /// </summary>
    IReadOnlyList<AvailableModel> ListModels();
}

/// <summary>
/// A client-facing model this proxy is configured to route, along with the provider key it resolves to.
/// </summary>
/// <param name="ModelName">The client-facing model name, as it appears in <see cref="ModelRouteEntry.ModelName"/>.</param>
/// <param name="Provider">The provider key this model routes to.</param>
public sealed record AvailableModel(string ModelName, string Provider);

/// <summary>
/// A model resolved to a concrete upstream provider, along with the credentials to authenticate the forwarded request.
/// </summary>
/// <param name="ModelName">The client-facing model name that was resolved.</param>
/// <param name="Provider">The provider key the model routes to.</param>
/// <param name="ProviderModelId">The model identifier to send to the upstream provider.</param>
/// <param name="UpstreamBaseUrl">The absolute base URL of the upstream provider.</param>
/// <param name="AuthHeaderName">The HTTP header name used to carry the provider credential.</param>
/// <param name="AuthHeaderValue">The resolved credential header value, or <see langword="null"/> if no API key is configured/available.</param>
public sealed record ResolvedModelRoute(
    string ModelName,
    string Provider,
    string ProviderModelId,
    Uri UpstreamBaseUrl,
    string AuthHeaderName,
    string? AuthHeaderValue);

/// <inheritdoc cref="IModelRouteResolver" />
public sealed class ModelRouteResolver : IModelRouteResolver
{
    private readonly Dictionary<string, (ModelRouteEntry Entry, ProviderOptions Provider)> _routes;
    private readonly IReadOnlyList<ModelRouteEntry> _orderedEntries;
    private readonly IEnvironmentVariableProvider _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRouteResolver"/> class.
    /// </summary>
    /// <param name="options">The model routing configuration.</param>
    /// <param name="environment">The environment variable accessor used to resolve provider API keys.</param>
    /// <exception cref="OptionsValidationException">Thrown when the bound configuration is inconsistent.</exception>
    public ModelRouteResolver(IOptions<ModelRoutingOptions> options, IEnvironmentVariableProvider environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        _environment = environment;

        var value = options.Value;
        value.EnsureValid();

        _orderedEntries = value.ModelList;
        _routes = value.ModelList.ToDictionary(
            entry => entry.ModelName,
            entry => (entry, value.Providers[entry.Provider]),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool TryResolve(string? modelName, [NotNullWhen(true)] out ResolvedModelRoute? route)
    {
        route = null;

        if (string.IsNullOrWhiteSpace(modelName) || !_routes.TryGetValue(modelName, out var match))
        {
            return false;
        }

        var (entry, provider) = match;
        var apiKey = ResolveApiKey(provider);

        var authHeaderValue = apiKey is null
            ? null
            : string.IsNullOrWhiteSpace(provider.AuthHeaderScheme)
                ? apiKey
                : $"{provider.AuthHeaderScheme} {apiKey}";

        route = new ResolvedModelRoute(
            entry.ModelName,
            entry.Provider,
            entry.ProviderModelId,
            new Uri(provider.BaseUrl, UriKind.Absolute),
            provider.AuthHeaderName,
            authHeaderValue);

        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<AvailableModel> ListModels() =>
        _orderedEntries.Select(entry => new AvailableModel(entry.ModelName, entry.Provider)).ToList();

    /// <summary>
    /// Resolves the API key for a provider, preferring a literal <see cref="ProviderOptions.ApiKey"/> value over
    /// the environment variable named by <see cref="ProviderOptions.ApiKeyEnvVar"/>.
    /// </summary>
    private string? ResolveApiKey(ProviderOptions provider)
    {
        if (!string.IsNullOrWhiteSpace(provider.ApiKey))
        {
            return provider.ApiKey;
        }

        return string.IsNullOrWhiteSpace(provider.ApiKeyEnvVar)
            ? null
            : _environment.GetVariable(provider.ApiKeyEnvVar);
    }
}
