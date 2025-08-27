// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class RetryPolicyDefinitionTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchDefaults()
        {
            var def = new RetryPolicyDefinition();

            def.Order.Should().Be((int)DefaultPolicyOrder.Retry);
            def.RetryCount.Should().Be(3);
            def.RetryDelay.Should().Be(TimeSpan.FromSeconds(1));
            def.UseJitter.Should().BeNull();
            def.ShouldRetry.Should().BeNull();
        }

        [Fact]
        public void Merge_WithPresetAndUser_ShouldOverrideCorrectly()
        {
            var preset = new RetryPolicyDefinition(order: 10)
            {
                RetryCount = 5,
                RetryDelay = TimeSpan.FromSeconds(2),
                UseJitter = true,
                ShouldRetry = _ => true
            };
            var user = new RetryPolicyDefinition(order: 20)
            {
                RetryCount = 4,
                RetryDelay = TimeSpan.FromSeconds(3),
                UseJitter = false,
                ShouldRetry = _ => false
            };
            var result = user.Merge(preset);

            result.Order.Should().Be(20);
            result.RetryCount.Should().Be(4);
            result.RetryDelay.Should().Be(TimeSpan.FromSeconds(3));
            result.UseJitter.Should().BeFalse();
            result.ShouldRetry.Should().NotBeNull();
            result.ShouldRetry!(new DelegateResult<HttpResponseMessage>(new HttpResponseMessage())).Should().BeFalse();
        }

        [Fact]
        public void BuildRetryBackoff_UseJitterFalse_ShouldReturnConstantDelays()
        {
            var def = new RetryPolicyDefinition(order: 1)
            {
                RetryCount = 4,
                RetryDelay = TimeSpan.FromMilliseconds(100),
                UseJitter = false
            };
            // Invoke private method via reflection
            var method = typeof(RetryPolicyDefinition)
                .GetMethod("BuildRetryBackoff", BindingFlags.Instance | BindingFlags.NonPublic);
            var backoff = (IEnumerable<TimeSpan>)method!
                .Invoke(def, null)!;

            backoff.Should().HaveCount(4);
            backoff.Should().OnlyContain(d => d == TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void BuildRetryBackoff_UseJitterTrue_ShouldReturnCorrectCount()
        {
            var def = new RetryPolicyDefinition(order: 1)
            {
                RetryCount = 5,
                RetryDelay = TimeSpan.FromMilliseconds(50),
                UseJitter = true
            };
            var method = typeof(RetryPolicyDefinition)
                .GetMethod("BuildRetryBackoff", BindingFlags.Instance | BindingFlags.NonPublic);
            var backoff = (IEnumerable<TimeSpan>)method!
                .Invoke(def, null)!;

            backoff.Should().HaveCount(5);
            // At least one element should differ from the constant delay to indicate jitter
            backoff.Distinct().Count().Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task BuildPolicy_ShouldRetry_OnException_UsingZeroDelay()
        {
            // Arrange
            var def = new RetryPolicyDefinition(order: 1)
            {
                RetryCount = 2,
                RetryDelay = TimeSpan.Zero
            };
            var policy = def.BuildPolicy();

            var attempt = 0;
            // Act
            var response = await policy.ExecuteAsync(() =>
            {
                attempt++;
                if (attempt <= 2)
                    throw new HttpRequestException();
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            });

            // Assert
            attempt.Should().Be(3);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task BuildPolicy_ShouldNotRetry_ResultWhenShouldRetryFalse()
        {
            // Arrange
            var def = new RetryPolicyDefinition(order: 1)
            {
                RetryCount = 3,
                RetryDelay = TimeSpan.Zero,
                ShouldRetry = _ => false
            };
            var policy = def.BuildPolicy();

            var callCount = 0;
            // Act
            var response = await policy.ExecuteAsync(() =>
            {
                callCount++;
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            });

            // Assert: no retries, single call
            callCount.Should().Be(1);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
