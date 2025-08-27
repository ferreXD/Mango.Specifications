// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that uses a cached token provider to apply Bearer tokens to HTTP requests for Mango HTTP clients.
    /// Inherit from <see cref="InstrumentedAuthStrategy"/> to provide OpenTelemetry instrumentation and logging.
    /// </summary>
    /// <remarks>
    /// This strategy retrieves tokens from an <see cref="ICachedTokenProvider"/> and applies them to outgoing requests.
    /// </remarks>
    public sealed class CachedTokenAuthStrategy : InstrumentedAuthStrategy
    {
        private readonly ICachedTokenProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedTokenAuthStrategy"/> class.
        /// </summary>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="provider">The cached token provider to retrieve tokens from.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if provider is null.</exception>
        public CachedTokenAuthStrategy(
            ActivitySource activitySource,
            ICachedTokenProvider provider,
            ILogger<CachedTokenAuthStrategy> logger) : base(activitySource, logger)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider), "CachedTokenProvider cannot be null.");

            _provider = provider;
        }

        /// <summary>
        /// Applies the Bearer token to the HTTP request using the cached token provider.
        /// Throws <see cref="MangoAuthenticationException"/> if the token is null or empty.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        /// <exception cref="MangoAuthenticationException">Thrown if the token is null or empty.</exception>
        protected override async ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _provider.GetTokenAsync(ct);
            if (string.IsNullOrEmpty(token))
            {
                Logger.LogError("Received null or empty token from CachedTokenProvider. Cannot apply authentication strategy.");
                throw new MangoAuthenticationException("Token cannot be null or empty.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
