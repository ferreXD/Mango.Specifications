// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    /// <summary>
    /// Defines a registry for managing and retrieving authentication strategy presets by name in Mango HTTP clients.
    /// Used to look up named preset configurations for authentication strategies.
    /// </summary>
    public interface IAuthenticationStrategyPresetRegistry
    {
        /// <summary>
        /// Gets the authentication strategy preset with the specified name.
        /// </summary>
        /// <param name="name">The name of the authentication strategy preset.</param>
        /// <returns>The <see cref="IAuthenticationStrategyPreset"/> associated with the specified name.</returns>
        IAuthenticationStrategyPreset Get(string name);
    }
}
