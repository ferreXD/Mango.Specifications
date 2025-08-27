// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that applies authentication to outgoing HTTP requests for Mango HTTP clients.
    /// Uses the provided <see cref="IAuthenticationStrategy"/> and <see cref="HttpAuthOptions"/> to control authentication behavior.
    /// </summary>
    /// <remarks>
    /// This handler should be added to the HTTP client pipeline to enable automatic authentication.
    /// </remarks>
    public sealed class HttpAuthenticationHandler : DelegatingHandler
    {
        private readonly HttpAuthOptions opts;
        private readonly IAuthenticationStrategy _strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="opts">The authentication options.</param>
        /// <param name="strategy">The authentication strategy to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown if strategy is null.</exception>
        public HttpAuthenticationHandler(HttpAuthOptions opts, IAuthenticationStrategy strategy)
            : base()
        {
            this.opts = opts;
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Sends the HTTP request and applies authentication if enabled and the condition is met.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (opts.Enabled && (opts.Condition?.Invoke(request) ?? true))
            {
                await _strategy.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
