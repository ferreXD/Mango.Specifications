// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Mango.Http;
    using Mango.Http.Logging;
    using System;

    public static class MangoHttpClientPresetsExtensions
    {
        /// <summary>
        /// Registers a Mango HTTP client with minimal features.
        /// This preset is suitable for high-performance scenarios with minimal overhead.
        /// It includes basic logging and diagnostics without additional resiliency policies.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="logBodies">Log request and response bodies. This is useful for debugging but may expose sensitive information, so use with caution.</param>
        /// <returns>The updated <see cref="IMangoHttpClientBuilder"/>.</returns>
        public static IMangoHttpClientBuilder UseMinClient(this IMangoHttpClientBuilder builder, bool logBodies = false)
        {
            builder
                .WithLogging(cfg => cfg
                    .Enable()
                    .UseLogger<DefaultHttpLogger>()
                    .LogRequestBody(logBodies)
                    .LogResponseBody(logBodies))
                .WithDiagnostics();

            return builder;
        }

        /// <summary>
        /// Registers a Mango HTTP client with minimal default resiliency policies.
        /// This preset is suitable for scenarios where basic resiliency is required without extensive overhead.
        /// It includes retry, bulkhead, timeout, and circuit breaker policies with conservative settings + the minimal logging and diagnostics.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder to configure.</param>
        /// <param name="logBodies">Log request and response bodies. This is useful for debugging but may expose sensitive information, so use with caution.</param>
        /// <returns>The updated <see cref="IMangoHttpClientBuilder"/>.</returns>
        public static IMangoHttpClientBuilder UseBalancedClient(this IMangoHttpClientBuilder builder, bool logBodies = false)
        {
            builder
                .WithResiliency(cfg => cfg
                    .WithRetry(policy => policy
                        .SetMaxRetryCount(10)
                        .SetDelay(TimeSpan.FromSeconds(2))
                        .SetUseJitter())
                    .WithBulkhead(policy => policy
                        .SetMaxParallelization(10_000)
                        .SetMaxQueueLength(0))
                    .WithTimeout(policy => policy
                        .SetTimeout(TimeSpan.FromSeconds(20)))
                    .WithCircuitBreaker(policy => policy
                        .SetFailureThreshold(5)
                        .SetBreakDuration(TimeSpan.FromSeconds(30))))
                .WithLogging(cfg => cfg
                    .Enable()
                    .UseLogger<DefaultHttpLogger>()
                    .LogRequestBody(logBodies)
                    .LogResponseBody(logBodies))
                .WithDiagnostics();

            return builder;
        }

        /// <summary>
        /// Registers a Mango HTTP client with OpenTelemetry support.
        /// This preset is suitable for applications that require distributed tracing and metrics collection.
        /// It includes default resiliency policies, logging, and diagnostics.
        /// Note: An ActivitySource must be registered in the application to use this configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logBodies">Log request and response bodies. This is useful for debugging but may expose sensitive information, so use with caution.</param>
        /// <returns></returns>
        public static IMangoHttpClientBuilder UseOpenTelemetryClient(this IMangoHttpClientBuilder builder, bool logBodies = false)
        {
            builder
                .WithResiliency(cfg => cfg
                    .WithRetry(policy => policy
                        .SetMaxRetryCount(10)
                        .SetDelay(TimeSpan.FromSeconds(2))
                        .SetUseJitter())
                    .WithBulkhead(policy => policy
                        .SetMaxParallelization(10_000)
                        .SetMaxQueueLength(0))
                    .WithTimeout(policy => policy
                        .SetTimeout(TimeSpan.FromSeconds(20)))
                    .WithCircuitBreaker(policy => policy
                        .SetFailureThreshold(5)
                        .SetBreakDuration(TimeSpan.FromSeconds(30))))
                .WithLogging(cfg => cfg
                    .Enable()
                    .UseLogger<OpenTelemetryHttpLogger>()
                    .LogRequestBody(logBodies)
                    .LogResponseBody(logBodies))
                .WithHeaders(cfg => cfg
                    .WithCorrelationIdHeader())
                .WithMetrics(cfg => cfg
                    .Enable())
                .WithDiagnostics();

            return builder;
        }
    }
}
