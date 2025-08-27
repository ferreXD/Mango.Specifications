// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    using System.Linq;

    /// <summary>
    /// Builder for configuring <see cref="HttpClientMetricsOptions"/> for Mango HTTP client metrics.
    /// Use this class to enable/disable metrics and add custom tags for metrics collection.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new HttpClientMetricsOptionsBuilder()
    ///     .Enable()
    ///     .WithAdditionalTag("env:prod")
    ///     .WithAdditionalTags("service:api", "region:us");
    /// var options = builder.Build();
    /// </code>
    /// </example>
    public class HttpClientMetricsOptionsBuilder
    {
        private HttpClientMetricsOptions _options = new HttpClientMetricsOptions();

        /// <summary>
        /// Enables metrics collection for the HTTP client.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public HttpClientMetricsOptionsBuilder Enable()
        {
            _options.Enabled = true;
            return this;
        }

        /// <summary>
        /// Disables metrics collection for the HTTP client.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public HttpClientMetricsOptionsBuilder Disable()
        {
            _options.Enabled = false;
            return this;
        }

        /// <summary>
        /// Adds a single custom tag for metrics collection.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpClientMetricsOptionsBuilder WithAdditionalTag(string tag)
        {
            _options.AdditionalTags.Add(tag);
            return this;
        }

        /// <summary>
        /// Adds multiple custom tags for metrics collection.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpClientMetricsOptionsBuilder WithAdditionalTags(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                _options.AdditionalTags = [];
                return this;
            }

            _options.AdditionalTags = tags.ToList();
            return this;
        }

        /// <summary>
        /// Builds the configured <see cref="HttpClientMetricsOptions"/>.
        /// </summary>
        /// <returns>The built <see cref="HttpClientMetricsOptions"/>.</returns>
        internal HttpClientMetricsOptions Build() => _options;
    }
}
