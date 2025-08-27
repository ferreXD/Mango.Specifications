// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Presets
{
    using FluentAssertions;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;

    internal class DummyPreset(string name) : IResiliencyPolicyPreset
    {
        public string Name { get; } = name;
        public bool Configured { get; private set; }
        public void Configure(ResiliencyPolicyOptionsBuilder builder) => Configured = true;
    }

    public class DefaultResiliencyPolicyPresetRegistryTests
    {
        [Fact]
        public void Get_WithExistingName_ReturnsPreset_IgnoringCase()
        {
            // Arrange
            var presetA = new DummyPreset("TestPreset");
            var presetB = new DummyPreset("Another");
            var registry = new DefaultResiliencyPolicyPresetRegistry(new[] { presetA, presetB });

            // Act
            var resultLower = registry.Get("testpreset");
            var resultExact = registry.Get("TestPreset");
            var resultUpper = registry.Get("TESTPRESET");

            // Assert
            resultLower.Should().BeSameAs(presetA);
            resultExact.Should().BeSameAs(presetA);
            resultUpper.Should().BeSameAs(presetA);
        }

        [Fact]
        public void Get_NonExistingName_ThrowsKeyNotFoundException()
        {
            // Arrange
            var preset = new DummyPreset("Exists");
            var registry = new DefaultResiliencyPolicyPresetRegistry(new[] { preset });

            // Act
            Action act = () => registry.Get("Missing");

            // Assert
            act.Should().Throw<KeyNotFoundException>()
               .WithMessage("Resiliency preset 'Missing' not found.");
        }

        [Fact]
        public void Get_MultiplePresets_FirstMatchIsReturned()
        {
            // Arrange
            var preset1 = new DummyPreset("Dup");
            var preset2 = new DummyPreset("dup");
            var registry = new DefaultResiliencyPolicyPresetRegistry(new[] { preset1, preset2 });

            // Act
            var result = registry.Get("DUP");

            // Assert
            result.Should().BeSameAs(preset1);
        }
    }
}
