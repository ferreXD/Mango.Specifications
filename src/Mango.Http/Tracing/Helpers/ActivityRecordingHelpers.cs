// ReSharper disable once CheckNamespace
namespace Mango.Http.Tracing
{
    using Mango.Http.Diagnostics.Constants;
    using System.Diagnostics;

    internal static class ActivityRecordingHelpers
    {
        private static Activity? GetActivity(HttpRequestMessage request)
        {
            return request.Options.TryGetValue(MangoTracingConstants.ActivityKey, out var act)
                ? act
                : Activity.Current;
        }


        /// <summary>
        /// Records a telemetry event for the specified request and event name.
        /// </summary>
        /// <param name="eventName">The name of the telemetry event.</param>
        /// <param name="request">The HTTP request associated with the event.</param>
        /// <param name="tags">Optional tags to associate with the event.</param>
        internal static void RecordEvent(string eventName, HttpRequestMessage request, Dictionary<string, object?>? tags = null)
        {
            var act = GetActivity(request);
            if (act is null || !act.IsAllDataRequested) return;

            tags ??= new Dictionary<string, object?>();

            var allTags = BuildBaseActivityTagsDictionary(request, act);

            foreach (var tag in tags) allTags[tag.Key] = tag.Value;

            foreach (var tag in allTags) act.SetTag(tag.Key, tag.Value);
            act.AddEvent(new ActivityEvent(eventName));
        }

        /// <summary>
        /// Records a telemetry exception event for the specified request and event name.
        /// </summary>
        /// <param name="eventName">The name of the telemetry event.</param>
        /// <param name="exception">The exception to record.</param>
        /// <param name="request">The HTTP request associated with the event.</param>
        /// <param name="tags">Optional tags to associate with the event.</param>
        internal static void RecordException(string eventName, Exception exception, HttpRequestMessage request, Dictionary<string, object?>? tags = null)
        {
            var act = GetActivity(request);
            if (act is null || !act.IsAllDataRequested) return;

            tags ??= new Dictionary<string, object?>();

            var allTags = BuildBaseActivityTagsDictionary(request, act);

            foreach (var tag in tags) allTags[tag.Key] = tag.Value;

            foreach (var tag in allTags) act.SetTag(tag.Key, tag.Value);
            act.AddEvent(new ActivityEvent(eventName));
            act.SetStatus(ActivityStatusCode.Error, exception.Message);
        }

        private static Dictionary<string, object?> BuildBaseActivityTagsDictionary(HttpRequestMessage request, Activity act)
        {
            var dict = new Dictionary<string, object?>
            {
                { MangoHttpTelemetryKeys.HttpMethod, request.Method.Method },
                { MangoHttpTelemetryKeys.HttpUrl, request.RequestUri?.ToString() ?? "unknown" },
                { MangoHttpTelemetryKeys.TransactionId, act.TraceId.ToString() },
                { MangoHttpTelemetryKeys.SpanId, act.SpanId.ToString() },
                { MangoHttpTelemetryKeys.TraceparentId, act.Id }
            };


            if (request.Headers.TryGetValues("X-Correlation-ID", out var cid))
                dict.Add(MangoHttpTelemetryKeys.CorrelationId, cid.FirstOrDefault());

            return dict;
        }
    }
}
