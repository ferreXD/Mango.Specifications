// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Mango.Http;
    using Mango.Http.Diagnostics;

    /// <summary>
    /// Extension methods for configuring diagnostics and telemetry in Mango HTTP clients.
    /// Use these methods to register custom or default diagnostics implementations for resiliency policies.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithDiagnostics<MyDiagnostics>();
    /// builder.WithDiagnostics(); // Registers default diagnostics
    /// </code>
    /// </example>
    public static class MangoDiagnosticBuilderExtensions
    {
        /// <summary>
        /// Registers a custom diagnostics implementation for Mango HTTP client resiliency policies.
        /// </summary>
        /// <typeparam name="TDiagnostics">The diagnostics implementation type.</typeparam>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <returns>The Mango HTTP client builder for chaining.</returns>
        public static IMangoHttpClientBuilder WithDiagnostics<TDiagnostics>(
            this IMangoHttpClientBuilder builder)
            where TDiagnostics : class, IResiliencyDiagnostics
        {
            builder.Services.TryAddSingleton<IResiliencyDiagnostics, TDiagnostics>();
            return builder;
        }

        /// <summary>
        /// Registers the default diagnostics implementation for Mango HTTP client resiliency policies.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <returns>The Mango HTTP client builder for chaining.</returns>
        public static IMangoHttpClientBuilder WithDiagnostics(
            this IMangoHttpClientBuilder builder)
            => builder.WithDiagnostics<DefaultResiliencyDiagnostics>();
    }
}
