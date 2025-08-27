// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.DependencyInjection
{
    using Constants;
    using Defaults;
    using FluentAssertions;
    using Mango.Http.Logging;
    using Mango.Http.Resiliency;
    using System;

    public class MangoHttpDefaultExtensionsTests
    {
        [Fact]
        public void WithDefaultResiliency_NullConfigurator_ThrowsArgumentNullException()
        {
            MangoResiliencyPolicyConfigurator cfg = null!;
            Action act = () => cfg.WithDefaultResiliency();
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("configurator");
        }

        [Fact]
        public void WithDefaultResiliency_InvokesDefaultConfiguration_AndReturnsConfigurator()
        {
            var cfg = new MangoResiliencyPolicyConfigurator();
            var defaultCalled = false;
            var original = ResiliencyPolicyDefaults.DefaultConfiguration;
            ResiliencyPolicyDefaults.DefaultConfiguration = c => defaultCalled = true;

            try
            {
                var returned = cfg.WithDefaultResiliency();
                returned.Should().BeSameAs(cfg);
                defaultCalled.Should().BeTrue("the default configuration delegate must be invoked");
            }
            finally
            {
                ResiliencyPolicyDefaults.DefaultConfiguration = original;
            }
        }

        [Fact]
        public void WithDefaultResiliency_WithCustomAction_InvokesBothDelegates()
        {
            var cfg = new MangoResiliencyPolicyConfigurator();
            bool defaultCalled = false, customCalled = false;
            var original = ResiliencyPolicyDefaults.DefaultConfiguration;
            ResiliencyPolicyDefaults.DefaultConfiguration = c => defaultCalled = true;

            try
            {
                var returned = cfg.WithDefaultResiliency(c => customCalled = true);
                returned.Should().BeSameAs(cfg);
                defaultCalled.Should().BeTrue("default configuration must run");
                customCalled.Should().BeTrue("custom action must run");
            }
            finally
            {
                ResiliencyPolicyDefaults.DefaultConfiguration = original;
            }
        }

        [Fact]
        public void WithDefaultLogging_Generic_NullConfigurator_ThrowsArgumentNullException()
        {
            HttpLoggingConfigurator cfg = null!;
            Action act = () => cfg.WithDefaultLogging<DefaultHttpLogger>();
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("configurator");
        }

        [Fact]
        public void WithDefaultLogging_Generic_InvokesDefaultAndUseLogger()
        {
            var configurator = new HttpLoggingConfigurator();

            var returned = configurator.WithDefaultLogging<OpenTelemetryHttpLogger>().Build();
            returned.Should().NotBeNull("the configurator should return a valid HttpLoggingOptions instance");
            returned.Enabled.Should().BeTrue("the default logging should be enabled");
            returned.LoggerType.Should().Be(typeof(OpenTelemetryHttpLogger), "the default logger type should be OpenTelemetryHttpLogger");
        }

        [Fact]
        public void WithDefaultLogging_NonGeneric_CallsGenericWithDefaultLogger()
        {
            var configurator = new HttpLoggingConfigurator();

            var returned = configurator.WithDefaultLogging().Build();
            returned.Should().NotBeNull("the configurator should return a valid HttpLoggingOptions instance");
            returned.Enabled.Should().BeTrue("the default logging should be enabled");
            returned.LoggerType.Should().Be(typeof(DefaultHttpLogger), "the default logger type should be OpenTelemetryHttpLogger");
        }
    }
}
