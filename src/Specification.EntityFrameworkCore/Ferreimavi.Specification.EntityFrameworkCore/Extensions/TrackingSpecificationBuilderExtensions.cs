namespace Mango.Specifications.EntityFrameworkCore
{
    using Mango.Specifications;

    /// <summary>
    /// EF Core–scoped builder extensions for controlling Entity Framework change tracking.
    /// These extensions are intentionally defined in the EF package so they are only visible
    /// to callers that reference <c>Ferreimavi.Specification.EntityFrameworkCore</c>.
    /// </summary>
    public static class TrackingSpecificationBuilderExtensions
    {
        #region Base builder (ISpecificationBuilder<T>)

        /// <summary>Specifies that entities should be tracked by the EF Core change tracker.</summary>
        public static ISpecificationBuilder<T> AsTracking<T>(this ISpecificationBuilder<T> builder)
            => AsTracking(builder, true);

        /// <summary>Conditionally specifies that entities should be tracked by the EF Core change tracker.</summary>
        public static ISpecificationBuilder<T> AsTracking<T>(this ISpecificationBuilder<T> builder, bool condition)
            => SpecificationBuilderExtensions.AsTracking(builder, condition);

        /// <summary>Specifies that entities should not be tracked by the EF Core change tracker.</summary>
        public static ISpecificationBuilder<T> AsNoTracking<T>(this ISpecificationBuilder<T> builder)
            => AsNoTracking(builder, true);

        /// <summary>Conditionally specifies that entities should not be tracked by the EF Core change tracker.</summary>
        public static ISpecificationBuilder<T> AsNoTracking<T>(this ISpecificationBuilder<T> builder, bool condition)
            => SpecificationBuilderExtensions.AsNoTracking(builder, condition);

        #endregion

        #region Projectable builder (ISpecificationBuilder<T, TResult>)

        /// <inheritdoc cref="AsTracking{T}(ISpecificationBuilder{T})"/>
        public static ISpecificationBuilder<T, TResult> AsTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder)
            => AsTracking(builder, true);

        /// <inheritdoc cref="AsTracking{T}(ISpecificationBuilder{T},bool)"/>
        public static ISpecificationBuilder<T, TResult> AsTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder, bool condition)
            => (ISpecificationBuilder<T, TResult>)AsTracking((ISpecificationBuilder<T>)builder, condition);

        /// <inheritdoc cref="AsNoTracking{T}(ISpecificationBuilder{T})"/>
        public static ISpecificationBuilder<T, TResult> AsNoTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder)
            => AsNoTracking(builder, true);

        /// <inheritdoc cref="AsNoTracking{T}(ISpecificationBuilder{T},bool)"/>
        public static ISpecificationBuilder<T, TResult> AsNoTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder, bool condition)
            => (ISpecificationBuilder<T, TResult>)AsNoTracking((ISpecificationBuilder<T>)builder, condition);

        #endregion

        #region Grouping builder (IGroupingSpecificationBuilder<T, TKey, TResult>)

        /// <inheritdoc cref="AsTracking{T}(ISpecificationBuilder{T})"/>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder)
            => AsTracking(builder, true);

        /// <inheritdoc cref="AsTracking{T}(ISpecificationBuilder{T},bool)"/>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, bool condition)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)AsTracking((ISpecificationBuilder<T>)builder, condition);

        /// <inheritdoc cref="AsNoTracking{T}(ISpecificationBuilder{T})"/>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsNoTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder)
            => AsNoTracking(builder, true);

        /// <inheritdoc cref="AsNoTracking{T}(ISpecificationBuilder{T},bool)"/>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsNoTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, bool condition)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)AsNoTracking((ISpecificationBuilder<T>)builder, condition);

        #endregion
    }
}
