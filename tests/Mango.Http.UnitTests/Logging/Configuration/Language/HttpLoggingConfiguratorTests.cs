// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Microsoft.Extensions.Logging;
    using Moq;

    public class HttpLoggingConfiguratorTests
    {
        [Fact]
        public void FluentMethods_ReturnConfigurator_AndReflectInBuild()
        {
            // Arrange
            var configurator = new HttpLoggingConfigurator();
            Func<HttpRequestMessage, bool> condition = req => req.Method == HttpMethod.Get;

            // Act
            var returned = configurator
                .Enable()
                .When(condition)
                .RequestLevel(LogLevel.Information)
                .SuccessResponseLevel(LogLevel.Warning)
                .ErrorLevel(LogLevel.Error)
                .LogRequestBody()
                .LogResponseBody()
                .MaxBodyLength(123)
                .ExcludeHeader("H1")
                .UseLogger<DefaultHttpLogger>();

            // Assert fluent returns
            returned.Should().BeSameAs(configurator);
            // Build yields expected options
            var opts = configurator.Build();
            opts.Enabled.Should().BeTrue();
            opts.Condition.Should().BeSameAs(condition);
            opts.RequestLevel.Should().Be(LogLevel.Information);
            opts.ResponseSuccessLevel.Should().Be(LogLevel.Warning);
            opts.ErrorLevel.Should().Be(LogLevel.Error);
            opts.LogRequestBody.Should().BeTrue();
            opts.LogResponseBody.Should().BeTrue();
            opts.MaxBodyLength.Should().Be(123);
            opts.ExcludedHeaders.Should().ContainSingle("H1");
            opts.LoggerType.Should().Be(typeof(DefaultHttpLogger));
            opts.UseCustomHandler.Should().BeFalse();
            opts.CustomHandlerType.Should().BeNull();
        }

        [Fact]
        public void Build_EnabledWithoutLoggerOrHandler_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurator = new HttpLoggingConfigurator().Enable();

            // Act
            Action act = () => configurator.Build();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Either a LoggerType or a CustomHandlerType must be configured when logging is enabled.");
        }

        [Fact]
        public void Build_EnabledWithLogger_Succeeds()
        {
            // Arrange
            var configurator = new HttpLoggingConfigurator()
                .Enable()
                .UseLogger<DefaultHttpLogger>();

            // Act
            Action act = () => configurator.Build();

            // Assert
            act.Should().NotThrow();
            configurator.Build().LoggerType.Should().Be(typeof(DefaultHttpLogger));
        }

        [Fact]
        public void Build_EnabledWithCustomHandler_Succeeds()
        {
            // Arrange
            var configurator = new HttpLoggingConfigurator()
                .Enable()
                .UseCustomHandler<CustomHandler>();

            // Act
            Action act = () => configurator.Build();

            // Assert
            act.Should().NotThrow();
            var opts = configurator.Build();
            opts.UseCustomHandler.Should().BeTrue();
            opts.CustomHandlerType.Should().Be(typeof(CustomHandler));
        }

        [Fact]
        public void Inspect_Configurator_WritesLines()
        {
            // Arrange
            var configurator = new HttpLoggingConfigurator()
                .Enable(false)
                .When(req => false)
                .RequestLevel(LogLevel.Debug)
                .SuccessResponseLevel(LogLevel.Information)
                .ErrorLevel(LogLevel.Critical)
                .LogRequestBody()
                .LogResponseBody()
                .MaxBodyLength(50)
                .ExcludeHeader("H1")
                .UseLogger<DefaultHttpLogger>()
                .UseCustomHandler<CustomHandler>();

            var lines = new List<string>();

            // Act
            configurator.Inspect(lines.Add);

            // Assert
            lines.Should().Contain(l => l.Contains("Enabled=False"));
            lines.Should().Contain(l => l.Contains("Condition=Custom"));
            lines.Should().Contain(l => l.Contains("Req=Debug, Res=Information, Err=Critical"));
            lines.Should().Contain(l => l.Contains("Body: Req=True, Res=True"));
            lines.Should().Contain(l => l.Contains("MaxLen=50"));
            lines.Should().Contain(l => l.Contains("ExcludedHeaders=[H1]"));
            lines.Should().Contain(l => l.Contains("LoggerType=DefaultHttpLogger"));
            lines.Should().Contain(l => l.Contains("CustomHandler=True:CustomHandler"));
        }

        [Fact]
        public void Inspect_ConfiguratorWithILogger_InvokesLoggerLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var configurator = new HttpLoggingConfigurator().Enable();

            // Act
            configurator.Inspect(mockLogger.Object);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("[Logging] Enabled=")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }

        private class CustomHandler : DelegatingHandler { }
    }
}
