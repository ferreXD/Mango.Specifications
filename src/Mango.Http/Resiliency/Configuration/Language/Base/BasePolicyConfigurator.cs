// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Base class for Mango HTTP resiliency policy configurators.
    /// Provides common properties and validation contract for derived configurators.
    /// </summary>
    public abstract class BasePolicyConfigurator(int order = 0)
    {
        /// <summary>
        /// Gets or sets the order of the policy in the pipeline.
        /// </summary>
        internal int Order { get; set; } = order;

        /// <summary>
        /// Validates the policy configuration.
        /// Must be implemented by derived configurators.
        /// </summary>
        internal abstract void Validate();
    }
}
