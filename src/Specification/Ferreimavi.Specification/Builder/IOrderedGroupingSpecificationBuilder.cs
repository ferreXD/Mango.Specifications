// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IOrderedGroupingSpecificationBuilder<T, TKey, TResult> : IGroupingSpecificationBuilder<T, TKey, TResult>
        where T : class
        where TResult : class
    {
        bool IsChainDiscarded { get; set; }
    }

    public interface IOrderedGroupingSpecificationBuilder<T, TKey> : IGroupingSpecificationBuilder<T, TKey>, IOrderedGroupingSpecificationBuilder<T, TKey, T> where T : class;
}