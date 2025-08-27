// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;

    public class BulkheadPolicyBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldReturnDefinitionWithDefaultValues()
        {
            // Arrange
            var builder = new BulkheadPolicyBuilder();

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be((int)DefaultPolicyOrder.Bulkhead);
            definition.MaxParallelization.Should().Be(10);
            definition.MaxQueuing.Should().Be(100);
        }

        [Fact]
        public void Build_AfterSettingValues_ShouldReturnDefinitionWithSetValues()
        {
            // Arrange
            var builder = new BulkheadPolicyBuilder()
                .SetOrder(5)
                .SetMaxParallelization(20)
                .SetMaxQueueLength(50);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(5);
            definition.MaxParallelization.Should().Be(20);
            definition.MaxQueuing.Should().Be(50);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Build_WithNegativeOrder_ShouldThrowArgumentOutOfRangeException(int invalidOrder)
        {
            // Arrange
            var builder = new BulkheadPolicyBuilder().SetOrder(invalidOrder);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithParameterName("_order");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Build_WithNonPositiveParallelization_ShouldThrowArgumentException(int invalidParallel)
        {
            // Arrange
            var builder = new BulkheadPolicyBuilder().SetMaxParallelization(invalidParallel);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("MaxParallelization must be greater than zero.*")
               .And.ParamName.Should().Be("_maxParallelization");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Build_WithNegativeQueueLength_ShouldThrowArgumentException(int invalidQueue)
        {
            // Arrange
            var builder = new BulkheadPolicyBuilder().SetMaxQueueLength(invalidQueue);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("MaxQueueLength cannot be negative.*")
               .And.ParamName.Should().Be("_maxQueueLength");
        }
    }
}
