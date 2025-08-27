// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;

    /// <summary>
    /// Default implementation of <see cref="IAuthenticationStrategyPresetRegistry"/> for Mango HTTP clients.
    /// Manages and retrieves authentication strategy presets by name using a case-insensitive dictionary.
    /// </summary>
    /// <remarks>
    /// This registry is typically used to look up named preset configurations for authentication strategies.
    /// </remarks>
    public sealed class DefaultAuthenticationStrategyPresetRegistry : IAuthenticationStrategyPresetRegistry
    {
        private readonly IReadOnlyDictionary<string, IAuthenticationStrategyPreset> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthenticationStrategyPresetRegistry"/> class.
        /// </summary>
        /// <param name="presets">The collection of authentication strategy presets to register.</param>
        public DefaultAuthenticationStrategyPresetRegistry(IEnumerable<IAuthenticationStrategyPreset> presets)
        {
            _map = presets
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the authentication strategy preset with the specified name.
        /// Throws <see cref="KeyNotFoundException"/> if the preset is not found.
        /// </summary>
        /// <param name="name">The name of the authentication strategy preset.</param>
        /// <returns>The <see cref="IAuthenticationStrategyPreset"/> associated with the specified name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the preset is not found.</exception>
        public IAuthenticationStrategyPreset Get(string name)
            => _map.TryGetValue(name, out var p)
                ? p
                : throw new KeyNotFoundException($"Auth preset '{name}' not found.");
    }
}
