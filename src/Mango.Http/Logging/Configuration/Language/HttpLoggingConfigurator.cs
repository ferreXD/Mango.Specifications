// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Fluent configurator for building <see cref="HttpLoggingOptions"/> for Mango HTTP client logging.
    /// Use this class to enable logging, set log levels, body capture, excluded headers, and custom logger/handler types.
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new HttpLoggingConfigurator()
    ///     .Enable()
    ///     .RequestLevel(LogLevel.Information)
    ///     .ResponseLevel(LogLevel.Information)
    ///     .ErrorLevel(LogLevel.Error)
    ///     .LogRequestBody()
    ///     .LogResponseBody()
    ///     .MaxBodyLength(2048)
    ///     .ExcludeHeader("Authorization")
    ///     .UseLogger<DefaultHttpLogger>()
    ///     .Build();
    /// </code>
    /// </example>
    public class HttpLoggingConfigurator
    {
        private readonly HttpLoggingOptionsBuilder _builder = new();

        /// <summary>
        /// Enables or disables logging.
        /// </summary>
        /// <param name="enabled">True to enable logging; false to disable.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator Enable(bool enabled = true)
            => Fluent(_ => _.Enable(enabled));

        /// <summary>
        /// Sets a predicate to determine when logging should occur.
        /// </summary>
        /// <param name="predicate">A function that returns true if logging should occur for the request.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator When(Func<HttpRequestMessage, bool> predicate)
            => Fluent(_ => _.When(predicate));

        /// <summary>
        /// Sets the log level for request start events.
        /// </summary>
        /// <param name="level">The log level for requests.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator RequestLevel(LogLevel level)
            => Fluent(_ => _.WithRequestLevel(level));

        /// <summary>
        /// Sets the log level for response stop events.
        /// Sets the log level for response stop events.
        /// </summary>
        /// <param name="level">The log level for responses.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator SuccessResponseLevel(LogLevel level)
            => Fluent(_ => _.WithSuccessResponseLevel(level));

        /// <summary>
        /// Sets the log level for error events.
        /// </summary>
        /// <param name="level">The log level for errors.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator ErrorLevel(LogLevel level)
            => Fluent(_ => _.WithErrorLevel(level));

        /// <summary>
        /// Enables or disables logging of the request body.
        /// </summary>
        /// <param name="enabled">True to log the request body; false otherwise.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator LogRequestBody(bool enabled = true)
            => Fluent(_ => _.LogRequestBody(enabled));

        /// <summary>
        /// Enables or disables logging of the response body.
        /// </summary>
        /// <param name="enabled">True to log the response body; false otherwise.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator LogResponseBody(bool enabled = true)
            => Fluent(_ => _.LogResponseBody(enabled));

        /// <summary>
        /// Sets the maximum number of characters to capture for request/response bodies.
        /// </summary>
        /// <param name="maxChars">The maximum body length.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator MaxBodyLength(int maxChars)
            => Fluent(_ => _.MaxBodyLength(maxChars));

        /// <summary>
        /// Excludes a header from logging.
        /// </summary>
        /// <param name="header">The header name to exclude.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator ExcludeHeader(string header)
            => Fluent(_ => _.ExcludeHeader(header));

        /// <summary>
        /// Specifies a custom logger implementation for HTTP events.
        /// </summary>
        /// <typeparam name="TLogger">The logger type to use.</typeparam>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator UseLogger<TLogger>() where TLogger : IMangoHttpLogger
            => Fluent(_ => _.UseLogger<TLogger>());

        /// <summary>
        /// Specifies a custom delegating handler for HTTP logging.
        /// </summary>
        /// <typeparam name="THandler">The handler type to use.</typeparam>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator UseCustomHandler<THandler>() where THandler : DelegatingHandler
            => Fluent(_ => _.UseCustomHandler<THandler>());

        /// <summary>
        /// Adds an inspector action to write logging output to a custom target.
        /// </summary>
        /// <param name="write">The action to write log output.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator Inspect(Action<string> write)
        {
            _builder.Inspect(write);
            return this;
        }

        /// <summary>
        /// Adds an inspector logger to receive logging output.
        /// </summary>
        /// <param name="logger">The logger to receive log output.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpLoggingConfigurator Inspect(ILogger logger)
        {
            _builder.Inspect(logger);
            return this;
        }

        /// <summary>
        /// Builds the configured <see cref="HttpLoggingOptions"/>.
        /// </summary>
        /// <returns>The built <see cref="HttpLoggingOptions"/>.</returns>
        public HttpLoggingOptions Build() => _builder.Build();

        /// <summary>
        /// Helper for fluent chaining of builder actions.
        /// </summary>
        /// <param name="action">The action to apply to the builder.</param>
        /// <returns>The configurator for chaining.</returns>
        private HttpLoggingConfigurator Fluent(Action<HttpLoggingOptionsBuilder> action)
        {
            action(_builder);
            return this;
        }
    }
}
