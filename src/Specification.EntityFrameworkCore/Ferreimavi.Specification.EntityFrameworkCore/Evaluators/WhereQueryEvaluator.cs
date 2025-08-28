// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    internal class WhereQueryEvaluator : IQueryEvaluator
    {
        public static WhereQueryEvaluator Instance { get; } = new();
        public bool IsCriteriaEvaluator => true;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class => specification.WhereExpressions.Aggregate(query, (current, whereExpression) => current.Where(whereExpression.Filter));
    }
}