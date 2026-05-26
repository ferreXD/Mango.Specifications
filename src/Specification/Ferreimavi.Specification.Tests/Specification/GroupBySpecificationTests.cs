namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class GroupBySpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturn_ActiveGroupedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("Jane", false) { Surname = "Connor" },
                new Customer("John", true) { Surname = "Connor" }
            };

            var spec = new CustomerGroupByNameSpecification();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            result
                .Count()
                .Should()
                .Be(2);
        }

        /// <summary>
        /// Regression test: <c>TResult ≠ T</c> must produce correctly projected DTO instances.
        /// This exercises the explicit-selector path of <see cref="GroupingSpecification{T,TKey,TResult}"/>
        /// so that each group element is a <see cref="CustomerActiveSummary"/> rather than a <see cref="Customer"/>.
        /// </summary>
        [Fact]
        public void Evaluate_ShouldProject_ToSummaryDto_WhenResultTypeDiffersFromEntityType()
        {
            // Arrange — TResult (CustomerActiveSummary) ≠ T (Customer)
            var customers = new[]
            {
                new Customer("John", true)  { Surname = "Doe" },
                new Customer("Jane", true)  { Surname = "Doe" },
                new Customer("Bob",  false) { Surname = "Smith" }
            };

            var spec = new CustomerGroupByActiveWithSummarySpecification();

            // Act
            var groups = spec.Evaluate(customers).ToList();

            // Assert — two groups, one per IsActive value
            groups.Should().HaveCount(2);

            var activeGroup = groups.Single(g => g.Key == true);
            activeGroup.Should().HaveCount(2);
            activeGroup.Should().AllSatisfy(s =>
            {
                s.IsActive.Should().BeTrue();
                s.Name.Should().NotBeNullOrEmpty();
            });

            var inactiveGroup = groups.Single(g => g.Key == false);
            inactiveGroup.Should().ContainSingle(s => s.Name == "Bob" && !s.IsActive);
        }
    }
}