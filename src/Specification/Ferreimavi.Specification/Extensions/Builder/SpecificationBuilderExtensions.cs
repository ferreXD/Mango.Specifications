// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides extension methods for specification builders to build specifications fluently.
    /// </summary>
    public static class SpecificationBuilderExtensions
    {
        #region Negation Extensions

        /// <summary>
        /// Negates the current specification criteria.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>A specification builder with negated criteria.</returns>
        public static ISpecificationBuilder<T> Not<T>(this ISpecificationBuilder<T> builder)
        {
            var spec = builder.Specification.Not();
            return spec.Query;
        }

        #endregion

        #region Where Extensions

        /// <summary>
        /// Adds a filter to the specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> Where<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, bool>> criteria) => Where(builder, criteria, true);

        /// <summary>
        /// Conditionally adds a filter to the specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <param name="condition">Whether the filter should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> Where<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, bool>> criteria, bool condition)
        {
            if (!condition) return builder;

            var info = new WhereExpressionInfo<T>(criteria);
            builder.Specification.AddWhere(info);

            return builder;
        }

        #endregion

        #region Include Extensions

        /// <summary>
        /// Specifies related entities to include in the query results.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="specificationBuilder">The specification builder.</param>
        /// <param name="includeExpression">The include expression.</param>
        /// <returns>An includable specification builder.</returns>
        public static IIncludableSpecificationBuilder<T, TProperty> Include<T, TProperty>(
            this ISpecificationBuilder<T> specificationBuilder,
            Expression<Func<T, TProperty>> includeExpression)
            where T : class
            => Include(specificationBuilder, includeExpression, true);

        /// <summary>
        /// Conditionally specifies related entities to include in the query results.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="specificationBuilder">The specification builder.</param>
        /// <param name="includeExpression">The include expression.</param>
        /// <param name="condition">Whether the include should be applied.</param>
        /// <returns>An includable specification builder.</returns>
        public static IIncludableSpecificationBuilder<T, TProperty> Include<T, TProperty>(
            this ISpecificationBuilder<T> specificationBuilder,
            Expression<Func<T, TProperty>> includeExpression,
            bool condition)
            where T : class
        {
            if (condition)
            {
                var info = new IncludeExpressionInfo(includeExpression, typeof(T), typeof(TProperty));
                specificationBuilder.Specification.AddInclude(info);
            }

            // Create an includable builder. The second parameter indicates whether the include was applied.
            return new IncludableSpecificationBuilder<T, TProperty>(specificationBuilder.Specification, !condition);
        }

        #endregion

        #region Ordering Extensions

        /// <summary>
        /// Specifies an ascending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T> OrderBy<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, object?>> expression)
            => builder.OrderBy(expression, true);

        /// <summary>
        /// Conditionally specifies an ascending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T> OrderBy<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, object?>> expression, bool condition)
        {
            // If multiple OrderBy calls are made, clear the previous ones.
            // This allows for a single OrderByDescending followed by multiple ThenBy / ThenByDescending calls.
            if (builder.Specification.OrderByExpressions.Any()) builder.Specification.ClearOrdering();

            var orderedSpecificationBuilder = new OrderedSpecificationBuilder<T>(builder.Specification, !condition);
            return orderedSpecificationBuilder.OrderByType(expression, OrderTypeEnum.OrderBy, condition);
        }

        /// <summary>
        /// Specifies a descending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T> OrderByDescending<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, object?>> expression)
            => builder.OrderByDescending(expression, true);

        /// <summary>
        /// Conditionally specifies a descending ordering for the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered specification builder.</returns>
        public static IOrderedSpecificationBuilder<T> OrderByDescending<T>(this ISpecificationBuilder<T> builder, Expression<Func<T, object?>> expression, bool condition)
        {
            // If multiple OrderBy calls are made, clear the previous ones.
            // This allows for a single OrderByDescending followed by multiple ThenBy / ThenByDescending calls.
            if (builder.Specification.OrderByExpressions.Any()) builder.Specification.ClearOrdering();

            var orderedSpecificationBuilder = new OrderedSpecificationBuilder<T>(builder.Specification, !condition);
            return orderedSpecificationBuilder.OrderByType(expression, OrderTypeEnum.OrderByDescending, condition);
        }

        #endregion

        #region Post Processing Extensions

        /// <summary>
        /// Adds a post-processing action to the specification to transform results after query execution.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="postProcessingExpression">The expression defining how to transform the results.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> PostProcessingAction<T>(this ISpecificationBuilder<T> builder, Expression<Func<IEnumerable<T>, IEnumerable<T>>> postProcessingExpression)
            => PostProcessingAction(builder, postProcessingExpression, true);

        /// <summary>
        /// Conditionally adds a post-processing action to the specification to transform results after query execution.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="postProcessingExpression">The expression defining how to transform the results.</param>
        /// <param name="condition">Whether the post-processing action should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> PostProcessingAction<T>(this ISpecificationBuilder<T> builder, Expression<Func<IEnumerable<T>, IEnumerable<T>>> postProcessingExpression, bool condition)
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
        /// <param name="builder">The specification builder.</param>
        /// <param name="count">The number of elements to skip.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> Skip<T>(this ISpecificationBuilder<T> builder, int? count)
        {
            builder.Specification.Skip = count;
            return builder;
        }

        /// <summary>
        /// Specifies the number of elements to return.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> Take<T>(this ISpecificationBuilder<T> builder, int? count)
        {
            builder.Specification.Take = count;
            return builder;
        }

        #endregion

        #region Tracking Extensions

        /// <summary>
        /// Specifies that the entities should be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> AsTracking<T>(this ISpecificationBuilder<T> builder) => AsTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that the entities should be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="condition">Whether tracking should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> AsTracking<T>(this ISpecificationBuilder<T> builder, bool condition)
        {
            if (!condition) return builder;

            builder.Specification.AsNoTracking = false;
            builder.Specification.AsTracking = true;

            return builder;
        }

        /// <summary>
        /// Specifies that the entities should not be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> AsNoTracking<T>(this ISpecificationBuilder<T> builder) => AsNoTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that the entities should not be tracked by the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="builder">The specification builder.</param>
        /// <param name="condition">Whether no-tracking should be applied.</param>
        /// <returns>The same specification builder instance.</returns>
        public static ISpecificationBuilder<T> AsNoTracking<T>(this ISpecificationBuilder<T> builder, bool condition)
        {
            if (!condition) return builder;

            builder.Specification.AsTracking = false;
            builder.Specification.AsNoTracking = true;

            return builder;
        }

        #endregion
    }
}