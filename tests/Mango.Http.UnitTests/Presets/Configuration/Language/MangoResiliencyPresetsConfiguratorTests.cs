// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Presets
{
    using FluentAssertions;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public class MangoResiliencyPresetsConfiguratorTests
    {
        [Fact]
        public void AddResiliencyPresets_NullServices_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => MangoResiliencyPresetsConfigurator.AddResiliencyPresets(null!, cfg => { });

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("services");
        }

        [Fact]
        public void AddResiliencyPresets_NullConfigure_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddResiliencyPresets(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("configure");
        }

        [Fact]
        public void AddResiliencyPresets_ValidConfiguration_RegistersRegistryWithPresets()
        {
            // Arrange
            var services = new ServiceCollection();
            // Configure two presets
            services.AddResiliencyPresets(cfg =>
            {
                cfg.WithPreset("P1", builder => builder.WithTimeout());
                cfg.WithPreset("P2", builder => builder.WithRetry());
            });

            // Act
            var sp = services.BuildServiceProvider();
            var registry = sp.GetService<IResiliencyPolicyPresetRegistry>();

            // Assert
            registry.Should().NotBeNull();
            // Should have both presets registered
            var preset1 = registry.Get("P1");
            var preset2 = registry.Get("p2"); // case-insensitive
            preset1.Should().NotBeNull();
            preset2.Should().NotBeNull();

            // Ensure that Configure on presets works: apply to builder
            var builder = new ResiliencyPolicyOptionsBuilder();
            preset1.Configure(builder);
            builder.Build().Policies.Should().ContainSingle(p => p is OperationTimeoutPolicyDefinition);

            builder = new ResiliencyPolicyOptionsBuilder();
            preset2.Configure(builder);
            builder.Build().Policies.Should().ContainSingle(p => p is RetryPolicyDefinition);
        }

        [Fact]
        public void AddResiliencyPresets_MultipleCalls_OverrideRegistry()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddResiliencyPresets(cfg => cfg.WithPreset("Only", b => b.WithTimeout()));
            var firstDescriptor = services.Last(sd => sd.ServiceType == typeof(IResiliencyPolicyPresetRegistry));

            // Act: second call
            services.AddResiliencyPresets(cfg => cfg.WithPreset("OnlyAgain", b => b.WithRetry()));
            var secondDescriptor = services.Last(sd => sd.ServiceType == typeof(IResiliencyPolicyPresetRegistry));

            // Assert: each call adds a descriptor
            services.Count(sd => sd.ServiceType == typeof(IResiliencyPolicyPresetRegistry)).Should().Be(2);
            firstDescriptor.ImplementationFactory.Should().NotBeSameAs(secondDescriptor.ImplementationFactory);
        }
    }
}
