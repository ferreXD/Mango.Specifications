// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using System;

    /// <summary>
    /// Configurator for the timeout resiliency policy in Mango HTTP clients.
    /// Use this class to set the timeout duration and its order in the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var timeout = new TimeoutPolicyConfigurator()
    ///     .SetTimeout(TimeSpan.FromSeconds(30))
    ///     .SetOrder(200);
    /// </code>
    /// </example>
    public sealed class TimeoutPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.TimeoutPerAttempt)
    {
        /// <summary>
        /// Gets or sets the timeout duration for the policy.
        /// </summary>
        internal TimeSpan Timeout { get; private set; }

        /// <summary>
        /// Sets the timeout duration for the policy.
        /// </summary>
        /// <param name="timeout">The timeout duration value.</param>
        /// <returns>The configurator for chaining.</returns>
        public TimeoutPolicyConfigurator SetTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the order of the timeout policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public TimeoutPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the timeout policy configuration.
        /// Throws an exception if the timeout duration is not greater than zero.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if Timeout is not greater than zero.</exception>
        internal override void Validate()
        {
            if (Timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException("Timeout must be greater than zero.", nameof(Timeout));
            }
        }
    }
}
