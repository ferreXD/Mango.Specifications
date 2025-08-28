namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerByNameSpecification : Specification<Customer, string>
    {
        public CustomerByNameSpecification(string name)
        {
            Query.Where(c => c.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));
            Query.Select(x => x.Name);
        }
    }
}