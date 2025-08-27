// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;

    /// <summary>
    /// Defines a factory contract for creating <see cref="IAuthenticationStrategy"/> instances in Mango HTTP clients.
    /// Implementations should provide logic to resolve authentication strategies from dependency injection.
    /// </summary>
    public interface IAuthenticationStrategyFactory
    {
        /// <summary>
        /// Creates an <see cref="IAuthenticationStrategy"/> using the provided service provider.
        /// </summary>
        /// <param name="provider">The service provider for resolving dependencies.</param>
        /// <returns>An instance of <see cref="IAuthenticationStrategy"/>.</returns>
        IAuthenticationStrategy Create(IServiceProvider provider);
    }
}
