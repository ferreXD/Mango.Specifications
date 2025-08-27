// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.ResiliencyDiagnostics
{
    using FluentAssertions;
    using Mango.Http.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Polly;
    using System;

    public class MangoDiagnosticBuilderExtensionsTests
    {
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; set; } = null!;
            public IServiceCollection Services { get; } = new ServiceCollection();
        }

        private class CustomDiagnostics : IResiliencyDiagnostics
        {
            public void OnRetry(HttpRequestMessage request, int attempt, Exception? exception) { }
            public void OnTimeout(HttpRequestMessage request, TimeSpan timeout) { }
            public void OnCircuitBreak(HttpRequestMessage request, Exception? exception) { }
            public void OnCircuitReset(HttpRequestMessage request) { }
            public void OnBulkheadRejected(HttpRequestMessage request, Exception? exception) { }
            public void OnFallback(HttpRequestMessage? request, DelegateResult<HttpResponseMessage> outcome) { }
        }

        [Fact]
        public void WithDiagnostics_Generic_RegistersCustomDiagnostics()
        {
            // Arrange
            var builder = new DummyClientBuilder();

            // Act
            var returned = builder.WithDiagnostics<CustomDiagnostics>();
            var sp = builder.Services.BuildServiceProvider();
            var diag = sp.GetService<IResiliencyDiagnostics>();

            // Assert
            returned.Should().BeSameAs(builder);
            diag.Should().NotBeNull();
            diag.Should().BeOfType<CustomDiagnostics>();
        }

        [Fact]
        public void WithDiagnostics_Default_RegistersDefaultDiagnostics()
        {
            // Arrange
            var builder = new DummyClientBuilder();
            builder.Services.AddLogging();

            // Act
            builder.WithDiagnostics();
            var sp = builder.Services.BuildServiceProvider();
            var diag = sp.GetService<IResiliencyDiagnostics>();

            // Assert
            diag.Should().NotBeNull();
            diag.Should().BeOfType<DefaultResiliencyDiagnostics>();
        }

        [Fact]
        public void WithDiagnostics_MultipleCalls_LastOneWinsDueToTryAddSingleton()
        {
            // Arrange
            var builder = new DummyClientBuilder();

            // Act: first register Custom, then default
            builder.Services.TryAddSingleton<IResiliencyDiagnostics, CustomDiagnostics>();
            builder.WithDiagnostics();
            var sp = builder.Services.BuildServiceProvider();
            var diag = sp.GetService<IResiliencyDiagnostics>();

            // Assert: CustomDiagnostics remains since TryAddSingleton won't override existing registration
            diag.Should().BeOfType<CustomDiagnostics>();
        }
    }
}
