namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Examples.Specifications;
    using Extensions;
    using FluentAssertions;
    using Helpers.Factories;

    public class ProjectableSpecificationReadRepositoryTests
    {
        [Fact]
        public async Task FirstOrDefaultAsync_WithProjectableSpecification_ReturnsProjectedResult()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female);

            // Act
            var result = await repository.FirstOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeAssignableTo<string>();

            result
                .Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public async Task SingleOrDefaultAsync_WithProjectableSpecification_ReturnsProjectedResult()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, 1);

            // Act
            var result = await repository.SingleOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeAssignableTo<string>();

            result
                .Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task SingleOrDefaultAsync_WithProjectableSpecification_ThrowsException()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female);

            // Act
            var action = new Func<Task>(() => repository.SingleOrDefaultAsync(spec));

            // Assert
            await action
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task ListAsync_WithProjectableSpecification_ReturnsProjectedResult(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, count);

            // Act
            var result = await repository.ListAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeAssignableTo<IEnumerable<string>>();

            result
                .Count
                .Should()
                .Be(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task CountAsync_WithProjectableSpecification_ReturnsCount(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, count);

            // Act
            var result = await repository.CountAsync(spec);

            // Assert
            result
                .Should()
                .Be(count);
        }


        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(5, true)]
        public async Task AnyAsync_WithProjectableSpecification_ReturnsExistence(int count, bool expected)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, count);

            // Act
            var result = await repository.AnyAsync(spec);

            // Assert
            result
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task AsAsyncEnumerable_WithProjectableSpecification_ReturnsAsyncEnumerable(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, count);

            // Act
            var result = repository.AsAsyncEnumerable(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            await foreach (var item in result)
            {
                item
                    .Should()
                    .NotBeNullOrWhiteSpace();

                item
                    .Should()
                    .BeAssignableTo<string>();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task WithSpecification_ShouldReturn_CorrectEvaluation(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var spec = new PersonFullNameSpecification(PersonFullNameSpecification.Gender.Female, count);

            // Act
            var results = context.Set<Person>().WithSpecification(spec);

            // Assert
            results
                .Should()
                .NotBeNull();

            results
                .Should()
                .BeAssignableTo<IEnumerable<string>>();

            results
                .Count()
                .Should()
                .Be(count);
        }
    }
}