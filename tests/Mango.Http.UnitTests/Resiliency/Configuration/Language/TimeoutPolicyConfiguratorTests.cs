// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;

    public class TimeoutPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultValues_ShouldBeExpected()
        {
            // Arrange & Act
            var config = new TimeoutPolicyConfigurator();

            // Assert
            config.Timeout.Should().Be(default(TimeSpan), "default Timeout should be TimeSpan.Zero");
            config.Order.Should().Be((int)DefaultPolicyOrder.Timeout);
        }

        [Fact]
        public void SetTimeout_ShouldUpdateValue_AndReturnSelf()
        {
            // Arrange
            var config = new TimeoutPolicyConfigurator();
            var span = TimeSpan.FromSeconds(5);

            // Act
            var returned = config.SetTimeout(span);

            // Assert
            returned.Should().BeSameAs(config);
            config.Timeout.Should().Be(span);
        }

        [Fact]
        public void Validate_WithoutSettingTimeout_ShouldThrowArgumentException()
        {
            // Arrange
            var config = new TimeoutPolicyConfigurator();

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Timeout must be greater than zero.*")
               .And.ParamName.Should().Be("Timeout");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveTimeout_ShouldThrow(int seconds)
        {
            // Arrange
            var config = new TimeoutPolicyConfigurator()
                .SetTimeout(TimeSpan.FromSeconds(seconds));

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Timeout must be greater than zero.*")
               .And.ParamName.Should().Be("Timeout");
        }

        [Fact]
        public void Validate_WithPositiveTimeout_ShouldNotThrow()
        {
            // Arrange
            var config = new TimeoutPolicyConfigurator()
                .SetTimeout(TimeSpan.FromMilliseconds(100));

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().NotThrow();
        }
    }
}
