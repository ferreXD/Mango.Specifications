// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides extension methods for grouping specification builders to build specifications fluently.
    /// </summary>
    public static class GroupingSpecificationBuilderExtensions
    {
        #region Selection Extensions

        /// <summary>
        /// Specifies a selector expression to transform each entity into a result for the grouping.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="selector">The selector expression.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> Select<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, TResult>> selector)
        {
            builder.Specification.GroupResultSelector = selector;
            return builder;
        }

        #endregion

        #region Where

        /// <summary>
        /// Adds a filter to the grouping specification with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> Where<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, bool>> criteria)
            => Where(builder, criteria, true);

        /// <summary>
        /// Conditionally adds a filter to the grouping specification with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="criteria">The filter criteria expression.</param>
        /// <param name="condition">Whether the filter should be applied.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> Where<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, bool>> criteria, bool condition)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)SpecificationBuilderExtensions.Where(builder, criteria, condition);

        #endregion

        #region Ordering Extensions

        /// <summary>
        /// Specifies an ascending ordering for the grouping query with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered grouping specification builder.</returns>
        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> OrderBy<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, object?>> expression)
            where T : class
            where TResult : class => OrderBy(builder, expression, true);

        /// <summary>
        /// Conditionally specifies an ascending ordering for the grouping query with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered grouping specification builder.</returns>
        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> OrderBy<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, object?>> expression, bool condition)
            where T : class
            where TResult : class
        {
            // If multiple OrderBy calls are made, clear the previous ones.
            // This allows for a single OrderByDescending followed by multiple ThenBy / ThenByDescending calls.
            if (builder.Specification.OrderByExpressions.Any()) builder.Specification.ClearOrdering();

            var orderedSpecificationBuilder = new OrderedGroupingSpecificationBuilder<T, TKey, TResult>(builder.Specification, !condition);
            return orderedSpecificationBuilder.OrderByType(expression, OrderTypeEnum.OrderBy, condition);
        }

        /// <summary>
        /// Specifies a descending ordering for the grouping query with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <returns>An ordered grouping specification builder.</returns>
        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> OrderByDescending<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, object?>> expression)
            where T : class
            where TResult : class => OrderByDescending(builder, expression, true);

        /// <summary>
        /// Conditionally specifies a descending ordering for the grouping query with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="expression">The ordering expression.</param>
        /// <param name="condition">Whether the ordering should be applied.</param>
        /// <returns>An ordered grouping specification builder.</returns>
        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> OrderByDescending<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, Expression<Func<T, object?>> expression, bool condition)
            where T : class
            where TResult : class
        {
            // If multiple OrderBy calls are made, clear the previous ones.
            // This allows for a single OrderByDescending followed by multiple ThenBy / ThenByDescending calls.
            if (builder.Specification.OrderByExpressions.Any()) builder.Specification.ClearOrdering();

            var orderedSpecificationBuilder = new OrderedGroupingSpecificationBuilder<T, TKey, TResult>(builder.Specification, !condition);
            return orderedSpecificationBuilder.OrderByType(expression, OrderTypeEnum.OrderByDescending, condition);
        }

        #endregion

        #region Include Extensions

        /// <summary>
        /// Includes the specified related entity in the query results using the specified include expression for a grouping with a
        /// result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="includeExpression">An expression specifying the related entity to include.</param>
        /// <returns>An includable grouping specification builder that can be used to chain further includes.</returns>
        public static IIncludableGroupingSpecificationBuilder<T, TKey, TResult, TProperty> Include<T, TKey, TResult, TProperty>(
            this IGroupingSpecificationBuilder<T, TKey, TResult> builder,
            Expression<Func<T, TProperty>> includeExpression)
            where T : class
            where TResult : class? => Include(builder, includeExpression, true);

        /// <summary>
        /// Conditionally includes the specified related entity in the query results using the specified include expression for a
        /// grouping with a result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <typeparam name="TProperty">The type of the property being included.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="includeExpression">An expression specifying the related entity to include.</param>
        /// <param name="condition">Whether the include should be applied.</param>
        /// <returns>An includable grouping specification builder that can be used to chain further includes.</returns>
        public static IIncludableGroupingSpecificationBuilder<T, TKey, TResult, TProperty> Include<T, TKey, TResult, TProperty>(
            this IGroupingSpecificationBuilder<T, TKey, TResult> builder,
            Expression<Func<T, TProperty>> includeExpression,
            bool condition)
            where T : class
            where TResult : class?
        {
            var includableSpecificationBuilder = SpecificationBuilderExtensions.Include(builder, includeExpression, condition);
            return new IncludableGroupingSpecificationBuilder<T, TKey, TResult, TProperty>((includableSpecificationBuilder.Specification as GroupingSpecification<T, TKey, TResult>)!, !condition);
        }

        #endregion

        #region Group By Extensions

        /// <summary>
        /// Specifies the expression to group entities by a key selector with a custom result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="groupBySelector">The expression to select the grouping key.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> GroupBy<T, TKey, TResult>(
            this IGroupingSpecificationBuilder<T, TKey, TResult> builder,
            Expression<Func<T, TKey>> groupBySelector)
            => GroupBy(builder, groupBySelector, true);

        /// <summary>
        /// Conditionally specifies the expression to group entities by a key selector with a custom result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="groupBySelector">The expression to select the grouping key.</param>
        /// <param name="condition">Whether the grouping should be applied.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> GroupBy<T, TKey, TResult>(
            this IGroupingSpecificationBuilder<T, TKey, TResult> builder,
            Expression<Func<T, TKey>> groupBySelector,
            bool condition)
        {
            if (!condition) return builder;

            builder.Specification.GroupBySelector = groupBySelector;
            return builder;
        }

        #endregion

        #region Pagination Extensions

        /// <summary>
        /// Specifies the number of elements to skip before returning the remaining elements with a result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="count">The number of elements to skip.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> Skip<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, int? count)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)SpecificationBuilderExtensions.Skip(builder, count);

        /// <summary>
        /// Specifies the number of elements to return with a result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>The same grouping specification builder instance.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> Take<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, int? count)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)SpecificationBuilderExtensions.Take(builder, count);

        #endregion

        #region Tracking Extensions

        /// <summary>
        /// Specifies that entities should be tracked by the database context with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <returns>The same grouping specification builder instance with tracking enabled.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder) => AsTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that entities should be tracked by the database context with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="condition">Whether the tracking should be applied.</param>
        /// <returns>The same grouping specification builder instance with tracking conditionally enabled.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, bool condition)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)SpecificationBuilderExtensions.AsTracking(builder, condition);

        /// <summary>
        /// Specifies that entities should not be tracked by the database context with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <returns>The same grouping specification builder instance with tracking disabled.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsNoTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder) => AsNoTracking(builder, true);

        /// <summary>
        /// Conditionally specifies that entities should not be tracked by the database context with result type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="builder">The grouping specification builder.</param>
        /// <param name="condition">Whether no-tracking should be applied.</param>
        /// <returns>The same grouping specification builder instance with tracking conditionally disabled.</returns>
        public static IGroupingSpecificationBuilder<T, TKey, TResult> AsNoTracking<T, TKey, TResult>(this IGroupingSpecificationBuilder<T, TKey, TResult> builder, bool condition)
            => (IGroupingSpecificationBuilder<T, TKey, TResult>)SpecificationBuilderExtensions.AsNoTracking(builder, condition);

        #endregion
    }
}