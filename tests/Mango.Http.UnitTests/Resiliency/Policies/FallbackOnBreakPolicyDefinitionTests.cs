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
    using System.Net;
    using System.Threading.Tasks;

    public class FallbackOnBreakPolicyDefinitionTests
    {
        [Fact]
        public void Constructor_NullOnBreakAction_ShouldThrow()
        {
            // Act
            Action act = () => new FallbackOnBreakPolicyDefinition(null!);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("onBreakAction must be set.");
        }

        [Fact]
        public async Task BuildPolicy_OnBrokenCircuit_ShouldInvokeOnBreakAction()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            var def = new FallbackOnBreakPolicyDefinition(OnBreakAction, order: 1);
            var policy = def.BuildPolicy();

            // Act
            // Force broken circuit by throwing BrokenCircuitException within fallback policy
            var response = await policy.ExecuteAsync(() => throw new BrokenCircuitException("Breaker open"));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task BuildPolicy_OnNonBrokenException_ShouldNotInvokeOnBreak()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            var def = new FallbackOnBreakPolicyDefinition(OnBreakAction, order: 1);
            var policy = def.BuildPolicy();

            // Act
            var response = await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));

            // Assert
            invoked.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task BuildPolicy_WithDiagnostics_ShouldInvokeOnFallback()
        {
            // Arrange
            var diagnostics = new Mock<IResiliencyDiagnostics>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://break");
            diagnostics.Setup(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()));

            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }
            var def = new FallbackOnBreakPolicyDefinition(OnBreakAction, order: 1);
            var policy = def.BuildPolicy(diagnostics.Object);

            var ctx = new Context();
            ctx.SetRequest(request);

            // Act
            await policy.ExecuteAsync((context, ct) => throw new BrokenCircuitException("Open"), ctx, CancellationToken.None);

            // Assert
            diagnostics.Verify(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()), Times.Once);
        }
    }
}
