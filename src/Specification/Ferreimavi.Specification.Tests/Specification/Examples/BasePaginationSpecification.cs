namespace Mango.Specifications.Tests.Specification.Examples
{
    internal class BasePaginationSpecification<T> : Specification<T>
    {
        public BasePaginationSpecification(int? skip = null, int? take = null)
        {
            Query.Skip(skip).Take(take);
        }
    }
}