using AgenticRouter.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AgenticRouter.Proxy
{
    /// <summary>
    /// Represents the proxy server, responsible for building and managing the Kestrel web host.
    /// </summary>
    public class ProxyServer
    {
        private readonly IHost _host;
        private readonly TelemetryPublisher? _telemetryPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyServer"/> class.
        /// </summary>
        /// <param name="logger">The logger for this instance (currently unused by Kestrel wiring, reserved for future diagnostics).</param>
        /// <param name="proxyMiddleware">
        /// The already-constructed middleware instance used to handle every request. Passed directly, rather than
        /// copying the application's DI container into the inner host, so the inner host can never end up with its
        /// own copy of application-level hosted service registrations (which previously caused unbounded recursive
        /// construction of <see cref="AgenticRouter.Hosting.ProxyHostedService"/>).
        /// </param>
        /// <param name="port">
        /// The localhost port Kestrel listens on. Defaults to 5001. Pass 0 to bind an ephemeral port (useful in
        /// tests to avoid flaking when the default port is already in use); the resolved address is available via
        /// <see cref="Addresses"/> once <see cref="StartAsync"/> completes.
        /// </param>
        /// <param name="telemetryPublisher">
        /// The outer application's <see cref="TelemetryPublisher"/> singleton (see its remarks for why this needs
        /// to be the concrete type, not <see cref="ITelemetryPublisher"/>: <see cref="StartAsync"/> attaches the
        /// inner host's real <see cref="IHubContext{TelemetryHub}"/> to it once Kestrel is listening). Optional and
        /// defaults to <see langword="null"/> so existing callers/tests that construct a plain proxy-forwarding
        /// server, with no telemetry hub, are unaffected; when null, the Kestrel pipeline still adds routing and
        /// maps <c>/telemetry/hub</c> (so the endpoint always exists), it just has nothing to attach events to.
        /// </param>
        public ProxyServer(ILogger<ProxyServer> logger, ProxyMiddleware proxyMiddleware, int port = 5001, TelemetryPublisher? telemetryPublisher = null)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(proxyMiddleware);
            ArgumentOutOfRangeException.ThrowIfNegative(port);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65535);

            _telemetryPublisher = telemetryPublisher;

            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        if (port == 0)
                        {
                            // ListenLocalhost throws for port 0. Bind a single IPv4 loopback address instead of
                            // dual-stack, since binding IPv4 and IPv6 separately for an ephemeral port would
                            // assign two different port numbers.
                            options.Listen(IPAddress.Loopback, port);
                        }
                        else
                        {
                            // Preserve dual-stack (IPv4 + IPv6) localhost binding for fixed ports.
                            options.ListenLocalhost(port);
                        }
                    });

                    // SignalR is registered into this inner host's own DI container (deliberately separate from
                    // the outer application container - see the constructor remarks above), not the outer one.
                    webBuilder.ConfigureServices(services => services.AddSignalR());

                    webBuilder.Configure(app =>
                    {
                        // UseRouting + a mapped hub handles only requests matching /telemetry/hub (and its
                        // SignalR negotiate/connect sub-paths); every other request - which is all real LLM API
                        // traffic - falls through unmatched to the terminal app.Run below, completely unchanged
                        // from before this endpoint existed.
                        app.UseRouting();
                        app.UseEndpoints(endpoints => endpoints.MapHub<TelemetryHub>("/telemetry/hub"));
                        app.Run(context => proxyMiddleware.InvokeAsync(context, _ => Task.CompletedTask));
                    });
                })
                .Build();
        }

        /// <summary>
        /// Gets the addresses Kestrel is actually listening on. Only meaningful after <see cref="StartAsync"/> completes.
        /// </summary>
        public IReadOnlyCollection<string> Addresses
        {
            get
            {
                var addresses = _host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
                return addresses is null ? [] : new List<string>(addresses);
            }
        }

        /// <summary>
        /// Starts the proxy server. Once Kestrel is listening, connects the outer application's
        /// <see cref="TelemetryPublisher"/> (if one was supplied) to this instance's real
        /// <see cref="IHubContext{TelemetryHub}"/>, so <see cref="Proxy.ProxyMiddleware"/> (constructed in
        /// the outer container, before this inner host existed) can publish through it.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _host.StartAsync(cancellationToken);

            _telemetryPublisher?.AttachHubContext(_host.Services.GetRequiredService<IHubContext<TelemetryHub>>());
        }

        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _host.StopAsync(cancellationToken);
        }
    }
}
