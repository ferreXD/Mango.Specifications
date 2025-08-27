// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a contract for applying authentication to HTTP requests in Mango HTTP clients.
    /// Implementations should provide logic to modify requests with authentication headers or tokens.
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Applies authentication to the specified HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation (optional).</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
