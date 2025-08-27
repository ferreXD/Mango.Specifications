// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using System;
    using System.Diagnostics;

    class StubPreset(string name) : IAuthenticationStrategyPreset
    {
        public string Name { get; } = name;
        public bool ConfigureCalled { get; private set; }

        public void Configure(HttpAuthOptionsBuilder builder)
        {
            ConfigureCalled = true;
            // for demonstration, also flip Enabled
            builder.Enable();
        }
    }

    public class HttpAuthConfiguratorTests
    {
        [Fact]
        public void PresetsAndActions_StartEmpty()
        {
            var cfg = new HttpAuthConfigurator();
            cfg.Presets.Should().BeEmpty();
            cfg.Actions.Should().BeEmpty();
        }

        [Fact]
        public void Enable_AddsEnableAction()
        {
            var cfg = new HttpAuthConfigurator();
            cfg.Enable().Should().BeSameAs(cfg);

            cfg.Actions.Should().ContainSingle();
            var builder = new HttpAuthOptionsBuilder();
            // invoke the stored action
            cfg.Actions[0](builder);
            builder.Options.Enabled.Should().BeTrue();
        }

        [Fact]
        public void When_AddsConditionAction()
        {
            Func<HttpRequestMessage, bool> pred = _ => true;
            var cfg = new HttpAuthConfigurator();
            cfg.When(pred).Should().BeSameAs(cfg);

            cfg.Actions.Should().ContainSingle();
            var builder = new HttpAuthOptionsBuilder();
            cfg.Actions[0](builder);
            builder.Options.Condition.Should().BeSameAs(pred);
        }

        [Fact]
        public void WithPreset_AddsPresetKeyAndUsePresetAction()
        {
            var key = "my-preset";
            var cfg = new HttpAuthConfigurator();
            cfg.WithPreset(key).Should().BeSameAs(cfg);

            cfg.Presets.Should().Equal(key);
            cfg.Actions.Should().ContainSingle();

            var builder = new HttpAuthOptionsBuilder();
            cfg.Actions[0](builder);
            builder.Options.PresetKeys.Should().ContainSingle(key);
        }

        [Fact]
        public void Build_AppliesPresetsThenActions()
        {
            // Arrange: one stub preset in registry, and one action
            var stub = new StubPreset("p1");
            var registry = new Mock<IAuthenticationStrategyPresetRegistry>();
            registry.Setup(r => r.Get("p1")).Returns(stub);

            var pred = new Func<HttpRequestMessage, bool>(_ => false);
            var cfg = new HttpAuthConfigurator()
                .WithPreset("p1")
                .UseBearerToken("MYTOKEN")
                .When(pred);

            // Act
            var opts = cfg.Build(registry.Object);

            // Assert: stub.Configure ran, action.Enable ran, When action ran
            stub.ConfigureCalled.Should().BeTrue();
            opts.Enabled.Should().BeTrue();
            opts.Condition.Should().BeSameAs(pred);
            opts.PresetKeys.Should().Contain("p1");

            // registry.Get must be called exactly once with "p1"
            registry.Verify(r => r.Get("p1"), Times.Once);
        }

        [Fact]
        public void Build_EnabledWithoutPresetOrFactory_Throws()
        {
            var cfg = new HttpAuthConfigurator().Enable();
            Action act = () => cfg.Build(Mock.Of<IAuthenticationStrategyPresetRegistry>());

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Authentication enabled but no strategy factory or preset key configured.");
        }

        [Fact]
        public async Task Build_WithBearerTokenFactory_WiresUpStrategyFactory()
        {
            // Arrange
            var token = "tkn";
            var cfg = new HttpAuthConfigurator()
                .Enable()
                .UseBearerToken(token);

            var registry = Mock.Of<IAuthenticationStrategyPresetRegistry>();

            // Act
            var opts = cfg.Build(registry);

            // Assert
            opts.Enabled.Should().BeTrue();
            opts.StrategyFactory.Should().NotBeNull();

            // verify that the factory produces a BearerTokenAuthStrategy that actually sets the header
            var services = new ServiceCollection()
                .AddSingleton(new ActivitySource("Test"))
                .AddLogging()
                .BuildServiceProvider();

            var strat = opts.StrategyFactory!(services);
            var req = new HttpRequestMessage(HttpMethod.Get, "http://x/");
            await strat.ApplyAsync(req, CancellationToken.None);

            req.Headers.Authorization!.Scheme.Should().Be("Bearer");
            req.Headers.Authorization!.Parameter.Should().Be(token);
        }
    }
}
