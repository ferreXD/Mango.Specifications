// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.DependencyInjection
{
    using Constants;
    using FluentAssertions;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class MangoHttpPresetRegistrationExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddResiliencyPolicyPreset_InvalidName_ThrowsArgumentException(string name)
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddResiliencyPolicyPreset(name!, builder => { });

            // Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("name")
               .WithMessage("Preset name cannot be null or whitespace.*");
        }

        [Fact]
        public void AddResiliencyPolicyPreset_ValidName_RegistersInlinePreset()
        {
            // Arrange
            var services = new ServiceCollection();
            var called = false;
            Action<ResiliencyPolicyOptionsBuilder> builderAction = b => called = true;

            // Act
            var returned = services.AddResiliencyPolicyPreset("MyPreset", builderAction);

            // Assert: returns same collection
            returned.Should().BeSameAs(services);

            // Build provider and resolve preset
            var sp = services.BuildServiceProvider();
            var preset = sp.GetRequiredService<IResiliencyPolicyPreset>();

            preset.Should().BeOfType<InlineResiliencyPolicyPreset>();
            preset.Name.Should().Be("MyPreset");

            // Verify Configure invokes our builderAction
            var optionsBuilder = new ResiliencyPolicyOptionsBuilder();
            preset.Configure(optionsBuilder);
            called.Should().BeTrue("InlineResiliencyPolicyPreset.Configure must invoke the provided builder action");
        }

        [Fact]
        public void AddDefaultMangoHttpPreset_RegistersDefaultResiliencyPreset()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returned = services.AddDefaultMangoHttpPreset();

            // Assert: returns same collection
            returned.Should().BeSameAs(services);

            // Build provider and resolve preset
            var sp = services.BuildServiceProvider();
            var preset = sp.GetRequiredService<IResiliencyPolicyPreset>();

            preset.Should().BeOfType<InlineResiliencyPolicyPreset>();
            preset.Name.Should().Be(ResiliencyPolicyDefaults.DefaultPolicyName);

            // Verify that configuring with the default builder applies the defaults
            var optionsBuilder = new ResiliencyPolicyOptionsBuilder();
            preset.Configure(optionsBuilder);

            // The default builder should set retry count to the default value
            var retryOptions = optionsBuilder.Build().Policies.OfType<RetryPolicyDefinition>().FirstOrDefault();

            retryOptions.Should().NotBeNull("the default builder should configure a retry policy");
            retryOptions.RetryCount.Should().Be(RetryPolicyDefaults.DefaultMaxRetryCount);
            retryOptions.RetryDelay.Should().Be(RetryPolicyDefaults.DefaultDelay);
            retryOptions.UseJitter.Should().Be(RetryPolicyDefaults.DefaultUseJitter);
        }
    }
}
