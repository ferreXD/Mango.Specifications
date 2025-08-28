// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class SpecificationGroupOperationsTests
    {
        [Fact]
        public void GroupOperation_AndGroupingEvaluation_ShouldReturn_SatisfyingCriteria()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Doe" },
                new Customer("Jane", true) { Surname = "Connor" }
            };

            // A basic specification that filters out inactive customers
            var activeCustomerSpecification = new ActiveCustomerSpecification();
            var customerByNameSpecification = new CustomerByNameSpecification("Jane");
            var orderByNameSpecification = new CustomerOrderByNameSpecification();
            var orderBySurnameSpecification = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpecification)
                .And(orderBySurnameSpecification)
                .OpenGroup(activeCustomerSpecification)
                .OpenGroup(customerByNameSpecification)
                .CloseGroup()
                .CloseGroup() as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.Left)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones

            // Assert
            var expected = string.Join(" | ", customers.Where(x => x.Name.Equals("Jane")).OrderBy(x => x.Name).ThenBy(x => x.Surname).Select(c => c.Name));

            result
                .Count()
                .Should()
                .Be(2);

            var resultStr = string.Join(" | ", result.Select(c => c.Name));
            resultStr
                .Should()
                .Be(expected);
        }

        [Fact]
        public void GroupOperation_OrGroupingEvaluation_ShouldReturn_SatisfyingCriteria()
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
            var orderingSpecification = new CustomerOrderByNameSpecification();

            var spec = new ComposableSpecificationBuilder<Customer>(orderingSpecification)
                .OpenGroup(activeCustomerSpecification)
                .Or(customerByNameSpecification)
                .CloseGroup()
                .ReturnRoot()
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.Left)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers); // We'll want only active ones

            // Assert
            var expected = string.Join(" | ", customers.OrderBy(x => x.Name).Select(c => c.Name));

            result
                .Count()
                .Should()
                .Be(3);

            var resultStr = string.Join(" | ", result.Select(c => c.Name));
            resultStr
                .Should()
                .Be(expected);
        }
    }
}