// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public static class OrderedProjectableSpecificationBuilderExtensions
    {
        public static IOrderedSpecificationBuilder<T, TResult> ThenBy<T, TResult>(
            this IOrderedSpecificationBuilder<T, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression) => ThenBy(orderedBuilder, orderExpression, true);

        public static IOrderedSpecificationBuilder<T, TResult> ThenBy<T, TResult>(
            this IOrderedSpecificationBuilder<T, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition) => (IOrderedSpecificationBuilder<T, TResult>)OrderedSpecificationBuilderExtensions.ThenBy(orderedBuilder, orderExpression, condition);

        public static IOrderedSpecificationBuilder<T, TResult> ThenByDescending<T, TResult>(
            this IOrderedSpecificationBuilder<T, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression) => ThenByDescending(orderedBuilder, orderExpression, true);

        public static IOrderedSpecificationBuilder<T, TResult> ThenByDescending<T, TResult>(
            this IOrderedSpecificationBuilder<T, TResult> orderedBuilder,
            Expression<Func<T, object?>> orderExpression,
            bool condition) => (IOrderedSpecificationBuilder<T, TResult>)OrderedSpecificationBuilderExtensions.ThenByDescending(orderedBuilder, orderExpression, condition);
    }
}