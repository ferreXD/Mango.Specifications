// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;

    /// <summary>
    /// Implementation of <see cref="IHttpClientMetricsProvider"/> that records HTTP client metrics using OpenTelemetry.
    /// Provides counters and histograms for requests, durations, and failures.
    /// </summary>
    public class OpenTelemetryHttpClientMetricsProvider : IHttpClientMetricsProvider
    {
        private static readonly Meter _meter = new("Mango.Http.Client");
        private static readonly Counter<long> _requests = _meter.CreateCounter<long>("http.request.count");
        private static readonly Histogram<double> _duration = _meter.CreateHistogram<double>("http.request.duration");

        /// <summary>
        /// Records an HTTP request event for the specified client and method using OpenTelemetry.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="tags">Additional tags to associate with the metric.</param>
        public void RecordRequest(string clientName, HttpMethod method, string[] tags)
        {
            var attrs = BuildAttributes(clientName, method, tags);
            _requests.Add(1, attrs.ToArray());
        }

        /// <summary>
        /// Records the duration of an HTTP request for the specified client and method using OpenTelemetry.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="duration">The duration of the request.</param>
        /// <param name="status">The HTTP status code of the response.</param>
        /// <param name="tags">Additional tags to associate with the metric.</param>
        public void RecordDuration(string clientName, HttpMethod method, TimeSpan duration, int status, string[] tags)
        {
            var attrs = BuildAttributes(clientName, method, tags);
            attrs = attrs.Append(new("http.status_code", status));

            _duration.Record(duration.TotalMilliseconds, attrs.ToArray());
        }

        /// <summary>
        /// Records a failure event for an HTTP request for the specified client and method using OpenTelemetry.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="tags">Additional tags to associate with the metric.</param>
        public void RecordFailure(string clientName, HttpMethod method, Exception ex, string[] tags)
        {
            var attrs = BuildAttributes(clientName, method, tags);
            attrs = attrs.Append(new("error", ex.GetType().Name));

            _requests.Add(1, attrs.ToArray());
        }

        /// <summary>
        /// Builds the set of attributes for OpenTelemetry metrics from client name, method, and tags.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="tags">Additional tags to associate with the metric.</param>
        /// <returns>An enumerable of key-value pairs representing metric attributes.</returns>
        private IEnumerable<KeyValuePair<string, object?>> BuildAttributes(
            string clientName, HttpMethod method, string[] tags)
        {
            yield return new("client", clientName);
            yield return new("method", method.Method);
            foreach (var t in tags) yield return new(t, "true");
        }
    }
}
