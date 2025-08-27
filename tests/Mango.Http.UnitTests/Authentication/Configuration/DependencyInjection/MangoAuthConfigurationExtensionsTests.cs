// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System.Diagnostics;

    class FakeMangoHttpClientBuilder(string name) : IMangoHttpClientBuilder
    {
        public string Name { get; set; } = name;
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    public class MangoAuthConfigurationExtensionsTests
    {
        [Fact]
        public void AddAuthPresets_ReturnsSameCollection_AndRegistersRegistryWithPresets()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returned = services.AddAuthPresets(cfg =>
                cfg.WithPreset("One", b => { })
                   .WithPreset("Two", b => { }));

            // Assert: fluent
            returned.Should().BeSameAs(services);

            // Build provider and resolve registry
            var sp = services.BuildServiceProvider();
            var registry = sp.GetRequiredService<IAuthenticationStrategyPresetRegistry>();

            // The registry should contain both presets, case-insensitive
            registry.Get("one").Name.Should().Be("One");
            registry.Get("TWO").Name.Should().Be("Two");
        }

        [Fact]
        public void WithAuthentication_NullBuilder_ThrowsArgumentNullException()
        {
            IMangoHttpClientBuilder nullBuilder = null!;
            Action act = () => MangoAuthConfigurationExtensions.WithAuthentication(
                nullBuilder!,
                cfg => { });

            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("builder");
        }

        [Fact]
        public void WithAuthentication_NullConfigure_ThrowsArgumentNullException()
        {
            var builder = new FakeMangoHttpClientBuilder("client");
            Action act = () => builder.WithAuthentication(null!);

            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("configure");
        }

        [Fact]
        public void WithAuthentication_UnnamedBuilder_ThrowsInvalidOperationException()
        {
            var builder = new FakeMangoHttpClientBuilder(null!);
            Action act = () => builder.WithAuthentication(cfg => { });

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("HttpClient must be named to configure authentication");
        }

        [Fact]
        public async Task WithAuthentication_ValidConfiguration_RegistersOptionsCorrectly()
        {
            // Arrange
            var clientName = "myClient";
            var token = "tok-123";

            // Prepare builder and ensure registry is available
            var builder = new FakeMangoHttpClientBuilder(clientName);
            builder.Services.AddAuthPresets(cfg => { /* no presets needed here */ });

            // Act: configure authentication
            builder.WithAuthentication(cfg =>
                cfg.Enable()
                   .UseBearerToken(token)
                   .When(req => req.Method == HttpMethod.Get));

            // Build service provider
            builder.Services
                .AddLogging()                            // for ILogger<T>
                .AddSingleton(new ActivitySource("X")); // for ActivitySource
            var sp = builder.Services.BuildServiceProvider();

            // Retrieve named options
            var monitor = sp.GetRequiredService<IOptionsMonitor<HttpAuthOptions>>();
            var opts = monitor.Get(clientName);

            // Assert options
            opts.Enabled.Should().BeTrue();
            opts.Condition.Should().NotBeNull();
            opts.StrategyFactory.Should().NotBeNull();
            opts.PresetKeys.Should().BeEmpty();

            // Ensure the Condition predicate is applied
            var getRequest = new HttpRequestMessage(HttpMethod.Get, "http://x/");
            opts.Condition!(getRequest).Should().BeTrue();
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "http://x/");
            opts.Condition!(postRequest).Should().BeFalse();

            // Ensure the strategy factory produces a working BearerTokenAuthStrategy
            var strategy = opts.StrategyFactory!(sp);
            using var req = new HttpRequestMessage(HttpMethod.Get, "http://x/");
            await strategy.ApplyAsync(req, CancellationToken.None);
            req.Headers.Authorization.Should().NotBeNull();
            req.Headers.Authorization!.Scheme.Should().Be("Bearer");
            req.Headers.Authorization.Parameter.Should().Be(token);
        }
    }
}
