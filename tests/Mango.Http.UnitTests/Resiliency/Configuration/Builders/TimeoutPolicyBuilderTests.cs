// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;

    public class TimeoutPolicyBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldReturnDefinitionWithDefaults()
        {
            // Arrange
            var builder = new TimeoutPolicyBuilder();

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be((int)DefaultPolicyOrder.Timeout);
            definition.Timeout.Should().Be(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void Build_AfterSettingTimeoutAndOrder_ShouldReturnDefinitionWithSetValues()
        {
            // Arrange
            var builder = new TimeoutPolicyBuilder()
                .SetTimeout(TimeSpan.FromMilliseconds(500))
                .SetOrder(200);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(200);
            definition.Timeout.Should().Be(TimeSpan.FromMilliseconds(500));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void SetTimeout_WithNonPositive_ShouldThrowArgumentException(int seconds)
        {
            // Arrange
            var builder = new TimeoutPolicyBuilder();
            var invalid = TimeSpan.FromSeconds(seconds);

            // Act
            Action act = () => builder.SetTimeout(invalid);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Timeout must be greater than zero.*")
               .And.ParamName.Should().Be("timeout");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void SetOrder_WithNegative_ShouldThrowArgumentException(int order)
        {
            // Arrange
            var builder = new TimeoutPolicyBuilder();

            // Act
            Action act = () => builder.SetOrder(order);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Order must be a non-negative integer.*")
               .And.ParamName.Should().Be("order");
        }

        [Fact]
        public void Build_WithoutSettingTimeout_ShouldRetainDefaultTimeout()
        {
            // Arrange & Act
            var definition = new TimeoutPolicyBuilder().SetOrder(300).Build();

            // Assert
            definition.Timeout.Should().Be(TimeSpan.FromSeconds(10));
            definition.Order.Should().Be(300);
        }

        [Fact]
        public void Build_WithoutSettingOrder_ShouldRetainDefaultOrder()
        {
            // Arrange & Act
            var definition = new TimeoutPolicyBuilder().SetTimeout(TimeSpan.FromSeconds(20)).Build();

            // Assert
            definition.Timeout.Should().Be(TimeSpan.FromSeconds(20));
            definition.Order.Should().Be((int)DefaultPolicyOrder.Timeout);
        }
    }
}
