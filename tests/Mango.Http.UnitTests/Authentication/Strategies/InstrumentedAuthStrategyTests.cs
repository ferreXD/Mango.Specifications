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

    // A concrete subclass for success tests
    internal class TestAuthStrategy(ActivitySource activitySource, ILogger logger) : InstrumentedAuthStrategy(activitySource, logger)
    {
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
            => ValueTask.CompletedTask;
    }

    // A concrete subclass that throws cancellation
    internal class CancellableAuthStrategy(ActivitySource activitySource, ILogger logger) : InstrumentedAuthStrategy(activitySource, logger)
    {
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }

    // A concrete subclass that throws a generic exception
    internal class ExceptionAuthStrategy(ActivitySource activitySource, ILogger logger) : InstrumentedAuthStrategy(activitySource, logger)
    {
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
            => throw new InvalidOperationException("something went wrong");
    }

    public class InstrumentedAuthStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger> _logger = new();

        [Fact]
        public void Ctor_WhenLoggerIsNull_Throws()
        {
            Action act = () => new TestAuthStrategy(_activitySource, null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("logger");
        }

        [Fact]
        public void Ctor_WhenActivitySourceIsNull_Throws()
        {
            Action act = () => new TestAuthStrategy(null!, _logger.Object);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("activitySource");
        }

        [Fact]
        public async Task ApplyAsync_WhenRequestIsNull_Throws()
        {
            var strategy = new TestAuthStrategy(_activitySource, _logger.Object);
            var act = async () => await strategy.ApplyAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>()
                     .WithParameterName("request");
        }

        [Fact]
        public async Task ApplyAsync_OnSuccess_InvokesInnerAndLogsDebugAndInformation()
        {
            var strategy = new TestAuthStrategy(_activitySource, _logger.Object);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

            await strategy.ApplyAsync(req); // should complete without error

            // verify Debug log: "Starting TestAuthStrategy"
            _logger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Starting TestAuthStrategy"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // verify Information log: "TestAuthStrategy applied in {n}ms"
            _logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("TestAuthStrategy applied in ") && v.ToString()!.EndsWith("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_WhenCanceled_LogsWarningAndRethrows()
        {
            var strategy = new CancellableAuthStrategy(_activitySource, _logger.Object);
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.test/");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await strategy.ApplyAsync(req, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();

            // verify Warning log: "{Strategy} canceled"
            _logger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "CancellableAuthStrategy canceled"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_WhenInnerThrows_LogsErrorAndWraps()
        {
            var strategy = new ExceptionAuthStrategy(_activitySource, _logger.Object);
            using var req = new HttpRequestMessage(HttpMethod.Put, "https://api.test/");

            var act = async () => await strategy.ApplyAsync(req);
            var ex = await act.Should().ThrowAsync<MangoAuthenticationException>()
                               .WithMessage("ExceptionAuthStrategy failed to apply authentication.*");

            ex.Which.InnerException.Should().BeOfType<InvalidOperationException>()
                                  .Which.Message.Contains("something went wrong");

            // verify Error log: "{Strategy} failed"
            _logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "ExceptionAuthStrategy failed"),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}