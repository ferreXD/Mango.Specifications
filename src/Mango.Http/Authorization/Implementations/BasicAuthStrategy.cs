// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that applies HTTP Basic Authentication to requests for Mango HTTP clients.
    /// Inherit from <see cref="InstrumentedAuthStrategy"/> to provide OpenTelemetry instrumentation and logging.
    /// </summary>
    /// <remarks>
    /// This strategy encodes the username and password as a Base64-encoded header value for Basic Authentication.
    /// </remarks>
    public sealed class BasicAuthStrategy : InstrumentedAuthStrategy
    {
        private readonly string _headerValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthStrategy"/> class.
        /// </summary>
        /// <param name="username">The username for Basic Authentication.</param>
        /// <param name="password">The password for Basic Authentication.</param>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentException">Thrown if username is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if password is null.</exception>
        public BasicAuthStrategy(
            string username, string password,
            ActivitySource activitySource,
            ILogger<BasicAuthStrategy> logger)
            : base(activitySource, logger)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username required", nameof(username));
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            var bytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            _headerValue = Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Applies the Basic Authentication header to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        protected override ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _headerValue);
            return default;
        }
    }
}
