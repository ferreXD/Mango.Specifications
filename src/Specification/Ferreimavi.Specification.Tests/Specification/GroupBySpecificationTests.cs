namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class GroupBySpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturn_ActiveGroupedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("Jane", false) { Surname = "Connor" },
                new Customer("John", true) { Surname = "Connor" }
            };

            var spec = new CustomerGroupByNameSpecification();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            result
                .Count()
                .Should()
                .Be(2);
        }
    }
}