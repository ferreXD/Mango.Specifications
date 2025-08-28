namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;
    using Dto;

    internal sealed class EmployeeSelectDtoSpecification : Specification<Employee, EmployeeDto>
    {
        public EmployeeSelectDtoSpecification()
        {
            Query
                .Include(x => x.BusinessEntity)
                .Select(x => new EmployeeDto
                {
                    FullName = $"{x.BusinessEntity.FirstName} {x.BusinessEntity.LastName}",
                    HireDate = x.HireDate
                })
                .OrderBy(x => x.BusinessEntity.FirstName)
                .ThenBy(x => x.BusinessEntity.LastName);
        }
    }
}