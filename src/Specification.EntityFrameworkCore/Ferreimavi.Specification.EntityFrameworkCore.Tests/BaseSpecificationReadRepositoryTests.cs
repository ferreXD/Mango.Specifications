namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Data;
    using Examples.Specifications;
    using Extensions;
    using FluentAssertions;
    using Helpers.Factories;

    public class BaseSpecificationReadRepositoryTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task GetById_ShouldReturn_Person(int id)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context);

            // Act
            var results = await repository.GetByIdAsync(id);

            // Assert
            results
                .Should()
                .NotBeNull();

            results
                .Should()
                .BeOfType<Person>();

            results
                .Should()
                .Match<Person>(x => x.BusinessEntityId == id);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task FirstOrDefaultAsync_ShouldReturn_Female_Employee(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var result = await repository.FirstOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeOfType<Employee>();

            result
                .Should()
                .Match<Employee>(x => x.Gender == "F");
        }

        [Fact]
        public async Task SingleOrDefaultAsync_ShouldReturn_Female_Employee()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(1);

            // Act
            var result = await repository.SingleOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .BeOfType<Employee>();

            result
                .Should()
                .Match<Employee>(x => x.Gender == "F");
        }


        [Fact]
        public async Task SingleOrDefaultAsync_Should_ThrowException()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(2);

            // Act
            var action = new Func<Task>(() => repository.SingleOrDefaultAsync(spec));

            // Assert
            await action
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task ListAsync_ShouldReturn_Female_Employee(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();

            results
                .Count
                .Should()
                .Be(count);

            results
                .Should()
                .AllSatisfy(x => x.Gender.Should().Be("F"));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task CountAsync_ShouldReturn_Employee_Count(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var result = await repository.CountAsync(spec);

            // Assert
            result
                .Should()
                .Be(count);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public async Task AnyAsync_ShouldReturn_Employee_Count(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var result = await repository.AnyAsync(spec);

            // Assert
            result
                .Should()
                .Be(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task AsAsyncEnumerable_WithProjectableSpecification_ReturnsAsyncEnumerable(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIsFemaleSpecification(count);

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
                    .NotBeNull();

                item
                    .Should()
                    .BeAssignableTo<Employee>();
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
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var results = context.Set<Employee>().WithSpecification(spec);

            // Assert
            results
                .Should()
                .NotBeNull();

            results
                .Should()
                .BeAssignableTo<IQueryable<Employee>>();

            results
                .Count()
                .Should()
                .Be(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task ToListAsync_ShouldReturn_CorrectEvaluation(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var results = await context.Set<Employee>().ToListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeNull();

            results
                .Should()
                .BeAssignableTo<IEnumerable<Employee>>();

            results
                .Count()
                .Should()
                .Be(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task ToEnumerableAsync_ShouldReturn_CorrectEvaluation(int count)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var spec = new EmployeeIsFemaleSpecification(count);

            // Act
            var results = await context.Set<Employee>().ToEnumerableAsync(spec);

            // Assert
            results
                .Should()
                .NotBeNull();

            results
                .Should()
                .BeAssignableTo<IEnumerable<Employee>>();

            results
                .Count()
                .Should()
                .Be(count);
        }
    }
}