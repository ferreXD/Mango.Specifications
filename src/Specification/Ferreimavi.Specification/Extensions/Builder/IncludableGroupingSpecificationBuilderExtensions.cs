// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public static class IncludableGroupingSpecificationBuilderExtensions
    {
        #region ThenInclude for IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty>

        public static IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty> ThenInclude<TEntity, TKey, TResult, TPreviousProperty, TProperty>(
            this IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TPreviousProperty> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
            where TResult : class
            => previousBuilder.ThenInclude(thenIncludeExpression, true);

        public static IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty> ThenInclude<TEntity, TKey, TResult, TPreviousProperty, TProperty>(
            this IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TPreviousProperty> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression,
            bool condition)
            where TEntity : class
            where TResult : class
        {
            if (condition && !previousBuilder.IsChainDiscarded)
            {
                var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty), typeof(TPreviousProperty));
                ((List<IncludeExpressionInfo>)previousBuilder.Specification.IncludeExpressions).Add(info);
            }

            var includeBuilder = new IncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty>(previousBuilder.Specification, !condition || previousBuilder.IsChainDiscarded);

            return includeBuilder;
        }

        public static IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty> ThenInclude<TEntity, TKey, TResult, TPreviousProperty, TProperty>(
            this IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, IEnumerable<TPreviousProperty>> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
            where TResult : class
            => previousBuilder.ThenInclude(thenIncludeExpression, true);

        public static IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty> ThenInclude<TEntity, TKey, TResult, TPreviousProperty, TProperty>(
            this IIncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, IEnumerable<TPreviousProperty>> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression,
            bool condition)
            where TEntity : class
            where TResult : class
        {
            if (condition && !previousBuilder.IsChainDiscarded)
            {
                var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty), typeof(IEnumerable<TPreviousProperty>));
                ((List<IncludeExpressionInfo>)previousBuilder.Specification.IncludeExpressions).Add(info);
            }

            var includeBuilder = new IncludableGroupingSpecificationBuilder<TEntity, TKey, TResult, TProperty>(previousBuilder.Specification, !condition || previousBuilder.IsChainDiscarded);
            return includeBuilder;
        }

        #endregion
    }
}