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

    public class AsyncTokenProviderStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger<AsyncTokenProviderStrategy>> _loggerMock = new();

        [Fact]
        public void Ctor_NullTokenProvider_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new AsyncTokenProviderStrategy(
                null!,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("tokenProvider");
        }

        [Fact]
        public async Task ApplyAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var strategy = new AsyncTokenProviderStrategy(
                ct => new ValueTask<string>("token"),
                _activitySource,
                _loggerMock.Object);

            // Act
            var act = async () => await strategy.ApplyAsync(null!);

            // Assert
            await act.Should()
                     .ThrowAsync<ArgumentNullException>()
                     .WithParameterName("request");
        }

        [Fact]
        public async Task ApplyAsync_ValidToken_SetsBearerHeaderAndLogs()
        {
            // Arrange
            var expectedToken = "valid-token-123";
            var strategy = new AsyncTokenProviderStrategy(
                ct => new ValueTask<string>(expectedToken),
                _activitySource,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

            // Act
            await strategy.ApplyAsync(request);

            // Assert header
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be(expectedToken);

            // verify Debug log at start
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Starting AsyncTokenProviderStrategy"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // verify Information log with elapsed ms
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.StartsWith("AsyncTokenProviderStrategy applied in ")
                    && v.ToString()!.EndsWith("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_EmptyToken_ThrowsWrappedMangoAuthenticationExceptionAndLogsError()
        {
            // Arrange
            var innerMessage = "Token cannot be null or empty.";
            var strategy = new AsyncTokenProviderStrategy(
                ct => new ValueTask<string>("   "),
                _activitySource,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/");

            // Act
            var act = async () => await strategy.ApplyAsync(request);

            var ex = await act.Should()
                              .ThrowAsync<MangoAuthenticationException>()
                              .WithMessage("AsyncTokenProviderStrategy failed to apply authentication.*");

            // The outer exception should wrap the inner MangoAuthenticationException
            ex.Which.InnerException.Should().BeOfType<MangoAuthenticationException>()
                .Which.Message.Should().Be(innerMessage);

            // verify Error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "AsyncTokenProviderStrategy failed"),
                It.IsAny<MangoAuthenticationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_TokenProviderThrows_ThrowsWrappedMangoAuthenticationExceptionAndLogsError()
        {
            // Arrange
            var providerEx = new InvalidOperationException("provider failure");
            var strategy = new AsyncTokenProviderStrategy(
                ct => throw providerEx,
                _activitySource,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Put, "https://api.test/");

            // Act
            var act = async () => await strategy.ApplyAsync(request);

            var ex = await act.Should()
                              .ThrowAsync<MangoAuthenticationException>()
                              .WithMessage("AsyncTokenProviderStrategy failed to apply authentication.*");

            ex.Which.InnerException.Should().Be(providerEx);

            // verify Error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "AsyncTokenProviderStrategy failed"),
                providerEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_CanceledToken_PropagatesOperationCanceledAndLogsWarning()
        {
            // Arrange
            var strategy = new AsyncTokenProviderStrategy(
                async ct =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return "won't get here";
                },
                _activitySource,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Delete, "https://api.test/");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var act = async () => await strategy.ApplyAsync(request, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();

            // verify Warning log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "AsyncTokenProviderStrategy canceled"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
