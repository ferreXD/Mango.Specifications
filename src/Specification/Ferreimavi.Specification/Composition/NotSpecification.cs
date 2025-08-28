// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Specification that negates the conditions of another specification.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    internal sealed class NotSpecification<T> : Specification<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotSpecification{T}" /> class.
        /// </summary>
        /// <param name="spec">The specification to negate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NotSpecification(ISpecification<T> spec)
        {
            var criteria = NotSpecificationHelper.ComposeNotCriteria(spec);
            Query.Where(criteria);
        }
    }

    /// <summary>
    /// Specification that negates the conditions of another specification with result projection.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal sealed class NotSpecification<T, TResult> : Specification<T, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotSpecification{T, TResult}" /> class.
        /// </summary>
        /// <param name="spec">The specification to negate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NotSpecification(ISpecification<T, TResult> spec)
        {
            var criteria = NotSpecificationHelper.ComposeNotCriteria(spec);
            Query.Where(criteria);
        }
    }

    /// <summary>
    /// Specification that negates the conditions of another grouping specification.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
    internal sealed class NotSpecification<T, TKey, TResult> : GroupingSpecification<T, TKey, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotSpecification{T, TKey, TResult}" /> class.
        /// </summary>
        /// <param name="spec">The specification to negate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NotSpecification(IGroupingSpecification<T, TKey, TResult> spec)
        {
            var criteria = NotSpecificationHelper.ComposeNotCriteria(spec);
            Query.Where(criteria);
        }
    }
}