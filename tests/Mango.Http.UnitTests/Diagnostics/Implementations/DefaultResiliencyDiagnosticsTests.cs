// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.ResiliencyDiagnostics
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Diagnostics.Constants;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Polly;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    public class DefaultResiliencyDiagnosticsTests : IDisposable
    {
        private readonly Mock<ILogger<DefaultResiliencyDiagnostics>> _loggerMock;
        private readonly DefaultResiliencyDiagnostics _diagnostics;
        private readonly ActivityListener _listener;
        private readonly List<Activity> _activities = new();

        public DefaultResiliencyDiagnosticsTests()
        {
            _loggerMock = new Mock<ILogger<DefaultResiliencyDiagnostics>>();
            _diagnostics = new DefaultResiliencyDiagnostics(_loggerMock.Object);

            _listener = new ActivityListener
            {
                ShouldListenTo = source => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity => _activities.Add(activity),
                ActivityStopped = activity => { }
            };
            ActivitySource.AddActivityListener(_listener);
        }

        public void Dispose()
        {
            _listener.Dispose();
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new DefaultResiliencyDiagnostics(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void OnRetry_WithException_RecordsExceptionEventAndLogsWarning()
        {
            using var activity = new Activity("test").Start();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://retry");
            var ex = new InvalidOperationException("fail");

            _diagnostics.OnRetry(request, 3, ex);

            // Check activity tags
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.HttpMethod, "GET"));
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.RetryAttempt, 3));
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.RetryException, nameof(InvalidOperationException)));
            // Check event
            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.Retry);
            evt.Should().NotBeNull();

            // Verify logger: standard Log method
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying request")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Fact]
        public void OnTimeout_RecordsTimeoutEventAndLogsWarning()
        {
            using var activity = new Activity("test2").Start();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://timeout");
            var timeout = TimeSpan.FromMilliseconds(500);

            _diagnostics.OnTimeout(request, timeout);

            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.TimeoutMs, timeout.TotalMilliseconds));
            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.Timeout);
            evt.Should().NotBeNull();

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("timed out")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnCircuitBreak_RecordsEvent_AndLogsWarning(bool withException)
        {
            using var activity = new Activity("test3").Start();
            var request = new HttpRequestMessage(HttpMethod.Delete, "http://circuit");
            var ex = withException ? new Exception("boom") : null;

            _diagnostics.OnCircuitBreak(request, ex);

            if (withException)
            {
                activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.CircuitBreakerException, ex!.GetType().Name));
            }
            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.CircuitBreakerTriggered);
            evt.Should().NotBeNull();

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Circuit breaker triggered")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Fact]
        public void OnCircuitReset_RecordsResetEvent_AndLogsInformation()
        {
            using var activity = new Activity("test4").Start();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://reset");

            _diagnostics.OnCircuitReset(request);

            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.CircuitBreakerReset);
            evt.Should().NotBeNull();
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Circuit breaker reset")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnBulkheadRejected_RecordsEvent_AndLogsWarning(bool withException)
        {
            using var activity = new Activity("test5").Start();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://bulkhead");
            Exception? ex = withException ? new InvalidOperationException() : null;

            _diagnostics.OnBulkheadRejected(request, ex);

            if (withException)
            {
                activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpTelemetryKeys.OnBulkheadRejected, ex!.GetType().Name));
            }
            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.Bulkhead);
            evt.Should().NotBeNull();

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bulkhead rejected")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Fact]
        public void OnFallback_NullRequest_LogsWarningOnly()
        {
            _diagnostics.OnFallback(null, new DelegateResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK)));
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fallback executed for request with no details")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnFallback_WithRequest_RecordsAndLogs(bool hasException)
        {
            using var activity = new Activity("test6").Start();
            var request = new HttpRequestMessage(HttpMethod.Put, "http://fallback");
            var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
            var exception = new Exception("fail");
            var outcome = hasException
                ? new DelegateResult<HttpResponseMessage>(exception)
                : new DelegateResult<HttpResponseMessage>(resp);

            _diagnostics.OnFallback(request, outcome);

            var evt = activity.Events.SingleOrDefault(e => e.Name == MangoHttpTelemetryKeys.Fallback);
            evt.Should().NotBeNull();

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fallback executed for request")),
                hasException ? exception : null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
        }
    }
}
