// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that logs HTTP requests, responses, and errors for Mango HTTP clients.
    /// Uses the provided <see cref="IMangoHttpLogger"/> to record request lifecycle events and durations.
    /// </summary>
    /// <remarks>
    /// This handler should be added to the HTTP client pipeline to enable automatic logging.
    /// </remarks>
    public sealed class HttpLoggingHandler : DelegatingHandler
    {
        private readonly IMangoHttpLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpLoggingHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used for HTTP request/response/error events.</param>
        public HttpLoggingHandler(IMangoHttpLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Sends the HTTP request and logs request, response, and error events.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await logger.LogRequestAsync(request);
                var response = await base.SendAsync(request, cancellationToken);
                sw.Stop();
                await logger.LogResponseAsync(request, response, sw.Elapsed);
                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                await logger.LogErrorAsync(request, ex, sw.Elapsed);
                throw;
            }
        }
    }
}
