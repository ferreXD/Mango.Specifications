// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IIncludableGroupingSpecificationBuilder<T, TKey, TResult, out TProperty> : IGroupingSpecificationBuilder<T, TKey, TResult>
        where T : class
        where TResult : class?
    {
        bool IsChainDiscarded { get; set; }
    }

    public interface IIncludableGroupingSpecificationBuilder<T, TKey, out TProperty> : IIncludableGroupingSpecificationBuilder<T, TKey, T, TProperty> where T : class;

    public interface IIncludableSpecificationBuilder<T, TResult, out TProperty> : ISpecificationBuilder<T, TResult>
        where T : class
        where TResult : class?
    {
        bool IsChainDiscarded { get; set; }
    }

    public interface IIncludableSpecificationBuilder<T, out TProperty> : ISpecificationBuilder<T> where T : class
    {
        bool IsChainDiscarded { get; set; }
    }
}