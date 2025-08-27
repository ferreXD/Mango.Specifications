namespace Mango.Http.UnitTests.Resiliency
{
    using Mango.Http.Resiliency;
    using FluentAssertions;
    using System;
    using Xunit;
    using Polly;

    public class ResiliencyOptionsTests
    {
        private static readonly Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> DummyFallback =
            (outcome, context, ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        private static readonly Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> DummyOnBreak =
            (outcome, context, ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        [Fact]
        public void Constructor_WithEmptyPolicies_ShouldHaveEmptyPolicies()
        {
            // Act
            var options = new ResiliencyOptions();

            // Assert
            options.Policies.Should().BeEmpty();
        }

        [Fact]
        public void Add_ShouldAppendPolicy_AndNotModifyOriginal()
        {
            // Arrange
            var options = new ResiliencyOptions();
            var custom = new CustomPolicyDefinition(
                order: 1,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());

            // Act
            var newOptions = options.Add(custom);

            // Assert
            options.Policies.Should().BeEmpty();
            newOptions.Policies.Should().ContainSingle()
                .Which.Should().Be(custom);
        }

        [Fact]
        public void Validate_NoPolicies_ShouldNotThrow()
        {
            // Arrange
            var options = new ResiliencyOptions();

            // Act / Assert
            options.Invoking(o => o.Validate()).Should().NotThrow();
        }

        [Fact]
        public void Validate_MixCustomAndBuiltIn_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var cb = new CircuitBreakerPolicyDefinition(order: 1);
            var custom = new CustomPolicyDefinition(
                order: 2,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());
            var options = new ResiliencyOptions([cb, custom]);

            // Act / Assert
            options.Invoking(o => o.Validate())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot mix a custom policy with built-in policies.");
        }

        [Fact]
        public void Constructor_DuplicateOrders_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var cb1 = new CircuitBreakerPolicyDefinition(order: 1);
            var cb2 = new CircuitBreakerPolicyDefinition(order: 1);

            // Act
            Action act = () => new ResiliencyOptions([cb1, cb2]);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Duplicate orders: 1");
        }

        [Fact]
        public void Constructor_FallbackWithoutCircuitBreaker_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var fallback = new FallbackPolicyDefinition(DummyFallback, order: 2);

            // Act
            Action act = () => new ResiliencyOptions([fallback]);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Fallback requires CircuitBreaker first.");
        }

        [Fact]
        public void Constructor_FallbackOnBreakWithoutCircuitBreaker_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var fbb = new FallbackOnBreakPolicyDefinition(DummyOnBreak, order: 2);

            // Act
            Action act = () => new ResiliencyOptions([fbb]);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("FallbackOnBreak requires CircuitBreaker first.");
        }

        [Fact]
        public void Constructor_FallbackNotLast_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var cb = new CircuitBreakerPolicyDefinition(order: 2);
            var fallback = new FallbackPolicyDefinition(DummyFallback, order: 1);

            // Act
            Action act = () => new ResiliencyOptions([cb, fallback]);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Fallback must be last.");
        }

        [Fact]
        public void Constructor_FallbackOnBreakNotImmediatelyPrecedingFallback_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var cb = new CircuitBreakerPolicyDefinition(order: 1);
            var fbb = new FallbackOnBreakPolicyDefinition(DummyOnBreak, order: 2);
            var fallback = new FallbackPolicyDefinition(DummyFallback, order: 4);

            // Act
            Action act = () => new ResiliencyOptions([cb, fbb, fallback]);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("FallbackOnBreak must immediately precede Fallback.");
        }

        [Fact]
        public void Constructor_ValidFallbackChain_ShouldSucceedAndOrderPoliciesCorrectly()
        {
            // Arrange
            var cb = new CircuitBreakerPolicyDefinition(order: 1);
            var fbb = new FallbackOnBreakPolicyDefinition(DummyOnBreak, order: 2);
            var fallback = new FallbackPolicyDefinition(DummyFallback, order: 3);

            // Act
            var options = new ResiliencyOptions([fbb, cb, fallback]);

            // Assert
            options.Policies.Select(p => p.Order)
                .Should().ContainInOrder([1, 2, 3]);
            options.Invoking(o => o.Validate()).Should().NotThrow();
        }

        [Fact]
        public void Constructor_OnlyCustomPolicies_NoValidationOnConstructor()
        {
            // Arrange
            var custom1 = new CustomPolicyDefinition(
                order: 1,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());
            var custom2 = new CustomPolicyDefinition(
                order: 2,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());

            // Act
            Action act = () => new ResiliencyOptions([custom1, custom2]);

            // Assert
            // Constructor should not throw; validation runs only on Validate()
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_MultipleCustomPolicies_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var custom1 = new CustomPolicyDefinition(
                order: 1,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());
            var custom2 = new CustomPolicyDefinition(
                order: 2,
                Policy: _ => Policy.NoOpAsync<HttpResponseMessage>());
            var options = new ResiliencyOptions([custom1, custom2]);

            // Act / Assert
            options.Invoking(o => o.Validate())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot mix a custom policy with built-in policies.");
        }
    }
}
