// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IGroupingSpecificationBuilder<T, TKey, TResult> : ISpecificationBuilder<T>
    {
        new GroupingSpecification<T, TKey, TResult> Specification { get; }
    }

    public interface IGroupingSpecificationBuilder<T, TKey> : IGroupingSpecificationBuilder<T, TKey, T>;
}