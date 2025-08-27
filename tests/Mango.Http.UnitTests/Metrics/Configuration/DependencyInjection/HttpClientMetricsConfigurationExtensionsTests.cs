// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Metrics
{
    using FluentAssertions;
    using Mango.Http.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;

    public class HttpClientMetricsConfigurationExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = "DummyClient";
            public IServiceCollection Services { get; } = new ServiceCollection();
        }


        // Stub class to allow instantiation in tests
        private class TestHandlerBuilder : HttpMessageHandlerBuilder
        {
            public override HttpMessageHandler Build() => throw new NotImplementedException();

            public override string Name { get; set; }
            public override IServiceProvider Services { get; }
            public override HttpMessageHandler PrimaryHandler { get; set; }
            public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

            public TestHandlerBuilder(string name, IServiceProvider services)
            {
                Name = name;
                Services = services;
                PrimaryHandler = new HttpClientHandler();
            }
        }

        [Fact]
        public void WithMetrics_UnnamedClient_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = null! };

            // Act
            Action act = () => builder.WithMetrics();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("HttpClient must be named");
        }

        [Fact]
        public void WithMetrics_DefaultOptions_RegistersOptionAndFactoryConfigurations()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "Client1" };
            var initialCount = builder.Services.Count;

            // Act
            var returned = builder.WithMetrics();

            // Assert
            returned.Should().BeSameAs(builder);
            // Options< HttpClientMetricsOptions > registration
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientMetricsOptions>))
                .Should().BeTrue("HttpClientMetricsOptions should be configured");
            // HttpClientFactoryOptions registration
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Should().BeTrue("HttpClientFactoryOptions should be configured with MetricsHandler insertion");
            // Ensure new registrations added
            builder.Services.Count.Should().BeGreaterThan(initialCount);
        }

        [Fact]
        public void WithMetrics_DisabledMetrics_DoesNotInsertHandler()
        {
            // Arrange: Disable metrics via configure
            var builder = new DummyClientBuilder { Name = "Client2" };

            // Act
            builder.WithMetrics(cfg => cfg.Disable());

            // Assert: We need to simulate DI build and invocation
            var sp = builder.Services
                .AddSingleton<IHttpClientMetricsProvider, NoOpHttpClientMetricsProvider>()
                .BuildServiceProvider();
            var factoryOptions = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("Client2");
            // Build HttpMessageHandlerBuilder stub
            var hb = new TestHandlerBuilder("Client2", sp);

            // Apply builder actions
            foreach (var action in factoryOptions.HttpMessageHandlerBuilderActions)
            {
                action(hb);
            }
            // Metrics disabled => no handler inserted at Metrics index
            hb.AdditionalHandlers.Should().BeEmpty();
        }
    }
}
