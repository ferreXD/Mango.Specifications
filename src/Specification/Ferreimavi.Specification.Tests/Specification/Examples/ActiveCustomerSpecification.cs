namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class ActiveCustomerSpecification : Specification<Customer>
    {
        public ActiveCustomerSpecification()
        {
            Query.Where(c => c.IsActive);
        }
    }
}