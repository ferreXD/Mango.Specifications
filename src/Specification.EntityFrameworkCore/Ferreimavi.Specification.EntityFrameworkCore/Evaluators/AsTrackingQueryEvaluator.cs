// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;

    internal class AsTrackingQueryEvaluator : IQueryEvaluator
    {
        public static AsTrackingQueryEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator => false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            if (!specification.AsTracking) return query;

            return query.AsTracking();
        }
    }
}