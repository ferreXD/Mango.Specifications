// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class CircuitBreakerPolicyBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldReturnDefinitionWithDefaults()
        {
            // Arrange
            var builder = new CircuitBreakerPolicyBuilder();

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be((int)DefaultPolicyOrder.CircuitBreaker);
            definition.FailureThreshold.Should().Be(5);
            definition.BreakDuration.Should().Be(TimeSpan.FromSeconds(30));
            // Default predicate should always return false
            var dummyResult = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage());
            definition.ShouldBreak!(dummyResult).Should().BeFalse();
        }

        [Fact]
        public void Build_AfterSettingValues_ShouldReturnDefinitionWithSetValues()
        {
            // Arrange
            Func<DelegateResult<HttpResponseMessage>, bool> predicate = dr => dr.Result.StatusCode == System.Net.HttpStatusCode.InternalServerError;
            var builder = new CircuitBreakerPolicyBuilder()
                .SetOrder(10)
                .SetFailureThreshold(7)
                .SetBreakDuration(TimeSpan.FromMilliseconds(500))
                .SetShouldBreakCondition(predicate);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(10);
            definition.FailureThreshold.Should().Be(7);
            definition.BreakDuration.Should().Be(TimeSpan.FromMilliseconds(500));
            definition.ShouldBreak!.Should().BeSameAs(predicate);
            // Verify predicate logic
            var okResult = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            definition.ShouldBreak(okResult).Should().BeFalse();
            var errorResult = new DelegateResult<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            definition.ShouldBreak(errorResult).Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Build_WithNegativeOrder_ShouldThrowArgumentOutOfRangeException(int invalidOrder)
        {
            // Arrange
            var builder = new CircuitBreakerPolicyBuilder().SetOrder(invalidOrder);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithParameterName("_order");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Build_WithNonPositiveFailureThreshold_ShouldThrowArgumentException(int invalidThreshold)
        {
            // Arrange
            var builder = new CircuitBreakerPolicyBuilder().SetFailureThreshold(invalidThreshold);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Failure threshold must be greater than zero.*")
               .And.ParamName.Should().Be("_failureThreshold");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Build_WithNonPositiveBreakDuration_ShouldThrowArgumentException(int seconds)
        {
            // Arrange
            var builder = new CircuitBreakerPolicyBuilder().SetBreakDuration(TimeSpan.FromSeconds(seconds));

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Break duration must be greater than zero.*")
               .And.ParamName.Should().Be("_breakDuration");
        }
    }
}
