namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using FluentAssertions;
    using Mango.Specifications;

    public class PaginationQueryEvaluatorTests
    {
        private sealed class EmptySpec<T> : Specification<T> where T : class { }

        private sealed class TakeSpec<T> : Specification<T> where T : class
        {
            public TakeSpec(int take) => Query.Take(take);
        }

        [Fact]
        public void GetQuery_WithNoPagination_ReturnsSameQueryableInstance()
        {
            // Arrange
            var query = Enumerable.Empty<string>().AsQueryable();
            var spec = new EmptySpec<string>(); // Skip = null, Take = null

            // Act
            var result = PaginationQueryEvaluator.Instance.GetQuery(query, spec);

            // Assert — guard must short-circuit and return the original instance unchanged
            result.Should().BeSameAs(query);
        }

        [Fact]
        public void GetQuery_WithTakeSet_ReturnsDifferentQueryable()
        {
            // Arrange
            var query = Enumerable.Empty<string>().AsQueryable();
            var spec = new TakeSpec<string>(10); // Take = 10

            // Act
            var result = PaginationQueryEvaluator.Instance.GetQuery(query, spec);

            // Assert — pagination path must produce a new queryable (Skip/Take appended)
            result.Should().NotBeSameAs(query);
        }
    }
}
