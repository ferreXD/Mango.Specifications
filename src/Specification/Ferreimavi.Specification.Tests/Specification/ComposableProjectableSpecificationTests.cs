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

            var spec = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
                .And(customerByNameSpecification)
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
        public void GroupPrecedence_OpenGroupWithNonProjectableSpec_ShouldEvaluateGroupAsUnit()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("Bob", false) { Surname = "Smith" }
            };

            var fullNameSpecification = new CustomerFullNameSpecification();
            var activeCustomerSpecification = new ActiveCustomerSpecification();     // Specification<Customer>  — triggers ISpecification<T> overload
            var customerByBobSpecification = new CustomerByNameSpecification("Bob"); // Specification<Customer, string>

            // Expression: fullName AND (active OR name="Bob")
            var spec = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
                .OpenGroup(activeCustomerSpecification)   // ISpecification<T> overload — the fixed overload
                .Or(customerByBobSpecification)
                .CloseGroup()
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Left)
                .Build();

            // Act
            var result = spec.Evaluate(customers).ToList();

            // Assert: John (active), Jane (active), Bob (name match) — all 3 satisfy (active OR name="Bob")
            result
                .Should()
                .HaveCount(3);

            result.Should().Contain("John Doe");
            result.Should().Contain("Jane Doe");
            result.Should().Contain("Bob Smith");
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

            var spec = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
                .And(customerByNameSpecification)
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