// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.DependencyInjection
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Mango.Http.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class MangoTypedHttpClientExtensionsTests
    {
        // A type with the required HttpClient constructor
        public class GoodClient(HttpClient client)
        {
            public HttpClient Client { get; } = client;
        }

        // A type without the required constructor
        public class BadClient
        {
            public BadClient() { }
        }

        // A base client class with HttpClient constructor
        public interface ITestService
        {
            HttpClient Client { get; }
            void DoSomething();
        }

        // A “bad” derived client lacking a HttpClient constructor
        public class BadDerivedClient : ITestService
        {
            public HttpClient Client { get; } = null!;
            public BadDerivedClient() { }
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }

        // A “good” derived client with exactly the HttpClient constructor
        public class GoodDerivedClient : ITestService
        {
            public HttpClient Client { get; }

            public GoodDerivedClient(HttpClient client)
            {
                Client = client ?? throw new ArgumentNullException(nameof(client));
            }

            public void DoSomething()
            {
                // Implementation here
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoTypedHttpClient_InvalidName_Throws(string name)
        {
            var services = new ServiceCollection();
            Action act = () => services.AddMangoTypedHttpClient<GoodClient>(name!, null);
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("clientName")
               .WithMessage("Client must be named.*");
        }

        [Fact]
        public void AddMangoTypedHttpClient_BadClientType_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();
            Action act = () => services.AddMangoTypedHttpClient<BadClient>("myClient", null);
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Type BadClient must have a single constructor accepting HttpClient.");
        }

        [Fact]
        public void AddMangoTypedHttpClient_ValidRegistersAndReturnsBuilder()
        {
            var services = new ServiceCollection();
            var builder = services.AddMangoTypedHttpClient<GoodClient>("gc", c => c.Timeout = TimeSpan.FromSeconds(5));

            builder.Should().NotBeNull();
            builder.Name.Should().Be("gc");
            builder.Services.Should().BeSameAs(services);

            // Ensure that the typed client is resolvable
            services.AddLogging();
            var sp = services.BuildServiceProvider();
            var gc = sp.GetRequiredService<GoodClient>();
            gc.Should().NotBeNull();
            gc.Client.Timeout.Should().Be(TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoTypedHttpClientWithOpenTelemetry_InvalidName_Throws(string name)
        {
            var services = new ServiceCollection();
            Action act = () => services.AddMangoTypedHttpClientWithOpenTelemetry<GoodClient>(name!, null);
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("clientName");
        }

        [Fact]
        public void AddMangoTypedHttpClientWithOpenTelemetry_BadClientType_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();
            Action act = () => services.AddMangoTypedHttpClientWithOpenTelemetry<BadClient>("c", null);
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Type BadClient must have a single constructor accepting HttpClient.");
        }

        [Fact]
        public void AddMangoTypedHttpClientWithOpenTelemetry_ValidRegistersAndReturnsBuilder()
        {
            var services = new ServiceCollection();
            // need OpenTelemetry defaults to satisfy underlying dependencies
            services.AddMangoOpenTelemetryDefaults();

            var builder = services.AddMangoTypedHttpClientWithOpenTelemetry<GoodClient>("gc2", client => { });

            builder.Should().NotBeNull();
            builder.Name.Should().Be("gc2");

            // Ensure typed client and mango logger & metrics provider are registered
            var sp = services.AddLogging().AddOptions().BuildServiceProvider();
            sp.GetRequiredService<IMangoHttpLogger>().Should().BeOfType<OpenTelemetryHttpLogger>();
            sp.GetRequiredService<IHttpClientMetricsProvider>().Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // client can be resolved
            var gc = sp.GetRequiredService<GoodClient>();
            gc.Should().NotBeNull();
        }

        [Fact]
        public void AddDefaultMangoTypedHttpClient_AppliesDefaults()
        {
            var services = new ServiceCollection();
            services.AddOptions().AddLogging();

            var builder = services.AddDefaultMangoTypedHttpClient<GoodClient>("dgc", client => client.BaseAddress = new Uri("http://x/"));

            builder.Should().NotBeNull();
            builder.Name.Should().Be("dgc");

            var sp = services.BuildServiceProvider();
            // DefaultHttpLogger should be registered via WithDefaults<DefaultHttpLogger>
            sp.GetRequiredService<IMangoHttpLogger>().Should().BeOfType<DefaultHttpLogger>();

            // GoodClient resolvable
            var gc = sp.GetRequiredService<GoodClient>();
            gc.Should().NotBeNull();
            gc.Client.BaseAddress.Should().Be(new Uri("http://x/"));
        }

        [Fact]
        public void AddDefaultMangoTypedHttpClientWithOpenTelemetry_AppliesDefaultsAndOpenTelemetry()
        {
            var services = new ServiceCollection();
            services.AddOptions().AddLogging().AddMangoOpenTelemetryDefaults();

            var builder = services.AddDefaultMangoTypedHttpClientWithOpenTelemetry<GoodClient>("dgc2", client => client.DefaultRequestHeaders.Add("X", "Y"));

            builder.Should().NotBeNull();
            builder.Name.Should().Be("dgc2");

            var sp = services.BuildServiceProvider();
            // Should end up with OpenTelemetryHttpLogger
            sp.GetRequiredService<IMangoHttpLogger>().Should().BeOfType<OpenTelemetryHttpLogger>();

            // Metrics provider also OpenTelemetry
            sp.GetRequiredService<IHttpClientMetricsProvider>().Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // GoodClient injection
            var gc = sp.GetRequiredService<GoodClient>();
            gc.Client.DefaultRequestHeaders.Contains("X").Should().BeTrue();
        }

        [Fact]
        public void AddDefaultMangoTypedHttpClient_GenericLogger_AppliesCustomLoggerType()
        {
            var services = new ServiceCollection();
            services.AddOptions().AddLogging().AddMangoOpenTelemetryDefaults();

            var builder = services.AddDefaultMangoTypedHttpClient<GoodClient, OpenTelemetryHttpLogger>("gc3", null);

            builder.Should().NotBeNull();
            builder.Name.Should().Be("gc3");

            var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IMangoHttpLogger>().Should().BeOfType<OpenTelemetryHttpLogger>();

            // GoodClient injection works
            sp.GetRequiredService<GoodClient>().Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoTypedHttpClient_TClientTImpl_InvalidName_Throws(string name)
        {
            var services = new ServiceCollection();
            var act = () =>
                services.AddMangoTypedHttpClient<ITestService, GoodDerivedClient>(name!, null);
            act.Should().Throw<ArgumentException>()
               .WithParameterName("clientName")
               .WithMessage("Client must be named.*");
        }

        [Fact]
        public void AddMangoTypedHttpClient_TClientTImpl_BadImplementation_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();
            var act = () =>
                services.AddMangoTypedHttpClient<ITestService, BadDerivedClient>("myClient", null);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Type BadDerivedClient must have a single constructor accepting HttpClient.");
        }

        [Fact]
        public void AddMangoTypedHttpClient_TClientTImpl_ValidRegistersAndReturnsBuilder()
        {
            var services = new ServiceCollection();
            var builder = services.AddMangoTypedHttpClient<ITestService, GoodDerivedClient>(
                "derivedClient",
                client => client.Timeout = TimeSpan.FromSeconds(123));

            builder.Should().NotBeNull();
            builder.Name.Should().Be("derivedClient");
            builder.Services.Should().BeSameAs(services);

            // Resolve the typed client and verify the HttpClient passed through config
            services.AddLogging(); // needed for builder internals
            var sp = services.BuildServiceProvider();
            var instance = sp.GetRequiredService<ITestService>();
            instance.Should().BeOfType<GoodDerivedClient>();
            instance.Client.Timeout.Should().Be(TimeSpan.FromSeconds(123));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddMangoTypedHttpClientWithOpenTelemetry_TClientTImpl_InvalidName_Throws(string name)
        {
            var services = new ServiceCollection();
            var act = () =>
                services.AddMangoTypedHttpClientWithOpenTelemetry<ITestService, GoodDerivedClient>(name!, null);
            act.Should().Throw<ArgumentException>()
               .WithParameterName("clientName");
        }

        [Fact]
        public void AddMangoTypedHttpClientWithOpenTelemetry_TClientTImpl_BadClientType_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();
            Action act = () =>
                services.AddMangoTypedHttpClientWithOpenTelemetry<ITestService, BadDerivedClient>(
                    "c", null);
            // Code checks typeof(TClient) for ctor(HttpClient)
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Type BadDerivedClient must have a single constructor accepting HttpClient.");
        }

        [Fact]
        public void AddMangoTypedHttpClientWithOpenTelemetry_TClientTImpl_ValidRegistersAndReturnsBuilder()
        {
            var services = new ServiceCollection();
            var builder = services.AddMangoTypedHttpClientWithOpenTelemetry<ITestService, GoodDerivedClient>(
                "otelDerived",
                client => client.DefaultRequestHeaders.Add("X-Test", "42"));

            builder.Should().NotBeNull();
            builder.Name.Should().Be("otelDerived");

            // Resolve logger and metrics provider
            services.AddLogging().AddOptions();
            var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IMangoHttpLogger>()
              .Should().BeOfType<OpenTelemetryHttpLogger>();
            sp.GetRequiredService<IHttpClientMetricsProvider>()
              .Should().BeOfType<OpenTelemetryHttpClientMetricsProvider>();

            // Resolve the typed client
            var instance = sp.GetRequiredService<ITestService>();
            instance.Should().BeOfType<GoodDerivedClient>();
            instance.Client.DefaultRequestHeaders.Contains("X-Test").Should().BeTrue();
        }
    }
}
