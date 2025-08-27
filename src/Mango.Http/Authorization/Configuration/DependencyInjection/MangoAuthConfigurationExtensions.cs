// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Http;
    using Mango.Http;
    using Mango.Http.Authorization;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Options;
    using System;

    /// <summary>
    /// Extension methods for configuring Mango HTTP authentication strategies and presets in the dependency injection container.
    /// </summary>
    public static class MangoAuthConfigurationExtensions
    {
        /// <summary>
        /// Registers authentication strategy presets using the provided configurator.
        /// </summary>
        /// <param name="services">The service collection to register the presets into.</param>
        /// <param name="configure">An action to configure authentication strategy presets.</param>
        /// <returns>The updated service collection.</returns>
        /// <example>
        /// <code>
        /// services.AddAuthPresets(cfg =>
        ///     cfg.WithPreset("Bearer", builder => builder.UseBearerToken("token")));
        /// </code>
        /// </example>
        public static IServiceCollection AddAuthPresets(
            this IServiceCollection services,
            Action<AuthenticationPresetConfigurator> configure)
        {
            var cfg = new AuthenticationPresetConfigurator();
            configure(cfg);
            var presets = cfg.Build();
            services.TryAddSingleton<IAuthenticationStrategyPresetRegistry>(new DefaultAuthenticationStrategyPresetRegistry(presets));
            return services;
        }

        /// <summary>
        /// Configures authentication for a Mango HTTP client using the provided configurator.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <param name="configure">An action to configure authentication options for the client.</param>
        /// <returns>The Mango HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the HTTP client is unnamed.</exception>
        /// <remarks>
        /// This method registers named authentication options and inserts the authentication handler at the correct pipeline position.
        /// </remarks>
        /// <example>
        /// <code>
        /// builder.WithAuthentication(cfg =>
        ///     cfg.Enable()
        ///        .UseBearerToken("token")
        ///        .When(request => request.RequestUri.Host == "api.example.com"));
        /// </code>
        /// </example>
        public static IMangoHttpClientBuilder WithAuthentication(
            this IMangoHttpClientBuilder builder,
            Action<HttpAuthConfigurator> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var clientName = builder.Name ?? throw new InvalidOperationException("HttpClient must be named to configure authentication");

            // 1) Build and register named options exactly once
            var cfg = new HttpAuthConfigurator();
            configure(cfg);
            builder.Services
                .AddOptions<HttpAuthOptions>(clientName)
                .Configure((HttpAuthOptions opts, IServiceProvider sp) =>
                {
                    var registry = sp.GetRequiredService<IAuthenticationStrategyPresetRegistry>();
                    var authOpts = cfg.Build(registry);

                    opts.Enabled = authOpts.Enabled;
                    opts.Condition = authOpts.Condition;
                    opts.PresetKeys = authOpts.PresetKeys;
                    opts.StrategyFactory = authOpts.StrategyFactory;
                })
                .Validate(o => !o.Enabled || o.StrategyFactory != null, "Auth enabled but no StrategyFactory configured.")
                .ValidateOnStart();

            // 2) Insert handler at a known index
            builder.Services.Configure<HttpClientFactoryOptions>(clientName, fo =>
            {
                fo.HttpMessageHandlerBuilderActions.Add(hb =>
                {
                    var opts = hb.Services
                        .GetRequiredService<IOptionsMonitor<HttpAuthOptions>>()
                        .Get(clientName);

                    if (!opts.Enabled) return;

                    var strategy = opts.StrategyFactory!(hb.Services);
                    hb.AdditionalHandlers.InsertByOrder(
                        new HttpAuthenticationHandler(opts, strategy),
                        MangoHttpHandlerOrder.Authentication);
                });
            });

            return builder;
        }
    }
}
