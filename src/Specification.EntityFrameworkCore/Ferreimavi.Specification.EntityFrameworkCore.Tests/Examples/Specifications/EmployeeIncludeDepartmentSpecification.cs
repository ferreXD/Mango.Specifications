namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;
    using Mango.Specifications.EntityFrameworkCore;

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