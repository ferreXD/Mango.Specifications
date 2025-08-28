namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Dto
{
    internal class EmployeeDto
    {
        internal string FullName { get; set; } = string.Empty;
        internal DateOnly HireDate { get; set; } = DateOnly.MinValue;
    }
}