// ReSharper disable once CheckNamespace
namespace Mango.Http
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents a builder for configuring Mango HTTP clients.
    /// </summary>
    public interface IMangoHttpClientBuilder
    {
        /// <summary>
        /// Gets the name of the HTTP client being configured.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the service collection used for dependency injection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
