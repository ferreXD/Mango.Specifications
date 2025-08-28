namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal sealed class EmployeeSelectJobTitleSpecification : Specification<Employee, string>
    {
        public EmployeeSelectJobTitleSpecification()
        {
            Query
                .Include(x => x.BusinessEntity)
                .Select(x => $"{x.BusinessEntity.FirstName} {x.BusinessEntity.LastName} - {x.JobTitle}");
        }
    }
}