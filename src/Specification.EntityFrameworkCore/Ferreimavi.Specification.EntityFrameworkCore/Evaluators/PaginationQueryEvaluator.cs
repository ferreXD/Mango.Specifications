// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    internal class PaginationQueryEvaluator : IQueryEvaluator
    {
        public static PaginationQueryEvaluator Instance { get; } = new();
        public bool IsCriteriaEvaluator => false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            var skip = specification.Skip ?? 0;
            var take = specification.Take ?? query.Count();

            return query
                .Skip(skip)
                .Take(take);
        }
    }
}