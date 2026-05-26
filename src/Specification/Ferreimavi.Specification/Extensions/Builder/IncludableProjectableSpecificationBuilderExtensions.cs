// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public static class IncludableProjectableSpecificationBuilderExtensions
    {
        public static IIncludableSpecificationBuilder<TEntity, TResult, TProperty> ThenInclude<TEntity, TResult, TPreviousProperty, TProperty>(
            this IIncludableSpecificationBuilder<TEntity, TResult, TPreviousProperty> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
            where TResult : class
            => ThenInclude(previousBuilder, thenIncludeExpression, true);

        public static IIncludableSpecificationBuilder<TEntity, TResult, TProperty> ThenInclude<TEntity, TResult, TPreviousProperty, TProperty>(
            this IIncludableSpecificationBuilder<TEntity, TResult, TPreviousProperty> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression,
            bool condition)
            where TEntity : class
            where TResult : class
        {
            if (condition && !previousBuilder.IsChainDiscarded)
            {
                var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty), typeof(TPreviousProperty));
                previousBuilder.Specification.AddInclude(info);
            }

            return new IncludableSpecificationBuilder<TEntity, TResult, TProperty>(previousBuilder.Specification, !condition || previousBuilder.IsChainDiscarded);
        }

        public static IIncludableSpecificationBuilder<TEntity, TResult, TProperty> ThenInclude<TEntity, TResult, TPreviousProperty, TProperty>(
            this IIncludableSpecificationBuilder<TEntity, TResult, IEnumerable<TPreviousProperty>> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
            where TResult : class
            => ThenInclude(previousBuilder, thenIncludeExpression, true);

        public static IIncludableSpecificationBuilder<TEntity, TResult, TProperty> ThenInclude<TEntity, TResult, TPreviousProperty, TProperty>(
            this IIncludableSpecificationBuilder<TEntity, TResult, IEnumerable<TPreviousProperty>> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression,
            bool condition)
            where TEntity : class
            where TResult : class
        {
            if (condition && !previousBuilder.IsChainDiscarded)
            {
                var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty), typeof(IEnumerable<TPreviousProperty>));
                previousBuilder.Specification.AddInclude(info);
            }

            return new IncludableSpecificationBuilder<TEntity, TResult, TProperty>(previousBuilder.Specification, !condition || previousBuilder.IsChainDiscarded);
        }
    }
}