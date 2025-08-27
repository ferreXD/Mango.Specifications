// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Presets
{
    using FluentAssertions;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;
    using System;

    public class InlineResiliencyPolicyPresetTests
    {
        [Fact]
        public void Constructor_NullName_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _ = new InlineResiliencyPolicyPreset(null!, _ => { });

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("name");
        }

        [Fact]
        public void Constructor_NullConfigure_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _ = new InlineResiliencyPolicyPreset("preset1", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("configure");
        }

        [Fact]
        public void Name_Property_ShouldReturnConstructorName()
        {
            // Arrange
            var name = "MyPreset";
            var preset = new InlineResiliencyPolicyPreset(name, _ => { });

            // Act
            var result = preset.Name;

            // Assert
            result.Should().Be(name);
        }

        [Fact]
        public void Configure_NullBuilder_ShouldThrowArgumentNullException()
        {
            // Arrange
            var preset = new InlineResiliencyPolicyPreset("preset", builder => { });

            // Act
            Action act = () => preset.Configure(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("builder");
        }

        [Fact]
        public void Configure_ShouldInvokeConfigureAction()
        {
            // Arrange
            var invoked = false;
            Action<ResiliencyPolicyOptionsBuilder> configureAction = b => invoked = true;
            var preset = new InlineResiliencyPolicyPreset("preset", configureAction);
            var builder = new ResiliencyPolicyOptionsBuilder();

            // Act
            preset.Configure(builder);

            // Assert
            invoked.Should().BeTrue();
        }
    }
}
