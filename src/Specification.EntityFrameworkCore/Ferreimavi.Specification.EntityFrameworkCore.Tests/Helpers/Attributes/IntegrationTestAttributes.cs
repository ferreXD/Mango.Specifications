namespace Mango.Specifications.EntityFrameworkCore.Tests.Helpers.Attributes
{
    using Factories;

    /// <summary>
    /// <see cref="FactAttribute"/> that skips the test when
    /// <see cref="DbContextFactory.ConnectionStringEnvVar"/> is not set.
    /// </summary>
    internal sealed class IntegrationFactAttribute : FactAttribute
    {
        public IntegrationFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(DbContextFactory.ConnectionStringEnvVar)))
                Skip = $"Integration test: '{DbContextFactory.ConnectionStringEnvVar}' environment variable is not set.";
        }
    }

    /// <summary>
    /// <see cref="TheoryAttribute"/> that skips the test when
    /// <see cref="DbContextFactory.ConnectionStringEnvVar"/> is not set.
    /// </summary>
    internal sealed class IntegrationTheoryAttribute : TheoryAttribute
    {
        public IntegrationTheoryAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(DbContextFactory.ConnectionStringEnvVar)))
                Skip = $"Integration test: '{DbContextFactory.ConnectionStringEnvVar}' environment variable is not set.";
        }
    }
}
