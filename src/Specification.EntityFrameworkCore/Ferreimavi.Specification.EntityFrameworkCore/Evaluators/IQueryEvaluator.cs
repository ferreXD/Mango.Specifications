namespace Mango.Specifications.EntityFrameworkCore
{
    public interface IQueryEvaluator
    {
        bool IsCriteriaEvaluator { get; }
        IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class;
    }
}
