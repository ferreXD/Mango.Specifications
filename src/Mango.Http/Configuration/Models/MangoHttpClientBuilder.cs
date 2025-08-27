// ReSharper disable once CheckNamespace
namespace Mango.Http
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Default implementation of <see cref="IMangoHttpClientBuilder"/> for configuring Mango HTTP clients.
    /// </summary>
    internal sealed class MangoHttpClientBuilder : IMangoHttpClientBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MangoHttpClientBuilder"/> class.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client being configured.</param>
        /// <param name="services">The service collection used for dependency injection.</param>
        public MangoHttpClientBuilder(string clientName, IServiceCollection services)
        {
            Name = clientName;
            Services = services;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }
    }
}
