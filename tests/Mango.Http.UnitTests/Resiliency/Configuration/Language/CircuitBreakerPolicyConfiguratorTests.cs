// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class CircuitBreakerPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultValues_ShouldBeExpected()
        {
            var config = new CircuitBreakerPolicyConfigurator();

            config.FailureThreshold.Should().Be(5);
            config.BreakDuration.Should().Be(TimeSpan.FromSeconds(30));
            // Default ShouldBreak returns false for any outcome
            var dummyResult = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage());
            config.ShouldBreak!(dummyResult).Should().BeFalse();
            config.Order.Should().Be((int)DefaultPolicyOrder.CircuitBreaker);
        }

        [Fact]
        public void SetFailureThreshold_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new CircuitBreakerPolicyConfigurator();

            var returned = config.SetFailureThreshold(10);

            returned.Should().BeSameAs(config);
            config.FailureThreshold.Should().Be(10);
        }

        [Fact]
        public void SetBreakDuration_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new CircuitBreakerPolicyConfigurator();
            var span = TimeSpan.FromSeconds(15);

            var returned = config.SetBreakDuration(span);

            returned.Should().BeSameAs(config);
            config.BreakDuration.Should().Be(span);
        }

        [Fact]
        public void SetShouldBreakCondition_ShouldUpdatePredicate_AndReturnSelf()
        {
            var config = new CircuitBreakerPolicyConfigurator();
            Func<DelegateResult<HttpResponseMessage>, bool> predicate = _ => true;

            var returned = config.SetShouldBreakCondition(predicate);

            returned.Should().BeSameAs(config);
            config.ShouldBreak.Should().BeSameAs(predicate);
        }

        [Fact]
        public void SetOrder_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new CircuitBreakerPolicyConfigurator();

            var returned = config.SetOrder(999);

            returned.Should().BeSameAs(config);
            config.Order.Should().Be(999);
        }

        [Fact]
        public void Validate_WithValidValues_ShouldNotThrow()
        {
            var config = new CircuitBreakerPolicyConfigurator()
                .SetFailureThreshold(1)
                .SetBreakDuration(TimeSpan.FromMilliseconds(1));

            Action act = () => config.Validate();

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_NonPositiveFailureThreshold_ShouldThrow(int invalid)
        {
            var config = new CircuitBreakerPolicyConfigurator()
                .SetFailureThreshold(invalid);

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("Failure threshold must be greater than zero.*")
               .And.ParamName.Should().Be("FailureThreshold");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_NonPositiveBreakDuration_ShouldThrow(int seconds)
        {
            var config = new CircuitBreakerPolicyConfigurator()
                .SetBreakDuration(TimeSpan.FromSeconds(seconds));

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("Break duration must be greater than zero.*")
               .And.ParamName.Should().Be("BreakDuration");
        }
    }
}
