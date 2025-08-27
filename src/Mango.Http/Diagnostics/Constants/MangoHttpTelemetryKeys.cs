namespace Mango.Http.Diagnostics.Constants
{
    public static class MangoHttpTelemetryKeys
    {
        public const string HttpMethod = "http.method";
        public const string HttpUrl = "http.url";
        public const string Timeout = "http.timeout";
        public const string TimeoutMs = "http.timeout";
        public const string Retry = "http.retry";
        public const string RetryAttempt = "http.retry.attempt";
        public const string RetryException = "http.retry.exception";
        public const string CircuitBreakerReset = "http.circuit.reset";
        public const string CircuitBreakerTriggered = "http.circuit.triggered";
        public const string CircuitBreakerException = "http.circuit.breaker.exception";
        public const string Bulkhead = "http.bulkhead";
        public const string OnBulkheadRejected = "http.bulkhead.rejected";
        public const string Fallback = "http.fallback";
        public const string TransactionId = "trace.id";
        public const string SpanId = "span.id";
        public const string TraceparentId = "traceparent.id";
        public const string CorrelationId = "correlation.id";
    }
}
