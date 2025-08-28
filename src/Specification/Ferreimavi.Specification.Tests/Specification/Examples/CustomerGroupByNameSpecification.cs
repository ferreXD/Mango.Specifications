namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerGroupByNameSpecification : GroupingSpecification<Customer, string>
    {
        public CustomerGroupByNameSpecification()
        {
            Query
                .Where(x => x.IsActive)
                .GroupBy(c => c.Name);
        }
    }
}