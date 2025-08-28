// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    internal static class NotSpecificationHelper
    {
        internal static Expression<Func<T, bool>> ComposeNotCriteria<T>(ISpecification<T> spec)
        {
            return spec.WhereExpressions
                .Select(x => ExpressionCombiner.Not(x.Filter))
                .Aggregate(ExpressionCombiner.AndAlso);
        }
    }
}