// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class BaseSpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturnOnlyEntitiesSatisfyingCriteria()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // A basic specification that filters out inactive customers
            var spec = new ActiveCustomerSpecification();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones

            // Assert
            result
                .Count()
                .Should()
                .Be(2);
        }

        [Fact]
        public void IsSatisfiedBy_ShouldReturnTrueForActiveCustomer()
        {
            // Arrange
            var customer = new Customer("Pepe", true);
            var spec = new ActiveCustomerSpecification();

            // Act
            var result = spec.IsSatisfiedBy(customer);

            // Assert
            result
                .Should()
                .BeTrue();
        }

        [Fact]
        public void IsSatisfiedBy_ShouldReturnFalseForInactiveCustomer()
        {
            // Arrange
            var customer = new Customer("Pepe", false);
            var spec = new ActiveCustomerSpecification();

            // Act
            var result = spec.IsSatisfiedBy(customer);

            // Assert
            result
                .Should()
                .BeFalse();
        }
    }
}