namespace Mango.Http.Authorization
{
    /// <summary>
    /// Defines a contract for authentication strategy presets in Mango HTTP clients.
    /// A preset provides a named configuration for one or more authentication strategies, which can be applied to HTTP clients.
    /// </summary>
    public interface IAuthenticationStrategyPreset
    {
        /// <summary>
        /// Gets the name of the authentication strategy preset.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Configures the specified <see cref="HttpAuthOptionsBuilder"/> with the preset's authentication strategies.
        /// </summary>
        /// <param name="builder">The options builder to configure.</param>
        void Configure(HttpAuthOptionsBuilder builder);
    }
}
