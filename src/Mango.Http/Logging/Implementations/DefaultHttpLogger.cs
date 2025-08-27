// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Default implementation of <see cref="BaseDefaultHttpLogger"/> for Mango HTTP clients.
    /// Uses standard logging and options to record HTTP request, response, and error events.
    /// </summary>
    /// <remarks>
    /// This logger is used if no custom logger type is specified in <see cref="HttpLoggingOptions"/>.
    /// </remarks>
    public sealed class DefaultHttpLogger : BaseDefaultHttpLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger used for HTTP events.</param>
        /// <param name="optionsMonitor">The options monitor for logging configuration.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        public DefaultHttpLogger(ILogger<DefaultHttpLogger> logger, IOptionsMonitor<HttpLoggingOptions> optionsMonitor, string clientName)
            : base(logger, optionsMonitor, clientName)
        {
        }
    }
}
