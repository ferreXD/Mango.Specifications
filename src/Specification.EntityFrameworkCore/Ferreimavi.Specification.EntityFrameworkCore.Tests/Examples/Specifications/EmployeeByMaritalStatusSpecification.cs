namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal sealed class EmployeeByMaritalStatusSpecification : Specification<Employee>
    {
        public EmployeeByMaritalStatusSpecification(MaritalStatus status)
        {
            var statusQuery = status switch
            {
                MaritalStatus.Single => "S",
                MaritalStatus.Married => "M",
                _ => throw new ArgumentOutOfRangeException()
            };

            Query
                .Where(x => x.MaritalStatus == statusQuery);
        }

        internal enum MaritalStatus
        {
            Single,
            Married
        }
    }
}