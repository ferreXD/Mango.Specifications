namespace Mango.Specifications.EntityFrameworkCore.Tests.Helpers.Factories
{
    using Data.Context;
    using Microsoft.EntityFrameworkCore;

    internal static class DbContextFactory
    {
        internal const string ConnectionStringEnvVar = "MANGO_TEST_CONNECTION_STRING";

        public static TestDbContext CreateTestDbContext()
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar)!;

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new TestDbContext(options);
        }
    }
}