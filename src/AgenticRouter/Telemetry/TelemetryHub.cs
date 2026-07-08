using Microsoft.AspNetCore.SignalR;

namespace AgenticRouter.Telemetry;

/// <summary>
/// SignalR hub dashboards connect to for live routing telemetry. Purely server-to-client push (see
/// <see cref="ITelemetryPublisher"/>) - clients never call methods on this hub, so it declares none.
/// Mapped at <c>/telemetry/hub</c> by <see cref="AgenticRouter.Proxy.ProxyServer"/>, alongside
/// (not replacing) the proxy's existing catch-all request forwarding.
/// </summary>
public sealed class TelemetryHub : Hub;
