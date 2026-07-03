using AgenticRouter.Router;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgenticRouter.Tests.Router;

/// <summary>
/// Covers the DI-validation placeholder for <see cref="IRouterModelClient"/>.
/// </summary>
public class NotImplementedRouterModelClientTests
{
    // Verifies that invoking the placeholder throws NotImplementedException rather than silently
    // succeeding or returning a fabricated response, since no real model backend is configured.
    [Fact]
    public async Task GetResponseAsync_Throws_NotImplementedException()
    {
        var client = new NotImplementedRouterModelClient(Mock.Of<ILogger<NotImplementedRouterModelClient>>());

        await Assert.ThrowsAsync<NotImplementedException>(
            () => client.GetResponseAsync("gpt-5.4", "prompt"));
    }

    // Verifies that an invocation attempt is logged at Error level (not silently swallowed), so an
    // operator can diagnose why the router/agent subsystem failed if it's ever accidentally exercised.
    [Fact]
    public async Task GetResponseAsync_LogsErrorBeforeThrowing()
    {
        var loggerMock = new Mock<ILogger<NotImplementedRouterModelClient>>();
        var client = new NotImplementedRouterModelClient(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(
            () => client.GetResponseAsync("gpt-5.4", "prompt"));

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("gpt-5.4", StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
