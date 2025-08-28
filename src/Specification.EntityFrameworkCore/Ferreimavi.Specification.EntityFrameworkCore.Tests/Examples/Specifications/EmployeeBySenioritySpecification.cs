namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal sealed class EmployeeBySenioritySpecification : Specification<Employee>
    {
        public EmployeeBySenioritySpecification(int years, ComparisonType comparisonType = ComparisonType.GreaterThan)
        {
            var date = new DateOnly(DateTime.Now.AddYears(-years).Year, 1, 1);

            switch (comparisonType)
            {
                case ComparisonType.GreaterThan:
                    Query
                        .Where(x => x.HireDate <= date);
                    break;
                case ComparisonType.LessThan:
                    Query
                        .Where(x => x.HireDate >= date);
                    break;
            }
        }

        internal enum ComparisonType
        {
            GreaterThan,
            LessThan
        }
    }
}