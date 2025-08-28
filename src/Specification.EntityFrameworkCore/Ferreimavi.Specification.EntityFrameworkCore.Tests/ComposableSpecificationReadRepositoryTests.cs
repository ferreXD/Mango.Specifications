namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Data;
    using Examples.Specifications;
    using FluentAssertions;
    using Helpers.Factories;

    public class ComposableSpecificationReadRepositoryTests
    {
        [Fact]
        public async Task ListAsync_Should_Evaluate_ComposedAndSpecification()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var genderSpecification = new EmployeeByGenderSpecification(PersonFullNameSpecification.Gender.Male);
            var maritalStatusSpecification = new EmployeeByMaritalStatusSpecification(EmployeeByMaritalStatusSpecification.MaritalStatus.Single);

            var spec = genderSpecification.AsComposable().And(maritalStatusSpecification).ReturnRoot().Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty()
                .And
                .AllSatisfy(x => x.MaritalStatus.Should().Be("S"))
                .And
                .AllSatisfy(x => x.Gender.Should().Be("M"));
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task ListAsync_Should_Evaluate_ComposedOrSpecification(int seniority)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var genderSpecification = new EmployeeByGenderSpecification(PersonFullNameSpecification.Gender.Female);
            var senioritySpecification = new EmployeeBySenioritySpecification(seniority, EmployeeBySenioritySpecification.ComparisonType.LessThan);

            var spec = genderSpecification.AsComposable().Or(senioritySpecification).ReturnRoot().Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();

            var seniorityDateTime = new DateOnly(DateTime.Now.AddYears(-seniority).Year, 1, 1);

            results
                .Should()
                .OnlyContain(x => x.Gender == "F" || x.HireDate >= seniorityDateTime);
        }


        [Theory]
        [InlineData(5, 15)]
        [InlineData(10, 20)]
        [InlineData(15, 25)]
        public async Task ListAsync_Should_Evaluate_ComposedGroupSpecification(int lowerEnd, int upperEnd)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var senioritySpecificationLessThan = new EmployeeBySenioritySpecification(upperEnd, EmployeeBySenioritySpecification.ComparisonType.LessThan);
            var senioritySpecificationGreaterThan = new EmployeeBySenioritySpecification(lowerEnd);

            var genderSpecification = new EmployeeByGenderSpecification(PersonFullNameSpecification.Gender.Female);

            var spec = genderSpecification
                .AsComposable()
                .OpenGroup(senioritySpecificationLessThan, ChainingType.Or)
                .And(senioritySpecificationGreaterThan)
                .CloseGroup()
                .ReturnRoot()
                .Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();

            var lowerEndSeniorityDateTime = new DateOnly(DateTime.Now.AddYears(-lowerEnd).Year, 1, 1);
            var upperEndSeniorityDateTime = new DateOnly(DateTime.Now.AddYears(-upperEnd).Year, 1, 1);

            results
                .Should()
                .OnlyContain(x => x.Gender == "F" || (x.HireDate <= lowerEndSeniorityDateTime && x.HireDate >= upperEndSeniorityDateTime));
        }

        [Fact]
        public async Task ListAsync_Should_Evaluate_ComposableProjectableSpecification()
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var fullNameSpecification = new EmployeeSelectFullNameSpecification();
            var jobTitleSpecification = new EmployeeSelectJobTitleSpecification();

            var spec = fullNameSpecification
                .AsComposable()
                .And(jobTitleSpecification)
                .ReturnRoot()
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Right)
                .Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();
            results
                .Should()
                .AllSatisfy(x => x.Should().Contain("-"));
        }

        [Theory]
        [InlineData(0, 5, 10, 15)]
        [InlineData(5, 10, 15, 20)]
        [InlineData(10, 15, 20, 25)]
        public async Task ListAsync_Should_Evaluate_ComposableProjectableGroupSpecification(int lowerEndFirst, int upperEndFirst, int lowerEndSecond, int upperEndSecond)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var senioritySpecificationGreaterThan = new EmployeeBySenioritySpecification(lowerEndFirst);
            var senioritySpecificationLessThan = new EmployeeBySenioritySpecification(upperEndFirst, EmployeeBySenioritySpecification.ComparisonType.LessThan);

            var senioritySpecificationGreaterThanSecond = new EmployeeBySenioritySpecification(lowerEndSecond);
            var senioritySpecificationLessThanSecond = new EmployeeBySenioritySpecification(upperEndSecond, EmployeeBySenioritySpecification.ComparisonType.LessThan);

            var selectDtoSpecification = new EmployeeSelectDtoSpecification();
            var jobTitleSpecification = new EmployeeSelectJobTitleSpecification();

            var spec = selectDtoSpecification
                .AsComposable()
                .And(jobTitleSpecification)
                .OpenGroup(senioritySpecificationGreaterThan)
                .And(senioritySpecificationLessThan)
                .OpenGroup(senioritySpecificationGreaterThanSecond, ChainingType.Or)
                .And(senioritySpecificationLessThanSecond)
                .CloseGroup()
                .CloseGroup()
                .ReturnRoot()
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Left)
                .Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            var lowerEndSeniorityDateTime = new DateOnly(DateTime.Now.AddYears(-lowerEndFirst).Year, 1, 1);
            var upperEndSeniorityDateTime = new DateOnly(DateTime.Now.AddYears(-upperEndFirst).Year, 1, 1);

            var lowerEndSeniorityDateTimeSecond = new DateOnly(DateTime.Now.AddYears(-lowerEndSecond).Year, 1, 1);
            var upperEndSeniorityDateTimeSecond = new DateOnly(DateTime.Now.AddYears(-upperEndSecond).Year, 1, 1);

            results
                .Should()
                .NotBeEmpty();

            results
                .Should()
                .AllSatisfy(x => x.FullName.Should().NotContain(" - "));

            results
                .Should()
                .OnlyContain(x =>
                    (x.HireDate >= upperEndSeniorityDateTime && x.HireDate <= lowerEndSeniorityDateTime)
                    || (x.HireDate >= upperEndSeniorityDateTimeSecond && x.HireDate <= lowerEndSeniorityDateTimeSecond)
                );
        }

        [Theory]
        [InlineData(0, 5, 10, 15)]
        [InlineData(5, 10, 15, 20)]
        [InlineData(10, 15, 20, 25)]
        public async Task ListAsync_Should_Evaluate_ComposableProjectableGroupSpecification_RightProjection(int lowerEndFirst, int upperEndFirst, int lowerEndSecond, int upperEndSecond)
        {
            // Arrange
            await using var context = DbContextFactory.CreateTestDbContext();
            var repository = new ReadRepositoryBase<Employee>(context);

            var senioritySpecificationGreaterThan = new EmployeeBySenioritySpecification(lowerEndFirst);
            var senioritySpecificationLessThan = new EmployeeBySenioritySpecification(upperEndFirst, EmployeeBySenioritySpecification.ComparisonType.LessThan);

            var senioritySpecificationGreaterThanSecond = new EmployeeBySenioritySpecification(lowerEndSecond);
            var senioritySpecificationLessThanSecond = new EmployeeBySenioritySpecification(upperEndSecond, EmployeeBySenioritySpecification.ComparisonType.LessThan);

            var fullNameSpecification = new EmployeeSelectFullNameSpecification();
            var jobTitleSpecification = new EmployeeSelectJobTitleSpecification();

            var spec = fullNameSpecification
                .AsComposable()
                .OpenGroup(senioritySpecificationGreaterThan)
                .And(senioritySpecificationLessThan)
                .OpenGroup(senioritySpecificationGreaterThanSecond, ChainingType.Or)
                .And(senioritySpecificationLessThanSecond)
                .CloseGroup()
                .CloseGroup()
                .And(jobTitleSpecification)
                .ReturnRoot()
                .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Right)
                .Build();

            // Act
            var results = await repository.ListAsync(spec);

            // Assert
            results
                .Should()
                .NotBeEmpty();

            results
                .Should()
                .AllSatisfy(x => x.Should().Contain(" - "));
        }
    }
}