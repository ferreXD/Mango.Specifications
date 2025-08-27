// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Builder for configuring a timeout resiliency policy for Mango HTTP clients.
    /// Use this class to set the timeout duration and order for the policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new TimeoutPolicyBuilder()
    ///     .SetTimeout(TimeSpan.FromSeconds(30))
    ///     .SetOrder(200);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class TimeoutPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the timeout policy in the pipeline.
        /// </summary>
        private int _order { get; set; } = (int)DefaultPolicyOrder.TimeoutPerAttempt;
        /// <summary>
        /// Gets or sets the timeout duration for the policy.
        /// </summary>
        private TimeSpan _timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Sets the timeout duration for the policy.
        /// </summary>
        /// <param name="timeout">The timeout duration value.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if timeout is not greater than zero.</exception>
        public TimeoutPolicyBuilder SetTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("Timeout must be greater than zero.", nameof(timeout));

            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the order of the timeout policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if order is negative.</exception>
        public TimeoutPolicyBuilder SetOrder(int order)
        {
            if (order < 0) throw new ArgumentException("Order must be a non-negative integer.", nameof(order));
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="OperationTimeoutPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="OperationTimeoutPolicyDefinition"/>.</returns>
        internal OperationTimeoutPolicyDefinition Build()
        {
            Validate();
            return new OperationTimeoutPolicyDefinition(_order)
            {
                Budget = _timeout
            };
        }

        /// <summary>
        /// Validates the timeout policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if timeout is not set.</exception>
        /// <exception cref="ArgumentException">Thrown if order is negative.</exception>
        private void Validate()
        {
            if (_timeout == default)
                throw new InvalidOperationException("Timeout must be set before building the policy.");
            if (_order < 0)
                throw new ArgumentException("Order must be a non-negative integer.", nameof(_order));
        }
    }
}
