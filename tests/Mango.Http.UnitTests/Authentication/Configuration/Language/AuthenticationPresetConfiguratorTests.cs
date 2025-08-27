// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using System.Linq;

    public class AuthenticationPresetConfiguratorTests
    {
        [Fact]
        public void Build_WithoutPresets_ReturnsEmpty()
        {
            // Arrange
            var configurator = new AuthenticationPresetConfigurator();

            // Act
            var presets = configurator.Build();

            // Assert
            presets.Should().BeEmpty();
        }

        [Fact]
        public void WithPreset_ReturnsConfigurator_ForChaining()
        {
            // Arrange
            var configurator = new AuthenticationPresetConfigurator();

            // Act
            var returned = configurator.WithPreset("MyPreset", _ => { });

            // Assert
            returned.Should().BeSameAs(configurator);
        }

        [Fact]
        public void Build_AfterWithPreset_ReturnsSinglePreset_WithCorrectName()
        {
            // Arrange
            var name = "InlinePreset";
            var configurator = new AuthenticationPresetConfigurator()
                .WithPreset(name, _ => { });

            // Act
            var presets = configurator.Build().ToList();

            // Assert
            presets.Should().HaveCount(1);
            presets[0].Name.Should().Be(name);
        }

        [Fact]
        public void Build_AfterMultipleWithPreset_ReturnsAllPresets_InOrder()
        {
            // Arrange
            var configurator = new AuthenticationPresetConfigurator()
                .WithPreset("First", _ => { })
                .WithPreset("Second", _ => { });

            // Act
            var names = configurator.Build().Select(p => p.Name).ToList();

            // Assert
            names.Should().ContainInOrder("First", "Second");
        }

        [Fact]
        public void Configure_OnEachPreset_InvokesUnderlyingAction_WithGivenBuilder()
        {
            // Arrange
            var builder = new HttpAuthOptionsBuilder();
            bool firstCalled = false, secondCalled = false;
            HttpAuthOptionsBuilder? firstBuilder = null, secondBuilder = null;

            var configurator = new AuthenticationPresetConfigurator()
                .WithPreset("P1", b =>
                {
                    firstCalled = true;
                    firstBuilder = b;
                })
                .WithPreset("P2", b =>
                {
                    secondCalled = true;
                    secondBuilder = b;
                });

            var presets = configurator.Build().ToList();

            // Act
            presets[0].Configure(builder);
            presets[1].Configure(builder);

            // Assert
            firstCalled.Should().BeTrue("the first preset's action should be invoked");
            firstBuilder.Should().BeSameAs(builder);
            secondCalled.Should().BeTrue("the second preset's action should be invoked");
            secondBuilder.Should().BeSameAs(builder);
        }
    }
}
