namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Data;
    using Examples.Specifications;
    using FluentAssertions;
    using Helpers.Factories;
    using Microsoft.EntityFrameworkCore;

    public class IncludeSpecificationTests
    {
        [Fact]
        public async Task Evaluate_ShouldInclude_PersonEntity()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIncludePersonSpecification();

            // Act
            var result = await repository.FirstOrDefaultAsync(spec);
            var expected = await context.Employees
                .Include(x => x.BusinessEntity)
                .OrderBy(x => x.BusinessEntity.FirstName)
                .ThenByDescending(x => x.HireDate)
                .FirstOrDefaultAsync();

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .BusinessEntity
                .Should()
                .NotBeNull();

            result
                .BusinessEntity
                .Should()
                .BeEquivalentTo(expected!.BusinessEntity);
        }

        [Fact]
        public async Task Evaluate_ShouldThenInclude_DepartmentEntity()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Employee>(context, new SpecificationEvaluator());
            var spec = new EmployeeIncludeDepartmentSpecification();

            // Act
            var result = await repository.FirstOrDefaultAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .EmployeeDepartmentHistories
                .Should()
                .NotBeEmpty();

            result
                .EmployeeDepartmentHistories
                .Select(x => x.Department)
                .Should()
                .NotBeEmpty()
                .And
                .AllSatisfy(x => x.Should().NotBeNull());
        }

        [Fact]
        public async Task Evaluate_ShouldInclude_WhenGrouping_EmployeeEntity()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();

            var repository = new ReadRepositoryBase<Person>(context, new SpecificationEvaluator());
            var spec = new PersonGroupByTypeIncludingEmployeeSpecification(50);

            // Act
            var result = await repository.ListAsync(spec);

            // Assert
            result
                .Should()
                .NotBeNull();

            result
                .Should()
                .NotBeEmpty();

            result
                .SelectMany(x => x)
                .Should()
                .NotBeEmpty()
                .And
                .AllSatisfy(x => x.Should().NotBeNull());
        }
    }
}