// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class ComposableProjectableSpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturn_LeftProjection()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" }
            };

            // A basic specification that filters out inactive customers
            var fullNameSpecification = new CustomerFullNameSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");

            var builder = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
                .And(customerByNameSpecification) as IComposableSpecificationBuilder<Customer, string>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Left)
                .Build();

            // Act
            var result = spec.Evaluate(customers);
            var names = string.Join(" | ", result);

            // Assert
            var expected = "John Doe | John Connor";

            result
                .Count()
                .Should()
                .Be(2);

            names
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_ShouldReturn_RightProjection()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" }
            };

            // A basic specification that filters out inactive customers
            var fullNameSpecification = new CustomerFullNameSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");

            var builder = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
                .And(customerByNameSpecification) as IComposableSpecificationBuilder<Customer, string>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Right)
                .Build();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones
            var names = string.Join(" | ", result);

            // Assert
            var expected = "John | John";

            result
                .Count()
                .Should()
                .Be(2);

            names
                .Should()
                .Be(expected);
        }
    }
}