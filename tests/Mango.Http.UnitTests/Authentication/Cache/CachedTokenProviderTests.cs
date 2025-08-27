// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Threading.Tasks;

    public class CachedTokenProviderTests
    {
        private readonly Mock<ILogger<CachedTokenProvider>> _loggerMock = new();

        [Fact]
        public void Ctor_NullTokenFactory_ThrowsArgumentNullException()
        {
            Action act = () => new CachedTokenProvider(
                _loggerMock.Object,
                null!,
                null);

            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("tokenFactory");
        }

        [Fact]
        public async Task GetTokenAsync_FirstCall_CallsFactoryCachesAndLogs()
        {
            // Arrange
            var callCount = 0;
            var now = DateTimeOffset.UtcNow;
            const string expectedToken = "token1";

            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct =>
                {
                    callCount++;
                    return new ValueTask<(string, DateTimeOffset)>((expectedToken, now.AddMinutes(5)));
                });

            // Act
            var token = await provider.GetTokenAsync();

            // Assert
            token.Should().Be(expectedToken);
            callCount.Should().Be(1, "factory runs on first miss");
            provider.RenewalCount.Should().Be(1);
            provider.FailureCount.Should().Be(0);
            provider.CancelCount.Should().Be(0);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Token cache is empty."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2), "should log empty-cache debug");

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.StartsWith("Token refreshed; expires at ")
                    && v.ToString()!.EndsWith(" UTC.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should log successful refresh");
        }

        [Fact]
        public async Task GetTokenAsync_SecondCallWithinMargin_ReturnsCachedWithoutCallingFactory()
        {
            // Arrange
            var callCount = 0;
            var now = DateTimeOffset.UtcNow;
            const string tokenValue = "cached";

            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct =>
                {
                    callCount++;
                    return new ValueTask<(string, DateTimeOffset)>((tokenValue, now.AddMinutes(5)));
                });

            // Act
            var first = await provider.GetTokenAsync();
            var second = await provider.GetTokenAsync();

            // Assert
            first.Should().Be(tokenValue);
            second.Should().Be(tokenValue);
            callCount.Should().Be(1, "factory should only run once when still valid");
            provider.RenewalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetTokenAsync_ExpiresWithinMargin_RefreshesAgain()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var callCount = 0;

            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct =>
                {
                    callCount++;
                    // First: expire in 10s (within default 30s margin), then in 5m
                    var expires = callCount == 1
                        ? now.AddSeconds(10)
                        : now.AddMinutes(5);
                    return new ValueTask<(string, DateTimeOffset)>(($"t{callCount}", expires));
                });

            // Act
            var first = await provider.GetTokenAsync();
            var second = await provider.GetTokenAsync();

            // Assert
            first.Should().Be("t1");
            second.Should().Be("t2");
            callCount.Should().Be(2, "factory should run again once the first token is within margin");
            provider.RenewalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetTokenAsync_CancellationWhileWaitingForLock_IncrementsCancelCountAndLogsAndThrows()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // cancel before we call

            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct => throw new InvalidOperationException("should not be called"));

            // Act
            Func<Task> act = () => provider.GetTokenAsync(cts.Token).AsTask();

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            provider.CancelCount.Should().Be(1);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Token refresh canceled waiting for lock."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should warn on lock wait cancellation");
        }

        [Fact]
        public async Task GetTokenAsync_CancellationInFactory_IncrementsCancelCountAndLogsAndThrows()
        {
            // Arrange: factory throws OCE
            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct => throw new OperationCanceledException("factory canceled"));

            // Act
            Func<Task> act = () => provider.GetTokenAsync().AsTask();

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            provider.CancelCount.Should().Be(1);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Token factory canceled."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should warn when factory cancels");
        }

        [Fact]
        public async Task GetTokenAsync_FactoryThrowsException_IncrementsFailureCountAndLogsAndThrows()
        {
            // Arrange
            var factoryEx = new Exception("oops");
            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct => throw factoryEx);

            // Act
            Func<Task> act = () => provider.GetTokenAsync().AsTask();

            // Assert
            var ex = await act.Should().ThrowAsync<Exception>();
            ex.Which.Should().BeSameAs(factoryEx);
            provider.FailureCount.Should().Be(1);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Token refresh failed."),
                factoryEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should error-log on factory failure");
        }

        [Fact]
        public async Task Invalidate_ClearsCacheAndLogsAndForcesRefresh()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var callCount = 0;
            const string tokenValue = "xyz";

            var provider = new CachedTokenProvider(
                _loggerMock.Object,
                ct =>
                {
                    callCount++;
                    return new ValueTask<(string, DateTimeOffset)>((tokenValue, now.AddMinutes(5)));
                });

            // seed cache
            var first = await provider.GetTokenAsync();
            first.Should().Be(tokenValue);
            provider.RenewalCount.Should().Be(1);

            // clear out prior log invocations
            _loggerMock.Invocations.Clear();

            // Act: invalidate and then fetch again
            provider.Invalidate();
            var second = await provider.GetTokenAsync();

            // Assert
            callCount.Should().Be(2, "invalidate forces a new factory call");
            second.Should().Be(tokenValue);
            provider.RenewalCount.Should().Be(2);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Token cache invalidated."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should log cache invalidation");
        }
    }
}
