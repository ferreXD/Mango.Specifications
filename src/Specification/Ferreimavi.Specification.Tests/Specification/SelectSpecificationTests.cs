// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class SelectSpecificationTests
    {
        [Fact]
        public void SelectOperation_Evaluation_ShouldReturn_Model()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new("John", false) { Surname = "Doe" },
                new("John", false) { Surname = "Carpenter" },
                new("Jane", true) { Surname = "Doe" },
                new("Jane", true) { Surname = "Rambo" },
                new("Janice", false) { Surname = "Doe" }
            };

            // A basic specification that filters out inactive customers
            var spec = new CustomerFullNameSpecification();

            // Act
            var result = spec.Evaluate(customers).ToList(); // We'll want only active ones

            // Assert
            var expected = string.Join(", ", customers.OrderBy(x => x.IsActive).ThenBy(x => x.Name).Select(x => $"{x.Name} {x.Surname}"));

            result
                .Count()
                .Should()
                .Be(5);

            result
                .Should()
                .BeAssignableTo<IEnumerable<string>>();

            string.Join(", ", result)
                .Should()
                .BeEquivalentTo(string.Join(", ", expected));
        }

        [Fact]
        public void SelectManyOperation_Evaluation_ShouldReturn_Model()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new("John", true) { Surname = "Doe", Orders = new List<string> { "Order1", "Order2" } },
                new("Jane", true) { Surname = "Doe", Orders = new List<string> { "Order3", "Order4" } }
            };

            // A basic specification that filters out inactive customers
            var spec = new CustomerOrdersSpecification();

            // Act
            var result = spec.Evaluate(customers).ToList(); // We'll want only active ones

            // Assert
            result
                .Count()
                .Should()
                .Be(4);

            result
                .Should()
                .BeAssignableTo<IEnumerable<string>>();
        }
    }
}