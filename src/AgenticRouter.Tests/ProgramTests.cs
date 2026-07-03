using AgenticRouter.Hosting;
using AgenticRouter.Models;
using AgenticRouter.Router;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AgenticRouter.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateHostBuilder_BuildsSuccessfully()
        {
            // Arrange
            var args = new string[] { };

            // Act
            var host = Program.CreateHostBuilder(args).Build();

            // Assert
            Assert.NotNull(host);
        }

        // Regression test: Host.CreateDefaultBuilder enables ServiceProviderOptions.ValidateOnBuild (and
        // ValidateScopes) only in the Development environment, so a missing/unresolvable dependency like
        // IRouterModelClient is silently tolerated when CreateHostBuilder_BuildsSuccessfully runs in the
        // default (non-Development) test environment, but throws eagerly at Build() time in Development.
        // This reproduces Program.cs's ConfigureServices registrations directly under that stricter mode,
        // without depending on ambient environment variables during test execution.
        [Fact]
        public void ConfigureServices_ProducesResolvableServiceGraph_UnderValidateOnBuild()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.Configure<RoutingOptions>(_ => { });
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            services.AddAgenticRouter();
            services.AddSingleton<IRouterModelClient, NotImplementedRouterModelClient>();

            using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            Assert.NotNull(provider.GetRequiredService<AgentAsARouter>());
        }
    }
}
