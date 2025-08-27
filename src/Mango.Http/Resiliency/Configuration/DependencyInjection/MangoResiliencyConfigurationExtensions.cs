// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Http;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Common.Helpers;
    using Mango.Http.Diagnostics;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;
    using System;

    /// <summary>
    /// Extension methods for configuring Mango HTTP client resiliency policies in the dependency injection container.
    /// Use these methods to add, configure, and validate resiliency policies for HTTP clients.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddHttpClient("myClient")
    ///     .WithResiliency(cfg =>
    ///         cfg.WithRetry(r => r.SetMaxRetryCount(3).SetDelay(TimeSpan.FromSeconds(2)))
    ///            .WithTimeout(t => t.SetTimeout(TimeSpan.FromSeconds(30)))
    ///            .WithCircuitBreaker(cb => cb.SetFailureThreshold(5).SetBreakDuration(TimeSpan.FromSeconds(30))));
    /// </code>
    /// </example>
    public static class MangoResiliencyConfigurationExtensions
    {
        /// <summary>
        /// Adds resiliency policies to the specified HTTP client builder.
        /// </summary>
        /// <param name="clientBuilder">The HTTP client builder to configure.</param>
        /// <param name="configure">Delegate to configure the MangoResiliencyPolicyConfigurator.</param>
        /// <param name="requireAtLeastOne">Indicates if at least one policy is required (throws if none are configured).</param>
        /// <returns>The configured HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if clientBuilder or configure is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if requireAtLeastOne is true and no policies are configured.</exception>
        public static IMangoHttpClientBuilder WithResiliency(
            this IMangoHttpClientBuilder clientBuilder,
            Action<MangoResiliencyPolicyConfigurator> configure,
            bool requireAtLeastOne = false)
        {
            Validate(clientBuilder, configure);
            var configuratorFactory = CreateConfiguratorFactory(configure);
            RegisterResiliencyOptions(clientBuilder, configuratorFactory, requireAtLeastOne);
            RegisterResiliencyHandler(clientBuilder, configuratorFactory, requireAtLeastOne);
            return clientBuilder;
        }

        /// <summary>
        /// Validates input arguments for the WithResiliency method.
        /// </summary>
        /// <param name="clientBuilder">The HTTP client builder.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <exception cref="ArgumentNullException">Thrown if clientBuilder or configure is null.</exception>
        private static void Validate(IMangoHttpClientBuilder clientBuilder, Action<MangoResiliencyPolicyConfigurator> configure)
        {
            if (clientBuilder == null)
                throw new ArgumentNullException(nameof(clientBuilder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
        }

        /// <summary>
        /// Creates a factory for MangoResiliencyPolicyConfigurator, applying presets and user configuration.
        /// </summary>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>A factory that builds <see cref="ResiliencyOptions"/> from the service provider.</returns>
        private static Func<IServiceProvider, ResiliencyOptions> CreateConfiguratorFactory(Action<MangoResiliencyPolicyConfigurator> configure)
        {
            return sp =>
            {
                var configurator = new MangoResiliencyPolicyConfigurator();
                var registry = sp.GetRequiredService<IResiliencyPolicyPresetRegistry>();

                // Apply the user configuration
                configure(configurator);

                return MangoResiliencyPolicyBuilder.Build(configurator, registry);
            };
        }

        /// <summary>
        /// Registers the resiliency options for the HTTP client, including validation.
        /// </summary>
        /// <param name="clientBuilder">The HTTP client builder.</param>
        /// <param name="configuratorFactory">The factory to build resiliency options.</param>
        /// <param name="requireAtLeastOne">Indicates if at least one policy is required.</param>
        private static void RegisterResiliencyOptions(
            IMangoHttpClientBuilder clientBuilder,
            Func<IServiceProvider, ResiliencyOptions> configuratorFactory,
            bool requireAtLeastOne = false)
        {
            var clientName = clientBuilder.Name ?? "DefaultMangoHttpClient";

            // Use the Configure overload that gives you the real IServiceProvider
            clientBuilder.Services
                .AddOptions<ResiliencyOptions>(clientName)
                .Configure((ResiliencyOptions options, IServiceProvider sp) =>
                {
                    // At runtime, sp is your application's root IServiceProvider—
                    // no BuildServiceProvider hacks needed
                    var configured = configuratorFactory(sp);

                    // Copy over the built policies
                    options = new ResiliencyOptions(configured.Policies);
                })
                // Keep your validation logic
                .Validate(o => !requireAtLeastOne || (o.Policies?.Count > 0), "At least one resiliency policy must be configured.")
                .ValidateOnStart();
        }

        /// <summary>
        /// Registers the MangoPolicyHandler with the HTTP client builder, using the configured policies.
        /// </summary>
        /// <param name="clientBuilder">The HTTP client builder.</param>
        /// <param name="configuratorFactory">The factory to build resiliency options.</param>
        /// <param name="requireAtLeastOne">Indicates if at least one policy is required.</param>
        private static void RegisterResiliencyHandler(
            IMangoHttpClientBuilder clientBuilder,
            Func<IServiceProvider, ResiliencyOptions> configuratorFactory,
            bool requireAtLeastOne = false)
        {
            var clientName = clientBuilder.Name ?? "DefaultMangoHttpClient";
            clientBuilder.Services.Configure<HttpClientFactoryOptions>(clientName, o =>
            {
                // Insert the MangoPolicyHandler as the first additional handler
                o.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    var diagnostics = builder.Services.GetService<IResiliencyDiagnostics>();
                    var options = configuratorFactory(builder.Services);

                    if (requireAtLeastOne && !options.Policies.Any())
                        throw new InvalidOperationException(
                            $"Resiliency requires at least one policy for client '{clientName}'.");

                    builder.AdditionalHandlers.InsertByOrder(new MangoPolicyHandler(options.Policies, diagnostics), MangoHttpHandlerOrder.Resiliency);
                });
            });
        }
    }
}
