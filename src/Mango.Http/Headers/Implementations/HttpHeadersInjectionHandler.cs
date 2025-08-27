// ReSharper disable once CheckNamespace
namespace Mango.Http.Headers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that injects custom HTTP headers into outgoing requests for Mango HTTP clients.
    /// Uses the provided <see cref="HttpHeadersOptions"/> to add headers before sending the request.
    /// </summary>
    /// <remarks>
    /// This handler should be added to the HTTP client pipeline to enable automatic header injection.
    /// </remarks>
    public sealed class HttpHeadersInjectionHandler : DelegatingHandler
    {
        private readonly HttpHeadersOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeadersInjectionHandler"/> class.
        /// </summary>
        /// <param name="options">The options containing custom headers to inject.</param>
        public HttpHeadersInjectionHandler(HttpHeadersOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Sends the HTTP request and injects custom headers as specified in <see cref="HttpHeadersOptions"/>.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var (key, valueFactory) in options.CustomHeaders)
            {
                var value = valueFactory.Invoke();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
