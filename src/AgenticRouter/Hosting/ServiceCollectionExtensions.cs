using AgenticRouter.Models;
using AgenticRouter.Proxy;
using AgenticRouter.Router;
using AgenticRouter.Telemetry;
using AgenticRouter.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AgenticRouter.Hosting
{
    /// <summary>
    /// Extension methods for setting up agentic router services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core services for the agentic router to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddAgenticRouter(this IServiceCollection services)
        {
            // Core Router
            services.AddSingleton<IRouterMemoryStore, JsonRouterMemoryStore>();
            services.AddSingleton<RouterMemory>();
            // Note: IRouterModelClient is not registered here as it will be context-specific.
            // It should be provided by a factory or a more specific DI scope.
            services.AddTransient<AgentAsARouter>();

            // Tools
            services.AddTransient<CheckSyntax>();
            services.AddTransient<RunVisibleTests>();
            services.AddTransient<EstimateQuality>();

            // Proxy
            services.AddOptions<ModelRoutingOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection(ModelRoutingOptions.SectionName).Bind(options));
            services.AddSingleton<IEnvironmentVariableProvider, EnvironmentVariableProvider>();
            services.AddSingleton<IModelRouteResolver, ModelRouteResolver>();
            services.AddSingleton<RequestInterceptor>();

            // Telemetry (live routing events broadcast to GUI dashboards over SignalR - see
            // docs/router/telemetry.md and Telemetry/TelemetryPublisher.cs for the outer/inner
            // DI-container bridging this singleton is responsible for).
            services.AddOptions<PricingOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection(PricingOptions.SectionName).Bind(options));
            services.AddSingleton<ISessionIdResolver, SessionIdResolver>();
            services.AddSingleton<IConversationTurnTracker, ConversationTurnTracker>();
            services.AddSingleton<IUsageExtractor, UsageExtractor>();
            services.AddSingleton<TelemetryPublisher>();
            services.AddSingleton<ITelemetryPublisher>(sp => sp.GetRequiredService<TelemetryPublisher>());

            services.AddSingleton<ProxyMiddleware>();

            // ProxyServer's inner Kestrel host is handed an already-constructed ProxyMiddleware instance rather
            // than a copy of this IServiceCollection. It never gets its own IHostedService registrations, so it
            // can never end up recursively constructing another ProxyHostedService.
            services.AddHostedService(sp =>
            {
                return new ProxyHostedService(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ProxyHostedService>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ProxyServer>>(),
                    sp.GetRequiredService<ProxyMiddleware>(),
                    telemetryPublisher: sp.GetRequiredService<TelemetryPublisher>());
            });

            return services;
        }
    }
}
