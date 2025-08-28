namespace Mango.Specifications.EntityFrameworkCore
{
    public interface IReadRepositoryBase<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

        Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<IGrouping<TKey, TResult>?> FirstOrDefaultAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<T?> SingleOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> SingleOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<IGrouping<TKey, TResult>?> SingleOrDefaultAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<List<IGrouping<TKey, TResult>>> ListAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<int> CountAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default);

        IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification);
        IAsyncEnumerable<TResult> AsAsyncEnumerable<TResult>(ISpecification<T, TResult> specification);
    }
}