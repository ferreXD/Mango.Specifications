namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal class PersonGroupByTypeIncludingEmployeeSpecification : GroupingSpecification<Person, string, Employee>
    {
        public PersonGroupByTypeIncludingEmployeeSpecification(int count)
        {
            Query
                .Where(x => x.Employee != null && x.Employee.Gender == "M")
                .Include(x => x.Employee)
                .GroupBy(x => x.PersonType)!
                .Select(x => x.Employee)
                .Take(count);
        }
    }
}