// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Specification that combines two specifications using a logical OR operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal sealed class OrSpecification<T, TResult> : Specification<T, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrSpecification{T, TResult}" /> class.
        /// </summary>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="orderingPolicy">The policy to use for combining ordering expressions.</param>
        /// <param name="paginationPolicy">The policy to use for combining pagination settings.</param>
        /// <param name="projectionPolicy">The policy to use for combining projections.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OrSpecification(
            ISpecification<T, TResult> left,
            ISpecification<T, TResult> right,
            OrderingEvaluationPolicy orderingPolicy = OrderingEvaluationPolicy.BothLeftPriority,
            PaginationEvaluationPolicy paginationPolicy = PaginationEvaluationPolicy.None,
            ProjectionEvaluationPolicy projectionPolicy = ProjectionEvaluationPolicy.Left)
        {
            // 1. Combine filter expressions using logical OR
            var criteria = CompositionHelpers.ComposeCriteria(left, right, ExpressionType.OrElse);
            Query.Where(criteria);

            // 2. Combine ordering
            CompositionHelpers.ComposeOrdering(this, left, right, orderingPolicy);

            // 3. Combine pagination
            CompositionHelpers.ComposePagination(this, left, right, paginationPolicy);

            // 4. Combine projection
            CompositionHelpers.ComposeProjection(this, left, right, projectionPolicy);
        }
    }

    /// <summary>
    /// Specification that combines two specifications using a logical OR operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    internal sealed class OrSpecification<T> : Specification<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrSpecification{T}" /> class.
        /// </summary>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="orderingPolicy">The policy to use for combining ordering expressions.</param>
        /// <param name="paginationPolicy">The policy to use for combining pagination settings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OrSpecification(
            ISpecification<T> left,
            ISpecification<T> right,
            OrderingEvaluationPolicy orderingPolicy = OrderingEvaluationPolicy.BothLeftPriority,
            PaginationEvaluationPolicy paginationPolicy = PaginationEvaluationPolicy.None)
        {
            // 1. Combine filter expressions using logical OR
            var criteria = CompositionHelpers.ComposeCriteria(left, right, ExpressionType.OrElse);
            Query.Where(criteria);

            // 2. Combine ordering
            CompositionHelpers.ComposeOrdering(this, left, right, orderingPolicy);

            // 3. Combine pagination
            CompositionHelpers.ComposePagination(this, left, right, paginationPolicy);
        }
    }
}