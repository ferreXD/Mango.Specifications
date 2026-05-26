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

            var spec = activeCustomerSpecification.AsComposable()
                .Or(customerByNameSpecification)
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

            var spec = activeCustomerSpecification.AsComposable()
                .Or(customerByNameSpecification)
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

            var spec = activeCustomerSpecification.AsComposable()
                .Or(customerByNameSpecification)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Doe", false));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void OrSpecification_MultiFilter_Evaluate_ShouldReturnOnlyMatchingEntities()
        {
            // Regression: before the two-step reduction fix, all customers passed because
            // multi-filter specs were OR'd flat: isActive OR containsJohn OR !isActive OR containsJane,
            // which collapses to true. The correct result is (isActive AND containsJohn) OR (!isActive AND containsJane).

            // Arrange
            var customers = new[]
            {
                new Customer("John Active", true),
                new Customer("Jane Inactive", false),
                new Customer("Bob Active", true),
                new Customer("Jane Active", true)
            };

            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            var rightSpec = new Specification<Customer>();
            rightSpec.Query.Where(c => !c.IsActive);
            rightSpec.Query.Where(c => c.Name.Contains("Jane", StringComparison.OrdinalIgnoreCase));

            var spec = leftSpec.AsComposable()
                .Or(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers).ToList();

            // Assert — only "John Active" (matches left) and "Jane Inactive" (matches right)
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "John Active");
            result.Should().Contain(c => c.Name == "Jane Inactive");
        }

        [Fact]
        public void OrSpecification_MultiFilter_IsSatisfiedBy_ShouldReturnTrue_WhenMatchingLeftSide()
        {
            // Regression: two-step reduction — left side is (isActive AND containsJohn)
            // Arrange
            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            var rightSpec = new Specification<Customer>();
            rightSpec.Query.Where(c => !c.IsActive);
            rightSpec.Query.Where(c => c.Name.Contains("Jane", StringComparison.OrdinalIgnoreCase));

            var spec = leftSpec.AsComposable()
                .Or(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Active", true));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OrSpecification_MultiFilter_IsSatisfiedBy_ShouldReturnFalse_WhenMatchingNeitherSide()
        {
            // Regression: before the fix this returned true because the flat OR was a tautology
            // Arrange
            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            var rightSpec = new Specification<Customer>();
            rightSpec.Query.Where(c => !c.IsActive);
            rightSpec.Query.Where(c => c.Name.Contains("Jane", StringComparison.OrdinalIgnoreCase));

            var spec = leftSpec.AsComposable()
                .Or(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act — "Jane Active" is active but not "John", and active so not matching right side either
            var result = spec.IsSatisfiedBy(new Customer("Jane Active", true));

            // Assert
            result.Should().BeFalse();
        }
    }
}