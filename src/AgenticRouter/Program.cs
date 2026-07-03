using AgenticRouter.Hosting;
using AgenticRouter.Router;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AgenticRouter;

/// <summary>
/// Application entrypoint for the AgenticRouter console host.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();
            Log.Information("AgenticRouter host created.");
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "AgenticRouter host terminated unexpectedly.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddAgenticRouter();

                // AddAgenticRouter registers AgentAsARouter (Transient) but deliberately does not register
                // IRouterModelClient, since it's meant to be context-specific. The proxy doesn't currently
                // invoke AgentAsARouter at all, but Host.CreateDefaultBuilder enables
                // ServiceProviderOptions.ValidateOnBuild in the Development environment, which eagerly
                // validates every registered service's constructor dependencies at startup regardless of
                // whether it's ever resolved. Register a placeholder that throws if actually invoked, rather
                // than letting startup fail for a subsystem that isn't wired up yet.
                services.AddSingleton<IRouterModelClient, NotImplementedRouterModelClient>();
            });
}

