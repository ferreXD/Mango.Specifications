// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    internal class WhereEvaluator : IInMemoryEvaluator
    {
        private WhereEvaluator()
        {
        }

        public static WhereEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator { get; } = true;

        public IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification)
            => specification.WhereExpressions.Aggregate(source, (current, whereExpression) => current.Where(whereExpression.FilterFunc));
    }
}