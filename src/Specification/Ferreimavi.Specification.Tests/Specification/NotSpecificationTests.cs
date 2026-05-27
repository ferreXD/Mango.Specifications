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

        [Fact]
        public void EmptySpecification_Not_ShouldReturnFalseForAnyEntity()
        {
            // Arrange
            var emptySpec = new Specification<Customer>();
            var notSpecification = emptySpec.Not();

            // Act
            var result = notSpecification.IsSatisfiedBy(new Customer("John Doe", true));

            // Assert
            result.Should().BeFalse();
        }
        [Fact]
        public void Builder_Not_Evaluate_ShouldExcludeNegatedEntities()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John Doe", true),
                new Customer("Jane Doe", true),
                new Customer("John Connor", true)
            };

            var orderingSpec = new CustomerOrderByNameSpecification();
            var activeSpec = new ActiveCustomerSpecification();

            // active AND NOT(name contains "John")  =>  only Jane Doe
            var spec = orderingSpec.AsComposable()
                .And(activeSpec)
                .Not(new CustomerByNameSpecification("John"))
                .Build();

            // Act
            var result = spec.Evaluate(customers).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(c => c.Name == "Jane Doe");
        }

        [Fact]
        public void Builder_Not_IsSatisfiedBy_ShouldReturnFalse_ForNegatedEntity()
        {
            // Arrange
            var orderingSpec = new CustomerOrderByNameSpecification();
            var activeSpec = new ActiveCustomerSpecification();

            var spec = orderingSpec.AsComposable()
                .And(activeSpec)
                .Not(new CustomerByNameSpecification("John"))
                .Build();

            // Active John matches activeSpec but fails NOT(containsJohn)
            spec.IsSatisfiedBy(new Customer("John Doe", true)).Should().BeFalse();
            // Inactive Jane fails activeSpec
            spec.IsSatisfiedBy(new Customer("Jane Doe", false)).Should().BeFalse();
            // Active Jane passes both
            spec.IsSatisfiedBy(new Customer("Jane Doe", true)).Should().BeTrue();
        }
    }
}