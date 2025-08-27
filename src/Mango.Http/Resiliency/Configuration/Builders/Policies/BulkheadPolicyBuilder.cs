// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Builder for configuring a bulkhead resiliency policy for Mango HTTP clients.
    /// Use this class to set parallelization and queue limits, and the order for the policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new BulkheadPolicyBuilder()
    ///     .SetMaxParallelization(10)
    ///     .SetMaxQueueLength(100)
    ///     .SetOrder(400);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class BulkheadPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the bulkhead policy in the pipeline.
        /// </summary>
        private int _order { get; set; } = (int)DefaultPolicyOrder.Bulkhead;
        /// <summary>
        /// Gets or sets the maximum number of parallel requests allowed by the bulkhead policy.
        /// </summary>
        private int _maxParallelization { get; set; } = 10;
        /// <summary>
        /// Gets or sets the maximum queue length for requests waiting to enter the bulkhead.
        /// </summary>
        private int _maxQueueLength { get; set; } = 100;

        /// <summary>
        /// Sets the maximum number of parallel requests allowed.
        /// </summary>
        /// <param name="maxParallelization">The maximum parallelization value.</param>
        /// <returns>The builder for chaining.</returns>
        public BulkheadPolicyBuilder SetMaxParallelization(int maxParallelization)
        {
            _maxParallelization = maxParallelization;
            return this;
        }

        /// <summary>
        /// Sets the maximum queue length for requests waiting to enter the bulkhead.
        /// </summary>
        /// <param name="maxQueueLength">The maximum queue length value.</param>
        /// <returns>The builder for chaining.</returns>
        public BulkheadPolicyBuilder SetMaxQueueLength(int maxQueueLength)
        {
            _maxQueueLength = maxQueueLength;
            return this;
        }

        /// <summary>
        /// Sets the order of the bulkhead policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        public BulkheadPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="BulkheadPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="BulkheadPolicyDefinition"/>.</returns>
        internal BulkheadPolicyDefinition Build()
        {
            Validate();
            return new BulkheadPolicyDefinition(_order)
            {
                MaxParallelization = _maxParallelization,
                MaxQueuing = _maxQueueLength
            };
        }

        /// <summary>
        /// Validates the bulkhead policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentException">Thrown if maxParallelization is not greater than zero or maxQueueLength is negative.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_maxParallelization <= 0)
            {
                throw new ArgumentException("MaxParallelization must be greater than zero.", nameof(_maxParallelization));
            }

            if (_maxQueueLength < 0)
            {
                throw new ArgumentException("MaxQueueLength cannot be negative.", nameof(_maxQueueLength));
            }
        }
    }
}
