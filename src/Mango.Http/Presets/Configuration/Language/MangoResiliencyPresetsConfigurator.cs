// ReSharper disable once CheckNamespace
namespace Mango.Http.Presets
{
    using Microsoft.Extensions.DependencyInjection;
    using Resiliency;
    using System;

    /// <summary>
    /// Extension methods and configurator for registering Mango HTTP resiliency policy presets in the dependency injection container.
    /// Use these methods to define, validate, and register named resiliency policy presets for HTTP clients.
    /// </summary>
    public static class MangoResiliencyPresetsConfigurator
    {
        /// <summary>
        /// Registers resiliency policy presets using the provided configurator.
        /// </summary>
        /// <param name="services">The service collection to register the presets into.</param>
        /// <param name="configure">An action to configure resiliency policy presets.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or configure is null.</exception>
        /// <example>
        /// <code>
        /// services.AddResiliencyPresets(cfg =>
        ///     cfg.WithPreset("Default", builder => builder.WithRetry().WithTimeout()));
        /// </code>
        /// </example>
        public static IServiceCollection AddResiliencyPresets(
            this IServiceCollection services,
            Action<ResiliencyPresetConfigurator> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var configurator = new ResiliencyPresetConfigurator();
            configure(configurator);

            var presets = configurator.Build();
            services.AddSingleton<IResiliencyPolicyPresetRegistry>(sp => new DefaultResiliencyPolicyPresetRegistry(presets));

            return services;
        }
    }

    /// <summary>
    /// Fluent configurator for building and validating Mango HTTP resiliency policy presets.
    /// Use this class to define named presets and ensure uniqueness before registration.
    /// </summary>
    public sealed class ResiliencyPresetConfigurator
    {
        private readonly List<IResiliencyPolicyPreset> _presets = [];

        /// <summary>
        /// Adds a named resiliency policy preset with the specified configuration.
        /// </summary>
        /// <param name="presetName">The name of the preset.</param>
        /// <param name="configurator">The configuration action for the preset.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if presetName is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if configurator is null.</exception>
        public ResiliencyPresetConfigurator WithPreset(string presetName,
            Action<ResiliencyPolicyOptionsBuilder> configurator)
        {
            if (string.IsNullOrWhiteSpace(presetName)) throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(presetName));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));

            var preset = new InlineResiliencyPolicyPreset(presetName, configurator);
            _presets.Add(preset);

            return this;
        }

        /// <summary>
        /// Adds an existing resiliency policy preset instance.
        /// </summary>
        /// <param name="preset">The preset instance to add.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if preset is null.</exception>
        public ResiliencyPresetConfigurator WithPreset(IResiliencyPolicyPreset preset)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            _presets.Add(preset);
            return this;
        }

        /// <summary>
        /// Builds and validates the collection of configured resiliency policy presets.
        /// </summary>
        /// <returns>The validated list of resiliency policy presets.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no presets are configured or duplicate names are found.</exception>
        internal List<IResiliencyPolicyPreset> Build()
        {
            Validate();
            return _presets;
        }

        /// <summary>
        /// Validates the configured resiliency policy presets for uniqueness and presence.
        /// Throws if no presets are configured or duplicate names exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no presets are configured or duplicate names are found.</exception>
        private void Validate()
        {
            if (_presets.Count == 0) throw new InvalidOperationException("No resiliency presets have been configured.");
            var duped = _presets.GroupBy(p => p.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duped.Any()) throw new InvalidOperationException($"Duplicate resiliency preset names found: {string.Join(", ", duped)}.");
        }
    }
}
