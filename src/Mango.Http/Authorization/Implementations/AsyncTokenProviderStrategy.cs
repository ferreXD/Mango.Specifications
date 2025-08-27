// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that uses an asynchronous token provider to apply Bearer tokens to HTTP requests.
    /// Inherit from <see cref="InstrumentedAuthStrategy"/> to provide OpenTelemetry instrumentation and logging.
    /// </summary>
    /// <remarks>
    /// This strategy is suitable for scenarios where tokens are retrieved asynchronously, such as OAuth flows or external providers.
    /// </remarks>
    public sealed class AsyncTokenProviderStrategy : InstrumentedAuthStrategy
    {
        private readonly Func<CancellationToken, ValueTask<string>> _tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTokenProviderStrategy"/> class.
        /// </summary>
        /// <param name="tokenProvider">A function that asynchronously provides the authentication token.</param>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if tokenProvider is null.</exception>
        public AsyncTokenProviderStrategy(
            Func<CancellationToken, ValueTask<string>> tokenProvider,
            ActivitySource activitySource,
            ILogger<AsyncTokenProviderStrategy> logger)
            : base(activitySource, logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        /// <summary>
        /// Applies the Bearer token to the HTTP request using the asynchronous token provider.
        /// Throws <see cref="MangoAuthenticationException"/> if the token is null or empty.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        /// <exception cref="MangoAuthenticationException">Thrown if the token is null or empty.</exception>
        protected override async ValueTask InnerApplyAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _tokenProvider(ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
                throw new MangoAuthenticationException("Token cannot be null or empty.");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
