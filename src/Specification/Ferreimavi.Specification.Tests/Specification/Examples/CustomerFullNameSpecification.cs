namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerFullNameSpecification : Specification<Customer, string>
    {
        public CustomerFullNameSpecification()
        {
            Query
                .Select(x => $"{x.Name} {x.Surname}")
                .OrderBy(x => x.IsActive)
                .ThenBy(x => x.Name);
        }
    }
}