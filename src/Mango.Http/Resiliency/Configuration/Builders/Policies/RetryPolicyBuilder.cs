// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;

    /// <summary>
    /// Builder for configuring a retry resiliency policy for Mango HTTP clients.
    /// Use this class to set retry count, delay, jitter, and custom retry conditions for the policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new RetryPolicyBuilder()
    ///     .SetMaxRetryCount(3)
    ///     .SetDelay(TimeSpan.FromSeconds(2))
    ///     .SetUseJitter(true);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class RetryPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the retry policy in the pipeline.
        /// </summary>
        private int _order { get; set; } = (int)DefaultPolicyOrder.Retry;
        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        private int _maxRetryCount { get; set; } = 3;
        /// <summary>
        /// Gets or sets the delay between retry attempts.
        /// </summary>
        private TimeSpan _delay { get; set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Gets or sets a value indicating whether to use jitter in retry delays.
        /// </summary>
        private bool _useJitter { get; set; } = false;
        /// <summary>
        /// Gets or sets the custom condition to determine when a retry should occur.
        /// </summary>
        private Func<DelegateResult<HttpResponseMessage>, bool>? _shouldRetry { get; set; } = null;

        /// <summary>
        /// Sets the order of the retry policy in the pipeline.
        /// </summary>
        public RetryPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of retry attempts.
        /// </summary>
        /// <param name="maxRetryCount">The maximum retry count value.</param>
        /// <returns>The builder for chaining.</returns>
        public RetryPolicyBuilder SetMaxRetryCount(int maxRetryCount)
        {
            _maxRetryCount = maxRetryCount;
            return this;
        }

        /// <summary>
        /// Sets the delay between retry attempts.
        /// </summary>
        /// <param name="delay">The delay value.</param>
        /// <returns>The builder for chaining.</returns>
        public RetryPolicyBuilder SetDelay(TimeSpan delay)
        {
            _delay = delay;
            return this;
        }

        /// <summary>
        /// Sets whether to use jitter in retry delays.
        /// </summary>
        /// <param name="useJitter">True to use jitter; otherwise, false.</param>
        /// <returns>The builder for chaining.</returns>
        public RetryPolicyBuilder SetUseJitter(bool useJitter = true)
        {
            _useJitter = useJitter;
            return this;
        }

        /// <summary>
        /// Sets a custom condition to determine when a retry should occur.
        /// </summary>
        /// <param name="condition">A function that returns true if a retry should occur.</param>
        /// <returns>The builder for chaining.</returns>
        public RetryPolicyBuilder SetShouldRetryCondition(Func<DelegateResult<HttpResponseMessage>, bool> condition)
        {
            _shouldRetry = condition;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="RetryPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="RetryPolicyDefinition"/>.</returns>
        internal RetryPolicyDefinition Build()
        {
            Validate();
            return new RetryPolicyDefinition(_order)
            {
                RetryCount = _maxRetryCount,
                RetryDelay = _delay,
                UseJitter = _useJitter,
                ShouldRetry = _shouldRetry
            };
        }

        /// <summary>
        /// Validates the retry policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentException">Thrown if maxRetryCount is not greater than zero or delay is not greater than zero.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_maxRetryCount < 0)
            {
                throw new ArgumentException("Max retry count must be greater or equal than zero.", nameof(_maxRetryCount));
            }
            if (_delay <= TimeSpan.Zero)
            {
                throw new ArgumentException("Delay must be greater than zero.", nameof(_delay));
            }
        }
    }
}
