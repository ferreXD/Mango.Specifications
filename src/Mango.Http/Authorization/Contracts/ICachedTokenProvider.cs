// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    /// <summary>
    /// Defines a contract for providing cached authentication tokens in Mango HTTP clients.
    /// Implementations should provide logic to retrieve and cache tokens for authentication scenarios.
    /// </summary>
    public interface ICachedTokenProvider
    {
        /// <summary>
        /// Gets a cached authentication token asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation (optional).</param>
        /// <returns>A ValueTask representing the asynchronous operation, containing the token string.</returns>
        ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}
