namespace Mango.Specifications.EntityFrameworkCore.Tests.Helpers.Factories
{
    using Data.Context;
    using Microsoft.EntityFrameworkCore;

    internal static class DbContextFactory
    {
        public static TestDbContext CreateTestDbContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().Options;
            var context = new TestDbContext(options);

            return context;
        }
    }
}