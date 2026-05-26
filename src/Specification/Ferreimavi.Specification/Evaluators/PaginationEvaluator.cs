// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class PaginationEvaluator : IInMemoryEvaluator
    {
        private PaginationEvaluator()
        {
        }

        public static PaginationEvaluator Instance { get; } = new();

        public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification)
        {
            if (specification.Skip is null && specification.Take is null) return query;

            var skip = specification.Skip ?? 0;
            var take = specification.Take ?? query.Count();

            return query
                .Skip(skip)
                .Take(take);
        }
    }
}