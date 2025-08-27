// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;

    /// <summary>
    /// Configurator for the retry resiliency policy in Mango HTTP clients.
    /// Use this class to set retry count, delay, jitter, and custom retry conditions.
    /// </summary>
    /// <example>
    /// <code>
    /// var retry = new RetryPolicyConfigurator()
    ///     .SetMaxRetryCount(3)
    ///     .SetDelay(TimeSpan.FromSeconds(2))
    ///     .SetUseJitter(true);
    /// </code>
    /// </example>
    public sealed class RetryPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.Retry)
    {
        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        internal int MaxRetryCount { get; private set; } = 3;
        /// <summary>
        /// Gets or sets the delay between retry attempts.
        /// </summary>
        internal TimeSpan Delay { get; private set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Gets or sets a value indicating whether to use jitter in retry delays.
        /// </summary>
        internal bool UseJitter { get; private set; } = false;
        /// <summary>
        /// Gets or sets the custom condition to determine when a retry should occur.
        /// </summary>
        internal Func<DelegateResult<HttpResponseMessage>, bool>? ShouldRetry { get; private set; } = _ => false;

        /// <summary>
        /// Sets the maximum number of retry attempts.
        /// </summary>
        /// <param name="maxRetryCount">The maximum retry count value.</param>
        /// <returns>The configurator for chaining.</returns>
        public RetryPolicyConfigurator SetMaxRetryCount(int maxRetryCount)
        {
            MaxRetryCount = maxRetryCount;
            return this;
        }

        /// <summary>
        /// Sets the delay between retry attempts.
        /// </summary>
        /// <param name="delay">The delay value.</param>
        /// <returns>The configurator for chaining.</returns>
        public RetryPolicyConfigurator SetDelay(TimeSpan delay)
        {
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Sets whether to use jitter in retry delays.
        /// </summary>
        /// <param name="useJitter">True to use jitter; otherwise, false.</param>
        /// <returns>The configurator for chaining.</returns>
        public RetryPolicyConfigurator SetUseJitter(bool useJitter)
        {
            UseJitter = useJitter;
            return this;
        }

        /// <summary>
        /// Sets a custom condition to determine when a retry should occur.
        /// </summary>
        /// <param name="condition">A function that returns true if a retry should occur.</param>
        /// <returns>The configurator for chaining.</returns>
        public RetryPolicyConfigurator SetShouldRetryCondition(Func<DelegateResult<HttpResponseMessage>, bool> condition)
        {
            ShouldRetry = condition;
            return this;
        }

        /// <summary>
        /// Validates the retry policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if MaxRetryCount is not greater than zero or Delay is not greater than zero.</exception>
        internal override void Validate()
        {
            if (MaxRetryCount <= 0)
            {
                throw new ArgumentException("Max retry count must be greater than zero.", nameof(MaxRetryCount));
            }
            if (Delay <= TimeSpan.Zero)
            {
                throw new ArgumentException("Delay must be greater than zero.", nameof(Delay));
            }
        }
    }
}
