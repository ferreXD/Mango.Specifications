// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class BearerTokenAuthStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger<BearerTokenAuthStrategy>> _loggerMock = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Ctor_NullOrEmptyToken_ThrowsArgumentNullException(string? token)
        {
            // Act
            Action act = () => new BearerTokenAuthStrategy(
                token!,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("token")
               .WithMessage("Token cannot be null or empty.*");
        }

        [Fact]
        public async Task ApplyAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var strategy = new BearerTokenAuthStrategy(
                "mytoken",
                _activitySource,
                _loggerMock.Object);

            // Act
            var act = async () => await strategy.ApplyAsync(null!);

            // Assert
            await act.Should()
                     .ThrowAsync<ArgumentNullException>()
                     .WithParameterName("request");
        }
    }
}
