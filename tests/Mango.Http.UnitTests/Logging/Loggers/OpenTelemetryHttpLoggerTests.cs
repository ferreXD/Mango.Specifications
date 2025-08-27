// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class OpenTelemetryHttpLoggerTests : IDisposable
    {
        private readonly ActivitySource _activitySource;
        private readonly ActivityListener _listener;
        private readonly List<Activity> _activities = new();
        private readonly Mock<ILogger<OpenTelemetryHttpLogger>> _loggerMock;
        private readonly Mock<IOptionsMonitor<HttpLoggingOptions>> _optionsMonitorMock;
        private readonly OpenTelemetryHttpLogger _otelLogger;
        private readonly HttpLoggingOptions _options;
        private const string ClientName = "clientX";

        public OpenTelemetryHttpLoggerTests()
        {
            // Setup ActivitySource and listener
            _activitySource = new ActivitySource("TestSourceOtelLogger");
            _listener = new ActivityListener
            {
                ShouldListenTo = src => src.Name == "TestSourceOtelLogger",
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity => _activities.Add(activity),
                ActivityStopped = activity => { }
            };
            ActivitySource.AddActivityListener(_listener);

            // Setup options
            _options = new HttpLoggingOptions
            {
                Enabled = true,
                Condition = null,
                ActivityEventPrefix = "myprefix",
                LogRequestBody = false,
                LogResponseBody = false,
                MaxBodyLength = 100
            };
            _optionsMonitorMock = new Mock<IOptionsMonitor<HttpLoggingOptions>>();
            _optionsMonitorMock.Setup(m => m.Get(ClientName)).Returns(_options);

            _loggerMock = new Mock<ILogger<OpenTelemetryHttpLogger>>();
            _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _otelLogger = new OpenTelemetryHttpLogger(_activitySource, _loggerMock.Object, _optionsMonitorMock.Object, ClientName);
        }

        public void Dispose()
        {
            _listener.Dispose();
            _activitySource.Dispose();
        }

        [Fact]
        public void Constructor_NullActivitySource_Throws()
        {
            // Act
            Action act = () => new OpenTelemetryHttpLogger(null!, _loggerMock.Object, _optionsMonitorMock.Object, ClientName);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("activitySource");
        }

        [Fact]
        public async Task LogRequestAsync_ShouldStartActivity_WithCorrectTags()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Put, "http://test/request");

            // Act
            await _otelLogger.LogRequestAsync(request);

            // Assert
            _activities.Should().ContainSingle();
            var act = _activities.Single();
            act.DisplayName.Should().Be("myprefix.request");
            act.Kind.Should().Be(ActivityKind.Client);
            act.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpMethod, "PUT"));
            act.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpUrl, "http://test/request"));
        }

        [Fact]
        public async Task LogResponseAsync_ShouldSetResponseTags_AndStopActivity()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test/resp");
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("body")
            };
            // Start request activity
            await _otelLogger.LogRequestAsync(request);
            var activity = _activities.Last();
            Activity.Current = activity;
            var elapsed = TimeSpan.FromMilliseconds(50);

            // Act
            await _otelLogger.LogResponseAsync(request, response, elapsed);

            // Assert
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpResponseStatusCode, (int)HttpStatusCode.Accepted));
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpResponseElapsed, elapsed.TotalMilliseconds));
            activity.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            activity.IsAllDataRequested.Should().BeTrue();
            // Should have stopped
            activity.Duration.Should().BeCloseTo(elapsed, TimeSpan.FromMilliseconds(50));
        }

        [Fact]
        public async Task LogErrorAsync_ShouldSetErrorTags_AndStopActivity()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test/error");
            var ex = new InvalidOperationException("fail");
            // Start request activity
            await _otelLogger.LogRequestAsync(request);
            var activity = _activities.Last();
            Activity.Current = activity;
            var elapsed = TimeSpan.FromMilliseconds(100);

            // Act
            await _otelLogger.LogErrorAsync(request, ex, elapsed);

            // Assert
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpFailure, $"ERROR - {ex.GetType().Name}"));
            activity.TagObjects.Should().Contain(new KeyValuePair<string, object?>(MangoHttpLoggerTelemetryKeys.HttpFailureReason, ex.Message));
            activity.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
