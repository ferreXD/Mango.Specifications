// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Provides methods to merge projection settings from different specifications.
    /// </summary>
    internal static class CompositionProjectionBuilder
    {
        /// <summary>
        /// Merges the projection settings from two specifications into a destination specification based on the specified policy.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The destination specification where the merged projection settings will be applied.</param>
        /// <param name="left">The left specification to merge.</param>
        /// <param name="right">The right specification to merge.</param>
        /// <param name="policy">The policy to use when merging projection settings.</param>
        /// <exception cref="InvalidOperationException">Thrown when the selected specification has no projection defined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified policy is not recognized.</exception>
        internal static void Merge<T, TResult>(
            Specification<T, TResult> specification,
            ISpecification<T, TResult> left,
            ISpecification<T, TResult> right,
            ProjectionEvaluationPolicy policy)
        {
            switch (policy)
            {
                case ProjectionEvaluationPolicy.Left:
                    specification.Selector = left.Selector;
                    specification.SelectorMany = left.SelectorMany;
                    break;
                case ProjectionEvaluationPolicy.Right:
                    specification.Selector = right.Selector;
                    specification.SelectorMany = right.SelectorMany;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
            }
        }
    }
}