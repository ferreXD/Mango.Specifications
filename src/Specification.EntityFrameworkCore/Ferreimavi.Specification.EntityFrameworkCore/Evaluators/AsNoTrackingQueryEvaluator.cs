// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;

    internal class AsNoTrackingQueryEvaluator : IQueryEvaluator
    {
        public static AsNoTrackingQueryEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator => false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            var shouldNoTrack = specification switch
            {
                Specification<T> concrete => concrete.AsNoTracking,
                IEFSpecification<T> ef    => ef.AsNoTracking,
                _                         => false
            };

            if (!shouldNoTrack) return query;

            return query.AsNoTracking();
        }
    }
}