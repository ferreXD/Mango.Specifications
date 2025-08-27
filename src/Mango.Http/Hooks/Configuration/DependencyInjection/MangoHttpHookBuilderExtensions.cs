// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Http;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Mango.Http.Hooks;
    using Options;
    using System;

    /// <summary>
    /// Extension methods for configuring HTTP request and response hooks in Mango HTTP clients.
    /// Use these methods to add custom asynchronous actions before requests and after responses.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithHooks(cfg =>
    ///     cfg.PreAsync((req, ctx, ct) => LogPreRequest(req))
    ///        .PostAsync((resp, ctx, ct) => LogPostResponse(resp)));
    /// </code>
    /// </example>
    public static class MangoHttpHookBuilderExtensions
    {
        /// <summary>
        /// Adds HTTP request and response hooks to the specified HTTP client builder.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="configure">Delegate to configure the request/response hook policy.</param>
        /// <returns>The configured HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if configure is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the builder name is null.</exception>
        public static IMangoHttpClientBuilder WithHooks(
            this IMangoHttpClientBuilder builder,
            Action<HttpRequestHookPolicyConfigurator> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);
            var name = builder.Name ?? throw new InvalidOperationException("Builder name cannot be null.");

            var configurator = new HttpRequestHookPolicyConfigurator();
            configure(configurator);

            var options = configurator.Build();

            builder.Services.Configure<HttpRequestHookOptions>(name, opts =>
            {
                opts.PreRequestAsync = options.PreRequestAsync;
                opts.PostResponseAsync = options.PostResponseAsync;
            });

            // Insert handler in a guaranteed position
            builder.Services.Configure<HttpClientFactoryOptions>(name, o =>
            {
                o.HttpMessageHandlerBuilderActions.Add(hb =>
                {
                    var sp = hb.Services;
                    var opts = sp.GetRequiredService<IOptionsMonitor<HttpRequestHookOptions>>().Get(name);

                    var handler = new MangoHttpHookHandler(opts);
                    hb.AdditionalHandlers.InsertByOrder(handler, MangoHttpHandlerOrder.Hooks);
                });
            });

            return builder;
        }
    }
}
