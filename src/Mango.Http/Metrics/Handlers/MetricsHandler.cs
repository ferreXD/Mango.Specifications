// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that records HTTP client metrics for requests, durations, and failures.
    /// Uses the provided metrics provider to track metrics for each request and response.
    /// </summary>
    /// <remarks>
    /// This handler should be added to the HTTP client pipeline to enable automatic metrics collection.
    /// </remarks>
    public sealed class MetricsHandler : DelegatingHandler
    {
        private readonly IHttpClientMetricsProvider metrics;
        private readonly string clientName;
        private readonly string[] additionalTags;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsHandler"/> class.
        /// </summary>
        /// <param name="metrics">The metrics provider to record metrics.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="additionalTags">Additional tags to associate with metrics.</param>
        public MetricsHandler(
            IHttpClientMetricsProvider metrics,
            string clientName,
            string[] additionalTags)
        {
            this.metrics = metrics;
            this.clientName = clientName;
            this.additionalTags = additionalTags;
        }

        /// <summary>
        /// Sends the HTTP request and records metrics for request, duration, and failures.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            metrics.RecordRequest(clientName, request.Method, additionalTags);
            var sw = Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                sw.Stop();
                metrics.RecordDuration(clientName, request.Method, sw.Elapsed, (int)response.StatusCode, additionalTags);

                if (!response.IsSuccessStatusCode)
                {
                    metrics.RecordFailure(clientName, request.Method, new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase})."), additionalTags);
                }

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                metrics.RecordFailure(clientName, request.Method, ex, additionalTags);
                throw;
            }
        }
    }
}
