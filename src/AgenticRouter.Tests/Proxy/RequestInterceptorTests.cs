using System.Text;
using System.Text.Json;
using AgenticRouter.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgenticRouter.Tests.Proxy;

/// <summary>
/// Covers request and response interception behavior.
/// </summary>
public class RequestInterceptorTests
{
    [Fact]
    public async Task InterceptRequestAsync_IncrementsCount_AndLogsStructuredMessage()
    {
        var loggerMock = new Mock<ILogger<RequestInterceptor>>();
        var interceptor = new RequestInterceptor(loggerMock.Object, ModelRouteResolverTestFactory.Empty());
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/test";

        await interceptor.InterceptRequestAsync(context);
        await interceptor.InterceptRequestAsync(context);

        Assert.Equal(2, interceptor.InterceptedRequestCount);
        VerifyLogContains(loggerMock, LogLevel.Information, "[INTERCEPTOR] Intercepting request for");
    }

    [Fact]
    public async Task InterceptResponseAsync_LogsStructuredMessage()
    {
        var loggerMock = new Mock<ILogger<RequestInterceptor>>();
        var interceptor = new RequestInterceptor(loggerMock.Object, ModelRouteResolverTestFactory.Empty());
        var context = new DefaultHttpContext();
        context.Request.Path = "/chat";
        context.Response.StatusCode = StatusCodes.Status200OK;

        await interceptor.InterceptResponseAsync(context);

        VerifyLogContains(loggerMock, LogLevel.Information, "[INTERCEPTOR] Intercepting response for");
    }

    [Fact]
    public async Task ResolveModelRouteAsync_KnownModel_RewritesBodyToProviderModelId()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4-2026-01", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var context = CreateContextWithBody("""{"model":"gpt-5.4","temperature":0.7}""");

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.True(result.IsSuccess);
        Assert.Equal("gpt-5.4-2026-01", result.Route!.ProviderModelId);

        using var document = JsonDocument.Parse(result.RewrittenBody!);
        Assert.Equal("gpt-5.4-2026-01", document.RootElement.GetProperty("model").GetString());
        Assert.Equal(0.7, document.RootElement.GetProperty("temperature").GetDouble());
    }

    [Fact]
    public async Task ResolveModelRouteAsync_UnknownModel_ReturnsFailure()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var context = CreateContextWithBody("""{"model":"not-a-known-model"}""");

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Route);
        Assert.Contains("not-a-known-model", result.ErrorMessage);
    }

    [Fact]
    public async Task ResolveModelRouteAsync_MissingModelField_ReturnsFailure()
    {
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), ModelRouteResolverTestFactory.Empty());
        var context = CreateContextWithBody("""{"messages":[]}""");

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ResolveModelRouteAsync_MalformedJson_ReturnsFailure()
    {
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), ModelRouteResolverTestFactory.Empty());
        var context = CreateContextWithBody("not-json");

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ResolveModelRouteAsync_EmptyBody_ReturnsFailure()
    {
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), ModelRouteResolverTestFactory.Empty());
        var context = CreateContextWithBody(string.Empty);

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("[1,2,3]")]
    [InlineData("\"gpt-5.4\"")]
    [InlineData("42")]
    public async Task ResolveModelRouteAsync_NonObjectJsonRoot_ReturnsFailure_WithoutThrowing(string body)
    {
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), ModelRouteResolverTestFactory.Empty());
        var context = CreateContextWithBody(body);

        var result = await interceptor.ResolveModelRouteAsync(context);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    // Verifies that ListAvailableModels delegates directly to the underlying resolver, so the interceptor
    // stays a thin pass-through for the model-discovery data ProxyMiddleware needs to answer /v1/models.
    [Fact]
    public void ListAvailableModels_DelegatesToModelRouteResolver()
    {
        var resolver = ModelRouteResolverTestFactory.CreateWithModelList(
            ("gpt-5.4", "openai", "gpt-5.4"),
            ("kimi-k2.5", "moonshot", "kimi-k2.5"));
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var models = interceptor.ListAvailableModels();

        Assert.Equal(resolver.ListModels(), models);
    }

    private static DefaultHttpContext CreateContextWithBody(string body)
    {
        var context = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        return context;
    }

    private static void VerifyLogContains(Mock<ILogger<RequestInterceptor>> loggerMock, LogLevel level, string expectedText)
    {
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(expectedText, StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
