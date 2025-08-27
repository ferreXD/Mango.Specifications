// ReSharper disable once CheckNamespace
namespace Mango.Http.Metrics
{
    /// <summary>
    /// Options for configuring metrics collection for Mango HTTP clients.
    /// Use this class to enable/disable metrics and specify custom tags for metrics events.
    /// </summary>
    public class HttpClientMetricsOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether metrics collection is enabled for the HTTP client.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of additional custom tags to associate with metrics events.
        /// </summary>
        public List<string> AdditionalTags { get; set; } = [];
    }
}
