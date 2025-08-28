namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerOrderByNameSpecification : Specification<Customer>
    {
        public CustomerOrderByNameSpecification()
        {
            Query.OrderBy(c => c.Name);
        }
    }
}