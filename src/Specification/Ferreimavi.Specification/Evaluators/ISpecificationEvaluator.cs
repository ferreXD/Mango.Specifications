// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface ISpecificationEvaluator
    {
        Task<IQueryable<IGrouping<TKey, TResult>>> GetQuery<T, TKey, TResult>(IQueryable<T> query, IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken) where T : class;
        IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query, ISpecification<T, TResult> specification) where T : class;
        IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification, bool evaluateCriteriaOnly = false) where T : class;
    }
}