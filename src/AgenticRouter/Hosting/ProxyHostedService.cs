using AgenticRouter.Proxy;
using AgenticRouter.Telemetry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AgenticRouter.Hosting
{
    /// <summary>
    /// A hosted service that manages the lifecycle of the proxy server.
    /// </summary>
    public class ProxyHostedService : IHostedService
    {
        private readonly ILogger<ProxyHostedService> _logger;
        private readonly ProxyServer _proxyServer;

        public ProxyHostedService(
            ILogger<ProxyHostedService> logger,
            ILogger<ProxyServer> proxyLogger,
            ProxyMiddleware proxyMiddleware,
            int port = 5001,
            TelemetryPublisher? telemetryPublisher = null)
        {
            _logger = logger;
            _proxyServer = new ProxyServer(proxyLogger, proxyMiddleware, port, telemetryPublisher);
        }

        /// <summary>
        /// Gets the addresses the underlying <see cref="ProxyServer"/> is actually listening on. Only meaningful
        /// after <see cref="StartAsync"/> completes.
        /// </summary>
        public System.Collections.Generic.IReadOnlyCollection<string> Addresses => _proxyServer.Addresses;

        /// <summary>
        /// Starts the proxy server.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Proxy Hosted Service is starting.");
            return _proxyServer.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Proxy Hosted Service is stopping.");
            return _proxyServer.StopAsync(cancellationToken);
        }
    }
}
