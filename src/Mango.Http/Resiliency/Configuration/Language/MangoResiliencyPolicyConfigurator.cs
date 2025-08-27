// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Fluent configurator for building <see cref="ResiliencyOptions"/> for Mango HTTP client resiliency policies.
    /// Use this class to add, configure, and order policies such as timeout, retry, circuit breaker, bulkhead, fallback, and custom policies.
    /// </summary>
    /// <example>
    /// <code>
    /// var configurator = new MangoResiliencyPolicyConfigurator()
    ///     .WithRetry(cfg => cfg.SetMaxRetryCount(3).SetDelay(TimeSpan.FromSeconds(2)))
    ///     .WithTimeout(cfg => cfg.SetTimeout(TimeSpan.FromSeconds(30)))
    ///     .WithCircuitBreaker(cfg => cfg.SetFailureThreshold(5).SetBreakDuration(TimeSpan.FromSeconds(30)))
    ///     .WithPreset("DefaultResiliencyPolicy");
    /// </code>
    /// </example>
    public sealed class MangoResiliencyPolicyConfigurator
    {
        internal MangoResiliencyPolicyConfigurator()
        {
        }

        private List<Action<ResiliencyPolicyOptionsBuilder>> _policyActions = new();
        private List<string> _presets = new();

        /// <summary>
        /// Gets the list of policy configuration actions to apply.
        /// </summary>
        public IReadOnlyList<Action<ResiliencyPolicyOptionsBuilder>> PolicyActions => _policyActions;
        /// <summary>
        /// Gets the list of preset names to apply.
        /// </summary>
        public IReadOnlyList<string> Presets => _presets;

        /// <summary>
        /// Adds a timeout policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithTimeout()
        {
            _policyActions.Add(builder => builder.WithTimeout());
            return this;
        }

        /// <summary>
        /// Adds a timeout policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the timeout policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithTimeout(Action<TimeoutPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithTimeout(configure));
            return this;
        }

        /// <summary>
        /// Adds a retry policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithRetry()
        {
            _policyActions.Add(builder => builder.WithRetry());
            return this;
        }

        /// <summary>
        /// Adds a retry policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the retry policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithRetry(Action<RetryPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithRetry(configure));
            return this;
        }

        /// <summary>
        /// Adds a circuit breaker policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithCircuitBreaker()
        {
            _policyActions.Add(builder => builder.WithCircuitBreaker());
            return this;
        }

        /// <summary>
        /// Adds a circuit breaker policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the circuit breaker policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithCircuitBreaker(Action<CircuitBreakerPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithCircuitBreaker(configure));
            return this;
        }

        /// <summary>
        /// Adds a bulkhead policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithBulkhead()
        {
            _policyActions.Add(builder => builder.WithBulkhead());
            return this;
        }

        /// <summary>
        /// Adds a bulkhead policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the bulkhead policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithBulkhead(Action<BulkheadPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithBulkhead(configure));
            return this;
        }

        /// <summary>
        /// Adds a fallback policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithFallback()
        {
            _policyActions.Add(builder => builder.WithFallback());
            return this;
        }

        /// <summary>
        /// Adds a fallback policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the fallback policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithFallback(Action<FallbackPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithFallback(configure));
            return this;
        }

        /// <summary>
        /// Adds a fallback-on-break policy with default configuration.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithFallbackOnBreak()
        {
            _policyActions.Add(builder => builder.WithFallbackOnBreak());
            return this;
        }

        /// <summary>
        /// Adds a fallback-on-break policy with custom configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the fallback-on-break policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithFallbackOnBreak(Action<FallbackOnBreakPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithFallbackOnBreak(configure));
            return this;
        }

        /// <summary>
        /// Adds a custom policy with the specified configuration.
        /// </summary>
        /// <param name="configure">The configuration action for the custom policy.</param>
        /// <returns>The configurator for chaining.</returns>
        public MangoResiliencyPolicyConfigurator WithCustomPolicy(Action<CustomPolicyBuilder> configure)
        {
            _policyActions.Add(builder => builder.WithCustomPolicy(configure));
            return this;
        }

        /// <summary>
        /// Adds a named preset to the configuration, which will be applied before user-defined policies.
        /// </summary>
        /// <param name="presetName">The name of the preset.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the preset name is null or whitespace.</exception>
        public MangoResiliencyPolicyConfigurator WithPreset(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(presetName));
            _presets.Add(presetName);
            return this;
        }
    }
}
