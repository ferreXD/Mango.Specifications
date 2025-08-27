// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;

    public class BulkheadPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultValues_ShouldBeExpected()
        {
            var config = new BulkheadPolicyConfigurator();

            config.MaxParallelization.Should().Be(10);
            config.MaxQueueLength.Should().Be(100);
            config.Order.Should().Be((int)DefaultPolicyOrder.Bulkhead);
        }

        [Fact]
        public void SetMaxParallelization_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new BulkheadPolicyConfigurator();

            var returned = config.SetMaxParallelization(5);

            returned.Should().BeSameAs(config);
            config.MaxParallelization.Should().Be(5);
        }

        [Fact]
        public void SetMaxQueueLength_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new BulkheadPolicyConfigurator();

            var returned = config.SetMaxQueueLength(42);

            returned.Should().BeSameAs(config);
            config.MaxQueueLength.Should().Be(42);
        }

        [Fact]
        public void SetOrder_ShouldUpdateValue_AndReturnSelf()
        {
            var config = new BulkheadPolicyConfigurator();

            var returned = config.SetOrder(123);

            returned.Should().BeSameAs(config);
            config.Order.Should().Be(123);
        }

        [Fact]
        public void Validate_WithValidValues_ShouldNotThrow()
        {
            var config = new BulkheadPolicyConfigurator()
                .SetMaxParallelization(1)
                .SetMaxQueueLength(0);

            Action act = () => config.Validate();

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithNonPositiveParallelization_ShouldThrow(int invalid)
        {
            var config = new BulkheadPolicyConfigurator()
                .SetMaxParallelization(invalid);

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("MaxParallelization must be greater than zero.*")
               .And.ParamName.Should().Be("MaxParallelization");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNegativeQueueLength_ShouldThrow(int invalid)
        {
            var config = new BulkheadPolicyConfigurator()
                .SetMaxParallelization(1)
                .SetMaxQueueLength(invalid);

            Action act = () => config.Validate();

            act.Should().Throw<ArgumentException>()
               .WithMessage("MaxQueueLength cannot be negative.*")
               .And.ParamName.Should().Be("MaxQueueLength");
        }
    }
}
