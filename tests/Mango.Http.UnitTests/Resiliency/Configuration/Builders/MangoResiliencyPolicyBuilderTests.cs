// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Http.Presets;
    using Mango.Http.Resiliency;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Stub preset interfaces
    internal class DummyPreset(Action<ResiliencyPolicyOptionsBuilder> configure) : IResiliencyPolicyPreset
    {
        public string Name { get; } = "DummyPreset";
        public void Configure(ResiliencyPolicyOptionsBuilder builder) => configure(builder);
    }

    internal class DummyRegistry(Dictionary<string, IResiliencyPolicyPreset> presets) : IResiliencyPolicyPresetRegistry
    {
        public IResiliencyPolicyPreset Get(string name)
            => presets.TryGetValue(name, out var p)
                ? p
                : throw new KeyNotFoundException($"Preset '{name}' not found.");
    }

    public class MangoResiliencyPolicyBuilderTests
    {
        [Fact]
        public void Build_WithoutConfigsOrPresets_ReturnsEmptyOptions()
        {
            // Arrange
            var cfg = new MangoResiliencyPolicyConfigurator();
            var registry = new DummyRegistry(new Dictionary<string, IResiliencyPolicyPreset>());

            // Act
            var options = MangoResiliencyPolicyBuilder.Build(cfg, registry);

            // Assert
            options.Policies.Should().BeEmpty();
        }

        [Fact]
        public void Build_WithCircuitBreakerPresetAndUserOverride_MergesCorrectly()
        {
            // Arrange: preset config sets threshold=2, breakDuration=5s
            var preset = new DummyPreset(builder =>
                builder.WithCircuitBreaker(cb => cb.SetFailureThreshold(2)
                                                  .SetBreakDuration(TimeSpan.FromSeconds(5))));
            var registry = new DummyRegistry(new Dictionary<string, IResiliencyPolicyPreset>
            {
                ["DummyPreset"] = preset
            });
            var cfg = new MangoResiliencyPolicyConfigurator()
                .WithPreset("DummyPreset")
                .WithCircuitBreaker(cb => cb.SetFailureThreshold(7));

            // Act
            var options = MangoResiliencyPolicyBuilder.Build(cfg, registry);

            // Assert: only one policy and it's merged
            var cbDef = options.Policies.OfType<CircuitBreakerPolicyDefinition>().Single();
            cbDef.FailureThreshold.Should().Be(7);      // user override
            cbDef.BreakDuration.Should().Be(TimeSpan.FromSeconds(5)); // preset value retained
        }

        [Fact]
        public void Build_WithUnknownPreset_ThrowsKeyNotFoundException()
        {
            // Arrange
            var cfg = new MangoResiliencyPolicyConfigurator().WithPreset("Unknown");
            var registry = new DummyRegistry(new Dictionary<string, IResiliencyPolicyPreset>());

            // Act
            Action act = () => MangoResiliencyPolicyBuilder.Build(cfg, registry);

            // Assert
            act.Should().Throw<KeyNotFoundException>()
               .WithMessage("Preset 'Unknown' not found.*");
        }

        [Fact]
        public void Build_WithRetryPresetOnly_AddsPresetExactlyOnce()
        {
            // Arrange: preset config sets retryCount=4
            var retryPreset = new DummyPreset(builder =>
                builder.WithRetry(r => r.SetMaxRetryCount(4).SetDelay(TimeSpan.FromMilliseconds(10))));
            var registry = new DummyRegistry(new Dictionary<string, IResiliencyPolicyPreset>
            {
                ["RetryPreset"] = retryPreset
            });
            var cfg = new MangoResiliencyPolicyConfigurator().WithPreset("RetryPreset");

            // Act
            var options = MangoResiliencyPolicyBuilder.Build(cfg, registry);

            // Assert: one retry policy with count=4
            var retryDef = options.Policies.OfType<RetryPolicyDefinition>().Single();
            retryDef.RetryCount.Should().Be(4);
            retryDef.RetryDelay.Should().Be(TimeSpan.FromMilliseconds(10));
        }
    }
}
