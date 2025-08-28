// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    internal class OrderQueryEvaluator : IQueryEvaluator
    {
        private OrderQueryEvaluator()
        {
        }

        public static OrderQueryEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator => false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class => ApplyOrdering(query, specification.OrderByExpressions.ToList());

        private static IQueryable<T> ApplyOrdering<T>(IQueryable<T> source, IReadOnlyList<OrderByExpressionInfo<T>> expressions)
        {
            // If we have no ordering criteria, just return the original sequence
            if (expressions.Count == 0) return source;

            // We'll hold our resulting ordered sequence in here once we apply the first order
            var firstExpression = expressions[0];
            var ordered = firstExpression.OrderType switch
            {
                OrderTypeEnum.OrderBy => source.OrderBy(firstExpression.KeySelector),
                OrderTypeEnum.OrderByDescending => source.OrderByDescending(firstExpression.KeySelector),
                _ => source.OrderBy(firstExpression.KeySelector)
            };

            // Iterate over each ordering expression in the order they were added
            for (var i = 1; i < expressions.Count; i++)
            {
                var expression = expressions[i];
                ordered = expression.OrderType switch
                {
                    OrderTypeEnum.ThenBy => ordered.ThenBy(expression.KeySelector),
                    OrderTypeEnum.ThenByDescending => ordered.ThenByDescending(expression.KeySelector),
                    _ => ordered.ThenBy(expression.KeySelector)
                };
            }

            // If somehow no valid ordering was found, return original source
            return ordered ?? source;
        }
    }
}