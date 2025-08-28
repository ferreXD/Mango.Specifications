// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public static class OrderedGroupingSpecificationBuilderExtensions
    {
        #region Internal Methods

        internal static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> OrderByType<T, TKey, TResult>(
            this IOrderedGroupingSpecificationBuilder<T, TKey, TResult> builder,
            Expression<Func<T, object?>> orderBy,
            OrderTypeEnum orderByType,
            bool condition)
            where T : class
            where TResult : class
        {
            if (!condition || builder.IsChainDiscarded)
            {
                if (!builder.IsChainDiscarded) builder.IsChainDiscarded = true;
                return builder;
            }

            var info = new OrderByExpressionInfo<T>(orderBy, orderByType);
            builder.Specification.AddOrderBy(info);

            return builder;
        }

        #endregion

        #region ThenBy for IOrderedGroupingSpecificationBuilder<T, TKey, TResult>

        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> ThenBy<T, TKey, TResult>(
            this IOrderedGroupingSpecificationBuilder<T, TKey, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression)
            where T : class
            where TResult : class
            => orderedBuilder.ThenBy(orderExpression, true);

        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> ThenBy<T, TKey, TResult>(
            this IOrderedGroupingSpecificationBuilder<T, TKey, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition)
            where T : class
            where TResult : class
            => orderedBuilder.OrderByType(orderExpression, OrderTypeEnum.ThenBy, condition);

        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> ThenByDescending<T, TKey, TResult>(
            this IOrderedGroupingSpecificationBuilder<T, TKey, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression)
            where T : class
            where TResult : class
            => orderedBuilder.ThenByDescending(orderExpression, true);

        public static IOrderedGroupingSpecificationBuilder<T, TKey, TResult> ThenByDescending<T, TKey, TResult>(
            this IOrderedGroupingSpecificationBuilder<T, TKey, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition)
            where T : class
            where TResult : class
            => orderedBuilder.OrderByType(orderExpression, OrderTypeEnum.ThenByDescending, condition);

        #endregion
    }
}