// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;

    public class HttpLoggingOptionsBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldNotThrow_AndDefaultValues()
        {
            var builder = new HttpLoggingOptionsBuilder();
            Action act = () => builder.Build();
            act.Should().NotThrow();
            var opts = builder.Build();
            opts.Enabled.Should().BeFalse();
            opts.Condition.Should().BeNull();
            opts.RequestLevel.Should().Be(LogLevel.Information);
            opts.ResponseSuccessLevel.Should().Be(LogLevel.Information);
            opts.ErrorLevel.Should().Be(LogLevel.Error);
            opts.LogRequestBody.Should().BeFalse();
            opts.LogResponseBody.Should().BeFalse();
            opts.MaxBodyLength.Should().Be(2048);
            opts.ExcludedHeaders.Should().BeEmpty();
            opts.LoggerType.Should().BeNull();
            opts.UseCustomHandler.Should().BeFalse();
            opts.CustomHandlerType.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Enable_ShouldSetEnabled(bool flag)
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable(flag);
            if (flag) builder.UseLogger<DefaultHttpLogger>();
            builder.Build().Enabled.Should().Be(flag);
        }

        [Fact]
        public void When_NullPredicate_ShouldThrowArgumentNullException()
        {
            var builder = new HttpLoggingOptionsBuilder();
            Action act = () => builder.When(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("predicate");
        }

        [Fact]
        public void When_ValidPredicate_ShouldSetCondition()
        {
            var builder = new HttpLoggingOptionsBuilder();
            Func<HttpRequestMessage, bool> cond = req => req.Method == HttpMethod.Get;
            builder.When(cond);
            builder.Build().Condition.Should().BeSameAs(cond);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Error)]
        public void WithRequestLevel_ShouldSetRequestLogLevel(LogLevel level)
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.WithRequestLevel(level);
            builder.Build().RequestLevel.Should().Be(level);
        }

        [Theory]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Critical)]
        public void WithResponseLevel_ShouldSetResponseLogLevel(LogLevel level)
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.WithSuccessResponseLevel(level);
            builder.Build().ResponseSuccessLevel.Should().Be(level);
        }

        [Theory]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.None)]
        public void WithErrorLevel_ShouldSetErrorLogLevel(LogLevel level)
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.WithErrorLevel(level);
            builder.Build().ErrorLevel.Should().Be(level);
        }

        [Fact]
        public void LogRequestBody_ShouldToggleFlag()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.LogRequestBody();
            builder.Build().LogRequestBody.Should().BeTrue();
            builder.LogRequestBody(false);
            builder.Build().LogRequestBody.Should().BeFalse();
        }

        [Fact]
        public void LogResponseBody_ShouldToggleFlag()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.LogResponseBody();
            builder.Build().LogResponseBody.Should().BeTrue();
            builder.LogResponseBody(false);
            builder.Build().LogResponseBody.Should().BeFalse();
        }

        [Fact]
        public void MaxBodyLength_Valid_ShouldSetValue()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.MaxBodyLength(100);
            builder.Build().MaxBodyLength.Should().Be(100);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void MaxBodyLength_Invalid_ShouldThrowArgumentOutOfRangeException(int value)
        {
            var builder = new HttpLoggingOptionsBuilder();
            Action act = () => builder.MaxBodyLength(value);
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("maxChars");
        }

        [Fact]
        public void ExcludeHeader_Valid_ShouldAddHeader()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.ExcludeHeader("Auth");
            builder.Build().ExcludedHeaders.Should().ContainSingle("Auth");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ExcludeHeader_Invalid_ShouldThrowArgumentException(string header)
        {
            var builder = new HttpLoggingOptionsBuilder();
            Action act = () => builder.ExcludeHeader(header!);
            act.Should().Throw<ArgumentException>().WithParameterName("header");
        }

        [Fact]
        public void UseLogger_ShouldSetLoggerType()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.UseLogger<DefaultHttpLogger>();
            builder.Build().LoggerType.Should().Be(typeof(DefaultHttpLogger));
        }

        private class CustomHandler : DelegatingHandler { }

        [Fact]
        public void UseCustomHandler_ShouldSetHandlerTypeAndFlag()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.UseCustomHandler<CustomHandler>();
            var opts = builder.Build();
            opts.UseCustomHandler.Should().BeTrue();
            opts.CustomHandlerType.Should().Be(typeof(CustomHandler));
        }

        [Fact]
        public void Build_EnabledWithoutLoggerOrHandler_ShouldThrowInvalidOperationException()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable();
            Action act = () => builder.Build();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Either a LoggerType or a CustomHandlerType must be configured when logging is enabled.");
        }

        [Fact]
        public void Build_EnabledWithLogger_ShouldNotThrow()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable().UseLogger<DefaultHttpLogger>();
            Action act = () => builder.Build();
            act.Should().NotThrow();
        }

        [Fact]
        public void Build_EnabledWithCustomHandler_ShouldNotThrow()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable().UseCustomHandler<CustomHandler>();
            Action act = () => builder.Build();
            act.Should().NotThrow();
        }

        [Fact]
        public void Inspect_Action_WritesExpectedLines()
        {
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable(false)
                   .When(req => false)
                   .WithRequestLevel(LogLevel.Debug)
                   .WithSuccessResponseLevel(LogLevel.Information)
                   .WithErrorLevel(LogLevel.Error)
                   .LogRequestBody(true)
                   .LogResponseBody(false)
                   .MaxBodyLength(50)
                   .ExcludeHeader("H1")
                   .UseLogger<DefaultHttpLogger>()
                   .UseCustomHandler<CustomHandler>();
            var lines = new List<string>();
            builder.Inspect(lines.Add);
            lines.Should().Contain(l => l.Contains("Enabled=False"));
            lines.Should().Contain(l => l.Contains("Condition=Custom"));
            lines.Should().Contain(l => l.Contains("Req=Debug, Res=Information, Err=Error"));
            lines.Should().Contain(l => l.Contains("Body: Req=True, Res=False, MaxLen=50"));
            lines.Should().Contain(l => l.Contains("ExcludedHeaders=[H1]"));
            lines.Should().Contain(l => l.Contains("LoggerType=DefaultHttpLogger"));
            lines.Should().Contain(l => l.Contains("CustomHandler=True:CustomHandler"));
        }

        [Fact]
        public void Inspect_ILogger_InvokesLogDebug()
        {
            var logger = new Mock<ILogger>();
            var builder = new HttpLoggingOptionsBuilder();
            builder.Enable();
            builder.Inspect(logger.Object);
            logger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("[Logging] Enabled=")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
