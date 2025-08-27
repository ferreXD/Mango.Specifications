// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class RetryPolicyBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldReturnDefinitionWithDefaults()
        {
            // Arrange
            var builder = new RetryPolicyBuilder();

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be((int)DefaultPolicyOrder.Retry);
            definition.RetryCount.Should().Be(3);
            definition.RetryDelay.Should().Be(TimeSpan.FromSeconds(2));
            definition.UseJitter.Should().BeFalse();
            // Default predicate should always return false
            var dummy = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage());
            definition.ShouldRetry!(dummy).Should().BeFalse();
        }

        [Fact]
        public void Build_AfterSettingValues_ShouldReturnDefinitionWithSetValues()
        {
            // Arrange
            Func<DelegateResult<HttpResponseMessage>, bool> predicate = dr => dr.Result.StatusCode == System.Net.HttpStatusCode.InternalServerError;
            var builder = new RetryPolicyBuilder()
                .SetOrder(42)
                .SetMaxRetryCount(5)
                .SetDelay(TimeSpan.FromMilliseconds(100))
                .SetUseJitter(true)
                .SetShouldRetryCondition(predicate);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(42);
            definition.RetryCount.Should().Be(5);
            definition.RetryDelay.Should().Be(TimeSpan.FromMilliseconds(100));
            definition.UseJitter.Should().BeTrue();
            definition.ShouldRetry.Should().BeSameAs(predicate);
            // Verify predicate logic
            var ok = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            definition.ShouldRetry(ok).Should().BeFalse();
            var error = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            definition.ShouldRetry(error).Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Build_WithNegativeOrder_ShouldThrowArgumentOutOfRangeException(int invalidOrder)
        {
            // Arrange
            var builder = new RetryPolicyBuilder().SetOrder(invalidOrder);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithParameterName("_order");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Build_WithNonPositiveRetryCount_ShouldThrowArgumentException(int invalidCount)
        {
            // Arrange
            var builder = new RetryPolicyBuilder().SetMaxRetryCount(invalidCount);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Max retry count must be greater or equal than zero.*")
               .And.ParamName.Should().Be("_maxRetryCount");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Build_WithNonPositiveDelay_ShouldThrowArgumentException(int seconds)
        {
            // Arrange
            var builder = new RetryPolicyBuilder().SetDelay(TimeSpan.FromSeconds(seconds));

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Delay must be greater than zero.*")
               .And.ParamName.Should().Be("_delay");
        }
    }
}
