// ReSharper disable once CheckNamespace
namespace Mango.Http.Diagnostics
{
    using Polly;
    using System;

    /// <summary>
    /// Defines a contract for diagnostics and telemetry events in Mango HTTP resiliency policies.
    /// Implementations can log, trace, or monitor events such as retries, timeouts, circuit breaks, bulkhead rejections, and fallbacks.
    /// </summary>
    public interface IResiliencyDiagnostics
    {
        /// <summary>
        /// Called when a retry attempt occurs.
        /// </summary>
        /// <param name="request">The HTTP request being retried.</param>
        /// <param name="attempt">The current retry attempt number.</param>
        /// <param name="exception">The exception that caused the retry, if any.</param>
        void OnRetry(HttpRequestMessage request, int attempt, Exception? exception);

        /// <summary>
        /// Called when a timeout occurs for a request.
        /// </summary>
        /// <param name="request">The HTTP request that timed out.</param>
        /// <param name="timeout">The timeout duration.</param>
        void OnTimeout(HttpRequestMessage request, TimeSpan timeout);

        /// <summary>
        /// Called when a circuit is broken due to failures.
        /// </summary>
        /// <param name="request">The HTTP request that triggered the circuit break.</param>
        /// <param name="exception">The exception that caused the circuit to break, if any.</param>
        void OnCircuitBreak(HttpRequestMessage request, Exception? exception);

        /// <summary>
        /// Called when a circuit is reset after being broken.
        /// </summary>
        /// <param name="request">The HTTP request that triggered the circuit reset.</param>
        void OnCircuitReset(HttpRequestMessage request);

        /// <summary>
        /// Called when a request is rejected by the bulkhead policy.
        /// </summary>
        /// <param name="request">The HTTP request that was rejected.</param>
        /// <param name="exception">The exception that caused the rejection, if any.</param>
        void OnBulkheadRejected(HttpRequestMessage request, Exception? exception = null);

        /// <summary>
        /// Called when a fallback policy is triggered for a request.
        /// </summary>
        /// <param name="request">The HTTP request for which fallback occurred (may be null).</param>
        /// <param name="outcome">The outcome of the HTTP response or exception.</param>
        void OnFallback(HttpRequestMessage? request, DelegateResult<HttpResponseMessage> outcome);
    }
}
