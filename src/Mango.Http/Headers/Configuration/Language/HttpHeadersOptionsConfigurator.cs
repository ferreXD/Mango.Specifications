// ReSharper disable once CheckNamespace
namespace Mango.Http.Headers
{
    using System.Diagnostics;

    /// <summary>
    /// Fluent configurator for building <see cref="HttpHeadersOptions"/> for Mango HTTP client custom headers.
    /// Use this class to add custom headers, correlation/request IDs, and build header options for injection.
    /// </summary>
    /// <example>
    /// <code>
    /// var configurator = new HttpHeadersOptionsConfigurator()
    ///     .WithCustomHeader("X-Api-Key", "my-key")
    ///     .WithCorrelationIdHeader()
    ///     .WithRequestIdHeader();
    /// var options = configurator.Build();
    /// </code>
    /// </example>
    public class HttpHeadersOptionsConfigurator
    {
        private readonly HttpHeadersOptionsBuilder _builder = new();

        /// <summary>
        /// Adds a custom header with a static value to the options.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <param name="overwrite">Whether to overwrite an existing header with the same name.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null or whitespace.</exception>
        public HttpHeadersOptionsConfigurator WithCustomHeader(string name, string value, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value), "Header value cannot be null or whitespace.");
            return WithCustomHeader(name, () => value, overwrite);
        }

        /// <summary>
        /// Adds a custom header with a dynamic value to the options.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">A function that returns the header value.</param>
        /// <param name="overwrite">Whether to overwrite an existing header with the same name.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public HttpHeadersOptionsConfigurator WithCustomHeader(string name, Func<string?> value, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "Header name cannot be null or whitespace.");
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            _builder.AddCustomHeader(name, value, overwrite);
            return this;
        }

        /// <summary>
        /// Adds a correlation ID header to the options, using the current activity's trace ID.
        /// </summary>
        /// <param name="headerName">The header name (default is "X-Correlation-ID").</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpHeadersOptionsConfigurator WithCorrelationIdHeader(string headerName = "X-Correlation-ID") => WithCustomHeader(headerName, () => Activity.Current?.TraceId.ToString());

        /// <summary>
        /// Adds a request ID header to the options, using the current activity's ID.
        /// </summary>
        /// <param name="headerName">The header name (default is "X-Request-ID").</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpHeadersOptionsConfigurator WithRequestIdHeader(string headerName = "X-Request-ID") => WithCustomHeader(headerName, () => Activity.Current?.Id);

        /// <summary>
        /// Builds the configured <see cref="HttpHeadersOptions"/>.
        /// </summary>
        /// <returns>The built <see cref="HttpHeadersOptions"/>.</returns>
        public HttpHeadersOptions Build() => _builder.Build();
    }
}
