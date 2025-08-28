namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    internal sealed class CustomerOrdersSpecification : Specification<Customer, string>
    {
        public CustomerOrdersSpecification()
        {
            Query.SelectMany(x => x.Orders);
        }
    }
}