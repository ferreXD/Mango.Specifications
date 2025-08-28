// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IOrderedSpecificationBuilder<T, TResult> : ISpecificationBuilder<T, TResult>, IOrderedSpecificationBuilder<T>;

    public interface IOrderedSpecificationBuilder<T> : ISpecificationBuilder<T>
    {
        bool IsChainDiscarded { get; set; }
    }
}