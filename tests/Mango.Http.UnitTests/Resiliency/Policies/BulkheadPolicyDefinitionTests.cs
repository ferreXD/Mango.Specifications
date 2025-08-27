// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly.Bulkhead;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class BulkheadPolicyDefinitionTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchDefaults()
        {
            // Act
            var def = new BulkheadPolicyDefinition();

            // Assert
            def.Order.Should().Be((int)DefaultPolicyOrder.Bulkhead);
            def.MaxParallelization.Should().Be(10);
            def.MaxQueuing.Should().Be(5);
        }

        [Fact]
        public void Merge_DefaultSelf_ShouldAdoptPresetValues()
        {
            // Arrange
            var preset = new BulkheadPolicyDefinition(order: 20)
            {
                MaxParallelization = 3,
                MaxQueuing = 2
            };
            var def = new BulkheadPolicyDefinition();

            // Act
            var result = def.Merge(preset);

            // Assert
            result.Order.Should().Be(20);
            result.MaxParallelization.Should().Be(3);
            result.MaxQueuing.Should().Be(2);
        }

        [Fact]
        public void Merge_CustomSelf_ShouldPreserveOwnValues()
        {
            // Arrange
            var preset = new BulkheadPolicyDefinition(order: 20)
            {
                MaxParallelization = 3,
                MaxQueuing = 2
            };
            var custom = new BulkheadPolicyDefinition(order: 30)
            {
                MaxParallelization = 7,
                MaxQueuing = 4
            };

            // Act
            var result = custom.Merge(preset);

            // Assert
            result.Order.Should().Be(30);
            result.MaxParallelization.Should().Be(7);
            result.MaxQueuing.Should().Be(4);
        }

        [Fact]
        public async Task BuildPolicy_ExceedsParallelization_ShouldRejectAdditionalCalls()
        {
            // Arrange
            var def = new BulkheadPolicyDefinition(order: 1)
            {
                MaxParallelization = 1,
                MaxQueuing = 0
            };
            var policy = def.BuildPolicy();

            // A TCS to hold the first task open
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            var task1 = policy.ExecuteAsync(() => tcs.Task);
            // ensure the first task is running
            await Task.Delay(10);

            // Act
            Func<Task> act = () => policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage()));

            // Assert reject immediately
            await act.Should().ThrowAsync<BulkheadRejectedException>();

            // Cleanup
            tcs.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
            var response1 = await task1;
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task BuildPolicy_QueueCapacity_ShouldQueueThenRejectBeyond()
        {
            // Arrange

            var def = new BulkheadPolicyDefinition(order: 1)
            {
                MaxParallelization = 1,
                MaxQueuing = 1
            };
            var policy = def.BuildPolicy();

            // First task blocks
            var tcs1 = new TaskCompletionSource<HttpResponseMessage>();
            var task1 = policy.ExecuteAsync(() => tcs1.Task);
            await Task.Delay(10);

            // Second task queued
            var queuedTcs = new TaskCompletionSource<HttpResponseMessage>();
            var task2 = policy.ExecuteAsync(() => queuedTcs.Task);
            await Task.Delay(10);

            // Third task should be rejected
            Func<Task> act = () => policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage()));
            await act.Should().ThrowAsync<BulkheadRejectedException>();

            // Release first to dequeue second
            tcs1.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
            // small delay to allow dequeuing
            await Task.Delay(10);

            // Complete queued task and assert
            queuedTcs.SetResult(new HttpResponseMessage(HttpStatusCode.Accepted));
            var response2 = await task2;
            response2.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }
    }
}
