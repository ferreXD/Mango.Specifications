// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    internal static class MangoHttpLoggerTelemetryKeys
    {
        internal const string HttpMethod = "http.request.method";
        internal const string HttpUrl = "url.full";

        internal const string HttpRequest = "http.request.start";
        internal const string HttpRequestHeaders = "http.request.headers";
        internal const string HttpRequestBody = "http.request.body";
        internal const string HttpRequestStartTime = "http.request.start_time";

        internal const string HttpResponse = "http.response.stop";
        internal const string HttpResponseStatusCode = "http.response.status_code";
        internal const string HttpResponseHeaders = "http.response.headers";
        internal const string HttpResponseBody = "http.response.body";
        internal const string HttpResponseElapsed = "http.response.elapsed";
        internal const string HttpResponseStopTime = "http.response.stop_time";

        internal const string HttpFailure = "http.request.error";
        internal const string HttpFailureReason = "http.failure.error.message";
        internal const string HttpFailureStackTrace = "http.failure.stack_trace";
        internal const string HttpFailureElapsed = "http.failure.elapsed";
        internal const string HttpFailureFailureTime = "http.failure.stop_time";
    }
}
