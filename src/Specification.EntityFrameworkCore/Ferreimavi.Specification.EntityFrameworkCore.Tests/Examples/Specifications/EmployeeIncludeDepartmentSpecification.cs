namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal class EmployeeIncludeDepartmentSpecification : Specification<Employee>
    {
        public EmployeeIncludeDepartmentSpecification()
        {
            Query
                .Include(x => x.EmployeeDepartmentHistories)
                .ThenInclude(x => x.Department)
                .AsNoTracking();
        }
    }
}