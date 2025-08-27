// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Moq;
    using Polly;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class FallbackPolicyDefinitionTests
    {
        [Fact]
        public void Constructor_NullFallbackAction_ShouldThrow()
        {
            // Act
            Action act = () => new FallbackPolicyDefinition(null!);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("fallbackAction must be set.");
        }

        [Fact]
        public async Task BuildPolicy_OnException_ShouldInvokeFallbackAction()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            var def = new FallbackPolicyDefinition(FallbackAction, order: 1);
            var policy = def.BuildPolicy();

            // Act
            var response = await policy.ExecuteAsync(() => throw new Exception("Failure"));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task BuildPolicy_OnNonSuccessResponse_ShouldInvokeFallbackAction()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
            }

            var def = new FallbackPolicyDefinition(FallbackAction, order: 1);
            var policy = def.BuildPolicy();

            // Act
            var response = await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task BuildPolicy_WithDiagnostics_ShouldInvokeOnFallback()
        {
            // Arrange
            var diagnostics = new Mock<IResiliencyDiagnostics>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://fallback");
            diagnostics.Setup(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()));

            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            var def = new FallbackPolicyDefinition(FallbackAction, order: 1);
            var policy = def.BuildPolicy(diagnostics.Object);

            var ctx = new Context();
            ctx.SetRequest(request);

            // Act
            await policy.ExecuteAsync((cx, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)), ctx, CancellationToken.None);

            // Assert
            diagnostics.Verify(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()), Times.Once);
        }
    }
}
