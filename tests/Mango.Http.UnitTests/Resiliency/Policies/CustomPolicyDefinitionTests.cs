// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class CustomPolicyDefinitionTests
    {
        [Fact]
        public void BuildPolicy_ShouldInvokeProvidedFactory_WithGivenDiagnostics()
        {
            // Arrange
            IResiliencyDiagnostics? capturedDiagnostics = null;
            var dummyPolicy = Policy.NoOpAsync<HttpResponseMessage>();
            Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> factory = diag =>
            {
                capturedDiagnostics = diag;
                return dummyPolicy;
            };
            var def = new CustomPolicyDefinition(order: 42, Policy: factory);
            var diagnostics = new NoOpDiagnostics();

            // Act
            var resultPolicy = def.BuildPolicy(diagnostics);

            // Assert
            resultPolicy.Should().BeSameAs(dummyPolicy);
            capturedDiagnostics.Should().BeSameAs(diagnostics);
        }

        [Fact]
        public void BuildPolicy_WithNullDiagnostics_ShouldPassNullToFactory()
        {
            // Arrange
            IResiliencyDiagnostics? capturedDiagnostics = new NoOpDiagnostics();
            var dummyPolicy = Policy.NoOpAsync<HttpResponseMessage>();
            Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> factory = diag =>
            {
                capturedDiagnostics = diag;
                return dummyPolicy;
            };
            var def = new CustomPolicyDefinition(order: 7, Policy: factory);

            // Act
            var resultPolicy = def.BuildPolicy(null);

            // Assert
            resultPolicy.Should().BeSameAs(dummyPolicy);
            capturedDiagnostics.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithNullPolicyFactory_ShouldNotThrowException()
        {
            // Arrange
            Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>>? factory = null;

            // Act
            Action act = () => _ = new CustomPolicyDefinition(order: 1, Policy: factory!);

            // Assert
            act.Should().NotThrow();
        }
    }
}
