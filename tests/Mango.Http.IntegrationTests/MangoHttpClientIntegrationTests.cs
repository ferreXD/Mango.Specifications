namespace Mango.Http.IntegrationTests
{
    using FluentAssertions;
    using Logging;
    using Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class MangoHttpClientIntegrationTests
    {
        private const string ClientName = "test-client";

        private ServiceProvider BuildProvider(Action<IServiceCollection> setup)
        {
            var services = new ServiceCollection()
                // required for logging and options in default‐client registrations
                .AddLogging()
                .AddOptions();

            setup(services);
            return services.BuildServiceProvider();
        }

        [Fact]
        public void AddMangoHttpClient_RegistersNamedClient_AndAppliesClientConfig()
        {
            var sp = BuildProvider(services =>
            {
                services.AddMangoHttpClient(
                    ClientName,
                    client => client.Timeout = TimeSpan.FromSeconds(42));
            });

            // resolve the factory and create the client
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(ClientName);

            client.Timeout.Should().Be(TimeSpan.FromSeconds(42));
        }

        [Fact]
        public void AddMangoHttpClient_WithOpenTelemetry_RegistersLoggerAndMetricsProvider()
        {
            var sp = BuildProvider(services => { services.AddMangoHttpClientWithOpenTelemetry(ClientName, null); });

            // IMangoHttpLogger should be OpenTelemetryHttpLogger
            var mangoLogger = sp.GetRequiredService<IMangoHttpLogger>();
            mangoLogger.Should().BeOfType<OpenTelemetryHttpLogger>();

            // IHttpClientMetricsProvider should be OpenTelemetryHttpClientMetricsProvider
            var metrics = sp.GetRequiredService<IHttpClientMetricsProvider>();
            metrics.Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // and the named client still exists
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(ClientName);
            client.Should().NotBeNull();
        }

        [Fact]
        public void AddDefaultMangoHttpClient_RegistersDefaultHttpLogger()
        {
            var sp = BuildProvider(services => { services.AddDefaultMangoHttpClient(ClientName, cli => { }); });

            // our IMangoHttpLogger should now be DefaultHttpLogger
            var logger = sp.GetRequiredService<IMangoHttpLogger>();
            logger.Should().BeOfType<DefaultHttpLogger>();

            // and we can still GetClient
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            factory.CreateClient(ClientName).Should().NotBeNull();
        }

        [Fact]
        public void AddDefaultMangoHttpClientWithOpenTelemetry_OverridesToOpenTelemetryLogger()
        {
            var sp = BuildProvider(services =>
            {
                // ensure OpenTelemetry defaults are in place first
                services.AddDefaultMangoHttpClientWithOpenTelemetry(ClientName, cli => { });
            });

            // IMangoHttpLogger should resolve to OpenTelemetryHttpLogger
            var logger = sp.GetRequiredService<IMangoHttpLogger>();
            logger.Should().BeOfType<OpenTelemetryHttpLogger>();

            // metrics provider still registered
            sp.GetRequiredService<IHttpClientMetricsProvider>()
                .Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // named client exists
            sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ClientName).Should().NotBeNull();
        }

        [Fact]
        public void AddDefaultMangoHttpClient_WithCustomLoggerType_RegistersThatLogger()
        {
            var sp = BuildProvider(services =>
            {
                services.AddLogging();
                services.AddOptions();

                // Register a custom logger type
                services.AddDefaultMangoHttpClient<MangoLoggerStub>(
                    ClientName,
                    cli => { });
            });

            // IMangoHttpLogger should now be the specified generic logger
            var logger = sp.GetRequiredService<IMangoHttpLogger>();
            logger.Should().BeOfType<MangoLoggerStub>();

            // and the client factory still works
            sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ClientName).Should().NotBeNull();
        }

        [Fact]
        public void WithDefaultsOnBuilder_Null_ThrowsArgumentNullException()
        {
            IMangoHttpClientBuilder nullBuilder = null!;
            Action act = () => nullBuilder!.WithDefaults<DefaultHttpLogger>();
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("builder");
        }

        private sealed class MangoLoggerStub : IMangoHttpLogger
        {
            public Task LogRequestAsync(HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }

            public Task LogResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
            {
                throw new NotImplementedException();
            }

            public Task LogErrorAsync(HttpRequestMessage request, Exception ex, TimeSpan elapsed)
            {
                throw new NotImplementedException();
            }
        }
    }
}
