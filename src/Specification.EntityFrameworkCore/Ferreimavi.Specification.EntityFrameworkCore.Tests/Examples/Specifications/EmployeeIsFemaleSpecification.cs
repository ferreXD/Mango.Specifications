namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal class EmployeeIsFemaleSpecification : Specification<Employee>
    {
        public EmployeeIsFemaleSpecification(int count)
        {
            Query
                .AsTracking()
                .Where(x => x.Gender == "F")
                .OrderByDescending(x => x.HireDate)
                .ThenBy(x => x.JobTitle)
                .Take(count);
        }
    }
}