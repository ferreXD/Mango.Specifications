// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helper to collect inline authentication strategy presets before dependency injection build.
    /// Use this class to define and register custom authentication presets for Mango HTTP clients.
    /// </summary>
    /// <example>
    /// <code>
    /// var configurator = new AuthenticationPresetConfigurator()
    ///     .WithPreset("Bearer", builder => builder.UseBearerToken("token"));
    /// var presets = configurator.Build();
    /// </code>
    /// </example>
    public sealed class AuthenticationPresetConfigurator
    {
        private readonly List<IAuthenticationStrategyPreset> _presets = new();

        /// <summary>
        /// Adds an inline authentication strategy preset with the specified name and configuration.
        /// </summary>
        /// <param name="name">The name of the preset.</param>
        /// <param name="configure">The configuration action for the preset.</param>
        /// <returns>The configurator for chaining.</returns>
        public AuthenticationPresetConfigurator WithPreset(
            string name,
            Action<HttpAuthOptionsBuilder> configure)
        {
            _presets.Add(new InlineAuthenticationStrategyPreset(name, configure));
            return this;
        }

        /// <summary>
        /// Builds and returns the collection of configured authentication strategy presets.
        /// </summary>
        /// <returns>An enumerable of <see cref="IAuthenticationStrategyPreset"/>.</returns>
        public IEnumerable<IAuthenticationStrategyPreset> Build() => _presets;
    }
}
