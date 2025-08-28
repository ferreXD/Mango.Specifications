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

            // Combine all filter expressions from both specifications
            var expressions = left.WhereExpressions
                .Concat(right.WhereExpressions)
                .ToArray();

            var count = expressions.Length;

            // Handle special cases to avoid unnecessary operations
            if (count == 0) return _ => true; // No filters means everything passes

            if (count == 1) return expressions[0].Filter; // Single filter can be returned directly

            // Aggregate multiple filters using the appropriate combiner
            return expressions
                .Select(x => x.Filter)
                .Aggregate(combiner);
        }
    }
}