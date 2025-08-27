// ReSharper disable once CheckNamespace
namespace Mango.Http.Common
{
    /// <summary>
    /// A delegating handler that performs no additional processing and simply forwards the HTTP request.
    /// Useful as a placeholder or for testing handler pipelines.
    /// </summary>
    internal sealed class NoOpHandler : DelegatingHandler
    {
        /// <summary>
        /// Forwards the HTTP request to the next handler in the pipeline without modification.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message from the next handler.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => await base.SendAsync(request, cancellationToken);
    }
}
