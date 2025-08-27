// ReSharper disable once CheckNamespace
namespace Mango.Http.Presets
{
    using Resiliency;
    using System;

    public sealed class InlineResiliencyPolicyPreset(string name, Action<ResiliencyPolicyOptionsBuilder> configure) : IResiliencyPolicyPreset
    {
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        private readonly Action<ResiliencyPolicyOptionsBuilder> _configure = configure ?? throw new ArgumentNullException(nameof(configure));

        public void Configure(ResiliencyPolicyOptionsBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            _configure(builder);
        }
    }
}
