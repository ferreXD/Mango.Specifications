namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    internal sealed class GroupByBusinessEntityIdSpecification : GroupingSpecification<Person, int>
    {
        public GroupByBusinessEntityIdSpecification()
        {
            Query
                .GroupBy(x => x.BusinessEntityId);
        }

        public GroupByBusinessEntityIdSpecification(int id)
        {
            Query
                .Include(x => x.Employee)
                .ThenInclude(x => x!.EmployeePayHistories)
                .GroupBy(x => x.BusinessEntityId)
                .Where(x => x.BusinessEntityId == id);
        }
    }
}