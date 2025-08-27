// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;

    public class PollyContextExtensionsTests
    {
        [Fact]
        public void PollyContextKeys_ShouldHaveExpectedValues()
        {
            PollyContextKeys.Request.Should().Be("Request");
            PollyContextKeys.CancellationToken.Should().Be("CancellationToken");
        }

        [Fact]
        public void SetRequest_And_TryGetRequest_ShouldStoreAndRetrieveHttpRequestMessage()
        {
            // Arrange
            var ctx = new Context();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            ctx.SetRequest(request);
            var result = ctx.TryGetRequest(out var retrieved);

            // Assert
            result.Should().BeTrue();
            retrieved.Should().BeSameAs(request);
        }

        [Fact]
        public void TryGetRequest_WithoutSet_ShouldReturnFalseAndNull()
        {
            // Arrange
            var ctx = new Context();

            // Act
            var result = ctx.TryGetRequest(out var retrieved);

            // Assert
            result.Should().BeFalse();
            retrieved.Should().BeNull();
        }

        [Fact]
        public void SetCancellation_And_TryGetCancellationToken_ShouldStoreAndRetrieveCancellationToken()
        {
            // Arrange
            var ctx = new Context();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Act
            ctx.SetCancellation(token);
            var result = ctx.TryGetCancellationToken(out var retrieved);

            // Assert
            result.Should().BeTrue();
            retrieved.Should().Be(token);
        }

        [Fact]
        public void TryGetCancellationToken_WithoutSet_ShouldReturnFalseAndNull()
        {
            // Arrange
            var ctx = new Context();

            // Act
            var result = ctx.TryGetCancellationToken(out var retrieved);

            // Assert
            result.Should().BeFalse();
            retrieved.Should().BeNull();
        }

        [Fact]
        public void TryGetCancellationToken_WithWrongType_ShouldReturnFalseAndNull()
        {
            // Arrange
            var ctx = new Context();
            ctx[PollyContextKeys.CancellationToken] = "not a token";

            // Act
            var result = ctx.TryGetCancellationToken(out var retrieved);

            // Assert
            result.Should().BeFalse();
            retrieved.Should().BeNull();
        }
    }
}
