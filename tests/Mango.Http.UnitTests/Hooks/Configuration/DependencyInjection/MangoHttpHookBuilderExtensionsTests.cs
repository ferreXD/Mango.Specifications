// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Hooks
{
    using FluentAssertions;
    using Mango.Http.Hooks;
    using Mango.Http.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Options;

    public class MangoHttpHookBuilderExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = null!;
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
        public void WithHooks_NullConfigure_ThrowsArgumentNullException()
        {
            var builder = new DummyClientBuilder { Name = "C" };
            Action act = () => builder.WithHooks(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configure");
        }

        [Fact]
        public void WithHooks_UnnamedBuilder_ThrowsInvalidOperationException()
        {
            var builder = new DummyClientBuilder { Name = null! };
            Action act = () => builder.WithHooks(cfg => { });
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Builder name cannot be null.");
        }

        [Fact]
        public void WithHooks_ValidConfiguration_RegistersOptionsAndHandlerInsertion()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "ClientH" };

            // Act
            builder.WithHooks(cfg => cfg
                .PreAsync((req, ctx, ct) => Task.CompletedTask)
                .PostAsync((resp, ctx, ct) => Task.CompletedTask));

            // Assert DI registrations
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<HttpRequestHookOptions>))
                .Should().BeTrue();
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                .Should().BeTrue();

            // Build service provider for invocation
            builder.Services.AddSingleton<IHttpClientMetricsProvider, NoOpHttpClientMetricsProvider>(); // for completeness
            var sp = builder.Services.BuildServiceProvider();

            // Validate HttpRequestHookOptions
            var monitor = sp.GetRequiredService<IOptionsMonitor<HttpRequestHookOptions>>();
            var opts = monitor.Get("ClientH");
            opts.PreRequestAsync.Should().NotBeNull();
            opts.PostResponseAsync.Should().NotBeNull();

            // Simulate handler builder actions
            var factoryOptions = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("ClientH");
            var hb = new TestHandlerBuilder("ClientH", sp);
            foreach (var action in factoryOptions.HttpMessageHandlerBuilderActions)
                action(hb);

            // Handler inserted at MangoHttpHandlerOrder.Hooks index
            var inserted = hb.AdditionalHandlers.OfType<MangoHttpHookHandler>().SingleOrDefault();
            inserted.Should().NotBeNull();
        }
    }
}
