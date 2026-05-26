// ReSharper disable once CheckNamespace

namespace Mango.Specifications.Tests
{
    using FluentAssertions;
    using Specification.Models;

    public class AsComposableExtensionTests
    {
        /// <summary>
        /// Minimal hand-rolled ISpecification&lt;T&gt; that does NOT extend Specification&lt;T&gt;.
        /// Used to verify that AsComposable rejects non-concrete implementations.
        /// </summary>
        private sealed class CustomSpecification<T> : ISpecification<T>
        {
            public IReadOnlyCollection<WhereExpressionInfo<T>> WhereExpressions => [];
            public IReadOnlyCollection<OrderByExpressionInfo<T>> OrderByExpressions => [];
            public IReadOnlyCollection<IncludeExpressionInfo> IncludeExpressions => [];
            public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction => null;
            public int? Skip => null;
            public int? Take => null;
            public ISpecificationBuilder<T> Query => throw new NotSupportedException();
            public IEnumerable<T> Evaluate(IEnumerable<T> entities) => entities;
            public bool IsSatisfiedBy(T entity) => true;
        }

        [Fact]
        public void AsComposable_ShouldThrow_WhenSpecificationIsNotConcreteSpecification()
        {
            // Arrange
            ISpecification<Customer> custom = new CustomSpecification<Customer>();

            // Act
            var act = () => custom.AsComposable();

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("specification");
        }

        [Fact]
        public void AsComposable_ShouldSucceed_WhenSpecificationIsConcreteSpecification()
        {
            // Arrange
            ISpecification<Customer> spec = new Specification<Customer>();

            // Act
            var act = () => spec.AsComposable();

            // Assert
            act.Should().NotThrow();
        }
    }
}
