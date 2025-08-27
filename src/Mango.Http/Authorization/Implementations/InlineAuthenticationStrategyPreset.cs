// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;

    /// <summary>
    /// Inline authentication strategy preset for Mango HTTP clients.
    /// Defines a named preset with a custom configuration action for <see cref="HttpAuthOptionsBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Use this preset to register and apply custom authentication strategies inline by name.
    /// </remarks>
    public sealed class InlineAuthenticationStrategyPreset : IAuthenticationStrategyPreset
    {
        /// <summary>
        /// Gets the name of the authentication strategy preset.
        /// </summary>
        public string Name { get; }
        private readonly Action<HttpAuthOptionsBuilder> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineAuthenticationStrategyPreset"/> class.
        /// </summary>
        /// <param name="name">The name of the preset.</param>
        /// <param name="configure">The action to configure the <see cref="HttpAuthOptionsBuilder"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if name or configure is null.</exception>
        public InlineAuthenticationStrategyPreset(string name, Action<HttpAuthOptionsBuilder> configure)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        /// <summary>
        /// Configures the specified <see cref="HttpAuthOptionsBuilder"/> using the preset's configuration action.
        /// </summary>
        /// <param name="builder">The options builder to configure.</param>
        public void Configure(HttpAuthOptionsBuilder builder)
            => _configure(builder);
    }
}
