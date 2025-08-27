// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Metrics
{
    using FluentAssertions;
    using Mango.Http.Metrics;
    using System;

    public class HttpClientMetricsOptionsBuilderTests
    {
        [Fact]
        public void DefaultBuild_ShouldHaveEnabledFalse_AndNoAdditionalTags()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            var options = builder.Build();

            // Assert
            options.Enabled.Should().BeFalse();
            options.AdditionalTags.Should().BeEmpty();
        }

        [Fact]
        public void Enable_ShouldSetEnabledTrue_AndReturnBuilder()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            var returned = builder.Enable();
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.Enabled.Should().BeTrue();
        }

        [Fact]
        public void Disable_AfterEnable_ShouldSetEnabledFalse_AndReturnBuilder()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            builder.Enable();
            var returned = builder.Disable();
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.Enabled.Should().BeFalse();
        }

        [Fact]
        public void WithAdditionalTag_ShouldAddTagToList()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            var returned = builder.WithAdditionalTag("tag1");
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.AdditionalTags.Should().ContainSingle().Which.Should().Be("tag1");
        }

        [Fact]
        public void WithAdditionalTag_MultipleCalls_ShouldAccumulateTags()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            builder.WithAdditionalTag("tag1");
            builder.WithAdditionalTag("tag2");
            var options = builder.Build();

            // Assert
            options.AdditionalTags.Should().Equal(new[] { "tag1", "tag2" });
        }

        [Fact]
        public void WithAdditionalTags_ParamsShouldSetTagsList()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder();

            // Act
            var returned = builder.WithAdditionalTags("a", "b", "c");
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.AdditionalTags.Should().Equal(new[] { "a", "b", "c" });
        }

        [Fact]
        public void WithAdditionalTags_EmptyArray_ShouldClearTags()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder()
                .WithAdditionalTag("existing");

            // Act
            builder.WithAdditionalTags(Array.Empty<string>());
            var options = builder.Build();

            // Assert
            options.AdditionalTags.Should().BeEmpty();
        }

        [Fact]
        public void WithAdditionalTags_NullArray_ShouldClearTags()
        {
            // Arrange
            var builder = new HttpClientMetricsOptionsBuilder()
                .WithAdditionalTag("existing");

            // Act
            var returned = builder.WithAdditionalTags(null!);
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.AdditionalTags.Should().BeEmpty();
        }
    }
}
