// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using Helpers;
    using System.Linq.Expressions;

    internal class ExpressionCombiner
    {
        /// <summary>
        /// Creates a single expression (x => expr1(x) AND expr2(x)).
        /// </summary>
        public static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2) => Combine(expr1, expr2, ExpressionType.AndAlso);

        /// <summary>
        /// Creates a single expression (x => expr1(x) OR expr2(x)).
        /// </summary>
        public static Expression<Func<T, bool>> OrElse<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2) => Combine(expr1, expr2, ExpressionType.OrElse);

        /// <summary>
        /// Creates a single expression (x => NOT expr(x)).
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>(
            Expression<Func<T, bool>> expr)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var exprBody = new ParameterReplacer(expr.Parameters[0], parameter).Visit(expr.Body);
            var body = Expression.Not(exprBody);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static Expression<Func<T, bool>> Combine<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2,
            ExpressionType mergeType)
        {
            // 1. Unify the parameters so EF sees a single parameter "x".
            var param = Expression.Parameter(typeof(T), "x");

            // Replace expr1's parameter with param
            var body1 = new ParameterReplacer(expr1.Parameters[0], param).Visit(expr1.Body);

            // Replace expr2's parameter with param
            var body2 = new ParameterReplacer(expr2.Parameters[0], param).Visit(expr2.Body);

            // 2. Build up the merged expression body
            var body = mergeType switch
            {
                ExpressionType.AndAlso => Expression.AndAlso(body1, body2),
                ExpressionType.OrElse => Expression.OrElse(body1, body2),
                _ => throw new NotSupportedException($"Merge type {mergeType} not supported.")
            };

            // 3. Return a single lambda with that unified parameter
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}