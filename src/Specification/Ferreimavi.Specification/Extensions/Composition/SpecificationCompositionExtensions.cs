// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Provides extension methods for composing specifications.
    /// </summary>
    public static class SpecificationCompositionExtensions
    {
        /// <summary>
        /// Creates a new specification that negates the result of the original specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="spec">The specification to negate.</param>
        /// <returns>A new specification that negates the original.</returns>
        public static ISpecification<T> Not<T>(this ISpecification<T> spec) => new NotSpecification<T>(spec);

        /// <summary>
        /// Creates a new projectable specification that negates the result of the original specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="spec">The specification to negate.</param>
        /// <returns>A new specification that negates the original.</returns>
        public static ISpecification<T, TResult> Not<T, TResult>(this ISpecification<T, TResult> spec) => new NotSpecification<T, TResult>(spec);

        /// <summary>
        /// Creates a new grouping specification that negates the result of the original specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key to group by.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="spec">The specification to negate.</param>
        /// <returns>A new specification that negates the original.</returns>
        public static IGroupingSpecification<T, TKey, TResult> Not<T, TKey, TResult>(this IGroupingSpecification<T, TKey, TResult> spec) => new NotSpecification<T, TKey, TResult>(spec);

        /// <summary>
        /// Converts a specification into a composable specification builder to enable fluent composition of specifications.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="specification">The specification to convert into a composable builder.</param>
        /// <returns>A composable specification builder that can be used to chain specifications together.</returns>
        public static IComposableSpecificationBuilder<T> AsComposable<T>(this ISpecification<T> specification)
            => new ComposableSpecificationBuilder<T>((specification as Specification<T>)!);

        /// <summary>
        /// Converts a specification with result projection into a composable specification builder to enable fluent composition of
        /// specifications.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="specification">The specification to convert into a composable builder.</param>
        /// <returns>
        /// A composable specification builder that can be used to chain specifications together while preserving the
        /// result projection.
        /// </returns>
        public static IComposableSpecificationBuilder<T, TResult> AsComposable<T, TResult>(this ISpecification<T, TResult> specification)
            => new ComposableSpecificationBuilder<T, TResult>((specification as Specification<T, TResult>)!);
    }
}