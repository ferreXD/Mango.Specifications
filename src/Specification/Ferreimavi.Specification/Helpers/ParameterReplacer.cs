namespace Mango.Specifications.Helpers
{
    using System.Linq.Expressions;

    internal class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        // If this parameter is the one we want to replace, return the new one.
        // Otherwise, behave normally.
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == oldParam ? newParam : base.VisitParameter(node);
    }
}