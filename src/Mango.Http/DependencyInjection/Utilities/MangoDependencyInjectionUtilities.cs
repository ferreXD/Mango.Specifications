// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Logging;
    using Mango.Http.Authorization;
    using Mango.Http.Constants;
    using Mango.Http.Diagnostics;
    using Mango.Http.Logging;
    using Mango.Http.Metrics;
    using Mango.Http.Presets;
    using Options;
    using System.Diagnostics;

    /// <summary>
    /// Provides utility extension methods for registering Mango HTTP OpenTelemetry defaults in the dependency injection container.
    /// </summary>
    public static class MangoDependencyInjectionUtilities
    {
        /// <summary>
        /// Registers the default Mango HTTP logger into the service collection.
        /// </summary>
        /// <param name="services">The service collection to register dependencies into.</param>
        /// <param name="name">The name of the logger, used for identifying the client in logs.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddMangoDefaultHttpLogger(this IServiceCollection services, string name = LoggingDefaults.DefaultClientName)
        {
            // Register the default logger
            services.TryAddSingleton<IMangoHttpLogger>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultHttpLogger>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>();
                return new DefaultHttpLogger(logger, optionsMonitor, name);
            });
            return services;
        }

        /// <summary>
        /// Registers Mango HTTP OpenTelemetry tracing, logging, and metrics defaults into the service collection.
        /// </summary>
        /// <param name="services">The service collection to register dependencies into.</param>
        /// <param name="name">The name of the logger, used for identifying the client in logs.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddMangoOpenTelemetryDefaults(this IServiceCollection services, string name = LoggingDefaults.DefaultClientName)
        {
            // Ensure tracing is set up by adding a fallback ActivitySource if none exists
            services.AddMangoHttpTracing();

            // Register the OpenTelemetry logger
            services.TryAddSingleton<IMangoHttpLogger>(sp =>
            {
                var activitySource = sp.GetRequiredService<ActivitySource>();
                var logger = sp.GetRequiredService<ILogger<OpenTelemetryHttpLogger>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpLoggingOptions>>();
                return new OpenTelemetryHttpLogger(activitySource, logger, optionsMonitor, name);
            });

            // Register the OpenTelemetry metrics
            services.TryAddSingleton<IHttpClientMetricsProvider, OpenTelemetryHttpClientMetricsProvider>();

            return services;
        }

        /// <summary>
        /// Registers the default Mango HTTP dependencies, including resiliency policies, authentication strategies, and diagnostics.
        /// </summary>
        /// <param name="services">The service collection to register dependencies into.</param>
        /// <returns>The updated service collection.</returns>
        internal static IServiceCollection AddMangoHttpDefaultDependencies(this IServiceCollection services)
        {
            // 2) Make sure all preset registries exist
            services.TryAddSingleton<IResiliencyPolicyPresetRegistry, DefaultResiliencyPolicyPresetRegistry>();
            services.TryAddSingleton<IAuthenticationStrategyPresetRegistry, DefaultAuthenticationStrategyPresetRegistry>();

            // 3) Register the diagnostics handler
            services.AddSingleton<IResiliencyDiagnostics, DefaultResiliencyDiagnostics>();

            // 4) Register default dependencies
            services.TryAddSingleton<IHttpClientMetricsProvider, NoOpHttpClientMetricsProvider>();

            return services;
        }
    }
}
