namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerOrderBySurnameSpecification : Specification<Customer>
    {
        public CustomerOrderBySurnameSpecification()
        {
            Query.OrderBy(c => c.Surname);
        }
    }
}