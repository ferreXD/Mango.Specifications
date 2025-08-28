// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class SpecificationValidator : ISpecificationValidator
    {
        public static ISpecificationValidator Default { get; } = new SpecificationValidator();

        public bool IsValid<T>(T entity, ISpecification<T> specification) => specification.WhereExpressions.All(whereExpression => whereExpression.FilterFunc(entity));
    }
}