// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Http;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Mango.Http.Metrics;
    using Options;

    /// <summary>
    /// Extension methods for configuring Mango HTTP client metrics in the dependency injection container.
    /// Use these methods to enable metrics collection and insert the metrics handler into the HTTP client pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithMetrics(cfg => cfg.Enable().WithAdditionalTag("env:prod"));
    /// </code>
    /// </example>
    public static class HttpClientMetricsConfigurationExtensions
    {
        /// <summary>
        /// Adds metrics collection to the specified HTTP client builder.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="configure">Optional delegate to configure metrics options.</param>
        /// <returns>The configured HTTP client builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the HTTP client is unnamed.</exception>
        public static IMangoHttpClientBuilder WithMetrics(
            this IMangoHttpClientBuilder builder,
            Action<HttpClientMetricsOptionsBuilder>? configure = null)
        {
            var name = builder.Name ?? throw new InvalidOperationException("HttpClient must be named");

            // Configure options
            var optsBuilder = new HttpClientMetricsOptionsBuilder();
            configure?.Invoke(optsBuilder);

            var options = optsBuilder.Build();

            // Register metrics options for the named client
            builder.Services
                .AddOptions<HttpClientMetricsOptions>(name)
                .Configure(o =>
                {
                    o.Enabled = options.Enabled;
                    o.AdditionalTags = options.AdditionalTags;
                });

            // Insert handler in a guaranteed position
            builder.Services.Configure<HttpClientFactoryOptions>(name, o =>
            {
                o.HttpMessageHandlerBuilderActions.Add(hb =>
                {
                    var sp = hb.Services;
                    var opts = sp.GetRequiredService<IOptionsMonitor<HttpClientMetricsOptions>>().Get(name);
                    if (!opts.Enabled) return;
                    var metrics = sp.GetService<IHttpClientMetricsProvider>() ?? new NoOpHttpClientMetricsProvider();
                    hb.AdditionalHandlers.InsertByOrder(new MetricsHandler(metrics, name, opts.AdditionalTags.ToArray()), MangoHttpHandlerOrder.Metrics);
                });
            });

            return builder;
        }
    }
}
