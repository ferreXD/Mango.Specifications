// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Hooks
{
    using FluentAssertions;
    using Mango.Http.Hooks;
    using Polly;

    public class HttpRequestHookOptionsBuilderTests
    {
        [Fact]
        public void WithPreRequestAsync_ShouldSetPreRequestHook_AndReturnBuilder()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder();
            Func<HttpRequestMessage, Context, CancellationToken, Task> preHook = (req, ctx, ct) => Task.CompletedTask;

            // Act
            var returned = builder.WithPreRequestAsync(preHook);
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.PreRequestAsync.Should().BeSameAs(preHook);
        }

        [Fact]
        public void WithPostResponseAsync_ShouldSetPostResponseHook_AndReturnBuilder()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder();
            Func<HttpResponseMessage, Context, CancellationToken, Task> postHook = (resp, ctx, ct) => Task.CompletedTask;

            // Act
            var returned = builder.WithPostResponseAsync(postHook);
            var options = builder.Build();

            // Assert
            returned.Should().BeSameAs(builder);
            options.PostResponseAsync.Should().BeSameAs(postHook);
        }

        [Fact]
        public void Validate_WithoutPreRequestAsync_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder().WithPreRequestAsync(null!);

            // Act
            Action act = () => builder.Validate();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Pre-request async hook is not set.");
        }

        [Fact]
        public void Validate_WithPreOnly_ShouldThrowInvalidOperationException_ForPostResponse()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder()
                .WithPreRequestAsync((req, ctx, ct) => Task.CompletedTask)
                .WithPostResponseAsync(null!);

            // Act
            Action act = () => builder.Validate();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Post-response async hook is not set.");
        }

        [Fact]
        public void Validate_WithBothHooks_ShouldNotThrow()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder()
                .WithPreRequestAsync((req, ctx, ct) => Task.CompletedTask)
                .WithPostResponseAsync((resp, ctx, ct) => Task.CompletedTask);

            // Act / Assert
            builder.Invoking(b => b.Validate()).Should().NotThrow();
        }

        [Fact]
        public void Build_WithoutHooks_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new HttpRequestHookOptionsBuilder().WithPreRequestAsync(null!);

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Pre-request async hook is not set.");
        }

        [Fact]
        public void Build_WithBothHooks_ShouldReturnOptions_WithHooksSet()
        {
            // Arrange
            Func<HttpRequestMessage, Context, CancellationToken, Task> preHook = (req, ctx, ct) => Task.CompletedTask;
            Func<HttpResponseMessage, Context, CancellationToken, Task> postHook = (resp, ctx, ct) => Task.CompletedTask;
            var builder = new HttpRequestHookOptionsBuilder()
                .WithPreRequestAsync(preHook)
                .WithPostResponseAsync(postHook);

            // Act
            var options = builder.Build();

            // Assert
            options.PreRequestAsync.Should().BeSameAs(preHook);
            options.PostResponseAsync.Should().BeSameAs(postHook);
        }
    }
}
