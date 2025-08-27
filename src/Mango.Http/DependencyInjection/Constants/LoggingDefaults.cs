// ReSharper disable once CheckNamespace
namespace Mango.Http.Constants
{
    using Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides default configuration for Mango HTTP logging.
    /// </summary>
    public static class LoggingDefaults
    {
        /// <summary>
        /// The default name for the HTTP client used in logging.
        /// </summary>
        public const string DefaultClientName = "Mango.Http.Client";
        /// <summary>
        /// The default maximum body length for HTTP logging.
        /// </summary>
        public const int DefaultMaxBodyLength = 1024 * 10; // 10 KB
        /// <summary>
        /// The default log level for request logging.
        /// </summary>
        public static LogLevel DefaultRequestLogLevel = LogLevel.Information;
        /// <summary>
        /// The default log level for response logging.
        /// </summary>
        public static LogLevel DefaultSuccessResponseLogLevel = LogLevel.Information;
        /// <summary>
        /// The default log level for error logging.
        /// </summary>
        public static LogLevel DefaultErrorLogLevel = LogLevel.Error;

        /// <summary>
        /// Default configuration for Mango HTTP logging.
        /// </summary>
        public static Action<HttpLoggingConfigurator> DefaultConfiguration =
            builder => builder
                .Enable()
                .LogRequestBody()
                .LogResponseBody()
                .MaxBodyLength(DefaultMaxBodyLength)
                .RequestLevel(DefaultRequestLogLevel)
                .SuccessResponseLevel(DefaultSuccessResponseLogLevel)
                .ErrorLevel(DefaultErrorLogLevel);
    }
}
