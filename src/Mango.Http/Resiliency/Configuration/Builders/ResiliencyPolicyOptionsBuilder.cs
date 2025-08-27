// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using System;

    /// <summary>
    /// Builder for configuring <see cref="ResiliencyOptions"/> for Mango HTTP client resiliency policies.
    /// Use this builder to add and configure policies such as timeout, retry, circuit breaker, bulkhead, fallback, and custom policies.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new ResiliencyPolicyOptionsBuilder()
    ///     .WithRetry(cfg => cfg.SetMaxRetryCount(3).SetDelay(TimeSpan.FromSeconds(2)))
    ///     .WithTimeout(cfg => cfg.SetTimeout(TimeSpan.FromSeconds(30)))
    ///     .WithCircuitBreaker(cfg => cfg.SetFailureThreshold(5).SetBreakDuration(TimeSpan.FromSeconds(30)));
    /// var options = builder.Build();
    /// </code>
    /// </example>
    public class ResiliencyPolicyOptionsBuilder
    {
        internal ResiliencyPolicyOptionsBuilder()
        {
        }

        private ResiliencyOptions _options = new();

        /// <summary>
        /// Adds a timeout policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithTimeout()
        {
            var builder = new TimeoutPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a timeout policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the timeout policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithTimeout(Action<TimeoutPolicyBuilder> cfg)
        {
            var builder = new TimeoutPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a retry policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithRetry()
        {
            var builder = new RetryPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a retry policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the retry policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithRetry(Action<RetryPolicyBuilder> cfg)
        {
            var builder = new RetryPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a circuit breaker policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithCircuitBreaker()
        {
            var builder = new CircuitBreakerPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a circuit breaker policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the circuit breaker policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithCircuitBreaker(Action<CircuitBreakerPolicyBuilder> cfg)
        {
            var builder = new CircuitBreakerPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a bulkhead policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithBulkhead()
        {
            var builder = new BulkheadPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a bulkhead policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the bulkhead policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithBulkhead(Action<BulkheadPolicyBuilder> cfg)
        {
            var builder = new BulkheadPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a fallback policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithFallback()
        {
            var builder = new FallbackPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a fallback policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the fallback policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithFallback(Action<FallbackPolicyBuilder> cfg)
        {
            var builder = new FallbackPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a fallback-on-break policy with default configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithFallbackOnBreak()
        {
            var builder = new FallbackOnBreakPolicyBuilder();
            _options = _options.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Adds a fallback-on-break policy with custom configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the fallback-on-break policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithFallbackOnBreak(Action<FallbackOnBreakPolicyBuilder> cfg)
        {
            var builder = new FallbackOnBreakPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Adds a custom policy with the specified configuration.
        /// </summary>
        /// <param name="cfg">The configuration action for the custom policy.</param>
        /// <returns>The builder for chaining.</returns>
        public ResiliencyPolicyOptionsBuilder WithCustomPolicy(Action<CustomPolicyBuilder> cfg)
        {
            var builder = new CustomPolicyBuilder();
            cfg(builder);

            var policy = builder.Build();
            _options = _options.Add(policy);
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="ResiliencyOptions"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="ResiliencyOptions"/>.</returns>
        public ResiliencyOptions Build()
        {
            _options.Validate();
            return _options;
        }
    }
}
