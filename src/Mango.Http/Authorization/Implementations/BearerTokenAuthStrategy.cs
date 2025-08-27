// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that applies a Bearer token to HTTP requests for Mango HTTP clients.
    /// Inherit from <see cref="InstrumentedAuthStrategy"/> to provide OpenTelemetry instrumentation and logging.
    /// </summary>
    /// <remarks>
    /// This strategy sets the Authorization header using the provided Bearer token.
    /// </remarks>
    public sealed class BearerTokenAuthStrategy : InstrumentedAuthStrategy
    {
        private readonly string _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenAuthStrategy"/> class.
        /// </summary>
        /// <param name="token">The Bearer token to use for authentication.</param>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if token is null or empty.</exception>
        public BearerTokenAuthStrategy(
            string token,
            ActivitySource activitySource,
            ILogger<BearerTokenAuthStrategy> logger) : base(activitySource, logger)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");

            _token = token;
        }

        /// <summary>
        /// Applies the Bearer token to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return default;
        }
    }
}
