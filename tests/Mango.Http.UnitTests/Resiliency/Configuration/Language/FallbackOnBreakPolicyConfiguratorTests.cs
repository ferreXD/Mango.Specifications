// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;
    using System.Threading.Tasks;

    public class FallbackOnBreakPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultOrder_ShouldBeFallbackOnBreak()
        {
            // Arrange & Act
            var config = new FallbackOnBreakPolicyConfigurator();

            // Assert
            config.Order.Should().Be((int)DefaultPolicyOrder.FallbackOnBreak);
        }

        [Fact]
        public void Validate_DefaultOnBreakNotSet_ShouldThrowArgumentNullException()
        {
            // Arrange
            var config = new FallbackOnBreakPolicyConfigurator();

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("OnBreak");
        }

        [Fact]
        public void SetOnBreak_WithValidDelegate_ShouldUpdateProperty_AndReturnSelf()
        {
            // Arrange
            var config = new FallbackOnBreakPolicyConfigurator();
            Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> action =
                (result, context, token) => Task.FromResult(new HttpResponseMessage());

            // Act
            var returned = config.SetOnBreak(action);

            // Assert
            returned.Should().BeSameAs(config);
            // Use reflection to access internal property if needed, but SetOnBreak ensures method chain and Validate
            config.Invoking(c => c.Validate()).Should().NotThrow();
        }

        [Fact]
        public void SetOnBreak_WithNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var config = new FallbackOnBreakPolicyConfigurator();

            // Act
            Action act = () => config.SetOnBreak(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("onBreakAction");
        }

        [Fact]
        public void SetOrder_ShouldUpdateOrder_AndReturnSelf()
        {
            // Arrange
            var config = new FallbackOnBreakPolicyConfigurator();

            // Act
            var returned = config.SetOrder(1234);

            // Assert
            returned.Should().BeSameAs(config);
            config.Order.Should().Be(1234);
        }

        [Fact]
        public void Validate_AfterSettingOnBreak_ShouldNotThrow()
        {
            // Arrange
            var config = new FallbackOnBreakPolicyConfigurator()
                .SetOnBreak((result, context, token) => Task.FromResult(new HttpResponseMessage()));

            // Act / Assert
            config.Invoking(c => c.Validate()).Should().NotThrow();
        }
    }
}
