namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal sealed class EmployeeSelectFullNameSpecification : Specification<Employee, string>
    {
        public EmployeeSelectFullNameSpecification()
        {
            Query
                .Include(x => x.BusinessEntity)
                .ThenInclude(x => x.EmailAddresses)
                .Include(x => x.EmployeeDepartmentHistories)
                .Select(x => $"{x.BusinessEntity.FirstName} {x.BusinessEntity.LastName}");
        }
    }
}