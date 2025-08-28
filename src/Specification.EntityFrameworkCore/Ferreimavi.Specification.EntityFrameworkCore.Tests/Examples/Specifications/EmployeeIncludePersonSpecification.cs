namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal class EmployeeIncludePersonSpecification : Specification<Employee>
    {
        public EmployeeIncludePersonSpecification()
        {
            Query
                .Include(x => x.BusinessEntity)
                .ThenInclude(x => x.EmailAddresses)
                .Include(x => x.EmployeeDepartmentHistories)
                .OrderBy(x => x.BusinessEntity.FirstName)
                .ThenByDescending(x => x.HireDate);
        }
    }
}