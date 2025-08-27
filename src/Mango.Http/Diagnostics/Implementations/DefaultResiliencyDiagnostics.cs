// ReSharper disable once CheckNamespace
namespace Mango.Http.Diagnostics
{
    using Constants;
    using Mango.Http.Tracing;
    using Microsoft.Extensions.Logging;
    using Polly;

    /// <summary>
    /// Default implementation of <see cref="IResiliencyDiagnostics"/> for Mango HTTP clients.
    /// Logs, traces, and records telemetry events for retries, timeouts, circuit breaks, bulkhead rejections, and fallbacks.
    /// </summary>
    public sealed class DefaultResiliencyDiagnostics : IResiliencyDiagnostics
    {
        private readonly ILogger<DefaultResiliencyDiagnostics> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResiliencyDiagnostics"/> class.
        /// </summary>
        /// <param name="logger">The logger used for diagnostics events.</param>
        public DefaultResiliencyDiagnostics(ILogger<DefaultResiliencyDiagnostics> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        }

        /// <inheritdoc/>
        public void OnRetry(HttpRequestMessage request, int attempt, Exception? exception)
        {
            var tags = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.RetryAttempt, attempt },
                { MangoHttpTelemetryKeys.HttpMethod, request.Method.Method },
                { MangoHttpTelemetryKeys.HttpUrl, request.RequestUri?.ToString() }
            };

            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.Retry, request, tags);

            logger.LogWarning(
                exception,
                "Retrying request {Request} (attempt {Attempt})",
                FormatException(request, exception),
                attempt);
        }

        /// <inheritdoc/>
        public void OnTimeout(HttpRequestMessage request, TimeSpan timeout)
        {
            var tags = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.TimeoutMs, timeout.TotalMilliseconds }
            };

            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.Timeout, request, tags);

            logger.LogWarning(
                "Request {Request} timed out after {Timeout} milliseconds",
                FormatRequest(request),
                timeout.TotalMilliseconds);
        }

        /// <inheritdoc/>
        public void OnCircuitBreak(HttpRequestMessage request, Exception? exception)
        {
            var tags = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.HttpMethod, request.Method.Method },
                { MangoHttpTelemetryKeys.HttpUrl, request.RequestUri?.ToString() ?? "unknown" }
            };

            if (exception is not null)
            {
                tags.Add(MangoHttpTelemetryKeys.CircuitBreakerException, exception.GetType().Name);
            }

            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.CircuitBreakerTriggered, request, tags);

            logger.LogWarning(
                exception,
                "Circuit breaker triggered for request {Request}",
                FormatException(request, exception));
        }

        /// <inheritdoc/>
        public void OnCircuitReset(HttpRequestMessage request)
        {
            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.CircuitBreakerReset, request);

            logger.LogInformation(
                "Circuit breaker reset for request {Request}",
                FormatRequest(request));
        }

        /// <inheritdoc/>
        public void OnBulkheadRejected(HttpRequestMessage request, Exception? exception)
        {
            var tags = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.HttpMethod, request.Method.Method },
                { MangoHttpTelemetryKeys.HttpUrl, request.RequestUri?.ToString() ?? "unknown" }
            };

            if (exception is not null)
            {
                tags.Add(MangoHttpTelemetryKeys.OnBulkheadRejected, exception.GetType().Name);
            }

            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.Bulkhead, request, tags);

            logger.LogWarning(
                exception,
                "Bulkhead rejected request {Request}",
                FormatException(request, exception));
        }

        /// <inheritdoc/>
        public void OnFallback(HttpRequestMessage? request, DelegateResult<HttpResponseMessage> outcome)
        {
            if (request is null)
            {
                logger.LogWarning(
                    "Fallback executed for request with no details. Outcome: {Outcome}",
                    outcome.Result?.ToString() ?? "No response");
                return;
            }

            var tags = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.HttpMethod, request.Method.Method },
                { MangoHttpTelemetryKeys.HttpUrl, request.RequestUri?.ToString() ?? "unknown" }
            };

            if (outcome.Exception is not null)
            {
                tags.Add(MangoHttpTelemetryKeys.RetryException, outcome.Exception.GetType().Name);
            }

            ActivityRecordingHelpers.RecordEvent(MangoHttpTelemetryKeys.Fallback, request, tags);

            logger.LogWarning(
                outcome.Exception,
                "Fallback executed for request {Request}. Outcome: {Outcome}",
                FormatRequest(request),
                outcome.Result?.ToString() ?? "No response");
        }

        /// <summary>
        /// Formats the exception and request details for logging.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="exception">The exception, if any.</param>
        /// <returns>A formatted string representing the request and exception.</returns>
        private static string FormatException(HttpRequestMessage request, Exception? exception)
            => exception is null ? FormatRequest(request) : $"{FormatRequest(request)} - {exception.GetType().Name}: {exception.Message}";

        /// <summary>
        /// Formats the request details for logging.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A formatted string representing the request.</returns>
        private static string FormatRequest(HttpRequestMessage request)
            => $"{request.Method} {request.RequestUri}";
    }
}
