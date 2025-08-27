// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Http.Presets;
    using Mango.Http.Resiliency;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;

    public class MangoResiliencyConfigurationExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = "default";
            public IServiceCollection Services { get; } = new ServiceCollection();
        }

        [Fact]
        public void WithResiliency_NullBuilder_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => MangoResiliencyConfigurationExtensions.WithResiliency(null!, cfg => { });

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("clientBuilder");
        }

        [Fact]
        public void WithResiliency_NullConfigure_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new DummyClientBuilder();

            // Act
            Action act = () => MangoResiliencyConfigurationExtensions.WithResiliency(builder, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("configure");
        }

        [Fact]
        public void WithResiliency_ValidBuilderAndConfigure_ReturnsBuilderAndRegistersServices()
        {
            // Arrange
            var builder = new DummyClientBuilder();
            var initialCount = builder.Services.Count;

            // Act
            var returned = MangoResiliencyConfigurationExtensions.WithResiliency(
                builder,
                cfg => cfg.WithRetry(),
                requireAtLeastOne: false
            );

            // Assert
            returned.Should().BeSameAs(builder);
            // Should have registered options and HttpClientFactoryOptions configuration
            builder.Services.Count.Should().BeGreaterThan(initialCount);
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<ResiliencyOptions>)).Should().BeTrue();
            builder.Services.Any(sd => sd.ServiceType == typeof(IConfigureOptions<Microsoft.Extensions.Http.HttpClientFactoryOptions>)).Should().BeTrue();
        }

        [Fact]
        public void CreateConfiguratorFactory_ConfiguresPoliciesCorrectly()
        {
            // Arrange: set up service provider with preset registry
            var services = new ServiceCollection();
            // Use empty registry stub
            services.AddSingleton<IResiliencyPolicyPresetRegistry>(new DummyRegistry());
            var sp = services.BuildServiceProvider();

            // Access private CreateConfiguratorFactory via reflection
            var createFactoryMethod = typeof(MangoResiliencyConfigurationExtensions)
                .GetMethod("CreateConfiguratorFactory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

            Action<MangoResiliencyPolicyConfigurator> configAction = cfg => cfg.WithTimeout();
            var factoryDelegate = (Func<IServiceProvider, ResiliencyOptions>)createFactoryMethod
                .Invoke(null, new object[] { configAction })!;

            // Act
            var options = factoryDelegate(sp);

            // Assert
            options.Policies.Should().ContainSingle()
                .Which.Should().BeOfType<OperationTimeoutPolicyDefinition>();
        }

        // Dummy registry returning no presets
        private class DummyRegistry : IResiliencyPolicyPresetRegistry
        {
            public IResiliencyPolicyPreset Get(string name)
                => throw new InvalidOperationException("Should not be called");
        }
    }
}
