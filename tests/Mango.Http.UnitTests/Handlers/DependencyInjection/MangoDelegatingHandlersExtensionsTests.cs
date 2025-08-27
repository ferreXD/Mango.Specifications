// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Handlers
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;

    public class MangoDelegatingHandlersExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = null!;
            public IServiceCollection Services { get; } = new ServiceCollection();
            public void Configure(HttpClient client) { }
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

        private class CustomHandler : DelegatingHandler { }

        [Fact]
        public void WithHandler_Generic_RegistersTransientAndInsertsHandlerAtOrder()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "ClientX" };

            // Act
            builder.WithHandler<CustomHandler>(1);
            var sp = builder.Services.BuildServiceProvider();

            // Assert Transient registration
            var handlerInstance1 = sp.GetService<CustomHandler>();
            var handlerInstance2 = sp.GetService<CustomHandler>();
            handlerInstance1.Should().NotBeNull();
            handlerInstance2.Should().NotBeNull();
            handlerInstance1.Should().NotBeSameAs(handlerInstance2, "transient should create new instance");

            // Assert factory options
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("ClientX");
            factoryOpts.HttpMessageHandlerBuilderActions.Should().ContainSingle();
            var hb = new TestHandlerBuilder("ClientX", sp);
            foreach (var action in factoryOpts.HttpMessageHandlerBuilderActions)
                action(hb);

            hb.AdditionalHandlers.Count.Should().BeGreaterThan(0);
            hb.AdditionalHandlers[0].Should().BeOfType<CustomHandler>();
        }

        [Fact]
        public void WithHandler_Generic_DefaultName_UsesDefaultClientName()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = null! };

            // Act
            builder.WithHandler<CustomHandler>(0);
            var sp = builder.Services.BuildServiceProvider();

            // Assert factory options under default name
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("DefaultMangoHttpClient");
            factoryOpts.HttpMessageHandlerBuilderActions.Should().ContainSingle();
        }

        [Fact]
        public void WithHandler_NonGeneric_ValidType_RegistersAndInserts()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "ClientY" };

            // Act
            builder.WithHandler(1, typeof(CustomHandler));
            var sp = builder.Services.BuildServiceProvider();

            // Assert Transient registration via DI
            var instance1 = sp.GetService<CustomHandler>();
            instance1.Should().NotBeNull();

            // Assert handler insertion
            var factoryOpts = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get("ClientY");
            var hb = new TestHandlerBuilder("ClientY", sp);
            foreach (var action in factoryOpts.HttpMessageHandlerBuilderActions)
                action(hb);

            hb.AdditionalHandlers.Should().ContainSingle().Which.Should().BeOfType<CustomHandler>();
        }

        [Fact]
        public void WithHandler_NonGeneric_InvalidType_ThrowsArgumentException()
        {
            // Arrange
            var builder = new DummyClientBuilder { Name = "ClientZ" };

            // Act
            Action act = () => builder.WithHandler(0, typeof(string));

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Type String must inherit from DelegatingHandler.");
        }
    }

}
