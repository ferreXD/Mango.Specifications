// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that applies a custom header to HTTP requests for Mango HTTP clients.
    /// Inherit from <see cref="InstrumentedAuthStrategy"/> to provide OpenTelemetry instrumentation and logging.
    /// </summary>
    /// <remarks>
    /// This strategy sets the specified header name and value on outgoing requests for authentication purposes.
    /// </remarks>
    public sealed class HeaderAuthenticationStrategy : InstrumentedAuthStrategy
    {
        private readonly string _headerName;
        private readonly string _headerValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderAuthenticationStrategy"/> class.
        /// </summary>
        /// <param name="headerName">The name of the header to add for authentication.</param>
        /// <param name="headerValue">The value of the header to add for authentication.</param>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if headerName or headerValue is null or empty.</exception>
        public HeaderAuthenticationStrategy(
            string headerName,
            string headerValue,
            ActivitySource activitySource,
            ILogger<HeaderAuthenticationStrategy> logger)
            : base(activitySource, logger)
        {
            if (string.IsNullOrEmpty(headerName))
                throw new ArgumentNullException(nameof(headerName), "Header name cannot be null or empty.");
            if (string.IsNullOrEmpty(headerValue))
                throw new ArgumentNullException(nameof(headerValue), "Header value cannot be null or empty.");

            _headerName = headerName;
            _headerValue = headerValue;
        }

        /// <summary>
        /// Applies the custom header to the HTTP request for authentication.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
        {
            request.Headers.Add(_headerName, _headerValue);
            return default;
        }
    }
}
