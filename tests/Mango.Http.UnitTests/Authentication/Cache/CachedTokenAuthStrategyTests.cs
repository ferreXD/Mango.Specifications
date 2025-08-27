// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class CachedTokenAuthStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ICachedTokenProvider> _providerMock = new();
        private readonly Mock<ILogger<CachedTokenAuthStrategy>> _loggerMock = new();

        [Fact]
        public void Ctor_NullProvider_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new CachedTokenAuthStrategy(
                _activitySource,
                null!,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("provider")
               .WithMessage("CachedTokenProvider cannot be null.*");
        }

        [Fact]
        public async Task ApplyAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var strategy = new CachedTokenAuthStrategy(
                _activitySource,
                _providerMock.Object,
                _loggerMock.Object);

            // Act
            var act = async () => await strategy.ApplyAsync(null!);

            // Assert
            await act.Should()
                     .ThrowAsync<ArgumentNullException>()
                     .WithParameterName("request");
        }

        [Fact]
        public async Task ApplyAsync_ValidToken_SetsAuthorizationHeaderAndLogs()
        {
            // Arrange
            var token = "valid-token";
            _providerMock
                .Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var strategy = new CachedTokenAuthStrategy(
                _activitySource,
                _providerMock.Object,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");

            // Act
            await strategy.ApplyAsync(request);

            // Assert: header applied
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be(token);

            // verify start debug log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Starting CachedTokenAuthStrategy"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // verify information log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.StartsWith("CachedTokenAuthStrategy applied in ")
                    && v.ToString()!.EndsWith("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // ensure provider was called exactly once
            _providerMock.Verify(p => p.GetTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_EmptyToken_ThrowsMangoAuthenticationExceptionAndLogsError()
        {
            // Arrange
            _providerMock
                .Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var strategy = new CachedTokenAuthStrategy(
                _activitySource,
                _providerMock.Object,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/");

            // Act
            var act = async () => await strategy.ApplyAsync(request);

            var ex = await act.Should()
                              .ThrowAsync<MangoAuthenticationException>()
                              .WithMessage("CachedTokenAuthStrategy failed to apply authentication.*");

            // inner exception should be the one thrown by InnerApplyAsync
            ex.Which.InnerException.Should().BeOfType<MangoAuthenticationException>()
                .Which.Message.Should().Be("Token cannot be null or empty.");

            // verify inner error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Received null or empty token from CachedTokenProvider. Cannot apply authentication strategy."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // verify outer error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "CachedTokenAuthStrategy failed"),
                It.IsAny<MangoAuthenticationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_ProviderThrowsOperationCanceled_ThrowsAndLogsWarning()
        {
            // Arrange
            var strategy = new CachedTokenAuthStrategy(
                _activitySource,
                _providerMock.Object,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Put, "https://example.com/");
            using var cts = new CancellationTokenSource();

            _providerMock
                .Setup(p => p.GetTokenAsync(cts.Token))
                .ThrowsAsync(new OperationCanceledException("canceled"));

            // Act
            var act = async () => await strategy.ApplyAsync(request, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();

            // verify warning log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "CachedTokenAuthStrategy canceled"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // provider called with the same token
            _providerMock.Verify(p => p.GetTokenAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_ProviderThrowsException_ThrowsWrappedMangoAuthenticationExceptionAndLogsError()
        {
            // Arrange
            var providerEx = new InvalidOperationException("factory fail");
            _providerMock
                .Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(providerEx);

            var strategy = new CachedTokenAuthStrategy(
                _activitySource,
                _providerMock.Object,
                _loggerMock.Object);

            using var request = new HttpRequestMessage(HttpMethod.Delete, "https://example.com/");

            // Act
            var act = async () => await strategy.ApplyAsync(request);

            var ex = await act.Should()
                              .ThrowAsync<MangoAuthenticationException>()
                              .WithMessage("CachedTokenAuthStrategy failed to apply authentication.*");

            ex.Which.InnerException.Should().Be(providerEx);

            // verify error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "CachedTokenAuthStrategy failed"),
                providerEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
