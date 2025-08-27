// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Http;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Mango.Http.Headers;
    using Options;

    /// <summary>
    /// Extension methods for configuring custom HTTP headers in Mango HTTP clients.
    /// Use these methods to add custom headers, correlation/request IDs, and inject header options into the HTTP client pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithHeaders(cfg =>
    ///     cfg.WithCustomHeader("X-Api-Key", "my-key")
    ///        .WithCorrelationIdHeader()
    ///        .WithRequestIdHeader());
    /// </code>
    /// </example>
    public static class MangoHeadersConfigurationExtensions
    {
        /// <summary>
        /// Adds custom headers to the specified HTTP client builder.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="configure">Delegate to configure the custom header options.</param>
        /// <returns>The configured HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if builder or configure is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the builder name is null or no custom headers are configured.</exception>
        public static IMangoHttpClientBuilder WithHeaders(
            this IMangoHttpClientBuilder builder,
            Action<HttpHeadersOptionsConfigurator> configure)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));

            var name = builder.Name ?? throw new InvalidOperationException("Builder name cannot be null. Ensure the builder is properly initialized before adding headers.");

            var configurator = new HttpHeadersOptionsConfigurator();
            configure(configurator);

            var options = configurator.Build();

            builder.Services
                .AddOptions<HttpHeadersOptions>(name)
                .Configure(opts =>
                {
                    opts.CustomHeaders = options.CustomHeaders;
                })
                .Validate(o =>
                {
                    if (o.CustomHeaders.Count == 0) throw new InvalidOperationException("At least one custom header must be configured.");

                    return true;
                });

            // Register the headers handler
            builder.Services.Configure<HttpClientFactoryOptions>(name, o =>
            {
                o.HttpMessageHandlerBuilderActions.Add(hb =>
                {
                    var sp = hb.Services;
                    var opts = sp.GetRequiredService<IOptionsMonitor<HttpHeadersOptions>>().Get(name);
                    hb.AdditionalHandlers.InsertByOrder(new HttpHeadersInjectionHandler(opts), MangoHttpHandlerOrder.Headers);
                });
            });

            return builder;
        }
    }
}
