// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class OrderedSpecificationBuilder<T, TResult>(Specification<T, TResult> specification, bool isChainDiscarded) :
        OrderedSpecificationBuilder<T>(specification, isChainDiscarded), IOrderedSpecificationBuilder<T, TResult>
    {
        public OrderedSpecificationBuilder(Specification<T, TResult> specification) : this(specification, false)
        {
        }

        public new Specification<T, TResult> Specification { get; } = specification;
    }

    public class OrderedSpecificationBuilder<T>(Specification<T> specification, bool isChainDiscarded) :
        SpecificationBuilder<T>(specification), IOrderedSpecificationBuilder<T>
    {
        public OrderedSpecificationBuilder(Specification<T> specification) : this(specification, false)
        {
        }

        public new Specification<T> Specification { get; } = specification;
        public bool IsChainDiscarded { get; set; } = isChainDiscarded;
    }
}