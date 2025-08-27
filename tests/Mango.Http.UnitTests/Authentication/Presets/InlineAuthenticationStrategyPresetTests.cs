// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using System;

    public class InlineAuthenticationStrategyPresetTests
    {
        [Fact]
        public void Ctor_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            Action<HttpAuthOptionsBuilder> configure = _ => { };

            // Act
            Action act = () => new InlineAuthenticationStrategyPreset(null!, configure);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("name");
        }

        [Fact]
        public void Ctor_NullConfigure_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new InlineAuthenticationStrategyPreset("preset", null!);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("configure");
        }

        [Fact]
        public void Name_Property_ReturnsCtorValue()
        {
            // Arrange
            var expectedName = "MyPreset";
            var preset = new InlineAuthenticationStrategyPreset(expectedName, _ => { });

            // Act
            var actualName = preset.Name;

            // Assert
            actualName.Should().Be(expectedName);
        }

        [Fact]
        public void Configure_InvokesConfigurationAction_WithGivenBuilder()
        {
            // Arrange
            var builder = new HttpAuthOptionsBuilder();
            var actionCalled = false;
            HttpAuthOptionsBuilder? passedBuilder = null;

            Action<HttpAuthOptionsBuilder> configure = b =>
            {
                actionCalled = true;
                passedBuilder = b;
            };

            var preset = new InlineAuthenticationStrategyPreset("preset", configure);

            // Act
            preset.Configure(builder);

            // Assert
            actionCalled.Should().BeTrue("the configuration action should be invoked");
            passedBuilder.Should().BeSameAs(builder, "the builder passed to Configure must be forwarded to the action");
        }
    }
}
