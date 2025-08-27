// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;

    public class DefaultAuthenticationStrategyPresetRegistryTests
    {
        private class StubPreset : IAuthenticationStrategyPreset
        {
            public string Name { get; }
            public StubPreset(string name) => Name = name;
            public void Configure(HttpAuthOptionsBuilder builder) { }
        }

        [Fact]
        public void Get_KnownName_ReturnsPreset()
        {
            // Arrange
            var preset = new StubPreset("Preset1");
            var registry = new DefaultAuthenticationStrategyPresetRegistry(new[] { preset });

            // Act
            var result = registry.Get("Preset1");

            // Assert
            result.Should().BeSameAs(preset);
        }

        [Theory]
        [InlineData("PRESET1", "Preset1")]
        [InlineData("preset1", "Preset1")]
        [InlineData("pReSeT1", "Preset1")]
        public void Get_DifferentCasing_ReturnsPreset(string lookupName, string originalName)
        {
            // Arrange
            var preset = new StubPreset(originalName);
            var registry = new DefaultAuthenticationStrategyPresetRegistry(new[] { preset });

            // Act
            var result = registry.Get(lookupName);

            // Assert
            result.Should().BeSameAs(preset);
        }

        [Fact]
        public void Get_DuplicateNames_PreservesFirstRegistered()
        {
            // Arrange
            var first = new StubPreset("MyPreset");
            var second = new StubPreset("mypreset"); // same key, different casing
            var registry = new DefaultAuthenticationStrategyPresetRegistry(new[] { first, second });

            // Act
            var result = registry.Get("MYPRESET");

            // Assert
            result.Should().BeSameAs(first, "the registry should keep the first preset for duplicate names");
        }

        [Fact]
        public void Get_UnknownName_ThrowsKeyNotFoundException()
        {
            // Arrange
            var preset = new StubPreset("Known");
            var registry = new DefaultAuthenticationStrategyPresetRegistry(new[] { preset });

            // Act
            Action act = () => registry.Get("Unknown");

            // Assert
            act.Should()
               .Throw<KeyNotFoundException>()
               .WithMessage("Auth preset 'Unknown' not found.");
        }
    }
}
