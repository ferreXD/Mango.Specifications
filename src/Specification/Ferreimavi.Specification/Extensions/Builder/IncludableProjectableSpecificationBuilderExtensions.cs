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
            var includableBuilder = ((IIncludableSpecificationBuilder<TEntity, TPreviousProperty>)previousBuilder).ThenInclude(thenIncludeExpression, condition);
            return new IncludableSpecificationBuilder<TEntity, TResult, TProperty>((includableBuilder.Specification as Specification<TEntity, TResult>)!, !condition || previousBuilder.IsChainDiscarded);
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
            var includableBuilder = ((IIncludableSpecificationBuilder<TEntity, IEnumerable<TPreviousProperty>>)previousBuilder).ThenInclude(thenIncludeExpression, condition);
            return new IncludableSpecificationBuilder<TEntity, TResult, TProperty>((includableBuilder.Specification as Specification<TEntity, TResult>)!, !condition || previousBuilder.IsChainDiscarded);
        }
    }
}