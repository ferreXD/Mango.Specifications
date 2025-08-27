// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Configurator for the bulkhead resiliency policy in Mango HTTP clients.
    /// Use this class to set parallelization and queue limits for bulkhead isolation.
    /// </summary>
    /// <example>
    /// <code>
    /// var bulkhead = new BulkheadPolicyConfigurator()
    ///     .SetMaxParallelization(10)
    ///     .SetMaxQueueLength(100)
    ///     .SetOrder(400);
    /// </code>
    /// </example>
    public class BulkheadPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.Bulkhead)
    {
        /// <summary>
        /// Gets or sets the maximum number of parallel requests allowed by the bulkhead policy.
        /// </summary>
        internal int MaxParallelization { get; set; } = 10;
        /// <summary>
        /// Gets or sets the maximum queue length for requests waiting to enter the bulkhead.
        /// </summary>
        internal int MaxQueueLength { get; set; } = 100;

        /// <summary>
        /// Sets the maximum number of parallel requests allowed.
        /// </summary>
        /// <param name="maxParallelization">The maximum parallelization value.</param>
        /// <returns>The configurator for chaining.</returns>
        public BulkheadPolicyConfigurator SetMaxParallelization(int maxParallelization)
        {
            MaxParallelization = maxParallelization;
            return this;
        }

        /// <summary>
        /// Sets the maximum queue length for requests waiting to enter the bulkhead.
        /// </summary>
        /// <param name="maxQueueLength">The maximum queue length value.</param>
        /// <returns>The configurator for chaining.</returns>
        public BulkheadPolicyConfigurator SetMaxQueueLength(int maxQueueLength)
        {
            MaxQueueLength = maxQueueLength;
            return this;
        }

        /// <summary>
        /// Sets the order of the bulkhead policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public BulkheadPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the bulkhead policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if MaxParallelization is not greater than zero or MaxQueueLength is negative.</exception>
        internal override void Validate()
        {
            if (MaxParallelization <= 0)
            {
                throw new ArgumentException("MaxParallelization must be greater than zero.", nameof(MaxParallelization));
            }

            if (MaxQueueLength < 0)
            {
                throw new ArgumentException("MaxQueueLength cannot be negative.", nameof(MaxQueueLength));
            }
        }
    }
}
