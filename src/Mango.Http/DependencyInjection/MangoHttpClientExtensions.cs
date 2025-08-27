// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Mango.Http;
    using Mango.Http.Defaults;
    using Mango.Http.Logging;
    using Mango.Http.Metrics;

    // TODO: Add unit test regarding defaults registration and later overriding of defaults. See how it behaves and if it needs to be adjusted.
    // TODO: Gotta change naming of WithDefault... method to something more descriptive, like UseMin(), UseBalanced(), etc.
    /// <summary>
    /// Provides extension methods for registering and configuring Mango HTTP clients in the dependency injection container.
    /// </summary>
    public static class MangoHttpClientExtensions
    {
        /// <summary>
        /// Registers a named Mango HTTP client and required dependencies.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        public static IMangoHttpClientBuilder AddMangoHttpClient(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            // 1) Register the named HttpClient
            services.AddHttpClient(clientName, client => { clientConfig?.Invoke(client); });

            return services.AddMangoHttpClientBuilder(clientName);
        }

        /// <summary>
        /// Registers a named Mango HTTP client and required dependencies for OpenTelemetry tracing in the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        public static IMangoHttpClientBuilder AddMangoHttpClientWithOpenTelemetry(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
        {
            // 1) Register OpenTelemetry dependencies
            services.AddMangoOpenTelemetryDefaults(clientName);

            var builder = services
                .AddMangoHttpClient(clientName, clientConfig)
                .WithTracingHandler();

            return builder;
        }

        /// <summary>
        /// Registers a Mango HTTP client with default configuration and logging using <see cref="DefaultHttpLogger"/>.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDefaultMangoHttpClient(this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            services
                .AddMangoDefaultHttpLogger()
                .AddDefaultMangoHttpClient<DefaultHttpLogger>(clientName, clientConfig);

            return services;
        }

        /// <summary>
        /// Registers a Mango HTTP client with default configuration and OpenTelemetry logging.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDefaultMangoHttpClientWithOpenTelemetry(this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
        {
            services
                .AddMangoHttpClientWithOpenTelemetry(clientName, clientConfig)
                .WithDefaults<OpenTelemetryHttpLogger>()
                .WithHeaders(cfg => cfg.WithCorrelationIdHeader())
                .WithMetrics(cfg => cfg.Enable());

            return services;
        }

        /// <summary>
        /// Registers a Mango HTTP client with default configuration and a custom logger implementation.
        /// </summary>
        /// <typeparam name="TLogger">The type of logger to use, implementing <see cref="IMangoHttpLogger"/>.</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDefaultMangoHttpClient<TLogger>(this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null) where TLogger : class, IMangoHttpLogger
        {
            services
                .AddMangoHttpClient(clientName, clientConfig)
                .WithDefaults<TLogger>();

            return services;
        }

        /// <summary>
        /// Configures the <see cref="IMangoHttpClientBuilder"/> with default settings using <see cref="DefaultHttpLogger"/>.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <returns>The configured <see cref="IMangoHttpClientBuilder"/>.</returns>
        public static IMangoHttpClientBuilder WithDefaults(
            this IMangoHttpClientBuilder builder) =>
            builder.WithDefaults<DefaultHttpLogger>();

        /// <summary>
        /// Configures the <see cref="IMangoHttpClientBuilder"/> with default settings for OpenTelemetry logging.
        /// An ActivitySource must be registered in the application to use this configuration.
        /// Must ensure that the OpenTelemetry SDK is configured in the application and the <see cref="IHttpClientMetricsProvider"/> is registered with an OpenTelemetry implementation (i.e., <see cref="OpenTelemetryHttpClientMetricsProvider"/>) as metrics are enabled by default.
        /// To register default dependencies, use <see cref="MangoDependencyInjectionUtilities.AddMangoOpenTelemetryDefaults"/>.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <returns>The configured <see cref="IMangoHttpClientBuilder"/>.</returns>
        public static IMangoHttpClientBuilder WithOpenTelemetryDefaults(
            this IMangoHttpClientBuilder builder) =>
            builder
                .WithDefaults<OpenTelemetryHttpLogger>()
                .WithHeaders(cfg => cfg.WithCorrelationIdHeader())
                .WithMetrics(cfg => cfg.Enable());

        /// <summary>
        /// Configures the <see cref="IMangoHttpClientBuilder"/> with default settings using the specified logger type.
        /// </summary>
        /// <typeparam name="TLogger">The type of logger to use, implementing <see cref="IMangoHttpLogger"/>.</typeparam>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <returns>The configured <see cref="IMangoHttpClientBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
        public static IMangoHttpClientBuilder WithDefaults<TLogger>(
            this IMangoHttpClientBuilder builder) where TLogger : class, IMangoHttpLogger
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder
                .WithResiliency(b => b.WithDefaultResiliency())
                .WithDiagnostics()
                .WithLogging(b => b.WithDefaultLogging<TLogger>());
        }

        internal static IMangoHttpClientBuilder AddMangoHttpClientBuilder(this IServiceCollection services, string clientName)
        {
            services.AddMangoHttpDefaultDependencies();

            // 5) Register your IMangoHttpClientBuilder
            var builder = new MangoHttpClientBuilder(clientName, services);

            return builder;
        }

        internal static IMangoHttpClientBuilder AddMangoHttpClientBuilderWithOpenTelemetry(this IServiceCollection services, string clientName)
        {
            services
                .AddMangoOpenTelemetryDefaults(clientName)
                .AddMangoHttpDefaultDependencies();

            // 5) Register your IMangoHttpClientBuilder
            var builder = new MangoHttpClientBuilder(clientName, services);

            return builder;
        }
    }
}