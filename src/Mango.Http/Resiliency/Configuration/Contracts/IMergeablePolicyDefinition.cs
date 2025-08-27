// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Defines a contract for resiliency policy definitions that can be merged.
    /// Used to combine preset and user-defined policies into a single effective policy.
    /// </summary>
    /// <typeparam name="T">The type of resiliency policy definition.</typeparam>
    public interface IMergeablePolicyDefinition<T>
        where T : ResiliencyPolicyDefinition
    {
        /// <summary>
        /// Merges a preset policy definition with a user-defined policy definition.
        /// </summary>
        /// <param name="preset">The preset policy definition.</param>
        /// <returns>The merged policy definition.</returns>
        T Merge(T preset);
    }
}
