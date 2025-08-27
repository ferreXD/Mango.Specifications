namespace Mango.Http.UnitTests.Presets
{
    using FluentAssertions;
    using Mango.Http.Presets;
    using Mango.Http.Resiliency;
    using System;
    using System.Linq;

    public class ResiliencyPresetConfiguratorTests
    {
        [Fact]
        public void WithPreset_NullName_ShouldThrowArgumentException()
        {
            var config = new ResiliencyPresetConfigurator();
            Action act = () => config.WithPreset(null!, b => { });
            act.Should().Throw<ArgumentException>()
               .WithParameterName("presetName");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void WithPreset_WhitespaceName_ShouldThrowArgumentException(string name)
        {
            var config = new ResiliencyPresetConfigurator();
            Action act = () => config.WithPreset(name, b => { });
            act.Should().Throw<ArgumentException>()
               .WithParameterName("presetName");
        }

        [Fact]
        public void WithPreset_NullConfigurator_ShouldThrowArgumentNullException()
        {
            var config = new ResiliencyPresetConfigurator();
            Action act = () => config.WithPreset("name", null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("configurator");
        }

        [Fact]
        public void WithPreset_InstanceNull_ShouldThrowArgumentNullException()
        {
            var config = new ResiliencyPresetConfigurator();
            Action act = () => config.WithPreset((IResiliencyPolicyPreset)null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("preset");
        }

        [Fact]
        public void Build_WithoutPresets_ShouldThrowInvalidOperationException()
        {
            var config = new ResiliencyPresetConfigurator();
            Action act = () => config.Build();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No resiliency presets have been configured.");
        }

        [Fact]
        public void Build_WithDuplicateNames_ShouldThrowInvalidOperationException()
        {
            var config = new ResiliencyPresetConfigurator()
                .WithPreset("dup", b => { })
                .WithPreset("dup", b => { });
            Action act = () => config.Build();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Duplicate resiliency preset names found: dup.");
        }

        [Fact]
        public void Build_WithMultipleDuplicateNames_ShouldListAllInMessage()
        {
            var config = new ResiliencyPresetConfigurator()
                .WithPreset("a", b => { })
                .WithPreset("b", b => { })
                .WithPreset("a", b => { })
                .WithPreset("b", b => { });
            Action act = () => config.Build();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Duplicate resiliency preset names found: a, b.");
        }

        [Fact]
        public void Build_WithValidPresets_ShouldReturnList()
        {
            var configuratorAction = new Action<ResiliencyPolicyOptionsBuilder>(b => b.WithTimeout());
            var instancePreset = new InlineResiliencyPolicyPreset("inst", b => b.WithRetry());
            var config = new ResiliencyPresetConfigurator()
                .WithPreset("first", configuratorAction)
                .WithPreset(instancePreset);
            var list = config.Build();
            list.Should().HaveCount(2);
            list.Select(p => p.Name).Should().Contain(new[] { "first", "inst" });
        }
    }
}
