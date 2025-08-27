// ReSharper disable once CheckNamespace
namespace Mango.Http.Registry
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a registry for managing Mango HTTP client builders by name.
    /// Allows registration, lookup, and enumeration of configured HTTP clients.
    /// </summary>
    public interface IMangoHttpClientRegistry
    {
        /// <summary>
        /// Registers a Mango HTTP client builder with the specified name.
        /// </summary>
        /// <param name="name">The name of the HTTP client.</param>
        /// <param name="builder">The Mango HTTP client builder to register.</param>
        void Register(string name, IMangoHttpClientBuilder builder);

        /// <summary>
        /// Attempts to retrieve a registered Mango HTTP client builder by name.
        /// </summary>
        /// <param name="name">The name of the HTTP client.</param>
        /// <param name="builder">When this method returns, contains the builder associated with the specified name, if found; otherwise, null.</param>
        /// <returns>True if the builder was found; otherwise, false.</returns>
        bool TryGet(string name, out IMangoHttpClientBuilder? builder);

        /// <summary>
        /// Gets a read-only dictionary of all registered Mango HTTP client builders by name.
        /// </summary>
        IReadOnlyDictionary<string, IMangoHttpClientBuilder> Clients { get; }
    }
}
