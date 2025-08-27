// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;

    /// <summary>
    /// Extension methods for registering Mango HTTP resiliency policy presets in the dependency injection container.
    /// </summary>
    public static class MangoHttpPresetRegistrationExtensions
    {
        /// <summary>
        /// Registers a resiliency policy preset with the specified name and builder in the service collection.
        /// </summary>
        /// <param name="services">The service collection to register the preset into.</param>
        /// <param name="name">The name of the resiliency policy preset.</param>
        /// <param name="builder">The configuration action for the preset.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="ArgumentException">Thrown if the preset name is null or whitespace.</exception>
        public static IServiceCollection AddResiliencyPolicyPreset(
            this IServiceCollection services,
            string name,
            Action<ResiliencyPolicyOptionsBuilder> builder)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(name));

            services.AddSingleton<IResiliencyPolicyPreset>(new InlineResiliencyPolicyPreset(name, builder));

            return services;
        }
    }
}
