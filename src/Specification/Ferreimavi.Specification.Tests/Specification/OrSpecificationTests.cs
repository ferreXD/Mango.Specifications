// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class OrSpecificationTests
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
            var customerByNameSpecification = new CustomerByNameSpecification("Jane");

            var builder = new ComposableSpecificationBuilder<Customer>(activeCustomerSpecification)
                .Or(customerByNameSpecification) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones

            // Assert
            result
                .Count()
                .Should()
                .Be(3);
        }

        [Fact]
        public void OrSpecification_IsSatisfiedBy_ShouldReturnTrue()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");

            var builder = new ComposableSpecificationBuilder<Customer>(activeCustomerSpecification)
                .Or(customerByNameSpecification) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Doe", false));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OrSpecification_IsSatisfiedBy_ShouldReturnFalse()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("Jane");

            var builder = new ComposableSpecificationBuilder<Customer>(activeCustomerSpecification)
                .Or(customerByNameSpecification) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Doe", false));

            // Assert
            result.Should().BeFalse();
        }
    }
}