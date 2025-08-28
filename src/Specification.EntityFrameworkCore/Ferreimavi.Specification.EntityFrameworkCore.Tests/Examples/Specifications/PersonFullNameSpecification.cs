namespace Mango.Specifications.EntityFrameworkCore.Tests.Examples.Specifications
{
    internal sealed class PersonFullNameSpecification : Specification<Person, string>
    {
        public enum Gender
        {
            Male,
            Female
        }

        public PersonFullNameSpecification(Gender gender)
        {
            var filter = gender switch
            {
                Gender.Female => "F",
                Gender.Male => "M",
                _ => throw new ArgumentOutOfRangeException()
            };

            Query
                .Include(x => x.Employee)
                .Where(x => x.Employee != null && x.Employee.Gender == filter)
                .Select(person => $"{person.FirstName} {person.LastName}")
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName);
        }

        public PersonFullNameSpecification(Gender gender, int count) : this(gender)
        {
            Query.Take(count);
        }
    }
}