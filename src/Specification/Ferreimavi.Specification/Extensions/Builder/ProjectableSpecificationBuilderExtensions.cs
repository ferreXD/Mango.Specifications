// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides extension methods for specification builders to build projectable specifications fluently.
    /// </summary>
    public static class ProjectableSpecificationBuilderExtensions
    {
        #region Negation Extensions

        /// <summary>
        /// Negates the current specification criteria.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>A specification builder with negated criteria.</returns>
        public static ISpecificationBuilder<T, TResult> Not<T, TResult>(this ISpecificationBuilder<T, TResult> builder)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.Not(builder);

        #endregion

        #region Selection Extensions

        /// <summary>
        /// Specifies a selector expression to transform each entity into a result.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="selector">The selector expression.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> Select<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, TResult>> selector)
        {
            builder.Specification.Selector = selector;
            return builder;
        }

        /// <summary>
        /// Specifies a selector expression to transform each entity into a collection of results.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="selectorMany">The selector expression that returns a collection.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> SelectMany<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, IEnumerable<TResult>>> selectorMany)
        {
            builder.Specification.SelectorMany = selectorMany;
            return builder;
        }

        #endregion

        #region Where Extensions

        /// <summary>
        /// Adds a filter to the specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> Where<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, bool>> criteria) => Where(builder, criteria, true);

        /// <summary>
        /// Conditionally adds a filter to the specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <param name="condition">Whether the filter should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> Where<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, bool>> criteria, bool condition)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.Where(builder, criteria, condition);

        #endregion

        #region Include Extensions

        /// <summary>
        /// Specifies related entities to include in the query results for projectable specifications.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="specificationBuilder">The specification builder.</param>
        /// <param name="includeExpression">The include expression.</param>
        /// <returns>An includable specification builder.</returns>
        public static IIncludableSpecificationBuilder<T, TResult, TProperty> Include<T, TResult, TProperty>(
            this ISpecificationBuilder<T, TResult> specificationBuilder,
            Expression<Func<T, TProperty>> includeExpression)
            where T : class
            where TResult : class
            => Include(specificationBuilder, includeExpression, true);

        /// <summary>
        /// Conditionally specifies related entities to include in the query results for projectable specifications.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="specificationBuilder">The specification builder.</param>
        /// <param name="includeExpression">The include expression.</param>
        /// <param name="condition">Whether the include should be applied.</param>
        /// <returns>An includable specification builder.</returns>
        public static IIncludableSpecificationBuilder<T, TResult, TProperty> Include<T, TResult, TProperty>(
            this ISpecificationBuilder<T, TResult> specificationBuilder,
            Expression<Func<T, TProperty>> includeExpression,
            bool condition)
            where T : class
            where TResult : class
        {
            var includeSpecificationBuilder = SpecificationBuilderExtensions.Include(specificationBuilder, includeExpression, condition);
            return new IncludableSpecificationBuilder<T, TResult, TProperty>((includeSpecificationBuilder.Specification as Specification<T, TResult>)!, !condition);
        }

        #endregion

        #region Ordering Extensions

        /// <summary>
        /// Specifies an ascending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T, TResult> OrderBy<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, object?>> expression)
            => OrderBy(builder, expression, true);

        /// <summary>
        /// Conditionally specifies an ascending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T, TResult> OrderBy<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, object?>> expression, bool condition)
        {
            var orderedSpecificationBuilder = SpecificationBuilderExtensions.OrderBy(builder, expression, condition);
            return new OrderedSpecificationBuilder<T, TResult>((orderedSpecificationBuilder.Specification as Specification<T, TResult>)!, !condition);
        }

        /// <summary>
        /// Specifies a descending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T, TResult> OrderByDescending<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, object?>> expression)
            => builder.OrderByDescending(expression, true);

        /// <summary>
        /// Conditionally specifies a descending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T, TResult> OrderByDescending<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<T, object?>> expression, bool condition)
        {
            var orderedSpecificationBuilder = SpecificationBuilderExtensions.OrderByDescending(builder, expression, condition);
            return new OrderedSpecificationBuilder<T, TResult>((orderedSpecificationBuilder.Specification as Specification<T, TResult>)!, !condition);
        }

        #endregion

        #region Post Processing Extensions

        /// <summary>
        /// Adds a post-processing action to transform the results after query execution.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="postProcessingExpression">The expression defining how to transform the results.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> PostProcessingAction<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<IEnumerable<TResult>, IEnumerable<TResult>>> postProcessingExpression)
            => PostProcessingAction(builder, postProcessingExpression, true);

        /// <summary>
        /// Conditionally adds a post-processing action to transform the results after query execution.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="postProcessingExpression">The expression defining how to transform the results.</param>
        /// <param name="condition">Whether the post-processing action should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> PostProcessingAction<T, TResult>(this ISpecificationBuilder<T, TResult> builder, Expression<Func<IEnumerable<TResult>, IEnumerable<TResult>>> postProcessingExpression, bool condition)
        {
            if (!condition) return builder;

            builder.Specification.PostProcessingAction = postProcessingExpression.Compile();
            return builder;
        }

        #endregion

        #region Pagination Extensions

        /// <summary>
        /// Specifies the number of elements to skip before returning the remaining elements.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="count">The number of elements to skip.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> Skip<T, TResult>(this ISpecificationBuilder<T, TResult> builder, int? count)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.Skip(builder, count);

        /// <summary>
        /// Specifies the number of elements to return.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> Take<T, TResult>(this ISpecificationBuilder<T, TResult> builder, int? count)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.Take(builder, count);

        #endregion

        #region Tracking Extensions

        /// <summary>
        /// Specifies that the entities should be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> AsTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder) => AsTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that the entities should be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="condition">Whether tracking should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> AsTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder, bool condition)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.AsTracking(builder, condition);

        /// <summary>
        /// Specifies that the entities should not be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> AsNoTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder) => AsNoTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that the entities should not be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="condition">Whether tracking should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T, TResult> AsNoTracking<T, TResult>(this ISpecificationBuilder<T, TResult> builder, bool condition)
            => (ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.AsNoTracking(builder, condition);

        #endregion
    }
}