using AgenticRouter.Models;
using AgenticRouter.Proxy;
using Microsoft.Extensions.Options;
using Moq;

namespace AgenticRouter.Tests.Proxy;

/// <summary>
/// Covers known-model allowlist resolution behavior for <see cref="ModelRouteResolver"/>.
/// </summary>
public class ModelRouteResolverTests
{
    [Fact]
    public void TryResolve_KnownModel_ReturnsUpstreamRouteWithInjectedCredential()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4-2026-01",
            baseUrl: "https://api.openai.com",
            authHeaderName: "Authorization",
            authHeaderScheme: "Bearer",
            apiKey: "sk-test-123");

        var resolved = resolver.TryResolve("gpt-5.4", out var route);

        Assert.True(resolved);
        Assert.Equal("gpt-5.4", route!.ModelName);
        Assert.Equal("gpt-5.4-2026-01", route.ProviderModelId);
        Assert.Equal("https://api.openai.com/", route.UpstreamBaseUrl.ToString());
        Assert.Equal("Authorization", route.AuthHeaderName);
        Assert.Equal("Bearer sk-test-123", route.AuthHeaderValue);
    }

    [Fact]
    public void TryResolve_IsCaseInsensitiveOnModelName()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");

        Assert.True(resolver.TryResolve("GPT-5.4", out _));
    }

    [Fact]
    public void TryResolve_UnknownModel_ReturnsFalse()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");

        var resolved = resolver.TryResolve("some-other-model", out var route);

        Assert.False(resolved);
        Assert.Null(route);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryResolve_NullOrWhitespaceModelName_ReturnsFalse(string? modelName)
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");

        Assert.False(resolver.TryResolve(modelName, out var route));
        Assert.Null(route);
    }

    [Fact]
    public void TryResolve_MissingApiKeyEnvironmentVariable_ResolvesWithNullAuthHeaderValue()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4",
            baseUrl: "https://api.openai.com",
            apiKey: null);

        var resolved = resolver.TryResolve("gpt-5.4", out var route);

        Assert.True(resolved);
        Assert.Null(route!.AuthHeaderValue);
    }

    [Fact]
    public void TryResolve_ProviderWithNoAuthScheme_UsesRawKeyAsHeaderValue()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "claude-opus-4.6",
            providerModelId: "claude-opus-4-6",
            baseUrl: "https://api.anthropic.com",
            authHeaderName: "x-api-key",
            authHeaderScheme: "",
            apiKey: "anthropic-secret");

        resolver.TryResolve("claude-opus-4.6", out var route);

        Assert.Equal("x-api-key", route!.AuthHeaderName);
        Assert.Equal("anthropic-secret", route.AuthHeaderValue);
    }

    [Fact]
    public void Constructor_ModelListReferencesUnknownProvider_ThrowsOptionsValidationException()
    {
        var options = new ModelRoutingOptions
        {
            Providers = new Dictionary<string, ProviderOptions>(),
            ModelList = [new ModelRouteEntry { ModelName = "gpt-5.4", Provider = "openai", ProviderModelId = "gpt-5.4" }]
        };

        Assert.Throws<OptionsValidationException>(
            () => new ModelRouteResolver(Options.Create(options), Mock.Of<IEnvironmentVariableProvider>()));
    }

    [Fact]
    public void Constructor_EmptyConfiguration_DoesNotThrow_AndResolvesNothing()
    {
        var resolver = ModelRouteResolverTestFactory.Empty();

        Assert.False(resolver.TryResolve("anything", out _));
    }

    // Verifies that a literal ApiKey configured directly on the provider is used over the ApiKeyEnvVar
    // lookup, so operators can supply a key without setting a process environment variable.
    [Fact]
    public void TryResolve_LiteralApiKeyConfigured_UsesLiteralKeyInsteadOfEnvironmentVariable()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4",
            baseUrl: "https://api.openai.com",
            authHeaderScheme: "Bearer",
            apiKey: "env-value-should-not-be-used",
            literalApiKey: "literal-value-from-appsettings");

        resolver.TryResolve("gpt-5.4", out var route);

        Assert.Equal("Bearer literal-value-from-appsettings", route!.AuthHeaderValue);
    }

    // Verifies that when no literal ApiKey is configured, resolution still falls back to the
    // ApiKeyEnvVar-named environment variable, preserving pre-existing behavior.
    [Fact]
    public void TryResolve_NoLiteralApiKeyConfigured_FallsBackToEnvironmentVariable()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4",
            baseUrl: "https://api.openai.com",
            authHeaderScheme: "Bearer",
            apiKey: "env-value-should-be-used",
            literalApiKey: null);

        resolver.TryResolve("gpt-5.4", out var route);

        Assert.Equal("Bearer env-value-should-be-used", route!.AuthHeaderValue);
    }

    // Verifies that a literal ApiKey consisting only of whitespace is treated as unset, so it does not
    // shadow a valid ApiKeyEnvVar fallback with an empty credential.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TryResolve_WhitespaceOnlyLiteralApiKey_FallsBackToEnvironmentVariable(string literalApiKey)
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4",
            baseUrl: "https://api.openai.com",
            authHeaderScheme: "Bearer",
            apiKey: "env-value-should-be-used",
            literalApiKey: literalApiKey);

        resolver.TryResolve("gpt-5.4", out var route);

        Assert.Equal("Bearer env-value-should-be-used", route!.AuthHeaderValue);
    }

    // Verifies that when neither a literal ApiKey nor a resolvable ApiKeyEnvVar value is present,
    // the route still resolves successfully but with no auth header value (forwarded unauthenticated).
    [Fact]
    public void TryResolve_NoLiteralApiKeyAndMissingEnvironmentVariable_ResolvesWithNullAuthHeaderValue()
    {
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4",
            baseUrl: "https://api.openai.com",
            apiKey: null,
            literalApiKey: null);

        var resolved = resolver.TryResolve("gpt-5.4", out var route);

        Assert.True(resolved);
        Assert.Null(route!.AuthHeaderValue);
    }

    // Verifies that ListModels() surfaces every configured model with its provider key, in the same order
    // they appear in configuration, so /v1/models can be answered deterministically from this data.
    [Fact]
    public void ListModels_ReturnsAllConfiguredModels_WithProviderKeys_InConfigurationOrder()
    {
        var resolver = ModelRouteResolverTestFactory.CreateWithModelList(
            ("gpt-5.4", "openai", "gpt-5.4"),
            ("claude-opus-4.6", "anthropic", "claude-opus-4-6"),
            ("kimi-k2.5", "moonshot", "kimi-k2.5"));

        var models = resolver.ListModels();

        Assert.Equal(
        [
            new AvailableModel("gpt-5.4", "openai"),
            new AvailableModel("claude-opus-4.6", "anthropic"),
            new AvailableModel("kimi-k2.5", "moonshot")
        ], models);
    }

    // Verifies that an empty configuration yields an empty model list rather than throwing, so /v1/models
    // can still return a valid (empty) response for a proxy with no routes configured yet.
    [Fact]
    public void ListModels_EmptyConfiguration_ReturnsEmptyList()
    {
        var resolver = ModelRouteResolverTestFactory.Empty();

        Assert.Empty(resolver.ListModels());
    }
}
