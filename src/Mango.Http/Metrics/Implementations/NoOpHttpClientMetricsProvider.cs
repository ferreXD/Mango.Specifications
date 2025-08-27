// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    using System;

    public sealed class NoOpHttpClientMetricsProvider : IHttpClientMetricsProvider
    {
        public void RecordRequest(string clientName, HttpMethod method, string[] additionalTags) { }
        public void RecordDuration(string clientName, HttpMethod method, TimeSpan duration, int status, string[] additionalTags) { }
        public void RecordFailure(string clientName, HttpMethod method, Exception ex, string[] additionalTags) { }
    }
}
