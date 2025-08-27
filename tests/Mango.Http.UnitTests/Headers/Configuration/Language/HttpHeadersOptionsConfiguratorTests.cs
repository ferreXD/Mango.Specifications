// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Headers
{
    using FluentAssertions;
    using Mango.Http.Headers;
    using System;
    using System.Diagnostics;

    public class HttpHeadersOptionsConfiguratorTests
    {
        [Fact]
        public void WithCustomHeader_StaticValue_AddsHeader()
        {
            var config = new HttpHeadersOptionsConfigurator();

            var returned = config.WithCustomHeader("X-Key", "Value");
            returned.Should().BeSameAs(config);

            var opts = config.Build();
            opts.CustomHeaders.Should().ContainKey("X-Key");
            opts.CustomHeaders["X-Key"].Invoke().Should().Be("Value");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithCustomHeader_Static_InvalidValue_Throws(string value)
        {
            var config = new HttpHeadersOptionsConfigurator();
            Action act = () => config.WithCustomHeader("X-Key", value);
            act.Should().Throw<ArgumentException>().WithParameterName("value");
        }

        [Fact]
        public void WithCustomHeader_Dynamic_InvalidName_Throws()
        {
            var config = new HttpHeadersOptionsConfigurator();
            Action act = () => config.WithCustomHeader(" ", () => "val");
            act.Should().Throw<ArgumentException>().WithParameterName("name");
        }

        [Fact]
        public void WithCustomHeader_Dynamic_NullValueFactory_Throws()
        {
            var config = new HttpHeadersOptionsConfigurator();
            var act = () => config.WithCustomHeader("X-Key", (string)null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("value");
        }

        [Fact]
        public void WithCustomHeader_DuplicateWithoutOverwrite_Throws()
        {
            var config = new HttpHeadersOptionsConfigurator();
            config.WithCustomHeader("X-Dup", "first");
            Action act = () => config.WithCustomHeader("X-Dup", "second", overwrite: false);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Header 'X-Dup' already exists. Set 'overwrite' to true to replace it.");
        }

        [Fact]
        public void WithCustomHeader_DuplicateWithOverwrite_ReplacesFactory()
        {
            var config = new HttpHeadersOptionsConfigurator();
            config.WithCustomHeader("X-Dup", "first");
            config.WithCustomHeader("X-Dup", "second", overwrite: true);

            var opts = config.Build();
            opts.CustomHeaders["X-Dup"].Invoke().Should().Be("second");
        }

        [Fact]
        public void WithCorrelationIdHeader_DefaultName_AddsHeader()
        {
            var config = new HttpHeadersOptionsConfigurator();
            using var act = new Activity("test").Start();
            var returned = config.WithCorrelationIdHeader();
            returned.Should().BeSameAs(config);

            var opts = config.Build();
            opts.CustomHeaders.Should().ContainKey("X-Correlation-ID");
            opts.CustomHeaders["X-Correlation-ID"].Invoke()
                .Should().Be(Activity.Current!.TraceId.ToString());
        }

        [Fact]
        public void WithCorrelationIdHeader_CustomName_AddsHeader()
        {
            var config = new HttpHeadersOptionsConfigurator();
            using var act = new Activity("test2").Start();
            config.WithCorrelationIdHeader("X-Trace");
            var opts = config.Build();
            opts.CustomHeaders.Should().ContainKey("X-Trace");
            opts.CustomHeaders["X-Trace"].Invoke()
                .Should().Be(Activity.Current!.TraceId.ToString());
        }

        [Fact]
        public void WithRequestIdHeader_DefaultName_AddsHeader()
        {
            var config = new HttpHeadersOptionsConfigurator();
            var activity = new Activity("test3");
            activity.Start();
            try
            {
                config.WithRequestIdHeader();
                var opts = config.Build();
                opts.CustomHeaders.Should().ContainKey("X-Request-ID");
                opts.CustomHeaders["X-Request-ID"].Invoke()
                    .Should().Be(Activity.Current!.Id);
            }
            finally { activity.Stop(); }
        }

        [Fact]
        public void WithRequestIdHeader_CustomName_AddsHeader()
        {
            var config = new HttpHeadersOptionsConfigurator();
            var activity = new Activity("test4");
            activity.Start();
            try
            {
                config.WithRequestIdHeader("Req-ID");
                var opts = config.Build();
                opts.CustomHeaders.Should().ContainKey("Req-ID");
                opts.CustomHeaders["Req-ID"].Invoke()
                    .Should().Be(Activity.Current!.Id);
            }
            finally { activity.Stop(); }
        }
    }
}
