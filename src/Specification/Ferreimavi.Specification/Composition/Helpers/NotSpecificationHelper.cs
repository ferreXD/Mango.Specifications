// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    internal static class NotSpecificationHelper
    {
        internal static Expression<Func<T, bool>> ComposeNotCriteria<T>(ISpecification<T> spec)
        {
            var expressions = spec.WhereExpressions
                .Select(x => ExpressionCombiner.Not(x.Filter))
                .ToList();

            if (expressions.Count == 0)
                return _ => false;

            return expressions.Aggregate(ExpressionCombiner.AndAlso);
        }
    }
}