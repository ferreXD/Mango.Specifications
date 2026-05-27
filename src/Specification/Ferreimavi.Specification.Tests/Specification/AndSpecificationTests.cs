// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class AndSpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturnOnlyEntitiesSatisfyingCriteria()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John Doe", true),
                new Customer("Jane Doe", true),
                new Customer("John Connor", true)
            };

            // A basic specification that filters out inactive customers
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");

            var spec = activeCustomerSpecification.AsComposable()
                .And(customerByNameSpecification)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones

            // Assert
            result
                .Count()
                .Should()
                .Be(2);
        }

        [Fact]
        public void AndSpecification_IsSatisfiedBy_ShouldReturnTrue()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");

            var spec = activeCustomerSpecification.AsComposable()
                .And(customerByNameSpecification)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Doe", true));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AndSpecification_IsSatisfiedBy_ShouldReturnFalse()
        {
            // Arrange
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("John");
            var spec = activeCustomerSpecification.AsComposable()
                .And(customerByNameSpecification)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("Jane Doe", true));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AndSpecification_MultiFilter_Evaluate_ShouldReturnOnlyEntitiesMatchingBothSides()
        {
            // Regression: when the left operand carries two Where clauses the AND combiner must
            // reduce them to a single predicate (clause1 AND clause2) before AND-ing with the
            // right side, rather than flattening all predicates at the same level. A flat merge
            // of (isActive, containsJohn, isVip) would produce the same result here by accident;
            // the two-step reduction makes the semantics explicit and consistent with OR.

            // Arrange
            var customers = new[]
            {
                new Customer("John Active", true),
                new Customer("Jane Active", true),
                new Customer("John Inactive", false),
                new Customer("Bob Active", true)
            };

            // Left operand: two Where clauses — isActive AND containsJohn
            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            // Right operand: single Where clause — containsActive (name literal)
            var rightSpec = new CustomerByNameSpecification("Active");

            var spec = leftSpec.AsComposable()
                .And(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers).ToList();

            // Assert — only "John Active" satisfies (isActive AND containsJohn) AND containsActive
            result.Should().HaveCount(1);
            result.Should().Contain(c => c.Name == "John Active");
        }

        [Fact]
        public void AndSpecification_MultiFilter_IsSatisfiedBy_ShouldReturnTrue_WhenBothSidesMatch()
        {
            // Regression: two-step reduction — left side is (isActive AND containsJohn)
            // Arrange
            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            var rightSpec = new CustomerByNameSpecification("Active");

            var spec = leftSpec.AsComposable()
                .And(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.IsSatisfiedBy(new Customer("John Active", true));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AndSpecification_MultiFilter_IsSatisfiedBy_ShouldReturnFalse_WhenLeftSecondClauseFails()
        {
            // Regression: before the two-step reduction fix, the left spec's two clauses could be
            // merged incorrectly, causing a customer that passes only the first Where clause to
            // appear as a match.

            // Arrange
            var leftSpec = new Specification<Customer>();
            leftSpec.Query.Where(c => c.IsActive);
            leftSpec.Query.Where(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));

            var rightSpec = new CustomerByNameSpecification("Active");

            var spec = leftSpec.AsComposable()
                .And(rightSpec)
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act — "Bob Active" is active and name contains "Active", but does NOT contain "John"
            var result = spec.IsSatisfiedBy(new Customer("Bob Active", true));

            // Assert
            result.Should().BeFalse();
        }
    }
}