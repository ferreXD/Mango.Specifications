// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class OrderedGroupingSpecificationBuilder<T, TKey>(GroupingSpecification<T, TKey> specification, bool isChainDiscarded)
        : OrderedGroupingSpecificationBuilder<T, TKey, T>(specification, isChainDiscarded), IOrderedGroupingSpecificationBuilder<T, TKey>
        where T : class
    {
        public OrderedGroupingSpecificationBuilder(GroupingSpecification<T, TKey> specification) : this(specification, false)
        {
        }

        public new GroupingSpecification<T, TKey> Specification { get; } = specification;
    }

    public class OrderedGroupingSpecificationBuilder<T, TKey, TResult>(GroupingSpecification<T, TKey, TResult> specification, bool isChainDiscarded)
        : GroupingSpecificationBuilder<T, TKey, TResult>(specification), IOrderedGroupingSpecificationBuilder<T, TKey, TResult>
        where T : class
        where TResult : class
    {
        public OrderedGroupingSpecificationBuilder(GroupingSpecification<T, TKey, TResult> specification) : this(specification, false)
        {
        }

        public new GroupingSpecification<T, TKey, TResult> Specification { get; } = specification;
        public bool IsChainDiscarded { get; set; } = isChainDiscarded;
    }
}