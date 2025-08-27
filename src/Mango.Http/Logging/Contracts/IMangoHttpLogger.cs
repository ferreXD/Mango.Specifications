// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    /// <summary>
    /// Defines a contract for logging HTTP requests, responses, and errors in Mango HTTP clients.
    /// Implementations should provide logic for recording request lifecycle events and durations.
    /// </summary>
    public interface IMangoHttpLogger
    {
        /// <summary>
        /// Logs the HTTP request event, including headers and optionally the body.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogRequestAsync(HttpRequestMessage request);

        /// <summary>
        /// Logs the HTTP response event, including headers and optionally the body.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed);

        /// <summary>
        /// Logs an error event for the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogErrorAsync(HttpRequestMessage request, Exception ex, TimeSpan elapsed);
    }
}
