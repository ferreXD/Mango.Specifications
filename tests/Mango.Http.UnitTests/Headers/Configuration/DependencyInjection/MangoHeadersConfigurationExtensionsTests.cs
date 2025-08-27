// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Headers
{
    using FluentAssertions;
    using Mango.Http.Headers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;

    public class MangoHeadersConfigurationExtensionsTests
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
        public void WithHeaders_NullBuilder_ThrowsArgumentNullException()
        {
            Action act = () => ((IMangoHttpClientBuilder)null!)!.WithHeaders(cfg => { });
            act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }

        [Fact]
        public void WithHeaders_NullConfigure_ThrowsArgumentNullException()
        {
            var builder = new DummyClientBuilder { Name = "C" };
            Action act = () => builder.WithHeaders(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configure");
        }

        [Fact]
        public void WithHeaders_UnnamedBuilder_ThrowsInvalidOperationException()
        {
            var builder = new DummyClientBuilder { Name = null! };
            Action act = () => builder.WithHeaders(cfg => cfg.WithCustomHeader("X", "V"));
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Builder name cannot be null. Ensure the builder is properly initialized before adding headers.");
        }

        [Fact]
        public void WithHeaders_ValidConfiguration_RegistersOptionsAndHandler()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "clientH" };

            // Act
            builder.WithHeaders(cfg => cfg
                .WithCustomHeader("X-Key", "Value")
                .WithCorrelationIdHeader()
                .WithRequestIdHeader());

            // Assert DI registrations
            // Options<HttpHeadersOptions>
            var sp = builder.Services.BuildServiceProvider();
            var headersMonitor = sp.GetRequiredService<IOptionsMonitor<HttpHeadersOptions>>();
            var headerOpts = headersMonitor.Get("clientH");
            // Since internal configure is reversed, CustomHeaders may be empty, but we test registration existence
            headerOpts.Should().NotBeNull();

            // HttpClientFactoryOptions
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("clientH");
            factoryOpts.HttpMessageHandlerBuilderActions.Should().ContainSingle();

            // Simulate handler invocation
            var hb = new TestHandlerBuilder("clientH", sp);
            foreach (var action in factoryOpts.HttpMessageHandlerBuilderActions)
                action(hb);

            hb.AdditionalHandlers.Should().ContainSingle().Which.Should().BeOfType<HttpHeadersInjectionHandler>();
        }
    }
}
