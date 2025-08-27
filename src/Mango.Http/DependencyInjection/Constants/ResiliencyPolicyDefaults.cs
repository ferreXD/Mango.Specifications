// ReSharper disable once CheckNamespace
namespace Mango.Http.Constants
{
    using Resiliency;
    using System;

    /// <summary>
    /// Provides default values and configuration delegates for Mango HTTP resiliency policies.
    /// </summary>
    public static class ResiliencyPolicyDefaults
    {
        /// <summary>
        /// The default name for the resiliency policy preset.
        /// </summary>
        public const string DefaultPolicyName = "DefaultResiliencyPolicy";

        /// <summary>
        /// The default configuration delegate for MangoResiliencyPolicyConfigurator.
        /// </summary>
        public static Action<MangoResiliencyPolicyConfigurator> DefaultConfiguration = configurator =>
        {
            configurator
                .WithRetry(opt => opt
                    .SetUseJitter(RetryPolicyDefaults.DefaultUseJitter)
                    .SetMaxRetryCount(RetryPolicyDefaults.DefaultMaxRetryCount)
                    .SetDelay(RetryPolicyDefaults.DefaultDelay))
                .WithTimeout(opt => opt
                    .SetTimeout(TimeoutPolicyDefaults.DefaultTimeout))
                .WithCircuitBreaker(opt => opt
                    .SetBreakDuration(CircuitBreakerPolicyDefaults.DefaultBreakDuration)
                    .SetFailureThreshold(CircuitBreakerPolicyDefaults.DefaultFailureThreshold))
                .WithBulkhead(opt => opt
                    .SetMaxParallelization(BulkheadPolicyDefaults.DefaultMaxConcurrentRequests)
                    .SetMaxQueueLength(BulkheadPolicyDefaults.DefaultMaxQueueSize));
        };

        /// <summary>
        /// The default builder delegate for ResiliencyPolicyOptionsBuilder.
        /// </summary>
        internal static Action<ResiliencyPolicyOptionsBuilder> DefaultBuilder = builder =>
        {
            builder
                .WithRetry(opt => opt
                    .SetUseJitter(RetryPolicyDefaults.DefaultUseJitter)
                    .SetMaxRetryCount(RetryPolicyDefaults.DefaultMaxRetryCount)
                    .SetDelay(RetryPolicyDefaults.DefaultDelay))
                .WithTimeout(opt => opt
                    .SetTimeout(TimeoutPolicyDefaults.DefaultTimeout))
                .WithCircuitBreaker(opt => opt
                    .SetBreakDuration(CircuitBreakerPolicyDefaults.DefaultBreakDuration)
                    .SetFailureThreshold(CircuitBreakerPolicyDefaults.DefaultFailureThreshold))
                .WithBulkhead(opt => opt
                    .SetMaxParallelization(BulkheadPolicyDefaults.DefaultMaxConcurrentRequests)
                    .SetMaxQueueLength(BulkheadPolicyDefaults.DefaultMaxQueueSize));
        };
    }

    /// <summary>
    /// Provides default values for the retry policy.
    /// </summary>
    public static class RetryPolicyDefaults
    {
        /// <summary>Default maximum retry count.</summary>
        public const int DefaultMaxRetryCount = 3;
        /// <summary>Default delay between retries.</summary>
        public static readonly TimeSpan DefaultDelay = TimeSpan.FromSeconds(2);
        /// <summary>Default value indicating whether to use jitter.</summary>
        public const bool DefaultUseJitter = false;
    }

    /// <summary>
    /// Provides default values for the timeout policy.
    /// </summary>
    public static class TimeoutPolicyDefaults
    {
        /// <summary>Default timeout duration.</summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Provides default values for the circuit breaker policy.
    /// </summary>
    public static class CircuitBreakerPolicyDefaults
    {
        /// <summary>Default failure threshold for circuit breaker.</summary>
        public const int DefaultFailureThreshold = 5;
        /// <summary>Default break duration for circuit breaker.</summary>
        public static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Provides default values for the bulkhead policy.
    /// </summary>
    public static class BulkheadPolicyDefaults
    {
        /// <summary>Default maximum number of concurrent requests.</summary>
        public const int DefaultMaxConcurrentRequests = 10;
        /// <summary>Default maximum queue size.</summary>
        public const int DefaultMaxQueueSize = 100;
    }
}
