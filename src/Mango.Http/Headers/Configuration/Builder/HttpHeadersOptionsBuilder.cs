// ReSharper disable once CheckNamespace
namespace Mango.Http.Headers
{
    /// <summary>
    /// Builder for configuring <see cref="HttpHeadersOptions"/> for Mango HTTP client custom headers.
    /// Use this class to add custom headers and build header options for injection.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new HttpHeadersOptionsBuilder()
    ///     .AddCustomHeader("X-Api-Key", () => "my-key")
    ///     .AddCustomHeader("X-Correlation-ID", () => Activity.Current?.TraceId.ToString());
    /// var options = builder.Build();
    /// </code>
    /// </example>
    internal class HttpHeadersOptionsBuilder
    {
        private readonly HttpHeadersOptions _options = new();

        /// <summary>
        /// Adds a custom header to the options with a dynamic value factory.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">A function that returns the header value.</param>
        /// <param name="overwrite">Whether to overwrite an existing header with the same name.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if name is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if header already exists and overwrite is false.</exception>
        public HttpHeadersOptionsBuilder AddCustomHeader(string name, Func<string?> value, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Header name cannot be null or whitespace.", nameof(name));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (!overwrite && _options.CustomHeaders.ContainsKey(name)) throw new InvalidOperationException($"Header '{name}' already exists. Set 'overwrite' to true to replace it.");

            _options.CustomHeaders[name] = value;
            return this;
        }

        /// <summary>
        /// Builds the configured <see cref="HttpHeadersOptions"/>.
        /// </summary>
        /// <returns>The built <see cref="HttpHeadersOptions"/>.</returns>
        internal HttpHeadersOptions Build() => _options;
    }
}
