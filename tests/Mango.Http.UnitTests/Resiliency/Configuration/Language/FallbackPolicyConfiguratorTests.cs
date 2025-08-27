// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class FallbackPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultOrder_ShouldBeFallback()
        {
            // Arrange & Act
            var config = new FallbackPolicyConfigurator();

            // Assert
            config.Order.Should().Be((int)DefaultPolicyOrder.Fallback);
        }

        [Fact]
        public void Validate_WithoutSettingFallbackAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var config = new FallbackPolicyConfigurator();

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("FallbackAction");
        }

        [Fact]
        public void SetFallbackAction_WithValidDelegate_ShouldUpdateProperty_AndReturnSelf()
        {
            // Arrange
            var config = new FallbackPolicyConfigurator();
            Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> action =
                (outcome, context, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var returned = config.SetFallbackAction(action);

            // Assert
            returned.Should().BeSameAs(config);
            // Validate should no longer throw
            config.Invoking(c => c.Validate()).Should().NotThrow();
        }

        [Fact]
        public void SetFallbackAction_WithNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var config = new FallbackPolicyConfigurator();

            // Act
            Action act = () => config.SetFallbackAction(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("fallbackAction");
        }

        [Fact]
        public void SetOrder_ShouldUpdateOrder_AndReturnSelf()
        {
            // Arrange
            var config = new FallbackPolicyConfigurator();

            // Act
            var returned = config.SetOrder(700);

            // Assert
            returned.Should().BeSameAs(config);
            config.Order.Should().Be(700);
        }
    }
}
