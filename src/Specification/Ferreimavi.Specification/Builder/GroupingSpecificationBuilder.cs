// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class GroupingSpecificationBuilder<T, TKey, TResult>(GroupingSpecification<T, TKey, TResult> specification) : SpecificationBuilder<T>(specification), IGroupingSpecificationBuilder<T, TKey, TResult>
    {
        public new GroupingSpecification<T, TKey, TResult> Specification { get; } = specification;
    }

    public class GroupingSpecificationBuilder<T, TKey>(GroupingSpecification<T, TKey> specification) : GroupingSpecificationBuilder<T, TKey, T>(specification), IGroupingSpecificationBuilder<T, TKey>
    {
    }
}