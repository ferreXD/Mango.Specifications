// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class PaginationSpecificationTests
    {
        [Fact]
        public void Evaluate_Take_ShouldReturn_Expected()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // A basic specification that filters out inactive customers
            var spec = new BasePaginationSpecification<Customer>(0, 2);

            // Act
            var result = spec.Evaluate(customers).ToList(); // We'll want only active ones

            // Assert
            var expected = string.Join(" | ", customers
                .Skip(0)
                .Take(2)
                .Select(x => x.Name));

            var names = string.Join(" | ", result.Select(x => x.Name));

            names
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_Skip_ShouldReturn_Expected()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // A basic specification that filters out inactive customers
            var spec = new BasePaginationSpecification<Customer>(1);

            // Act
            var result = spec.Evaluate(customers).ToList(); // We'll want only active ones

            // Assert
            var expected = string.Join(" | ", customers
                .Skip(1)
                .Select(x => x.Name));

            var names = string.Join(" | ", result.Select(x => x.Name));

            names
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_Pagination_ShouldReturn_Expected()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // A basic specification that filters out inactive customers
            var spec = new BasePaginationSpecification<Customer>(1, 1);

            // Act
            var result = spec.Evaluate(customers).ToList(); // We'll want only active ones

            // Assert
            var expected = string.Join(" | ", customers
                .Skip(1)
                .Take(1)
                .Select(x => x.Name));

            var names = string.Join(" | ", result.Select(x => x.Name));

            names
                .Should()
                .Be(expected);
        }
        [Fact]
        public void Evaluate_NoPagination_ShouldReturn_AllItems_Unchanged()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // Skip and Take are both null — evaluator must return the original sequence untouched
            var spec = new BasePaginationSpecification<Customer>();

            // Act
            var result = spec.Evaluate(customers).ToList();

            // Assert — identity: all items returned in original order
            result.Should().HaveCount(customers.Length);
            result.Select(x => x.Name).Should().ContainInOrder(customers.Select(x => x.Name));
        }
    }
}