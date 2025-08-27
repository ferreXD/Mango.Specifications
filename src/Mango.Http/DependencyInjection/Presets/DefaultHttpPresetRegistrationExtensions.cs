// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Mango.Http.Constants;

    /// <summary>
    /// Extension methods for registering the default Mango HTTP resiliency policy preset.
    /// </summary>
    public static class DefaultHttpPresetRegistrationExtensions
    {
        /// <summary>
        /// Registers the default Mango HTTP resiliency policy preset in the service collection.
        /// </summary>
        /// <param name="services">The service collection to register the preset into.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddDefaultMangoHttpPreset(this IServiceCollection services)
            => services.AddResiliencyPolicyPreset(ResiliencyPolicyDefaults.DefaultPolicyName, ResiliencyPolicyDefaults.DefaultBuilder);
    }
}
