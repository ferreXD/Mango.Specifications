// ReSharper disable once CheckNamespace
namespace Mango.Http.Presets
{
    using Resiliency;

    /// <summary>
    /// Defines a contract for a resiliency policy preset in Mango HTTP clients.
    /// A preset provides a named configuration for one or more resiliency policies, which can be applied to HTTP clients.
    /// </summary>
    public interface IResiliencyPolicyPreset
    {
        /// <summary>
        /// Gets the name of the resiliency policy preset.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Configures the specified <see cref="ResiliencyPolicyOptionsBuilder"/> with the preset's policies.
        /// </summary>
        /// <param name="options">The options builder to configure.</param>
        void Configure(ResiliencyPolicyOptionsBuilder options);
    }
}
