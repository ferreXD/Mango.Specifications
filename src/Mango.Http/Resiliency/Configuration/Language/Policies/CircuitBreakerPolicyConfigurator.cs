// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;

    /// <summary>
    /// Configurator for the circuit breaker resiliency policy in Mango HTTP clients.
    /// Use this class to set failure thresholds, break durations, and custom break conditions for circuit breaker isolation.
    /// </summary>
    /// <example>
    /// <code>
    /// var circuitBreaker = new CircuitBreakerPolicyConfigurator()
    ///     .SetFailureThreshold(5)
    ///     .SetBreakDuration(TimeSpan.FromSeconds(30))
    ///     .SetOrder(600);
    /// </code>
    /// </example>
    public sealed class CircuitBreakerPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.CircuitBreaker)
    {
        /// <summary>
        /// Gets or sets the failure threshold for the circuit breaker.
        /// </summary>
        internal int FailureThreshold { get; private set; } = 5;
        /// <summary>
        /// Gets or sets the break duration for the circuit breaker.
        /// </summary>
        internal TimeSpan BreakDuration { get; private set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Gets or sets the custom condition to determine when the circuit should break.
        /// </summary>
        internal Func<DelegateResult<HttpResponseMessage>, bool>? ShouldBreak { get; private set; } = _ => false;

        /// <summary>
        /// Sets the failure threshold for the circuit breaker.
        /// </summary>
        /// <param name="threshold">The failure threshold value.</param>
        /// <returns>The configurator for chaining.</returns>
        public CircuitBreakerPolicyConfigurator SetFailureThreshold(int threshold)
        {
            FailureThreshold = threshold;
            return this;
        }

        /// <summary>
        /// Sets the break duration for the circuit breaker.
        /// </summary>
        /// <param name="duration">The break duration value.</param>
        /// <returns>The configurator for chaining.</returns>
        public CircuitBreakerPolicyConfigurator SetBreakDuration(TimeSpan duration)
        {
            BreakDuration = duration;
            return this;
        }

        /// <summary>
        /// Sets a custom condition to determine when the circuit should break.
        /// </summary>
        /// <param name="condition">A function that returns true if the circuit should break.</param>
        /// <returns>The configurator for chaining.</returns>
        public CircuitBreakerPolicyConfigurator SetShouldBreakCondition(Func<DelegateResult<HttpResponseMessage>, bool> condition)
        {
            ShouldBreak = condition;
            return this;
        }

        /// <summary>
        /// Sets the order of the circuit breaker policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public CircuitBreakerPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the circuit breaker policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if FailureThreshold is not greater than zero or BreakDuration is not greater than zero.</exception>
        internal override void Validate()
        {
            if (FailureThreshold <= 0)
            {
                throw new ArgumentException("Failure threshold must be greater than zero.", nameof(FailureThreshold));
            }
            if (BreakDuration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Break duration must be greater than zero.", nameof(BreakDuration));
            }
        }
    }
}
