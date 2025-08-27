// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;

    /// <summary>
    /// Builder for configuring a circuit breaker resiliency policy for Mango HTTP clients.
    /// Use this class to set failure thresholds, break durations, custom break conditions, and order for the policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new CircuitBreakerPolicyBuilder()
    ///     .SetFailureThreshold(5)
    ///     .SetBreakDuration(TimeSpan.FromSeconds(30))
    ///     .SetOrder(600);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class CircuitBreakerPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the circuit breaker policy in the pipeline.
        /// </summary>
        private int _order { get; set; } = (int)DefaultPolicyOrder.CircuitBreaker;
        /// <summary>
        /// Gets or sets the failure threshold for the circuit breaker.
        /// </summary>
        private int _failureThreshold { get; set; } = 5;
        /// <summary>
        /// Gets or sets the break duration for the circuit breaker.
        /// </summary>
        private TimeSpan _breakDuration { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Gets or sets the custom condition to determine when the circuit should break.
        /// </summary>
        private Func<DelegateResult<HttpResponseMessage>, bool>? _shouldBreak { get; set; } = null;

        /// <summary>
        /// Sets the failure threshold for the circuit breaker.
        /// </summary>
        /// <param name="threshold">The failure threshold value.</param>
        /// <returns>The builder for chaining.</returns>
        public CircuitBreakerPolicyBuilder SetFailureThreshold(int threshold)
        {
            _failureThreshold = threshold;
            return this;
        }

        /// <summary>
        /// Sets the break duration for the circuit breaker.
        /// </summary>
        /// <param name="duration">The break duration value.</param>
        /// <returns>The builder for chaining.</returns>
        public CircuitBreakerPolicyBuilder SetBreakDuration(TimeSpan duration)
        {
            _breakDuration = duration;
            return this;
        }

        /// <summary>
        /// Sets a custom condition to determine when the circuit should break.
        /// </summary>
        /// <param name="condition">A function that returns true if the circuit should break.</param>
        /// <returns>The builder for chaining.</returns>
        public CircuitBreakerPolicyBuilder SetShouldBreakCondition(Func<DelegateResult<HttpResponseMessage>, bool> condition)
        {
            _shouldBreak = condition;
            return this;
        }

        /// <summary>
        /// Sets the order of the circuit breaker policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        public CircuitBreakerPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="CircuitBreakerPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="CircuitBreakerPolicyDefinition"/>.</returns>
        internal CircuitBreakerPolicyDefinition Build()
        {
            Validate();
            return new CircuitBreakerPolicyDefinition(_order)
            {
                FailureThreshold = _failureThreshold,
                ShouldBreak = _shouldBreak ?? (_ => false),
                BreakDuration = _breakDuration
            };
        }

        /// <summary>
        /// Validates the circuit breaker policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentException">Thrown if failureThreshold is not greater than zero or breakDuration is not greater than zero.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_failureThreshold <= 0)
            {
                throw new ArgumentException("Failure threshold must be greater than zero.", nameof(_failureThreshold));
            }
            if (_breakDuration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Break duration must be greater than zero.", nameof(_breakDuration));
            }
        }
    }
}
