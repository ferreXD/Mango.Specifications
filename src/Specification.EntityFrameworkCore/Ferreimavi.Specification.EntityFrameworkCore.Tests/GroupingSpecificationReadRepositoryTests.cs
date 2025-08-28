namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Examples.Specifications;
    using FluentAssertions;
    using Helpers.Factories;

    public class GroupingSpecificationReadRepositoryTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task FirstOrDefaultAsync_ShouldReturn_GroupedBy_PersonType(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonGroupByTypeIncludingEmployeeSpecification(count);

            // Act
            var result = await repository.FirstOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .AllSatisfy(x => x.Gender.Should().Be("M"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task SingleOrDefaultAsync_ShouldReturn_GroupedBy_BusinessEntity(int id)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new GroupByBusinessEntityIdSpecification(id);

            // Act
            var result = await repository.SingleOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeAssignableTo<IGrouping<int, Person>>();

            result
                .Count()
                .Should()
                .Be(1);

            result
                .Should()
                .AllSatisfy(x => x.BusinessEntityId.Should().Be(id));
        }


        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task SingleOrDefaultAsync_Should_ThrowException(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonGroupByTypeIncludingEmployeeSpecification(count);

            // Act
            var action = new Func<Task>(() => repository.SingleOrDefaultAsync(spec));

            // Assert
            await action
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ListAsync_ShouldReturn_GroupedBy_PersonType(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonGroupByTypeIncludingEmployeeSpecification(count);

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();

            results
                .Count()
                .Should()
                .Be(count);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task CountAsync_ShouldReturn_GroupedBy_BusinessEntity_Count(int id)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new GroupByBusinessEntityIdSpecification(id);

            // Act
            var result = await repository.CountAsync(spec);

            // Assert
            result
                .Should()
                .Be(1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task AnyAsync_ShouldReturn_GroupedBy_BusinessEntity_Existence(int id)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new GroupByBusinessEntityIdSpecification(id);

            // Act
            var result = await repository.AnyAsync(spec);

            // Assert
            result
                .Should()
                .Be(true);
        }
    }
}