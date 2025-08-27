// ReSharper disable once CheckNamespace
namespace Mango.Http.Defaults
{
    using Constants;
    using Logging;
    using Resiliency;

    /// <summary>
    /// Extension methods for applying default resiliency and logging configurations to Mango HTTP clients.
    /// </summary>
    public static class MangoHttpDefaultExtensions
    {
        /// <summary>
        /// Applies the default resiliency policy configuration to the specified configurator.
        /// </summary>
        /// <param name="configurator">The Mango resiliency policy configurator.</param>
        /// <param name="action">An optional action to further customize the configurator.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <example>
        /// <code>
        /// configurator.WithDefaultResiliency(cfg => cfg.WithTimeout());
        /// </code>
        /// </example>
        public static MangoResiliencyPolicyConfigurator WithDefaultResiliency(
            this MangoResiliencyPolicyConfigurator configurator,
            Action<MangoResiliencyPolicyConfigurator>? action = null)
        {
            if (configurator is null) throw new ArgumentNullException(nameof(configurator));

            ResiliencyPolicyDefaults.DefaultConfiguration.Invoke(configurator);
            action?.Invoke(configurator);

            return configurator;
        }

        /// <summary>
        /// Applies the default logging configuration and specified logger type to the logging configurator.
        /// </summary>
        /// <typeparam name="TLogger">The logger type to use.</typeparam>
        /// <param name="configurator">The HTTP logging configurator.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <example>
        /// <code>
        /// configurator.WithDefaultLogging<DefaultHttpLogger>();
        /// </code>
        /// </example>
        public static HttpLoggingConfigurator WithDefaultLogging<TLogger>(
            this HttpLoggingConfigurator configurator) where TLogger : IMangoHttpLogger
        {
            if (configurator is null) throw new ArgumentNullException(nameof(configurator));

            LoggingDefaults.DefaultConfiguration.Invoke(configurator);
            configurator.UseLogger<TLogger>();

            return configurator;
        }

        /// <summary>
        /// Applies the default logging configuration using the default Mango HTTP logger.
        /// </summary>
        /// <param name="builder">The HTTP logging configurator.</param>
        /// <returns>The configurator for chaining.</returns>
        public static HttpLoggingConfigurator WithDefaultLogging(
            this HttpLoggingConfigurator builder) =>
            builder.WithDefaultLogging<DefaultHttpLogger>();

        /// <summary>
        /// Applies the default logging configuration using the OpenTelemetry HTTP logger.
        /// </summary>
        /// <param name="builder">The HTTP logging configurator.</param>
        /// <returns>The configurator for chaining.</returns>
        public static HttpLoggingConfigurator WithDefaultOpenTelemetry(
            this HttpLoggingConfigurator builder) =>
            builder.WithDefaultLogging<OpenTelemetryHttpLogger>();
    }
}