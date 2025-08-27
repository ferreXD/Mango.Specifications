// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Mango.Http;
    using Mango.Http.Logging;
    using System;
    using System.Net.Http;

    /// <summary>
    /// Extension methods for registering and configuring typed Mango HTTP clients in the dependency injection container.
    /// These methods wrap and delegate to <see cref="MangoHttpClientExtensions"/> to ensure consistent configuration.
    /// </summary>
    public static class MangoTypedHttpClientExtensions
    {
        /// <summary>
        /// Registers a typed Mango HTTP client and required dependencies using a custom implementation type.
        /// </summary>
        /// <typeparam name="TClient">The typed client interface/class (must have a constructor accepting HttpClient).</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the client.</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TImplementation"/> does not have a constructor accepting HttpClient.</exception>
        public static IMangoHttpClientBuilder AddMangoTypedHttpClient<TClient, TImplementation>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            if (typeof(TImplementation).GetConstructor(new[] { typeof(HttpClient) }) == null)
                throw new InvalidOperationException(
                    $"Type {typeof(TImplementation).Name} must have a single constructor accepting HttpClient.");

            clientConfig ??= _ => { };

            // Register the HttpClient and capture the builder
            services.AddHttpClient<TClient, TImplementation>(clientName, clientConfig);

            // Apply Mango policies and return the builder
            return services.AddMangoHttpClientBuilder(clientName);
        }

        /// <summary>
        /// Registers a typed Mango HTTP client and required dependencies.
        /// </summary>
        /// <typeparam name="TClient">The typed client class (must have a constructor accepting HttpClient).</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TClient"/> does not have a constructor accepting HttpClient.</exception>
        public static IMangoHttpClientBuilder AddMangoTypedHttpClient<TClient>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            if (typeof(TClient).GetConstructor(new[] { typeof(HttpClient) }) == null)
                throw new InvalidOperationException(
                    $"Type {typeof(TClient).Name} must have a single constructor accepting HttpClient.");

            clientConfig ??= _ => { };

            // Register the HttpClient and capture the builder
            services.AddHttpClient<TClient>(clientName, clientConfig);

            // Apply Mango policies and return the builder
            return services.AddMangoHttpClientBuilder(clientName);
        }

        /// <summary>
        /// Registers a typed Mango HTTP client and required dependencies for OpenTelemetry tracing using a custom implementation type.
        /// </summary>
        /// <typeparam name="TClient">The typed client interface/class.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the client.</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TClient"/> does not have a constructor accepting HttpClient.</exception>
        public static IMangoHttpClientBuilder AddMangoTypedHttpClientWithOpenTelemetry<TClient, TImplementation>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            if (typeof(TImplementation).GetConstructor(new[] { typeof(HttpClient) }) == null)
                throw new InvalidOperationException(
                    $"Type {typeof(TImplementation).Name} must have a single constructor accepting HttpClient.");

            clientConfig ??= _ => { };
            services.AddHttpClient<TClient, TImplementation>(clientName, clientConfig);
            return services.AddMangoHttpClientBuilderWithOpenTelemetry(clientName);
        }

        /// <summary>
        /// Registers a typed Mango HTTP client and required dependencies for OpenTelemetry tracing.
        /// </summary>
        /// <typeparam name="TClient">The typed client class (must have a constructor accepting HttpClient).</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="clientName"/> is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TClient"/> does not have a constructor accepting HttpClient.</exception>
        public static IMangoHttpClientBuilder AddMangoTypedHttpClientWithOpenTelemetry<TClient>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client must be named.", nameof(clientName));

            if (typeof(TClient).GetConstructor(new[] { typeof(HttpClient) }) == null)
                throw new InvalidOperationException(
                    $"Type {typeof(TClient).Name} must have a single constructor accepting HttpClient.");

            clientConfig ??= _ => { };
            services.AddHttpClient<TClient>(clientName, clientConfig);
            return services.AddMangoHttpClientBuilderWithOpenTelemetry(clientName);
        }

        /// <summary>
        /// Registers a typed Mango HTTP client with default logger (<see cref="DefaultHttpLogger"/>) and default policies.
        /// </summary>
        /// <typeparam name="TClient">The typed client class (must have a constructor accepting HttpClient).</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        public static IMangoHttpClientBuilder AddDefaultMangoTypedHttpClient<TClient>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
        {
            return services
                .AddMangoDefaultHttpLogger()
                .AddMangoTypedHttpClient<TClient>(clientName, clientConfig)
                .WithDefaults<DefaultHttpLogger>();
        }

        /// <summary>
        /// Registers a typed Mango HTTP client with default logger (<see cref="DefaultHttpLogger"/>) and OpenTelemetry.
        /// </summary>
        /// <typeparam name="TClient">The typed client class (must have a constructor accepting HttpClient).</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        public static IMangoHttpClientBuilder AddDefaultMangoTypedHttpClientWithOpenTelemetry<TClient>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
        {
            return services
                .AddMangoTypedHttpClientWithOpenTelemetry<TClient>(clientName, clientConfig)
                .WithDefaults<OpenTelemetryHttpLogger>()
                .WithHeaders(cfg => cfg.WithCorrelationIdHeader())
                .WithMetrics(cfg => cfg.Enable());
        }

        /// <summary>
        /// Registers a typed Mango HTTP client with default policies and a custom logger implementation.
        /// </summary>
        /// <typeparam name="TClient">The typed client class (must have a constructor accepting HttpClient).</typeparam>
        /// <typeparam name="TLogger">The custom logger type implementing <see cref="IMangoHttpLogger"/>.</typeparam>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="clientConfig">Optional configuration action for the <see cref="HttpClient"/>.</param>
        /// <returns>An <see cref="IMangoHttpClientBuilder"/> for further Mango configuration.</returns>
        public static IMangoHttpClientBuilder AddDefaultMangoTypedHttpClient<TClient, TLogger>(
            this IServiceCollection services,
            string clientName,
            Action<HttpClient>? clientConfig = null)
            where TClient : class
            where TLogger : class, IMangoHttpLogger
        {
            return services
                .AddMangoTypedHttpClient<TClient>(clientName, clientConfig)
                .WithDefaults<TLogger>();
        }
    }
}