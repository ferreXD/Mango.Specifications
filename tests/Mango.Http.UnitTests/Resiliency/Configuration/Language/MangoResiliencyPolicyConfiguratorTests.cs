// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;

    public class MangoResiliencyPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeEmptyCollections()
        {
            // Arrange & Act
            var config = new MangoResiliencyPolicyConfigurator();

            // Assert
            config.PolicyActions.Should().BeEmpty();
            config.Presets.Should().BeEmpty();
        }

        [Fact]
        public void ChainingMethods_ShouldReturnSelf_AndAddCorrectPolicyActions()
        {
            // Arrange
            var config = new MangoResiliencyPolicyConfigurator();

            // Act
            var returned = config
                .WithTimeout()
                .WithRetry()
                .WithCircuitBreaker()
                .WithBulkhead()
                .WithFallback()
                .WithFallbackOnBreak()
                .WithCustomPolicy(_ => { });

            // Assert
            returned.Should().BeSameAs(config);
            config.PolicyActions.Should().HaveCount(7);
        }

        [Fact]
        public void WithPreset_ValidName_ShouldAddToPresets()
        {
            // Arrange
            var config = new MangoResiliencyPolicyConfigurator();

            // Act
            var returned = config.WithPreset("MyPreset");

            // Assert
            returned.Should().BeSameAs(config);
            config.Presets.Should().ContainSingle().Which.Should().Be("MyPreset");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithPreset_InvalidName_ShouldThrow(string name)
        {
            // Arrange
            var config = new MangoResiliencyPolicyConfigurator();

            // Act
            Action act = () => config.WithPreset(name!);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Preset name cannot be null or whitespace.*")
               .And.ParamName.Should().Be("presetName");
        }
    }
}
