// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using Diagnostics;
    using FluentAssertions;
    using Mango.Http.Resiliency;
    using Polly;
    using System;

    public class CustomPolicyConfiguratorTests
    {
        [Fact]
        public void DefaultPolicyFactory_ShouldBeNonNull_AndThrowNotImplementedException()
        {
            // Arrange
            var config = new CustomPolicyConfigurator();

            // Act
            Action act = () => _ = config.PolicyFactory(null);

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [Fact]
        public void SetPolicyFactory_WithValidFactory_ShouldUpdateFactory_AndReturnSelf()
        {
            // Arrange
            var config = new CustomPolicyConfigurator();
            Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> factory =
                diag => Policy.NoOpAsync<HttpResponseMessage>();

            // Act
            var returned = config.SetPolicyFactory(factory);

            // Assert
            returned.Should().BeSameAs(config);
            config.PolicyFactory.Should().BeSameAs(factory);
        }

        [Fact]
        public void SetPolicyFactory_WithNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var config = new CustomPolicyConfigurator();

            // Act
            Action act = () => config.SetPolicyFactory(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("policyFactory");
        }

        [Fact]
        public void SetOrder_ShouldUpdateOrder_AndReturnSelf()
        {
            // Arrange
            var config = new CustomPolicyConfigurator();

            // Act
            var returned = config.SetOrder(999);

            // Assert
            returned.Should().BeSameAs(config);
            config.Order.Should().Be(999);
        }

        [Fact]
        public void Validate_DefaultFactoryNotNull_ShouldNotThrow()
        {
            // Arrange
            var config = new CustomPolicyConfigurator();

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_AfterSettingFactory_ShouldNotThrow()
        {
            // Arrange
            var config = new CustomPolicyConfigurator()
                .SetPolicyFactory(_ => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.Zero));

            // Act
            Action act = () => config.Validate();

            // Assert
            act.Should().NotThrow();
        }
    }
}
