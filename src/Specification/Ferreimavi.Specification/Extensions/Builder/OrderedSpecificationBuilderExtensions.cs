// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public static class OrderedSpecificationBuilderExtensions
    {
        public static IOrderedSpecificationBuilder<T> ThenBy<T>(
            this IOrderedSpecificationBuilder<T> orderedBuilder,
            Expression<Func<T, object?>> orderExpression)
            => orderedBuilder.ThenBy(orderExpression, true);

        public static IOrderedSpecificationBuilder<T> ThenBy<T>(
            this IOrderedSpecificationBuilder<T> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition) => orderedBuilder.OrderByType(orderExpression, OrderTypeEnum.ThenBy, condition);

        public static IOrderedSpecificationBuilder<T> ThenByDescending<T>(
            this IOrderedSpecificationBuilder<T> orderedBuilder,
            Expression<Func<T, object?>> orderExpression)
            => orderedBuilder.ThenByDescending(orderExpression, true);

        public static IOrderedSpecificationBuilder<T> ThenByDescending<T>(
            this IOrderedSpecificationBuilder<T> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition) => orderedBuilder.OrderByType(orderExpression, OrderTypeEnum.ThenByDescending, condition);

        #region Internal Methods

        internal static IOrderedSpecificationBuilder<T> OrderByType<T>(
            this IOrderedSpecificationBuilder<T> builder,
            Expression<Func<T, object?>> orderBy,
            OrderTypeEnum orderByType,
            bool condition)
        {
            if (!condition || builder.IsChainDiscarded)
            {
                if (!builder.IsChainDiscarded) builder.IsChainDiscarded = true;
                return builder;
            }

            var orderByExpressionInfo = new OrderByExpressionInfo<T>(orderBy, orderByType);
            builder.Specification.AddOrderBy(orderByExpressionInfo);

            return builder;
        }

        #endregion
    }
}