namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    using Data;

    internal sealed class EmployeeByGenderSpecification : Specification<Employee>
    {
        public EmployeeByGenderSpecification(PersonFullNameSpecification.Gender gender)
        {
            var genderQuery = gender switch
            {
                PersonFullNameSpecification.Gender.Male => "M",
                PersonFullNameSpecification.Gender.Female => "F",
                _ => throw new ArgumentOutOfRangeException()
            };

            Query
                .Where(x => x.Gender == genderQuery);
        }
    }
}