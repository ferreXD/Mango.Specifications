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

    public class FallbackPolicyBuilderTests
    {
        [Fact]
        public void Build_WithoutFallbackAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new FallbackPolicyBuilder();

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("_fallbackAction");
        }

        [Fact]
        public void SetFallbackAction_Null_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new FallbackPolicyBuilder();

            // Act
            Action act = () => builder.SetFallbackAction(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("fallbackAction");
        }

        [Fact]
        public void SetOnBreak_Valid_And_SetOrder_ShouldConfigureBuilder()
        {
            // Arrange
            Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> action =
                (outcome, context, token) => Task.FromResult(new HttpResponseMessage());
            var builder = new FallbackPolicyBuilder()
                .SetOrder(700)
                .SetFallbackAction(action);

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(700);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Build_WithNegativeOrder_ShouldThrowArgumentOutOfRangeException(int invalid)
        {
            // Arrange
            var builder = new FallbackPolicyBuilder().SetOrder(invalid).SetFallbackAction((_, __, ___) => Task.FromResult(new HttpResponseMessage()));

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithParameterName("_order");
        }

        [Fact]
        public async Task Policy_ShouldInvokeFallbackAction_OnException()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            var builder = new FallbackPolicyBuilder().SetFallbackAction(FallbackAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics: null);

            // Act
            var response = await policy.ExecuteAsync(() => throw new Exception("error"));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Policy_ShouldInvokeFallbackAction_OnNonSuccessResult()
        {
            // Arrange
            var invoked = false;
            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                invoked = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
            }
            var builder = new FallbackPolicyBuilder().SetFallbackAction(FallbackAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics: null);

            // Act
            var response = await policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            // Assert
            invoked.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Policy_WithDiagnostics_ShouldInvokeOnFallback()
        {
            // Arrange
            var diagnostics = new Mock<IResiliencyDiagnostics>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://fallback");
            diagnostics.Setup(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()));

            Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken ct)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            var builder = new FallbackPolicyBuilder().SetFallbackAction(FallbackAction);
            var definition = builder.Build();
            var policy = definition.BuildPolicy(diagnostics.Object);

            var ctx = new Context();
            ctx.SetRequest(request);

            // Act
            var response = await policy.ExecuteAsync((context, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)), ctx, CancellationToken.None);

            // Assert
            diagnostics.Verify(d => d.OnFallback(request, It.IsAny<DelegateResult<HttpResponseMessage>>()), Times.Once);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
