namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    using Data;
    using Examples.Specifications;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;

    public class CreateShallowSelectorTests
    {
        /// <summary>
        /// Minimal DbContext backed by the InMemory provider.
        /// Navigation properties on Person are ignored so that the context can be
        /// built without pulling in the full AdventureWorks entity graph.
        /// </summary>
        private sealed class PersonInMemoryContext : DbContext
        {
            public PersonInMemoryContext(DbContextOptions<PersonInMemoryContext> options) : base(options) { }

            public DbSet<Person> People { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>(entity =>
                {
                    entity.HasKey(e => e.BusinessEntityId);
                    entity.Ignore(e => e.BusinessEntity);
                    entity.Ignore(e => e.BusinessEntityContacts);
                    entity.Ignore(e => e.Customers);
                    entity.Ignore(e => e.EmailAddresses);
                    entity.Ignore(e => e.Employee);
                    entity.Ignore(e => e.Password);
                    entity.Ignore(e => e.PersonCreditCards);
                    entity.Ignore(e => e.PersonPhones);
                });
            }
        }

        [Fact]
        public async Task GetQuery_WithIdentityGroupResultSelector_InvokesCreateShallowSelector_AndReturnsGroupedPersons()
        {
            // Arrange – spin up a fresh in-memory database for this test run.
            var options = new DbContextOptionsBuilder<PersonInMemoryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new PersonInMemoryContext(options);

            var seeded = new[]
            {
                new Person { BusinessEntityId = 1, PersonType = "EM", FirstName = "Alice", LastName = "Smith", NameStyle = false, EmailPromotion = 0, Rowguid = Guid.NewGuid(), ModifiedDate = DateTime.UtcNow },
                new Person { BusinessEntityId = 2, PersonType = "EM", FirstName = "Bob",   LastName = "Jones", NameStyle = false, EmailPromotion = 1, Rowguid = Guid.NewGuid(), ModifiedDate = DateTime.UtcNow },
                new Person { BusinessEntityId = 3, PersonType = "IN", FirstName = "Carol", LastName = "White", NameStyle = false, EmailPromotion = 0, Rowguid = Guid.NewGuid(), ModifiedDate = DateTime.UtcNow },
            };

            context.People.AddRange(seeded);
            await context.SaveChangesAsync();

            // GroupByBusinessEntityIdSpecification() sets GroupResultSelector = x => x
            // (via GroupingSpecification<T,TKey> base constructor), which causes
            // SpecificationEvaluator.GetQuery to take the IsIdentitySelector branch
            // and delegate to CreateShallowSelector<Person, Person>().
            var spec = new GroupByBusinessEntityIdSpecification();

            // Act
            var groups = await SpecificationEvaluator.Default.GetQuery(
                context.People.AsQueryable(),
                spec,
                CancellationToken.None);

            // Assert – three distinct BusinessEntityId values → three groups.
            groups.Should().HaveCount(3);

            foreach (var group in groups)
            {
                var expected = seeded.Single(p => p.BusinessEntityId == group.Key);

                // Each group contains exactly one person (all IDs are distinct).
                group.Should().ContainSingle();

                var person = group.Single();

                // CreateShallowSelector must have correctly bound BusinessEntityId.
                person.BusinessEntityId.Should().Be(expected.BusinessEntityId,
                    because: "CreateShallowSelector should bind BusinessEntityId from the source entity");

                // … and at least one other writable scalar property.
                person.FirstName.Should().Be(expected.FirstName,
                    because: "CreateShallowSelector should bind FirstName from the source entity");
            }
        }
    }
}