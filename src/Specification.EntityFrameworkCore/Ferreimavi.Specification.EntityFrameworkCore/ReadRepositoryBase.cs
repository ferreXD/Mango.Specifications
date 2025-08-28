namespace Mango.Specifications.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;

    public class ReadRepositoryBase<T>(DbContext context, ISpecificationEvaluator specificationEvaluator) : IReadRepositoryBase<T> where T : class
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();
        private readonly IQueryable<T> _query = context.Set<T>().AsQueryable();

        public ReadRepositoryBase(DbContext context) : this(context, new SpecificationEvaluator())
        {
        }

        #region Get By Id Async

        public virtual async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull => await _dbSet.FindAsync([id], cancellationToken);

        #endregion


        #region As Async Enumerable

        public virtual IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification) => ApplySpecification(specification).AsAsyncEnumerable();
        public virtual IAsyncEnumerable<TResult> AsAsyncEnumerable<TResult>(ISpecification<T, TResult> specification) => ApplySpecification(specification).AsAsyncEnumerable();

        #endregion


        #region First Or Default Async

        public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        public virtual async Task<IGrouping<TKey, TResult>?> FirstOrDefaultAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => (await ApplySpecification(specification, cancellationToken)).FirstOrDefault();

        #endregion


        #region Single Or Default Async

        public virtual async Task<T?> SingleOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
        public virtual async Task<TResult?> SingleOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
        public virtual async Task<IGrouping<TKey, TResult>?> SingleOrDefaultAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => (await ApplySpecification(specification, cancellationToken)).SingleOrDefault();

        #endregion


        #region List Async

        public virtual async Task<List<T>> ListAsync(CancellationToken cancellationToken = default) => await _query.ToListAsync(cancellationToken);
        public virtual async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).ToListAsync(cancellationToken);
        public virtual async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).ToListAsync(cancellationToken);
        public virtual async Task<List<IGrouping<TKey, TResult>>> ListAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => (await ApplySpecification(specification, cancellationToken)).ToList();

        #endregion


        #region Count Async

        public virtual async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).CountAsync(cancellationToken);
        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default) => await _query.CountAsync(cancellationToken);
        public virtual async Task<int> CountAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => (await ApplySpecification(specification, cancellationToken)).Count();

        #endregion


        #region Any Async

        public virtual async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) => await ApplySpecification(specification).AnyAsync(cancellationToken);
        public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default) => await _query.AnyAsync(cancellationToken);
        public virtual async Task<bool> AnyAsync<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => (await ApplySpecification(specification, cancellationToken)).Any();

        #endregion


        #region Protected Methods

        protected virtual IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false) => specificationEvaluator.GetQuery(_query, specification, evaluateCriteriaOnly);

        protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification) => specificationEvaluator.GetQuery(_query, specification);

        protected virtual async Task<IQueryable<IGrouping<TKey, TResult>>> ApplySpecification<TKey, TResult>(IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken = default) => await specificationEvaluator.GetQuery(_query, specification, cancellationToken);

        #endregion
    }
}