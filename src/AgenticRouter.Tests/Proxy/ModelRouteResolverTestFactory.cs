using AgenticRouter.Models;
using AgenticRouter.Proxy;
using Microsoft.Extensions.Options;
using Moq;

namespace AgenticRouter.Tests.Proxy;

/// <summary>
/// Builds <see cref="IModelRouteResolver"/> instances for tests without needing real environment variables.
/// </summary>
internal static class ModelRouteResolverTestFactory
{
    public const string ApiKeyEnvVar = "TEST_PROVIDER_API_KEY";

    public static IModelRouteResolver Create(
        string modelName,
        string providerModelId,
        string baseUrl,
        string authHeaderName = "Authorization",
        string authHeaderScheme = "Bearer",
        string? apiKey = "test-api-key",
        string providerName = "test-provider",
        string? literalApiKey = null)
    {
        var options = new ModelRoutingOptions
        {
            Providers = new Dictionary<string, ProviderOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [providerName] = new ProviderOptions
                {
                    BaseUrl = baseUrl,
                    ApiKey = literalApiKey,
                    ApiKeyEnvVar = ApiKeyEnvVar,
                    AuthHeaderName = authHeaderName,
                    AuthHeaderScheme = authHeaderScheme
                }
            },
            ModelList =
            [
                new ModelRouteEntry { ModelName = modelName, Provider = providerName, ProviderModelId = providerModelId }
            ]
        };

        var environment = new Mock<IEnvironmentVariableProvider>();
        environment.Setup(e => e.GetVariable(ApiKeyEnvVar)).Returns(apiKey);

        return new ModelRouteResolver(Options.Create(options), environment.Object);
    }

    public static IModelRouteResolver Empty() =>
        new ModelRouteResolver(Options.Create(new ModelRoutingOptions()), Mock.Of<IEnvironmentVariableProvider>());

    /// <summary>
    /// Builds a resolver configured with several models across one or more providers, for tests that need
    /// to observe ordering or multi-provider behavior (e.g. <see cref="IModelRouteResolver.ListModels"/>).
    /// </summary>
    public static IModelRouteResolver CreateWithModelList(params (string ModelName, string Provider, string ProviderModelId)[] models)
    {
        var providers = new Dictionary<string, ProviderOptions>(StringComparer.OrdinalIgnoreCase);
        foreach (var providerName in models.Select(m => m.Provider).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            providers[providerName] = new ProviderOptions { BaseUrl = "https://example.com" };
        }

        var options = new ModelRoutingOptions
        {
            Providers = providers,
            ModelList = models
                .Select(m => new ModelRouteEntry { ModelName = m.ModelName, Provider = m.Provider, ProviderModelId = m.ProviderModelId })
                .ToList()
        };

        return new ModelRouteResolver(Options.Create(options), Mock.Of<IEnvironmentVariableProvider>());
    }
}
