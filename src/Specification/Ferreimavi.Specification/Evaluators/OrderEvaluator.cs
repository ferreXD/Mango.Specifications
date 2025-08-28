// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Evaluator that applies ordering to a sequence based on expressions in a specification.
    /// </summary>
    internal sealed class OrderEvaluator : IInMemoryEvaluator
    {
        private OrderEvaluator()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the OrderEvaluator.
        /// </summary>
        public static OrderEvaluator Instance { get; } = new();

        /// <summary>
        /// Evaluates the ordering expressions in the specification and applies them to the query.
        /// </summary>
        /// <typeparam name="T">The type of entities in the query.</typeparam>
        /// <param name="query">The source query to order.</param>
        /// <param name="specification">The specification containing ordering expressions.</param>
        /// <returns>An ordered sequence based on the specification's ordering expressions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification)
        {
            var orderExpressions = specification.OrderByExpressions;
            return orderExpressions.Count == 0 ? query : ApplyOrdering(query, orderExpressions);
        }

        /// <summary>
        /// Applies ordering to a sequence based on a list of ordering expressions.
        /// </summary>
        /// <typeparam name="T">The type of entities in the source sequence.</typeparam>
        /// <param name="source">The source sequence to order.</param>
        /// <param name="expressions">The ordering expressions to apply.</param>
        /// <returns>An ordered sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<T> ApplyOrdering<T>(IEnumerable<T> source, IReadOnlyCollection<OrderByExpressionInfo<T>> expressions)
        {
            if (expressions.Count == 0) return source;

            // Get the first expression and apply the initial ordering
            var firstExpression = expressions.First();
            var ordered = ApplyFirstOrdering(source, firstExpression);

            // If there's only one expression, we're done
            if (expressions.Count == 1) return ordered;

            // For subsequent expressions (skipping the first), apply ThenBy/ThenByDescending
            return expressions.Skip(1).Aggregate(ordered, ApplyThenOrdering);
        }

        /// <summary>
        /// Applies the first ordering expression to a sequence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IOrderedEnumerable<T> ApplyFirstOrdering<T>(IEnumerable<T> source, OrderByExpressionInfo<T> expression)
        {
            return expression.OrderType switch
            {
                OrderTypeEnum.OrderBy => source.OrderBy(expression.KeySelectorFunc),
                OrderTypeEnum.OrderByDescending => source.OrderByDescending(expression.KeySelectorFunc),
                _ => source.OrderBy(expression.KeySelectorFunc) // Default to OrderBy
            };
        }

        /// <summary>
        /// Applies a subsequent ordering expression to an already ordered sequence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IOrderedEnumerable<T> ApplyThenOrdering<T>(IOrderedEnumerable<T> ordered, OrderByExpressionInfo<T> expression)
        {
            return expression.OrderType switch
            {
                OrderTypeEnum.ThenBy or OrderTypeEnum.OrderBy => ordered.ThenBy(expression.KeySelectorFunc),
                OrderTypeEnum.ThenByDescending or OrderTypeEnum.OrderByDescending => ordered.ThenByDescending(expression.KeySelectorFunc),
                _ => ordered.ThenBy(expression.KeySelectorFunc) // Default to ThenBy
            };
        }
    }
}