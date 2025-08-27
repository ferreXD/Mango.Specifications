// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class RetryPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultValues_ShouldBeExpected()
        {
            var config = new RetryPolicyConfigurator();

            config.MaxRetryCount.Should().Be(3);
            config.Delay.Should().Be(TimeSpan.FromSeconds(2));
            config.UseJitter.Should().BeFalse();
            // Default ShouldRetry returns false for any outcome
            var dummyResult = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage());
            config.ShouldRetry!(dummyResult).Should().BeFalse();
            config.Order.Should().Be((int)DefaultPolicyOrder.Retry);
        }

        [Fact]
        public void SetMaxRetryCount_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new RetryPolicyConfigurator();

            var returned = config.SetMaxRetryCount(5);

            returned.Should().BeSameAs(config);
            config.MaxRetryCount.Should().Be(5);
        }

        [Fact]
        public void SetDelay_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new RetryPolicyConfigurator();
            var span = TimeSpan.FromMilliseconds(500);

            var returned = config.SetDelay(span);

            returned.Should().BeSameAs(config);
            config.Delay.Should().Be(span);
        }

        [Fact]
        public void SetUseJitter_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new RetryPolicyConfigurator();

            var returned = config.SetUseJitter(true);

            returned.Should().BeSameAs(config);
            config.UseJitter.Should().BeTrue();
        }

        [Fact]
        public void SetShouldRetryCondition_ShouldUpdatePredicate_AndReturnSelf()
        {
            var config = new RetryPolicyConfigurator();
            Func<DelegateResult<HttpResponseMessage>, bool> predicate = _ => true;

            var returned = config.SetShouldRetryCondition(predicate);

            returned.Should().BeSameAs(config);
            config.ShouldRetry.Should().BeSameAs(predicate);
        }

        [Fact]
        public void Validate_WithValidValues_ShouldNotThrow()
        {
            var config = new RetryPolicyConfigurator()
                .SetMaxRetryCount(1)
                .SetDelay(TimeSpan.FromMilliseconds(1));

            Action act = () => config.Validate();

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Validate_NonPositiveRetryCount_ShouldThrow(int invalid)
        {
            var config = new RetryPolicyConfigurator()
                .SetMaxRetryCount(invalid);

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("Max retry count must be greater than zero.*")
               .And.ParamName.Should().Be("MaxRetryCount");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_NonPositiveDelay_ShouldThrow(int seconds)
        {
            var config = new RetryPolicyConfigurator()
                .SetDelay(TimeSpan.FromSeconds(seconds));

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("Delay must be greater than zero.*")
               .And.ParamName.Should().Be("Delay");
        }
    }
}
