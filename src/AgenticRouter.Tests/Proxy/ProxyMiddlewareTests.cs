using AgenticRouter.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AgenticRouter.Tests.Proxy;

/// <summary>
/// Covers request forwarding behavior for <see cref="ProxyMiddleware"/>.
/// </summary>
public class ProxyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_KnownModel_ForwardsToResolvedUpstream_RewritesBody_AndInjectsCredential()
    {
        var loggerMock = new Mock<ILogger<ProxyMiddleware>>();
        var resolver = ModelRouteResolverTestFactory.Create(
            modelName: "gpt-5.4",
            providerModelId: "gpt-5.4-2026-01",
            baseUrl: "https://example.com",
            authHeaderName: "Authorization",
            authHeaderScheme: "Bearer",
            apiKey: "secret-key");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://example.com/chat?x=1", request.RequestUri!.ToString());
            Assert.True(request.Headers.Contains("X-Trace"));
            Assert.Equal("Bearer secret-key", request.Headers.GetValues("Authorization").Single());

            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(body);
            Assert.Equal("gpt-5.4-2026-01", document.RootElement.GetProperty("model").GetString());

            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("forwarded", Encoding.UTF8, "text/plain")
            };
            response.Headers.Add("X-From-Upstream", "true");
            return response;
        });

        var middleware = new ProxyMiddleware(loggerMock.Object, interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/chat";
        context.Request.QueryString = new QueryString("?x=1");
        context.Request.Headers["X-Trace"] = "abc";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status202Accepted, context.Response.StatusCode);
        Assert.Equal("true", context.Response.Headers["X-From-Upstream"].ToString());
        Assert.Equal(1, interceptor.InterceptedRequestCount);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Equal("forwarded", responseBody);

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Proxy middleware caught request to", StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotForwardToTheProxysOwnAddress_EvenWhenRequestHostMatchesIt()
    {
        // Regression test: the forwarding target must come from the resolved upstream route, never from
        // context.Request.Host, otherwise the proxy would forward a request back to itself indefinitely.
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(request =>
        {
            Assert.Equal("api.openai.com", request.RequestUri!.Host);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        });

        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/v1/chat/completions";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnknownModel_Returns400_AndNeverCallsUpstream()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(_ => throw new InvalidOperationException("Upstream should never be called for an unknown model."));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/v1/chat/completions";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"totally-unknown-model"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(responseBody);
        Assert.Equal("invalid_request_error", document.RootElement.GetProperty("error").GetProperty("type").GetString());
        Assert.Contains("totally-unknown-model", document.RootElement.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_StripsHeadersNominatedByRequestConnectionHeader()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://example.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(request =>
        {
            Assert.False(request.Headers.Contains("X-Nominated"));
            Assert.True(request.Headers.Contains("X-Kept"));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        });

        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/chat";
        context.Request.Headers["Connection"] = "X-Nominated";
        context.Request.Headers["X-Nominated"] = "should-be-stripped";
        context.Request.Headers["X-Kept"] = "should-be-forwarded";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StripsHeadersNominatedByResponseConnectionHeader()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://example.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") };
            response.Headers.Add("Connection", "X-Custom");
            response.Headers.Add("X-Custom", "should-be-stripped");
            response.Headers.Add("X-Kept", "should-be-forwarded");
            return Task.FromResult(response);
        });

        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/chat";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.False(context.Response.Headers.ContainsKey("X-Custom"));
        Assert.False(context.Response.Headers.ContainsKey("Connection"));
        Assert.Equal("should-be-forwarded", context.Response.Headers["X-Kept"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenForwardingFails_ThrowsHttpRequestException()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var handler = new DelegatingHandlerStub(_ => throw new HttpRequestException("upstream unavailable"));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/fail";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;

        await Assert.ThrowsAsync<HttpRequestException>(() => middleware.InvokeAsync(context, _ => Task.CompletedTask));
    }

    // Verifies that GET /v1/models is answered locally from the configured ModelList, in OpenAI's model
    // list shape, and never reaches the upstream HTTP handler (there is no single upstream to forward a
    // multi-provider model list to).
    [Fact]
    public async Task InvokeAsync_GetModelsList_ReturnsConfiguredModels_AsOpenAiShapedList_WithoutCallingUpstream()
    {
        var resolver = ModelRouteResolverTestFactory.CreateWithModelList(
            ("gpt-5.4", "openai", "gpt-5.4-2026-01"),
            ("claude-opus-4.6", "anthropic", "claude-opus-4-6"));
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var handler = new DelegatingHandlerStub(_ => throw new InvalidOperationException("Upstream should never be called for /v1/models."));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/v1/models";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        using var document = JsonDocument.Parse(await reader.ReadToEndAsync(TestContext.Current.CancellationToken));

        Assert.Equal("list", document.RootElement.GetProperty("object").GetString());
        var data = document.RootElement.GetProperty("data").EnumerateArray().ToList();
        Assert.Equal(2, data.Count);

        Assert.Equal("gpt-5.4", data[0].GetProperty("id").GetString());
        Assert.Equal("model", data[0].GetProperty("object").GetString());
        Assert.Equal(0, data[0].GetProperty("created").GetInt64());
        Assert.Equal("openai", data[0].GetProperty("owned_by").GetString());

        Assert.Equal("claude-opus-4.6", data[1].GetProperty("id").GetString());
        Assert.Equal("anthropic", data[1].GetProperty("owned_by").GetString());
    }

    // Verifies that an empty ModelList still yields a valid, empty OpenAI-shaped response rather than an
    // error, so a freshly configured proxy with no routes yet doesn't break model discovery.
    [Fact]
    public async Task InvokeAsync_GetModelsList_EmptyModelList_ReturnsEmptyDataArray()
    {
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), ModelRouteResolverTestFactory.Empty());
        var handler = new DelegatingHandlerStub(_ => throw new InvalidOperationException("Upstream should never be called for /v1/models."));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/v1/models";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        using var document = JsonDocument.Parse(await reader.ReadToEndAsync(TestContext.Current.CancellationToken));

        Assert.Empty(document.RootElement.GetProperty("data").EnumerateArray());
    }

    // Verifies the path match is case-insensitive, since client conventions for path casing vary.
    [Fact]
    public async Task InvokeAsync_GetModelsList_IsCaseInsensitiveOnPath()
    {
        var resolver = ModelRouteResolverTestFactory.CreateWithModelList(("gpt-5.4", "openai", "gpt-5.4"));
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var handler = new DelegatingHandlerStub(_ => throw new InvalidOperationException("Upstream should never be called for /v1/models."));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/V1/MODELS";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    // Verifies that a trailing slash on the path is tolerated, since some clients/proxies normalize
    // requests to include one (e.g. GET /v1/models/) and it should still be treated as model discovery.
    [Fact]
    public async Task InvokeAsync_GetModelsList_TrailingSlashIsTolerated()
    {
        var resolver = ModelRouteResolverTestFactory.CreateWithModelList(("gpt-5.4", "openai", "gpt-5.4"));
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);
        var handler = new DelegatingHandlerStub(_ => throw new InvalidOperationException("Upstream should never be called for /v1/models."));
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/v1/models/";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    // Verifies that a non-GET request to the /v1/models path is not treated as a model-discovery request:
    // it still falls through to normal per-model routing, since the discovery short-circuit is GET-only.
    [Fact]
    public async Task InvokeAsync_PostToModelsPath_IsNotTreatedAsModelsListRequest_AndIsForwardedNormally()
    {
        var resolver = ModelRouteResolverTestFactory.Create("gpt-5.4", "gpt-5.4-2026-01", "https://api.openai.com");
        var interceptor = new RequestInterceptor(Mock.Of<ILogger<RequestInterceptor>>(), resolver);

        var handler = new DelegatingHandlerStub(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        });
        var middleware = new ProxyMiddleware(Mock.Of<ILogger<ProxyMiddleware>>(), interceptor, new HttpClient(handler));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("127.0.0.1:5001");
        context.Request.Path = "/v1/models";
        var requestBody = Encoding.UTF8.GetBytes("""{"model":"gpt-5.4"}""");
        context.Request.Body = new MemoryStream(requestBody);
        context.Request.ContentLength = requestBody.Length;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public DelegatingHandlerStub(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request);
    }
}
