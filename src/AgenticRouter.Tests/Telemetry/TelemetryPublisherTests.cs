using AgenticRouter.Telemetry;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="TelemetryPublisher"/>: the outer/inner DI-container bridge to SignalR.</summary>
public class TelemetryPublisherTests
{
    private static RoutingTelemetryEvent SampleEvent() => new(
        SessionId: "sess-1",
        TurnNumber: 1,
        IsSessionSynthesized: false,
        RequestedModel: "gpt-5.4",
        ResolvedModel: "gpt-5.4",
        Provider: "openai",
        IsFallback: false,
        PromptTokens: 100,
        CompletionTokens: 20,
        EstimatedCostUsd: 0.001m,
        IsStreaming: false,
        LatencyToHeadersMs: 250,
        TotalDurationMs: 800,
        StatusCode: 200,
        TimestampUtc: DateTimeOffset.UtcNow);

    [Fact]
    public async Task PublishAsync_BeforeHubContextAttached_CompletesWithoutThrowing()
    {
        var publisher = new TelemetryPublisher();

        await publisher.PublishAsync(SampleEvent());
    }

    [Fact]
    public async Task PublishAsync_AfterAttach_SendsToAllClientsWithExpectedMethodName()
    {
        var clientProxyMock = new Mock<IClientProxy>();
        var hubClientsMock = new Mock<IHubClients>();
        hubClientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        var hubContextMock = new Mock<IHubContext<TelemetryHub>>();
        hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

        var publisher = new TelemetryPublisher();
        publisher.AttachHubContext(hubContextMock.Object);

        var telemetryEvent = SampleEvent();
        await publisher.PublishAsync(telemetryEvent);

        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "RoutingTelemetry",
                It.Is<object?[]>(args => args.Length == 1 && ReferenceEquals(args[0], telemetryEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void AttachHubContext_Null_Throws()
    {
        var publisher = new TelemetryPublisher();

        Assert.Throws<ArgumentNullException>(() => publisher.AttachHubContext(null!));
    }

    [Fact]
    public async Task PublishAsync_SendFails_DoesNotThrow()
    {
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock
            .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("hub connection failed"));
        var hubClientsMock = new Mock<IHubClients>();
        hubClientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        var hubContextMock = new Mock<IHubContext<TelemetryHub>>();
        hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

        var publisher = new TelemetryPublisher();
        publisher.AttachHubContext(hubContextMock.Object);

        // Must not throw: telemetry publishing failures must never affect the caller (the proxy's
        // request-handling path).
        await publisher.PublishAsync(SampleEvent());
    }

    [Fact]
    public async Task AttachHubContext_CalledTwice_LastAttachmentWins()
    {
        var firstClientProxyMock = new Mock<IClientProxy>();
        var firstHubClientsMock = new Mock<IHubClients>();
        firstHubClientsMock.Setup(c => c.All).Returns(firstClientProxyMock.Object);
        var firstHubContextMock = new Mock<IHubContext<TelemetryHub>>();
        firstHubContextMock.Setup(h => h.Clients).Returns(firstHubClientsMock.Object);

        var secondClientProxyMock = new Mock<IClientProxy>();
        var secondHubClientsMock = new Mock<IHubClients>();
        secondHubClientsMock.Setup(c => c.All).Returns(secondClientProxyMock.Object);
        var secondHubContextMock = new Mock<IHubContext<TelemetryHub>>();
        secondHubContextMock.Setup(h => h.Clients).Returns(secondHubClientsMock.Object);

        var publisher = new TelemetryPublisher();
        publisher.AttachHubContext(firstHubContextMock.Object);
        publisher.AttachHubContext(secondHubContextMock.Object);

        await publisher.PublishAsync(SampleEvent());

        firstClientProxyMock.Verify(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
        secondClientProxyMock.Verify(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
