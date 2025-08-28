// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Helper methods for composing specifications.
    /// </summary>
    internal static class CompositionHelpers
    {
        /// <summary>
        /// Combines the filter criteria from two specifications into a single expression.
        /// </summary>
        /// <typeparam name="T">The type of entity the specifications apply to.</typeparam>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="mergeType">The logical operation to use when merging expressions.</param>
        /// <returns>A combined filter expression.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Expression<Func<T, bool>> ComposeCriteria<T>(
            ISpecification<T> left,
            ISpecification<T> right,
            ExpressionType mergeType) => ExpressionFlattener.Flatten(left, right, mergeType);

        /// <summary>
        /// Composes ordering expressions from two specifications into the destination specification.
        /// </summary>
        /// <typeparam name="T">The type of entity the specifications apply to.</typeparam>
        /// <param name="destination">The destination specification to receive the combined ordering.</param>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="policy">The policy to use for combining orderings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ComposeOrdering<T>(
            Specification<T> destination,
            ISpecification<T> left,
            ISpecification<T> right,
            OrderingEvaluationPolicy policy)
        {
            var mergedOrders = CompositionOrderingBuilder.Build(left, right, policy).ToList();
            if (mergedOrders.Any()) ((List<OrderByExpressionInfo<T>>)destination.OrderByExpressions).AddRange(mergedOrders);
        }

        /// <summary>
        /// Composes pagination settings from two specifications into the destination specification.
        /// </summary>
        /// <typeparam name="T">The type of entity the specifications apply to.</typeparam>
        /// <param name="destination">The destination specification to receive the combined pagination.</param>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="policy">The policy to use for combining pagination.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ComposePagination<T>(
            Specification<T> destination,
            ISpecification<T> left,
            ISpecification<T> right,
            PaginationEvaluationPolicy policy) => CompositionPaginationBuilder.Merge(destination, left, right, policy);

        /// <summary>
        /// Composes projection expressions from two specifications into the destination specification.
        /// </summary>
        /// <typeparam name="T">The type of entity the specifications apply to.</typeparam>
        /// <typeparam name="TResult">The result type of the projection.</typeparam>
        /// <param name="destination">The destination specification to receive the combined projection.</param>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="policy">The policy to use for combining projections.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ComposeProjection<T, TResult>(
            Specification<T, TResult> destination,
            ISpecification<T, TResult> left,
            ISpecification<T, TResult> right,
            ProjectionEvaluationPolicy policy) => CompositionProjectionBuilder.Merge(destination, left, right, policy);
    }
}