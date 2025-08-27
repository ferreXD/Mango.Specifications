// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Moq;
    using Polly;
    using Polly.CircuitBreaker;
    using System;
    using System.Threading.Tasks;

    public class CircuitBreakerPolicyDefinitionTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchDefaults()
        {
            var def = new CircuitBreakerPolicyDefinition();

            def.Order.Should().Be((int)DefaultPolicyOrder.CircuitBreaker);
            def.FailureThreshold.Should().Be(5);
            def.BreakDuration.Should().Be(TimeSpan.FromSeconds(30));
            def.ShouldBreak.Should().BeNull();
        }

        [Fact]
        public void Merge_DefaultSelf_ShouldAdoptPresetValues()
        {
            // Arrange
            var preset = new CircuitBreakerPolicyDefinition(order: 10)
            {
                FailureThreshold = 2,
                BreakDuration = TimeSpan.FromSeconds(5),
                ShouldBreak = _ => true
            };
            var def = new CircuitBreakerPolicyDefinition();

            // Act
            var result = def.Merge(preset);

            // Assert
            result.Order.Should().Be(10);
            result.FailureThreshold.Should().Be(2);
            result.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
            result.ShouldBreak.Should().NotBeNull();
            result.ShouldBreak!(new DelegateResult<HttpResponseMessage>(new HttpResponseMessage())).Should().BeTrue();
        }

        [Fact]
        public void Merge_CustomSelf_ShouldPreserveOwnValues()
        {
            // Arrange
            var preset = new CircuitBreakerPolicyDefinition(order: 10)
            {
                FailureThreshold = 2,
                BreakDuration = TimeSpan.FromSeconds(5),
                ShouldBreak = _ => true
            };
            var custom = new CircuitBreakerPolicyDefinition(order: 20)
            {
                FailureThreshold = 3,
                BreakDuration = TimeSpan.FromSeconds(7),
                ShouldBreak = _ => false
            };

            // Act
            var result = custom.Merge(preset);

            // Assert
            result.Order.Should().Be(20);
            result.FailureThreshold.Should().Be(3);
            result.BreakDuration.Should().Be(TimeSpan.FromSeconds(7));
            result.ShouldBreak!(new DelegateResult<HttpResponseMessage>(new HttpResponseMessage())).Should().BeFalse();
        }

        [Fact]
        public async Task BuildPolicy_ShouldOpenCircuitAfterFailures()
        {
            // Arrange: threshold = 2
            var def = new CircuitBreakerPolicyDefinition(order: 1)
            {
                FailureThreshold = 2,
                BreakDuration = TimeSpan.FromMilliseconds(1000)
            };
            var policy = def.BuildPolicy();

            // Act: two failures
            await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)));
            await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)));

            // Assert: next call should throw BrokenCircuitException
            Func<Task> act = () => policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            await act.Should().ThrowAsync<BrokenCircuitException>();
        }

        [Fact]
        public async Task BuildPolicy_ShouldResetCircuitAfterDuration()
        {
            // Arrange: threshold =1, break duration short
            var def = new CircuitBreakerPolicyDefinition(order: 1)
            {
                FailureThreshold = 1,
                BreakDuration = TimeSpan.FromMilliseconds(100)
            };
            var policy = def.BuildPolicy();

            // Trigger break
            await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)));

            // Immediately still broken
            var act = async () => await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            await act.Should().ThrowAsync<BrokenCircuitException<HttpResponseMessage>>();

            // Wait for reset
            await Task.Delay(200);

            // Now allowed through
            var response = await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public void BuildPolicy_ShouldUseShouldBreakPredicate()
        {
            // Arrange: treat only 500 as failure
            var def = new CircuitBreakerPolicyDefinition(order: 1)
            {
                FailureThreshold = 1,
                BreakDuration = TimeSpan.FromSeconds(1),
                ShouldBreak = dr => dr.Result.StatusCode == System.Net.HttpStatusCode.InternalServerError
            };
            var policy = def.BuildPolicy();

            // Act & Assert
            // 400 should not break
            Func<Task> nonBreaking = () => policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)));
            nonBreaking.Should().NotThrowAsync();

            // 500 should break
            Func<Task> breaking = () => policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)));
            breaking.Should().ThrowAsync<BrokenCircuitException>();
        }

        [Fact]
        public async Task BuildPolicy_WithDiagnostics_ShouldInvokeOnBreakAndOnReset()
        {
            // Arrange
            var diagnostics = new Mock<IResiliencyDiagnostics>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            diagnostics.Setup(d => d.OnCircuitBreak(request, It.IsAny<Exception>()));
            diagnostics.Setup(d => d.OnCircuitReset(request));

            var def = new CircuitBreakerPolicyDefinition(order: 1)
            {
                FailureThreshold = 1,
                BreakDuration = TimeSpan.FromMilliseconds(50)
            };
            var policy = def.BuildPolicy(diagnostics.Object);

            var context = new Context();
            context.SetRequest(request);

            // Trigger break
            await policy.ExecuteAsync((ctx, ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)), context, CancellationToken.None);
            diagnostics.Verify(d => d.OnCircuitBreak(request, It.IsAny<Exception>()), Times.Once);

            // Reset after duration
            await Task.Delay(60);
            await policy.ExecuteAsync((ctx, ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)), context, CancellationToken.None);
            diagnostics.Verify(d => d.OnCircuitReset(request), Times.Once);
        }
    }
}
