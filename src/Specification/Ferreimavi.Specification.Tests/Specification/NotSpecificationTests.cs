// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class NotSpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturnOnlyEntitiesSatisfyingCriteria()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John Doe", true),
                new Customer("Jane Doe", false),
                new Customer("John Connor", true)
            };

            // A basic specification that filters out inactive customers
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var notSpecification = activeCustomerSpecification.Not();

            // Act
            var result = notSpecification.Evaluate(customers); // We'll want only active ones

            // Assert
            result
                .Count()
                .Should()
                .Be(1);
        }

        [Fact]
        public void NotSpecification_IsSatisfiedBy_ShouldReturnTrue()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var notSpecification = activeCustomerSpecification.Not();

            // Act
            var result = notSpecification.IsSatisfiedBy(new Customer("John Doe", false));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void NotSpecification_IsSatisfiedBy_ShouldReturnFalse()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var notSpecification = activeCustomerSpecification.Not();

            // Act
            var result = notSpecification.IsSatisfiedBy(new Customer("John Doe", true));

            // Assert
            result.Should().BeFalse();
        }
    }
}