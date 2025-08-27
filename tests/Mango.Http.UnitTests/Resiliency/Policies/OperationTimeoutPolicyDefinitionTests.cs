// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using Polly.Timeout;
    using System;
    using System.Threading.Tasks;

    public class OperationTimeoutPolicyDefinitionTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchDefaults()
        {
            // Act
            var def = new OperationTimeoutPolicyDefinition();

            // Assert
            def.Order.Should().Be((int)DefaultPolicyOrder.Timeout);
            def.Timeout.Should().Be(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void Merge_DefaultSelf_ShouldAdoptPresetValues()
        {
            // Arrange
            var preset = new OperationTimeoutPolicyDefinition(order: 5)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            var def = new OperationTimeoutPolicyDefinition();

            // Act
            var result = def.Merge(preset);

            // Assert
            result.Order.Should().Be(5);
            result.Timeout.Should().Be(TimeSpan.FromSeconds(20));
        }

        [Fact]
        public void Merge_CustomSelf_ShouldPreserveOwnValues()
        {
            // Arrange
            var preset = new OperationTimeoutPolicyDefinition(order: 5)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            var custom = new OperationTimeoutPolicyDefinition(order: 7)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Act
            var result = custom.Merge(preset);

            // Assert
            result.Order.Should().Be(7);
            result.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task BuildPolicy_ExecuteWithinTimeout_ReturnsResponse()
        {
            // Arrange
            var def = new OperationTimeoutPolicyDefinition(order: 1)
            {
                Timeout = TimeSpan.FromMilliseconds(100)
            };
            var policy = def.BuildPolicy();

            // Act
            var response = await policy.ExecuteAsync(() =>
                Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task BuildPolicy_OperationTimesOut_ThrowsTimeoutRejectedException()
        {
            // Arrange
            var def = new OperationTimeoutPolicyDefinition(order: 1)
            {
                Timeout = TimeSpan.FromMilliseconds(50)
            };
            var policy = def.BuildPolicy();

            // Create a cancellation token that will cancel after 1 second
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1));

            var ct = cts.Token;

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutRejectedException>(async () =>
                await policy.ExecuteAsync(async (_, token) =>
                {
                    // Simulate a long-running operation
                    await Task.Delay(2000);
                    token.ThrowIfCancellationRequested();

                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                }, new Context(), ct));
        }
    }
}
