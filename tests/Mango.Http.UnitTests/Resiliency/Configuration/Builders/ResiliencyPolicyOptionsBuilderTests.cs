// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using System;
    using System.Linq;

    public class ResiliencyPolicyOptionsBuilderTests
    {
        [Fact]
        public void Build_WithoutPolicies_ShouldReturnEmptyPolicies()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder();

            // Act
            var options = builder.Build();

            // Assert
            options.Policies.Should().BeEmpty();
        }

        [Fact]
        public void WithTimeout_Default_ShouldAddTimeoutPolicy()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder();

            // Act
            var options = builder.WithTimeout().Build();

            // Assert
            options.Policies.Should().ContainSingle()
                .Which.Should().BeOfType<OperationTimeoutPolicyDefinition>();
        }

        [Fact]
        public void WithTimeout_Custom_ShouldConfigureTimeoutAndOrder()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder();
            var customTimeout = TimeSpan.FromMilliseconds(123);
            var customOrder = 77;

            // Act
            var options = builder.WithTimeout(cfg => cfg.SetTimeout(customTimeout).SetOrder(customOrder)).Build();

            // Assert
            var timeoutDef = options.Policies.OfType<OperationTimeoutPolicyDefinition>().Single();
            timeoutDef.Timeout.Should().Be(customTimeout);
            timeoutDef.Order.Should().Be(customOrder);
        }

        [Fact]
        public void WithRetry_Default_ShouldAddRetryPolicy()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder();

            // Act
            var options = builder.WithRetry().Build();

            // Assert
            options.Policies.Should().ContainSingle()
                .Which.Should().BeOfType<RetryPolicyDefinition>();
        }

        [Fact]
        public void WithRetry_Custom_ShouldConfigureRetryCountAndDelay()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder();
            var count = 5;
            var delay = TimeSpan.FromSeconds(2);

            // Act
            var options = builder.WithRetry(cfg => cfg.SetMaxRetryCount(count).SetDelay(delay)).Build();

            // Assert
            var retryDef = options.Policies.OfType<RetryPolicyDefinition>().Single();
            retryDef.RetryCount.Should().Be(count);
            retryDef.RetryDelay.Should().Be(delay);
        }

        [Fact]
        public void WithMultiplePolicies_ShouldAddAllAndSortByOrder()
        {
            // Arrange
            var builder = new ResiliencyPolicyOptionsBuilder()
                .WithRetry(cfg => cfg.SetOrder(50))
                .WithTimeout(cfg => cfg.SetOrder(10))
                .WithCircuitBreaker(cfg => cfg.SetOrder(30));

            // Act
            var options = builder.Build();

            // Assert
            options.Policies.Should().HaveCount(3);
            options.Policies.Select(p => p.Order).Should().BeInAscendingOrder();
            options.Policies.Select(p => p).Should().ContainInOrder(
                options.Policies.OfType<OperationTimeoutPolicyDefinition>().Single(),
                options.Policies.OfType<CircuitBreakerPolicyDefinition>().Single(),
                options.Policies.OfType<RetryPolicyDefinition>().Single()
            );
        }
    }
}
