using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Publishes <see cref="RoutingTelemetryEvent"/>s to every connected dashboard.
/// </summary>
public interface ITelemetryPublisher
{
    /// <summary>
    /// Publishes an event. Must never throw and must never meaningfully block the caller (request
    /// forwarding must not slow down because a dashboard is slow to receive, or because no dashboard
    /// is connected at all) - implementations are responsible for fault isolation.
    /// </summary>
    Task PublishAsync(RoutingTelemetryEvent telemetryEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="ITelemetryPublisher"/>. A mutable bridge between the outer application's DI
/// container (where <see cref="AgenticRouter.Proxy.ProxyMiddleware"/> lives and where this type is
/// registered as a singleton) and the inner Kestrel host's own, deliberately separate DI container
/// (where SignalR's real <see cref="IHubContext{TelemetryHub}"/> lives - see the constructor remarks
/// on <see cref="AgenticRouter.Proxy.ProxyServer"/> for why the two containers are kept apart).
/// <see cref="AttachHubContext"/> is called once, after the inner host starts, to connect the two.
/// Before that call (or if it's never called, e.g. in tests that construct <see cref="AgenticRouter.Proxy.ProxyMiddleware"/>
/// directly without a running <see cref="AgenticRouter.Proxy.ProxyServer"/>), publishing is a safe no-op.
/// </summary>
public sealed class TelemetryPublisher : ITelemetryPublisher
{
    private const string ClientMethodName = "RoutingTelemetry";

    private readonly ILogger<TelemetryPublisher>? _logger;
    private volatile IHubContext<TelemetryHub>? _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryPublisher"/> class.
    /// </summary>
    /// <param name="logger">Optional logger. Publish failures are logged at Debug level, not surfaced, since they must never affect request handling.</param>
    public TelemetryPublisher(ILogger<TelemetryPublisher>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Connects this publisher to the real SignalR hub context, once the inner Kestrel host has
    /// started and that context exists. Safe to call more than once (the last call wins); safe to
    /// never call (publishing simply stays a no-op).
    /// </summary>
    public void AttachHubContext(IHubContext<TelemetryHub> hubContext)
    {
        ArgumentNullException.ThrowIfNull(hubContext);
        _hubContext = hubContext;
    }

    /// <inheritdoc />
    public async Task PublishAsync(RoutingTelemetryEvent telemetryEvent, CancellationToken cancellationToken = default)
    {
        var hubContext = _hubContext;
        if (hubContext is null)
        {
            return;
        }

        try
        {
            await hubContext.Clients.All.SendAsync(ClientMethodName, telemetryEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // Telemetry is best-effort observability, never a request-handling dependency: a
            // disconnected/slow client, or any other SignalR failure, must never surface here.
            _logger?.LogDebug(ex, "Failed to publish routing telemetry event; continuing without it.");
        }
    }
}
