// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface ISpecificationValidator
    {
        bool IsValid<T>(T entity, ISpecification<T> specification);
    }
}