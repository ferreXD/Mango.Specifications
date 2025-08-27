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

    public class HeaderAuthenticationStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger<HeaderAuthenticationStrategy>> _loggerMock = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Ctor_NullOrEmptyHeaderName_ThrowsArgumentNullException(string headerName)
        {
            // Arrange
            var headerValue = "some-value";

            // Act
            Action act = () => new HeaderAuthenticationStrategy(
                headerName!,
                headerValue,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("headerName")
               .WithMessage("Header name cannot be null or empty.*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Ctor_NullOrEmptyHeaderValue_ThrowsArgumentNullException(string headerValue)
        {
            // Arrange
            var headerName = "X-Custom-Auth";

            // Act
            Action act = () => new HeaderAuthenticationStrategy(
                headerName,
                headerValue!,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("headerValue")
               .WithMessage("Header value cannot be null or empty.*");
        }

        [Fact]
        public async Task ApplyAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var strategy = new HeaderAuthenticationStrategy(
                "X-Test",
                "value",
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
