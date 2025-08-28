// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class IncludableGroupingSpecificationBuilder<T, TKey, TResult, TProperty>(GroupingSpecification<T, TKey, TResult> specification, bool isChainDiscarded) :
        IncludableSpecificationBuilder<T, TProperty>(specification, isChainDiscarded), IIncludableGroupingSpecificationBuilder<T, TKey, TResult, TProperty>
        where T : class
        where TResult : class?
    {
        public IncludableGroupingSpecificationBuilder(GroupingSpecification<T, TKey, TResult> specification)
            : this(specification, false)
        {
        }

        public new GroupingSpecification<T, TKey, TResult> Specification { get; } = specification;
    }

    public class IncludableGroupingSpecificationBuilder<T, TKey, TProperty>(GroupingSpecification<T, TKey, T> specification, bool isChainDiscarded) :
        IncludableGroupingSpecificationBuilder<T, TKey, T, TProperty>(specification, isChainDiscarded),
        IIncludableGroupingSpecificationBuilder<T, TKey, TProperty>
        where T : class
    {
        public IncludableGroupingSpecificationBuilder(GroupingSpecification<T, TKey, T> specification)
            : this(specification, false)
        {
        }
    }

    public class IncludableSpecificationBuilder<T, TResult, TProperty>(Specification<T, TResult> specification, bool isChainDiscarded) :
        IncludableSpecificationBuilder<T, TProperty>(specification, isChainDiscarded), IIncludableSpecificationBuilder<T, TResult, TProperty>
        where T : class
        where TResult : class
    {
        public IncludableSpecificationBuilder(Specification<T, TResult> specification)
            : this(specification, false)
        {
        }

        public new Specification<T, TResult> Specification { get; } = specification;
    }

    public class IncludableSpecificationBuilder<T, TProperty>(Specification<T> specification, bool isChainDiscarded) : IIncludableSpecificationBuilder<T, TProperty>
        where T : class
    {
        public IncludableSpecificationBuilder(Specification<T> specification)
            : this(specification, false)
        {
        }

        public Specification<T> Specification { get; } = specification;
        public bool IsChainDiscarded { get; set; } = isChainDiscarded;
    }
}