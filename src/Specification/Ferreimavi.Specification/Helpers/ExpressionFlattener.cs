// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Helper class for flattening expressions from multiple specifications.
    /// </summary>
    internal static class ExpressionFlattener
    {
        /// <summary>
        /// Flattens filter expressions from two specifications into a single expression using the specified logical operation.
        /// </summary>
        /// <typeparam name="T">The type of entity the specifications apply to.</typeparam>
        /// <param name="left">The left specification.</param>
        /// <param name="right">The right specification.</param>
        /// <param name="type">The logical operation type (AND/OR).</param>
        /// <returns>A single combined filter expression.</returns>
        /// <exception cref="InvalidDataException">Thrown when an invalid expression type is provided.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Expression<Func<T, bool>> Flatten<T>(ISpecification<T> left, ISpecification<T> right, ExpressionType type)
        {
            // Get the appropriate expression combiner based on the operation type
            Func<Expression<Func<T, bool>>, Expression<Func<T, bool>>, Expression<Func<T, bool>>> combiner = type switch
            {
                ExpressionType.AndAlso => ExpressionCombiner.AndAlso,
                ExpressionType.OrElse => ExpressionCombiner.OrElse,
                _ => throw new InvalidDataException("Invalid expression type")
            };

            // Step 1: AND-reduce each side independently so that multi-filter specs are
            // treated as a single logical unit before the cross-side operator is applied.
            // Without this step, OR would flatten to f1 OR f2 OR f3 OR f4 instead of
            // the correct (f1 AND f2) OR (f3 AND f4).
            var leftReduced = Reduce<T>(left.WhereExpressions.Select(x => x.Filter).ToList());
            var rightReduced = Reduce<T>(right.WhereExpressions.Select(x => x.Filter).ToList());

            // Step 2: combine the two reduced operands with the requested operator
            return (leftReduced, rightReduced) switch
            {
                (null, null) => _ => true,
                ({ } l, null) => l,
                (null, { } r) => r,
                ({ } l, { } r) => combiner(l, r)
            };
        }

        private static Expression<Func<T, bool>>? Reduce<T>(List<Expression<Func<T, bool>>> expressions) =>
            expressions.Count switch
            {
                0 => null,
                1 => expressions[0],
                _ => expressions.Aggregate(ExpressionCombiner.AndAlso)
            };
    }
}