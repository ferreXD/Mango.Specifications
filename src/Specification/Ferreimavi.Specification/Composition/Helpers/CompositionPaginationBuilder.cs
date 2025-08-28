// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Provides methods to merge pagination settings from different specifications.
    /// </summary>
    public static class CompositionPaginationBuilder
    {
        /// <summary>
        /// Merges the pagination settings from two specifications into a destination specification based on the specified policy.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="destination">The destination specification where the merged pagination settings will be applied.</param>
        /// <param name="left">The left specification to merge.</param>
        /// <param name="right">The right specification to merge.</param>
        /// <param name="policy">The policy to use when merging pagination settings.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there is a conflict in pagination settings and the policy is
        /// <see cref="PaginationEvaluationPolicy.ThrowOnConflict" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified policy is not recognized.</exception>
        public static void Merge<T>(
            Specification<T> destination,
            ISpecification<T> left,
            ISpecification<T> right,
            PaginationEvaluationPolicy policy)
        {
            switch (policy)
            {
                case PaginationEvaluationPolicy.Left:
                    destination.Skip = left.Skip;
                    destination.Take = left.Take;
                    break;
                case PaginationEvaluationPolicy.Right:
                    destination.Skip = right.Skip;
                    destination.Take = right.Take;
                    break;
                case PaginationEvaluationPolicy.None:
                    destination.Skip = null;
                    destination.Take = null;
                    break;
                case PaginationEvaluationPolicy.ThrowOnConflict:
                    // Check for conflicts in Skip and Take values.
                    if (left.Skip != null && right.Skip != null && left.Skip != right.Skip) throw new InvalidOperationException("Pagination conflict: left and right specifications have different Skip values.");
                    if (left.Take != null && right.Take != null && left.Take != right.Take) throw new InvalidOperationException("Pagination conflict: left and right specifications have different Take values.");
                    destination.Skip = left.Skip ?? right.Skip;
                    destination.Take = left.Take ?? right.Take;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
            }
        }
    }
}