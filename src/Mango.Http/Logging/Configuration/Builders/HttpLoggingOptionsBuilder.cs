// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Fluent builder for configuring <see cref="HttpLoggingOptions"/> for Mango HTTP client logging.
    /// Use this class to enable logging, set log levels, body capture, excluded headers, and custom logger/handler types.
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new HttpLoggingOptionsBuilder()
    ///     .Enable()
    ///     .WithRequestLevel(LogLevel.Information)
    ///     .WithResponseLevel(LogLevel.Information)
    ///     .WithErrorLevel(LogLevel.Error)
    ///     .LogRequestBody()
    ///     .LogResponseBody()
    ///     .MaxBodyLength(2048)
    ///     .ExcludeHeader("Authorization")
    ///     .UseLogger<DefaultHttpLogger>()
    ///     .Build();
    /// </code>
    /// </example>
    public sealed class HttpLoggingOptionsBuilder
    {
        private readonly HttpLoggingOptions _opts = new();

        /// <summary>
        /// Enables or disables logging.
        /// </summary>
        /// <param name="enabled">True to enable logging; false to disable.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder Enable(bool enabled = true)
        {
            _opts.Enabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets a predicate to determine when logging should occur.
        /// </summary>
        /// <param name="predicate">A function that returns true if logging should occur for the request.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if predicate is null.</exception>
        public HttpLoggingOptionsBuilder When(Func<HttpRequestMessage, bool> predicate)
        {
            _opts.Condition = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return this;
        }

        /// <summary>
        /// Sets the log level for request start events.
        /// </summary>
        /// <param name="level">The log level for requests.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder WithRequestLevel(LogLevel level)
        {
            _opts.RequestLevel = level;
            return this;
        }

        /// <summary>
        /// Sets the log level for response stop events.
        /// </summary>
        /// <param name="level">The log level for responses.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder WithSuccessResponseLevel(LogLevel level)
        {
            _opts.ResponseSuccessLevel = level;
            return this;
        }

        /// <summary>
        /// Sets the log level for error events.
        /// </summary>
        /// <param name="level">The log level for errors.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder WithErrorLevel(LogLevel level)
        {
            _opts.ErrorLevel = level;
            return this;
        }

        /// <summary>
        /// Enables or disables logging of the request body.
        /// </summary>
        /// <param name="enabled">True to log the request body; false otherwise.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder LogRequestBody(bool enabled = true)
        {
            _opts.LogRequestBody = enabled;
            return this;
        }

        /// <summary>
        /// Enables or disables logging of the response body.
        /// </summary>
        /// <param name="enabled">True to log the response body; false otherwise.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder LogResponseBody(bool enabled = true)
        {
            _opts.LogResponseBody = enabled;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of characters to capture for request/response bodies.
        /// </summary>
        /// <param name="maxChars">The maximum body length.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if maxChars is less than or equal to zero.</exception>
        public HttpLoggingOptionsBuilder MaxBodyLength(int maxChars)
        {
            if (maxChars <= 0) throw new ArgumentOutOfRangeException(nameof(maxChars));
            _opts.MaxBodyLength = maxChars;
            return this;
        }

        /// <summary>
        /// Excludes a header from logging.
        /// </summary>
        /// <param name="header">The header name to exclude.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if header is null or whitespace.</exception>
        public HttpLoggingOptionsBuilder ExcludeHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) throw new ArgumentException("Header cannot be empty", nameof(header));
            _opts.ExcludedHeaders.Add(header);
            return this;
        }

        /// <summary>
        /// Specifies a custom logger implementation for HTTP events.
        /// </summary>
        /// <typeparam name="TLogger">The logger type to use.</typeparam>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder UseLogger<TLogger>() where TLogger : IMangoHttpLogger
        {
            _opts.LoggerType = typeof(TLogger);
            return this;
        }

        /// <summary>
        /// Specifies a custom delegating handler for HTTP logging.
        /// </summary>
        /// <typeparam name="THandler">The handler type to use.</typeparam>
        /// <returns>The builder for chaining.</returns>
        public HttpLoggingOptionsBuilder UseCustomHandler<THandler>() where THandler : DelegatingHandler
        {
            _opts.UseCustomHandler = true;
            _opts.CustomHandlerType = typeof(THandler);
            return this;
        }

        /// <summary>
        /// Adds an inspector action to write logging output to a custom target.
        /// </summary>
        /// <param name="write">The action to write log output.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if write is null.</exception>
        public HttpLoggingOptionsBuilder Inspect(Action<string> write)
        {
            write = write ?? throw new ArgumentNullException(nameof(write));
            write($"[Logging] Enabled={_opts.Enabled}");
            write($"[Logging] Condition={(_opts.Condition != null ? "Custom" : "Always")}");
            write($"[Logging] Levels: Req={_opts.RequestLevel}, Res={_opts.ResponseSuccessLevel}, Err={_opts.ErrorLevel}");
            write($"[Logging] Body: Req={_opts.LogRequestBody}, Res={_opts.LogResponseBody}, MaxLen={_opts.MaxBodyLength}");
            write($"[Logging] ExcludedHeaders=[{string.Join(',', _opts.ExcludedHeaders)}]");
            write($"[Logging] LoggerType={_opts.LoggerType?.Name}");
            write($"[Logging] CustomHandler={_opts.UseCustomHandler}:{_opts.CustomHandlerType?.Name}");
            return this;
        }

        /// <summary>
        /// Adds an inspector logger to receive logging output.
        /// </summary>
        /// <param name="logger">The logger to receive log output.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if logger is null.</exception>
        public HttpLoggingOptionsBuilder Inspect(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return Inspect((x) => logger.LogDebug(x));
        }

        /// <summary>
        /// Validates the logging options before building.
        /// Throws if logging is enabled but neither a logger nor custom handler is configured.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if logging is enabled but neither a logger nor custom handler is configured.</exception>
        private void Validate()
        {
            if (!_opts.Enabled)
                return;
            if (!_opts.UseCustomHandler && _opts.LoggerType == null)
                throw new InvalidOperationException("Either a LoggerType or a CustomHandlerType must be configured when logging is enabled.");
        }

        /// <summary>
        /// Builds and validates the configured <see cref="HttpLoggingOptions"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="HttpLoggingOptions"/>.</returns>
        public HttpLoggingOptions Build()
        {
            Validate();
            return _opts;
        }
    }
}
