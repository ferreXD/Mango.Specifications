// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class CompositeAuthStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger<CompositeAuthStrategy>> _loggerMock = new();

        [Fact]
        public void Ctor_NullLogger_ThrowsArgumentNullException()
        {
            var strategies = new[] { new Mock<IAuthenticationStrategy>().Object };
            Action act = () => new CompositeAuthStrategy(
                _activitySource,
                null!,
                strategies);

            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("logger");
        }

        [Fact]
        public void Ctor_NullStrategies_ThrowsArgumentNullException()
        {
            Action act = () => new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                null!);

            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("strategies");
        }

        [Fact]
        public void Ctor_EmptyStrategies_ThrowsArgumentException()
        {
            Action act = () => new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                Array.Empty<IAuthenticationStrategy>());

            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("strategies")
               .WithMessage("At least one authentication strategy must be provided.*");
        }

        [Fact]
        public void Ctor_StrategiesContainOnlyNulls_ThrowsArgumentException()
        {
            var list = new IAuthenticationStrategy[] { null!, null! };
            Action act = () => new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                list);

            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("strategies");
        }

        [Fact]
        public void Ctor_NullActivitySource_ThrowsArgumentNullException()
        {
            var mockStrat = new Mock<IAuthenticationStrategy>().Object;
            Action act = () => new CompositeAuthStrategy(
                null!,
                _loggerMock.Object,
                new[] { mockStrat });

            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("activitySource");
        }

        [Fact]
        public async Task ApplyAsync_NullRequest_ThrowsArgumentNullException()
        {
            var strat = new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                new[] { new Mock<IAuthenticationStrategy>().Object });

            var act = async () => await strat.ApplyAsync(null!);

            await act.Should()
                     .ThrowAsync<ArgumentNullException>()
                     .WithParameterName("request");
        }

        [Fact]
        public async Task ApplyAsync_AllStrategiesSucceed_CallsEachInOrderAndLogs()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://test/");
            var calls = new List<string>();
            var mockStrat1 = new Mock<IAuthenticationStrategy>();
            mockStrat1
                .Setup(s => s.ApplyAsync(request, It.IsAny<CancellationToken>()))
                .Returns((HttpRequestMessage req, CancellationToken ct) =>
                {
                    calls.Add("first");
                    return ValueTask.CompletedTask;
                });

            var mockStrat2 = new Mock<IAuthenticationStrategy>();
            mockStrat2
                .Setup(s => s.ApplyAsync(request, It.IsAny<CancellationToken>()))
                .Returns((HttpRequestMessage req, CancellationToken ct) =>
                {
                    calls.Add("second");
                    return default;
                });

            var strat = new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                new[] { mockStrat1.Object, mockStrat2.Object });

            // Act
            await strat.ApplyAsync(request);

            // Assert
            calls.Should().Equal("first", "second");

            // Verify debug and info logs for each strategy
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Applying auth strategy")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.StartsWith("Auth strategy")
                    && v.ToString()!.EndsWith("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ApplyAsync_StrategyThrowsOperationCanceled_LogsWarningAndPropagates()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://test/");
            var mockStrat = new Mock<IAuthenticationStrategy>();
            mockStrat
                .Setup(s => s.ApplyAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            var strat = new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                new[] { mockStrat.Object, new Mock<IAuthenticationStrategy>().Object });

            // Act
            var act = async () => await strat.ApplyAsync(request);

            await act.Should().ThrowAsync<OperationCanceledException>();

            // Warning log for the failed strategy
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("canceled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Ensure subsequent strategies are not invoked
            mockStrat.Verify(s => s.ApplyAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_StrategyThrowsException_LogsErrorAndWraps()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Put, "https://test/");
            var innerEx = new InvalidOperationException("fail");
            var mockStrat = new Mock<IAuthenticationStrategy>();
            mockStrat
                .Setup(s => s.ApplyAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(innerEx);

            var strat = new CompositeAuthStrategy(
                _activitySource,
                _loggerMock.Object,
                new[] { mockStrat.Object });

            // Act
            var act = async () => await strat.ApplyAsync(request);

            var ex = await act.Should()
                              .ThrowAsync<MangoAuthenticationException>()
                              .WithMessage("Strategy * failed to apply authentication.*");

            ex.Which.InnerException.Should().Be(innerEx);

            // verify error log
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("failed")),
                innerEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
