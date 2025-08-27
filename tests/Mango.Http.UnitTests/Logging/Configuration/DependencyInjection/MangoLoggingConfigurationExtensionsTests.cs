// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class MangoLoggingConfigurationExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = null!;
            public IServiceCollection Services { get; } = new ServiceCollection();
        }

        private class TestHandlerBuilder : HttpMessageHandlerBuilder
        {
            public override HttpMessageHandler Build() => throw new NotImplementedException();

            public override string Name { get; set; }
            public override IServiceProvider Services { get; }
            public override System.Net.Http.HttpMessageHandler PrimaryHandler { get; set; }
            public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();
            public TestHandlerBuilder(string name, IServiceProvider services)
            {
                Name = name;
                Services = services;
                PrimaryHandler = new HttpClientHandler();
            }
        }

        [Fact]
        public void WithLogging_NullConfigure_ThrowsArgumentNullException()
        {
            var builder = new DummyClientBuilder { Name = "c" };
            Action act = () => builder.WithLogging(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configure");
        }

        [Fact]
        public void WithLogging_UnnamedBuilder_ThrowsInvalidOperationException()
        {
            var builder = new DummyClientBuilder { Name = null! };
            Action act = () => builder.WithLogging(cfg => { });
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("MangoHttpClient must have a name to enable logging.");
        }

        [Fact]
        public void WithLogging_ValidConfiguration_RegistersOptionsAndLoggerAndHandler()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "clientA" };

            // Act
            builder.WithLogging(cfg => cfg
                .Enable()
                .RequestLevel(LogLevel.Information)
                .SuccessResponseLevel(LogLevel.Warning)
                .ErrorLevel(LogLevel.Error)
                .UseLogger<DefaultHttpLogger>());

            builder.Services.AddLogging();
            builder.Services.AddSingleton<IMangoHttpLogger>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultHttpLogger>>();
                var monitor = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>();
                return new DefaultHttpLogger(logger, monitor, "clientA");
            });

            // Assert DI services
            var sp = builder.Services.BuildServiceProvider();
            // Options for HttpLoggingOptions
            var monitor = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>();
            var opts = monitor.Get("clientA");
            opts.Enabled.Should().BeTrue();
            opts.RequestLevel.Should().Be(LogLevel.Information);
            opts.ResponseSuccessLevel.Should().Be(LogLevel.Warning);
            opts.ErrorLevel.Should().Be(LogLevel.Error);
            opts.LoggerType.Should().Be(typeof(DefaultHttpLogger));
            opts.UseCustomHandler.Should().BeFalse();
            // Logger registration
            var logger = sp.GetService<IMangoHttpLogger>();
            logger.Should().BeOfType<DefaultHttpLogger>();
            // HttpClientFactoryOptions
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("clientA");
            factoryOpts.HttpMessageHandlerBuilderActions.Should().ContainSingle();
            // Simulate handler builder
            var hb = new TestHandlerBuilder("clientA", sp);
            foreach (var action in factoryOpts.HttpMessageHandlerBuilderActions)
                action(hb);
            hb.AdditionalHandlers.Should().ContainSingle().Which.Should().BeOfType<HttpLoggingHandler>();
        }

        [Fact]
        public void WithLogging_UseCustomHandler_RegistersCustomHandlerType()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "clientB" };

            // Act
            builder.WithLogging(cfg => cfg
                .Enable()
                .UseCustomHandler<CustomHandler>());

            // Assert DI registrations
            var sp = builder.Services.BuildServiceProvider();
            var monitor = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>();
            var opts = monitor.Get("clientB");
            opts.Enabled.Should().BeTrue();
            opts.UseCustomHandler.Should().BeTrue();
            opts.CustomHandlerType.Should().Be(typeof(CustomHandler));
            // Custom handler registration
            var custom = sp.GetService<CustomHandler>();
            custom.Should().NotBeNull();
            // Handler insertion
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("clientB");
            var hb = new TestHandlerBuilder("clientB", sp);
            foreach (var action in factoryOpts.HttpMessageHandlerBuilderActions)
                action(hb);
            hb.AdditionalHandlers.Should().ContainSingle().Which.Should().BeOfType<CustomHandler>();
        }

        private class CustomHandler : DelegatingHandler { }
    }
}
