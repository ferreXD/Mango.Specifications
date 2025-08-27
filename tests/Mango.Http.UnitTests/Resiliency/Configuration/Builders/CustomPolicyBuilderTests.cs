// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using Polly.NoOp;
    using System;

    public class CustomPolicyBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldReturnDefinition_WithDefaultOrderAndFactory()
        {
            // Arrange
            var builder = new CustomPolicyBuilder();

            // Act
            var definition = builder.Build();

            // Assert
            definition.Order.Should().Be(1000);
            // Executing the default factory should throw NotImplementedException
            Action act = () => _ = definition.BuildPolicy(null);
            act.Should().Throw<NotImplementedException>();
        }

        [Fact]
        public void SetPolicyFactory_Null_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new CustomPolicyBuilder();

            // Act
            Action act = () => builder.SetPolicyFactory(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("policyFactory");
        }

        [Fact]
        public void SetPolicyFactory_Valid_ShouldUpdateFactory_AndReturnSelf()
        {
            // Arrange
            var builder = new CustomPolicyBuilder();
            Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> factory = diag => Policy.NoOpAsync<HttpResponseMessage>();

            // Act
            var returned = builder.SetPolicyFactory(factory);
            var definition = returned.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            // Definition should use the provided factory without throwing
            var policy = definition.BuildPolicy(null);
            policy.Should().BeOfType<AsyncNoOpPolicy<HttpResponseMessage>>();
        }

        [Fact]
        public void SetOrder_ShouldUpdateOrder_AndReturnSelf()
        {
            // Arrange
            var builder = new CustomPolicyBuilder();

            // Act
            var returned = builder.SetOrder(1234);
            var definition = returned.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            definition.Order.Should().Be(1234);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Build_WithNegativeOrder_ShouldThrowArgumentOutOfRangeException(int invalid)
        {
            // Arrange
            var builder = new CustomPolicyBuilder().SetOrder(invalid);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithParameterName("_order");
        }
    }
}
