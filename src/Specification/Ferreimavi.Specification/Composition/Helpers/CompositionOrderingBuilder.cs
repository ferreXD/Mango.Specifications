// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Provides methods to build ordering expressions by combining specifications.
    /// </summary>
    internal static class CompositionOrderingBuilder
    {
        /// <summary>
        /// Builds a collection of ordering expressions by combining two specifications based on the specified evaluation policy.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="left">The left specification to evaluate.</param>
        /// <param name="right">The right specification to evaluate.</param>
        /// <param name="evaluationType">The policy determining how to combine the ordering expressions.</param>
        /// <returns>A collection of order by expressions based on the evaluation policy.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the evaluation type is not recognized.</exception>
        internal static IEnumerable<OrderByExpressionInfo<T>> Build<T>(ISpecification<T> left, ISpecification<T> right, OrderingEvaluationPolicy evaluationType)
        {
            return evaluationType switch
            {
                OrderingEvaluationPolicy.Left => left.OrderByExpressions,
                OrderingEvaluationPolicy.Right => right.OrderByExpressions,
                OrderingEvaluationPolicy.BothLeftPriority => left.OrderByExpressions.Concat(right.OrderByExpressions),
                OrderingEvaluationPolicy.BothRightPriority => right.OrderByExpressions.Concat(left.OrderByExpressions),
                OrderingEvaluationPolicy.None => Enumerable.Empty<OrderByExpressionInfo<T>>(),
                _ => throw new ArgumentOutOfRangeException(nameof(evaluationType), evaluationType, null)
            };
        }
    }
}