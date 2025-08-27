// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Hosting;
    using Http;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Mango.Http.Logging;
    using Options;

    /// <summary>
    /// Extension methods for configuring HTTP logging in Mango HTTP clients.
    /// Use these methods to enable logging, set log levels, body capture, excluded headers, and custom logger/handler types.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithLogging(cfg =>
    ///     cfg.Enable()
    ///        .RequestLevel(LogLevel.Information)
    ///        .ResponseLevel(LogLevel.Information)
    ///        .ErrorLevel(LogLevel.Error)
    ///        .LogRequestBody()
    ///        .LogResponseBody()
    ///        .MaxBodyLength(2048)
    ///        .ExcludeHeader("Authorization")
    ///        .UseLogger<DefaultHttpLogger>());
    /// </code>
    /// </example>
    public static class MangoLoggingConfigurationExtensions
    {
        /// <summary>
        /// Adds HTTP logging to the specified Mango HTTP client builder.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="configure">Delegate to configure the logging options.</param>
        /// <param name="env">Optional host environment for development-specific logging.</param>
        /// <returns>The configured HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if configure is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the client name is null or logging options are invalid.</exception>
        public static IMangoHttpClientBuilder WithLogging(
            this IMangoHttpClientBuilder builder,
            Action<HttpLoggingConfigurator> configure,
            IHostEnvironment? env = null)
        {
            ArgumentNullException.ThrowIfNull(configure);
            builder.Services.AddMangoHttpTracing();

            var clientName = builder.Name ?? throw new InvalidOperationException("MangoHttpClient must have a name to enable logging.");

            var configurator = new HttpLoggingConfigurator();
            configure(configurator);

            if (env?.IsDevelopment() ?? false) configurator.Enable();

            // Register Logger or Custom Handler
            var opts = configurator.Build();

            // Add named options for logging
            builder.Services.AddOptions<HttpLoggingOptions>(clientName)
                .Configure(o =>
                {
                    o.Enabled = opts.Enabled;
                    o.Condition = opts.Condition;
                    o.RequestLevel = opts.RequestLevel;
                    o.ResponseSuccessLevel = opts.ResponseSuccessLevel;
                    o.ErrorLevel = opts.ErrorLevel;
                    o.LogRequestBody = opts.LogRequestBody;
                    o.LogResponseBody = opts.LogResponseBody;
                    o.MaxBodyLength = opts.MaxBodyLength;
                    o.ExcludedHeaders = opts.ExcludedHeaders.ToHashSet();
                    o.ActivityEventPrefix = opts.ActivityEventPrefix;
                    o.LoggerType = opts.LoggerType;
                    o.UseCustomHandler = opts.UseCustomHandler;
                    o.CustomHandlerType = opts.CustomHandlerType;
                })
                .PostConfigure(o => { o.Condition ??= _ => true; })
                .Validate(o => o.Enabled && (o.LoggerType != null || o.UseCustomHandler && o.CustomHandlerType != null),
                    "HttpLoggingOptions must have either LoggerType or CustomHandlerType set when enabled.")
                .ValidateOnStart();

            // Register the logger if existent
            if (opts is { UseCustomHandler: true, CustomHandlerType: not null }) builder.Services.AddTransient(opts.CustomHandlerType);
            else if (opts.LoggerType != null) builder.Services.TryAddSingleton(typeof(IMangoHttpLogger), opts.LoggerType!);
            else builder.Services.TryAddSingleton<IMangoHttpLogger, DefaultHttpLogger>();

            // Insert handler in a guaranteed position
            builder.Services.Configure<HttpClientFactoryOptions>(clientName, o =>
                {
                    o.HttpMessageHandlerBuilderActions.Add(hb =>
                    {
                        var sp = hb.Services;
                        var opts = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>().Get(clientName);
                        if (!opts.Enabled) return;

                        if (opts is { UseCustomHandler: true, CustomHandlerType: not null })
                        {
                            var customHandler = hb.Services.GetRequiredService(opts.CustomHandlerType);
                            hb.AdditionalHandlers.InsertByOrder((DelegatingHandler)customHandler, MangoHttpHandlerOrder.Logging);
                            return;
                        }

                        if (opts.LoggerType == null) throw new InvalidOperationException("HttpLoggingOptions must have LoggerType set when enabled.");

                        var logger = hb.Services.GetRequiredService<IMangoHttpLogger>();
                        hb.AdditionalHandlers.InsertByOrder(new HttpLoggingHandler(logger), MangoHttpHandlerOrder.Logging);
                    });
                });

            return builder;
        }
    }
}
