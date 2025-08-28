// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Examples;
    using Specification.Models;

    public class OrderBySpecificationTests
    {
        [Fact]
        public void Evaluate_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", true),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            // A basic specification that orders customers by name
            var spec = new CustomerOrderByNameSpecification();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => c.Name));
            var expected = "Pepa | Pepe | Pepo";

            result
                .First()
                .Name
                .Should()
                .Be("Pepa");

            result
                .Last()
                .Name
                .Should()
                .Be("Pepo");

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("Pepe", false),
                new Customer("Pepa", true),
                new Customer("Pepo", true)
            };

            var orderBySpec = new CustomerOrderByNameSpecification();
            var activeSpec = new ActiveCustomerSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderBySpec)
                .And(activeSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => c.Name));
            var expected = "Pepa | Pepo";

            result
                .Count()
                .Should()
                .Be(2);

            result
                .First()
                .Name
                .Should()
                .Be("Pepa");

            result
                .Last()
                .Name
                .Should()
                .Be("Pepo");

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_BothLeftPriorityOrderings_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", false) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" },
                new Customer("Jane", true) { Surname = "Doe" }
            };

            var orderByNameSpec = new CustomerOrderByNameSpecification();
            var orderBySurnameSpec = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpec)
                .And(orderBySurnameSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => $"{c.Name} {c.Surname}"));
            var expected = "Jane Doe | John Connor | John Doe";

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_BothRightPriorityOrderings_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", false) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" },
                new Customer("Jane", true) { Surname = "Doe" }
            };

            var orderByNameSpec = new CustomerOrderByNameSpecification();
            var orderBySurnameSpec = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpec)
                .And(orderBySurnameSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothRightPriority)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => $"{c.Name} {c.Surname}"));
            var expected = "John Connor | Jane Doe | John Doe";

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_LeftOrderings_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", false) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" },
                new Customer("Jane", true) { Surname = "Doe" }
            };

            var orderByNameSpec = new CustomerOrderByNameSpecification();
            var orderBySurnameSpec = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpec)
                .And(orderBySurnameSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.Left)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => $"{c.Name} {c.Surname}"));
            var expected = "Jane Doe | John Doe | John Connor";

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_RightOrderings_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", false) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" },
                new Customer("Jane", true) { Surname = "Doe" }
            };

            var orderByNameSpec = new CustomerOrderByNameSpecification();
            var orderBySurnameSpec = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpec)
                .And(orderBySurnameSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.Right)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => $"{c.Name} {c.Surname}"));
            var expected = "John Connor | John Doe | Jane Doe";

            joined
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Evaluate_CompositeSpecification_NoneOrderings_ShouldReturnEntitiesOrderedByName()
        {
            // Arrange
            var customers = new[]
            {
                new Customer("John", false) { Surname = "Doe" },
                new Customer("John", true) { Surname = "Connor" },
                new Customer("Jane", true) { Surname = "Doe" }
            };

            var orderByNameSpec = new CustomerOrderByNameSpecification();
            var orderBySurnameSpec = new CustomerOrderBySurnameSpecification();

            var builder = new ComposableSpecificationBuilder<Customer>(orderByNameSpec)
                .And(orderBySurnameSpec) as IComposableSpecificationBuilder<Customer>;

            var spec = builder!
                .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
                .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
                .Build();

            // Act
            var result = spec.Evaluate(customers);

            // Assert
            var joined = string.Join(" | ", result.Select(c => $"{c.Name} {c.Surname}"));
            var expected = "John Doe | John Connor | Jane Doe";

            joined
                .Should()
                .Be(expected);
        }
    }
}