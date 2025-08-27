// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.DependencyInjection
{
    using Authorization;
    using Diagnostics;
    using FluentAssertions;
    using Http.Metrics;
    using Http.Presets;
    using Mango.Http.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public class MangoHttpClientExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoHttpClient_InvalidName_ThrowsArgumentException(string name)
        {
            var services = new ServiceCollection();
            Action act = () => services.AddMangoHttpClient(name!, null);
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("clientName")
               .WithMessage("Client must be named.*");
        }

        [Fact]
        public void AddMangoHttpClient_ValidName_RegistersServicesAndReturnsBuilder()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<HttpClient> clientConfig = c => c.Timeout = TimeSpan.FromSeconds(1);

            // Act
            var builder = services.AddMangoHttpClient("myClient", clientConfig);

            // Assert builder
            builder.Should().NotBeNull();
            builder.Name.Should().Be("myClient");
            builder.Services.Should().BeSameAs(services);

            // Assert DI registrations
            // HttpClientFactory registration
            var hasHttpClient = services.Any(d =>
                d.ServiceType == typeof(IHttpClientFactory) ||
                d.ServiceType.FullName == "Microsoft.Extensions.Http.IHttpClientFactory");
            hasHttpClient.Should().BeTrue("AddHttpClient should register the named HttpClientFactory");

            // Resiliency and authentication preset registries
            services.Should().ContainSingle(d => d.ServiceType == typeof(IResiliencyPolicyPresetRegistry)
                                                && d.ImplementationType == typeof(DefaultResiliencyPolicyPresetRegistry));
            services.Should().ContainSingle(d => d.ServiceType == typeof(IAuthenticationStrategyPresetRegistry)
                                                && d.ImplementationType == typeof(DefaultAuthenticationStrategyPresetRegistry));

            // Diagnostics handler
            services.Should().ContainSingle(d => d.ServiceType == typeof(IResiliencyDiagnostics)
                                                && d.ImplementationType == typeof(DefaultResiliencyDiagnostics));

            // Default metrics provider
            services.Should().ContainSingle(d => d.ServiceType == typeof(IHttpClientMetricsProvider)
                                                && d.ImplementationType == typeof(NoOpHttpClientMetricsProvider));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoHttpClientWithOpenTelemetry_InvalidName_ThrowsArgumentException(string name)
        {
            var services = new ServiceCollection();
            Action<HttpClient> cfg = null!;
            Action act = () => services.AddMangoHttpClientWithOpenTelemetry(name!, cfg);
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("clientName");
        }

        [Fact]
        public void AddMangoHttpClientWithOpenTelemetry_ValidName_RegistersOpenTelemetryAndReturnsBuilder()
        {
            var services = new ServiceCollection();
            Action<HttpClient> clientConfig = null!;
            var builder = services.AddMangoHttpClientWithOpenTelemetry("otelClient", clientConfig);

            builder.Should().NotBeNull();
            builder.Name.Should().Be("otelClient");

            // Should include registrations from AddMangoHttpClient
            services.Should().Contain(d => d.ServiceType == typeof(IResiliencyPolicyPresetRegistry));
            services.Should().Contain(d => d.ServiceType == typeof(IAuthenticationStrategyPresetRegistry));
            services.Should().Contain(d => d.ServiceType == typeof(IResiliencyDiagnostics));

            // And from AddMangoOpenTelemetryDefaults
            services.Should().Contain(d => d.ServiceType == typeof(IMangoHttpLogger)); // Cannot check implementation type here as it uses a factory
            services.Should().Contain(d => d.ServiceType == typeof(IHttpClientMetricsProvider)
                                           && d.ImplementationType == typeof(OpenTelemetryHttpClientMetricsProvider));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddDefaultMangoHttpClient_InvalidName_ThrowsArgumentException(string name)
        {
            var services = new ServiceCollection();
            Action<HttpClient> cfg = _ => { };
            Action act = () => services.AddDefaultMangoHttpClient(name!, cfg);
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("clientName");
        }

        [Fact]
        public void AddDefaultMangoHttpClient_ValidName_RegistersDefaultLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();

            // Act
            services.AddDefaultMangoHttpClient("defClient", c => { });

            // Assert IMangoHttpLogger registration
            var desc = services.FirstOrDefault(d => d.ServiceType == typeof(IMangoHttpLogger));
            desc.Should().NotBeNull();
            desc.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // The factory should produce DefaultHttpLogger when dependencies are present
            var sp = services.BuildServiceProvider();
            var logger = sp.GetRequiredService<IMangoHttpLogger>();
            logger.Should().BeOfType<DefaultHttpLogger>();
        }

        [Fact]
        public void AddDefaultMangoHttpClientWithOpenTelemetry_RegistersFullPipeline()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.AddMangoOpenTelemetryDefaults();

            // Act
            services.AddDefaultMangoHttpClientWithOpenTelemetry("clientX", c => { });

            // Assert that IMangoHttpLogger was overridden to DefaultHttpLogger (via WithDefaults<T>)
            var loggerDesc = services.Last(d => d.ServiceType == typeof(IMangoHttpLogger));
            loggerDesc.ImplementationFactory.Should().NotBeNull();

            // Presence of headers/metrics configuration implies no exceptions and registrations exist
            services.Should().Contain(d => d.ServiceType == typeof(IHttpClientMetricsProvider));
        }

        [Fact]
        public void AddDefaultMangoHttpClientT_Generic_RegistersPipeline()
        {
            var services = new ServiceCollection();
            // Should not throw
            Action act = () => services.AddDefaultMangoHttpClient<OpenTelemetryHttpLogger>("genClient", c => { });
            act.Should().NotThrow();

            // Should register IMangoHttpLogger mapping
            services.Should().Contain(d => d.ServiceType == typeof(IMangoHttpLogger));
        }

        [Fact]
        public void WithDefaultsOnBuilder_NullBuilder_ThrowsArgumentNullException()
        {
            IMangoHttpClientBuilder nullBuilder = null!;
            Action act = () => MangoHttpClientExtensions.WithDefaults<DefaultHttpLogger>(nullBuilder!);
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("builder");
        }
    }
}
