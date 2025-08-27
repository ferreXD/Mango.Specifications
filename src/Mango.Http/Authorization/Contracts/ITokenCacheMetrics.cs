// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    /// <summary>
    /// Defines a contract for token cache metrics in Mango HTTP clients.
    /// Provides properties to track token renewal and failure counts for authentication scenarios.
    /// </summary>
    public interface ITokenCacheMetrics
    {
        /// <summary>
        /// Gets the number of times the token has been renewed.
        /// </summary>
        long RenewalCount { get; }

        /// <summary>
        /// Gets the number of token renewal failures.
        /// </summary>
        long FailureCount { get; }
    }
}
