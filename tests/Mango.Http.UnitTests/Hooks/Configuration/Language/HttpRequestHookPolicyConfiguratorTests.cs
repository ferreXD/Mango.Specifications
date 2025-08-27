// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Hooks
{
    using FluentAssertions;
    using Mango.Http.Hooks;
    using Polly;

    public class HttpRequestHookPolicyConfiguratorTests
    {
        [Fact]
        public void PreAsync_ShouldReturnConfigurator_AndSetPreHook()
        {
            // Arrange
            var configurator = new HttpRequestHookPolicyConfigurator();
            Func<HttpRequestMessage, Context, CancellationToken, Task> preHook = (req, ctx, ct) => Task.CompletedTask;

            // Act
            var returned = configurator.PreAsync(preHook);
            var options = configurator.Build();

            // Assert
            returned.Should().BeSameAs(configurator);
            options.PreRequestAsync.Should().BeSameAs(preHook);
        }

        [Fact]
        public void PostAsync_ShouldReturnConfigurator_AndSetPostHook()
        {
            // Arrange
            var configurator = new HttpRequestHookPolicyConfigurator();
            Func<HttpResponseMessage, Context, CancellationToken, Task> postHook = (resp, ctx, ct) => Task.CompletedTask;

            // Act
            var returned = configurator.PostAsync(postHook);
            var options = configurator.Build();

            // Assert
            returned.Should().BeSameAs(configurator);
            options.PostResponseAsync.Should().BeSameAs(postHook);
        }

        [Fact]
        public void Build_WithoutPreOrPost_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configurator = new HttpRequestHookPolicyConfigurator().PreAsync(null!);

            // Act
            Action act = () => configurator.Build();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Pre-request async hook is not set.");
        }

        [Fact]
        public void Build_WithOnlyPreAsync_ShouldThrowInvalidOperationException_ForPost()
        {
            // Arrange
            var configurator = new HttpRequestHookPolicyConfigurator()
                .PreAsync((req, ctx, ct) => Task.CompletedTask)
                .PostAsync(null!);

            // Act
            Action act = () => configurator.Build();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Post-response async hook is not set.");
        }

        [Fact]
        public void Build_WithBothPreAndPost_ShouldReturnOptions_WithHooksSet()
        {
            // Arrange
            Func<HttpRequestMessage, Context, CancellationToken, Task> preHook = (req, ctx, ct) => Task.CompletedTask;
            Func<HttpResponseMessage, Context, CancellationToken, Task> postHook = (resp, ctx, ct) => Task.CompletedTask;
            var configurator = new HttpRequestHookPolicyConfigurator()
                .PreAsync(preHook)
                .PostAsync(postHook);

            // Act
            var options = configurator.Build();

            // Assert
            options.PreRequestAsync.Should().BeSameAs(preHook);
            options.PostResponseAsync.Should().BeSameAs(postHook);
        }
    }
}
