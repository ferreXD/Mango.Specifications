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

    public class FallbackOnBreakPolicyBuilderTests
    {
        [Fact]
        public void Build_WithoutOnBreakAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new FallbackOnBreakPolicyBuilder();

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("_onBreak");
        }

        [Fact]
        public void SetOnBreak_Null_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new FallbackOnBreakPolicyBuilder();

            // Act
            Action act = () => builder.SetOnBreak(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("onBreakAction");
        }

        [Fact]
        public void SetOnBreak_Valid_And_SetOrder_ShouldConfigureBuilder()
        {
            // Arrange
            Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> action =
                (_, __, ___) => Task.FromResult(new HttpResponseMessage());
            var builder = new FallbackOnBreakPolicyBuilder()
                .SetOrder(650)
                .SetOnBreak(action);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(650);
        }

        [Fact]
        public async Task Policy_ShouldInvokeOnBreakAction_WhenBrokenCircuitException()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            var builder = new FallbackOnBreakPolicyBuilder().SetOnBreak(OnBreakAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics: null);

            // Act
            var response = await policy.ExecuteAsync(() => throw new BrokenCircuitException("open"));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Policy_ShouldNotInvokeOnBreakAction_WhenNoException()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            var builder = new FallbackOnBreakPolicyBuilder().SetOnBreak(OnBreakAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics: null);

            // Act
            var response = await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));

            // Assert
            invoked.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Policy_WithDiagnostics_ShouldInvokeOnFallback()
        {
            // Arrange
            var diagnostics = new Mock<IResiliencyDiagnostics>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://break");
            diagnostics.Setup(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()));

            Task<HttpResponseMessage> OnBreakAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }
            var builder = new FallbackOnBreakPolicyBuilder().SetOnBreak(OnBreakAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics.Object);

            var ctx = new Context();
            ctx.SetRequest(request);

            // Act
            var response = await policy.ExecuteAsync((context, ct) => throw new BrokenCircuitException("open"), ctx, CancellationToken.None);

            // Assert
            diagnostics.Verify(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()), Times.Once);
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
