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
            var skip = specification.Skip ?? 0;
            var take = specification.Take ?? query.Count();

            return query
                .Skip(skip)
                .Take(take);
        }
    }
}