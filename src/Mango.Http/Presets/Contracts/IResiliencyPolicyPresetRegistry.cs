// ReSharper disable once CheckNamespace
namespace Mango.Http.Presets
{
    /// <summary>
    /// Defines a registry for managing and retrieving resiliency policy presets by name.
    /// Used to look up named preset configurations for Mango HTTP clients.
    /// </summary>
    public interface IResiliencyPolicyPresetRegistry
    {
        /// <summary>
        /// Gets the resiliency policy preset with the specified name.
        /// </summary>
        /// <param name="name">The name of the resiliency policy preset.</param>
        /// <returns>The <see cref="IResiliencyPolicyPreset"/> associated with the specified name.</returns>
        IResiliencyPolicyPreset Get(string name);
    }
}
