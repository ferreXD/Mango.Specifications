// ReSharper disable once CheckNamespace
namespace Mango.Http.Presets
{
    using System;

    public sealed class DefaultResiliencyPolicyPresetRegistry(IEnumerable<IResiliencyPolicyPreset> presets) : IResiliencyPolicyPresetRegistry
    {
        public IResiliencyPolicyPreset Get(string name)
            => presets.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Resiliency preset '{name}' not found.");
    }
}
