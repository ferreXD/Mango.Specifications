// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    using System;

    /// <summary>
    /// Defines a contract for recording HTTP client metrics in Mango HTTP clients.
    /// Implementations should provide logic for tracking requests, durations, and failures.
    /// </summary>
    public interface IHttpClientMetricsProvider
    {
        /// <summary>
        /// Records an HTTP request event for the specified client and method.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="additionalTags">Additional tags to associate with the metric.</param>
        void RecordRequest(string clientName, HttpMethod method, string[] additionalTags);

        /// <summary>
        /// Records the duration of an HTTP request for the specified client and method.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="duration">The duration of the request.</param>
        /// <param name="status">The HTTP status code of the response.</param>
        /// <param name="additionalTags">Additional tags to associate with the metric.</param>
        void RecordDuration(string clientName, HttpMethod method, TimeSpan duration, int status, string[] additionalTags);

        /// <summary>
        /// Records a failure event for an HTTP request for the specified client and method.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="additionalTags">Additional tags to associate with the metric.</param>
        void RecordFailure(string clientName, HttpMethod method, Exception ex, string[] additionalTags);
    }
}
