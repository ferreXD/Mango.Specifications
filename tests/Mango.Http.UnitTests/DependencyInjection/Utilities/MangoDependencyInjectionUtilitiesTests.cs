// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.DependencyInjection
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Mango.Http.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics;

    public class MangoDependencyInjectionUtilitiesTests
    {
        [Fact]
        public void AddMangoOpenTelemetryDefaults_ReturnsSameCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returned = services.AddMangoOpenTelemetryDefaults();

            // Assert
            returned.Should().BeSameAs(services);
        }

        [Fact]
        public void AddMangoOpenTelemetryDefaults_RegistersRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLogging();
            services.AddMangoOpenTelemetryDefaults();
            var sp = services.BuildServiceProvider();

            // Assert: IMangoHttpLogger → OpenTelemetryHttpLogger
            var logger = sp.GetRequiredService<IMangoHttpLogger>();
            logger.Should().BeOfType<OpenTelemetryHttpLogger>();

            // Assert: IHttpClientMetricsProvider → OpenTelemetryHttpClientMetricsProvider
            var metrics = sp.GetRequiredService<IHttpClientMetricsProvider>();
            metrics.Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // Assert: ActivitySource is registered by AddMangoHttpTracing
            var activitySource = sp.GetService<ActivitySource>();
            activitySource.Should().NotBeNull("AddMangoHttpTracing should register a fallback ActivitySource");
        }
    }
}
