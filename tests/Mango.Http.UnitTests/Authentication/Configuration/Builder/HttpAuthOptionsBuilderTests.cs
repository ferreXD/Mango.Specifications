// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Diagnostics;

    public class HttpAuthOptionsBuilderTests
    {
        [Fact]
        public void Enable_ShouldSetEnabledTrue()
        {
            var builder = new HttpAuthOptionsBuilder();
            builder.Options.Enabled.Should().BeFalse("authentication is disabled by default");

            var returned = builder.Enable();
            returned.Should().BeSameAs(builder, "Enable() should return the same builder for chaining");
            builder.Options.Enabled.Should().BeTrue("Enable() should set Options.Enabled to true");
        }

        [Fact]
        public void When_NullPredicate_ThrowsArgumentNullException()
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.When(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_ValidPredicate_SetsCondition()
        {
            var builder = new HttpAuthOptionsBuilder();
            Func<HttpRequestMessage, bool> predicate = req => req.Method == HttpMethod.Get;

            var ret = builder.When(predicate);
            ret.Should().BeSameAs(builder);
            builder.Options.Condition.Should().BeSameAs(predicate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UsePreset_NullOrWhitespaceKey_ThrowsArgumentException(string key)
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UsePreset(key!);
            act.Should().Throw<ArgumentException>()
               .WithParameterName("key");
        }

        [Fact]
        public void UsePreset_ValidKey_AddsToPresetKeys()
        {
            var builder = new HttpAuthOptionsBuilder();
            var ret = builder.UsePreset("myKey");
            ret.Should().BeSameAs(builder);

            builder.Options.PresetKeys.Should().ContainSingle("myKey");
        }

        [Fact]
        public void UseStrategy_NullFactory_ThrowsArgumentNullException()
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseStrategy(null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("factory");
        }

        [Fact]
        public void UseStrategy_ValidFactory_SetsStrategyFactoryAndEnables()
        {
            var builder = new HttpAuthOptionsBuilder();
            var factory = new Func<IServiceProvider, IAuthenticationStrategy>(_ => Mock.Of<IAuthenticationStrategy>());

            var ret = builder.UseStrategy(factory);
            ret.Should().BeSameAs(builder);

            builder.Options.Enabled.Should().BeTrue();
            builder.Options.StrategyFactory.Should().BeSameAs(factory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UseBearerToken_NullOrWhitespaceToken_ThrowsArgumentException(string token)
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseBearerToken(token!);
            act.Should().Throw<ArgumentException>()
               .WithParameterName("token");
        }

        [Fact]
        public async Task UseBearerToken_ValidToken_SetsFactoryToBearerStrategy()
        {
            var token = "abc123";
            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var logger = Mock.Of<ILogger<BearerTokenAuthStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource)))
                  .Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<BearerTokenAuthStrategy>)))
                  .Returns(logger);

            builder.UseBearerToken(token);
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<BearerTokenAuthStrategy>();

            // verify that applying the strategy sets the header correctly
            var request = new HttpRequestMessage(HttpMethod.Get, "http://x/");
            await strat.ApplyAsync(request, CancellationToken.None);
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization!.Parameter.Should().Be(token);
        }

        [Theory]
        [InlineData(null, "pw")]
        [InlineData("", "pw")]
        [InlineData("   ", "pw")]
        [InlineData("user", null)]
        [InlineData("user", "")]
        [InlineData("user", "   ")]
        public void UseBasic_InvalidArgs_ThrowsArgumentException(string user, string pw)
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseBasic(user!, pw!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task UseBasic_ValidArgs_SetsFactoryToBasicStrategy()
        {
            var user = "alice";
            var pw = "secret";
            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var logger = Mock.Of<ILogger<BasicAuthStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource)))
                  .Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<BasicAuthStrategy>)))
                  .Returns(logger);

            builder.UseBasic(user, pw);
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<BasicAuthStrategy>();

            var request = new HttpRequestMessage(HttpMethod.Post, "http://y/");
            await strat.ApplyAsync(request, CancellationToken.None);
            var header = request.Headers.Authorization;
            header!.Scheme.Should().Be("Basic");
            header!.Parameter.Should().Be(Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{user}:{pw}")));
        }

        [Theory]
        [InlineData(null, "v")]
        [InlineData("", "v")]
        [InlineData("  ", "v")]
        public void UseHeader_InvalidName_ThrowsArgumentException(string name, string value)
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseHeader(name!, () => value);
            act.Should().Throw<ArgumentException>()
               .WithParameterName("name");
        }

        [Fact]
        public void UseHeader_NullValueFactory_ThrowsArgumentNullException()
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseHeader("X", null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("valueFactory");
        }

        [Fact]
        public async Task UseHeader_ValidParams_SetsFactoryToHeaderStrategy()
        {
            var headerName = "X-Auth";
            var headerValue = "val";
            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var logger = Mock.Of<ILogger<HeaderAuthenticationStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource)))
                  .Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<HeaderAuthenticationStrategy>)))
                  .Returns(logger);

            builder.UseHeader(headerName, () => headerValue);
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<HeaderAuthenticationStrategy>();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://z/");
            await strat.ApplyAsync(request, CancellationToken.None);
            request.Headers.GetValues(headerName).Should().ContainSingle(headerValue);
        }

        [Fact]
        public void UseTokenProvider_NullProvider_ThrowsArgumentNullException()
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act = () => builder.UseTokenProvider(null!);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("provider");
        }

        [Fact]
        public async Task UseTokenProvider_ValidProvider_SetsFactoryToAsyncTokenProvider()
        {
            var token = "xtoken";
            Func<CancellationToken, ValueTask<string>> provider = ct => new ValueTask<string>(token);
            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var logger = Mock.Of<ILogger<AsyncTokenProviderStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource)))
                  .Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<AsyncTokenProviderStrategy>)))
                  .Returns(logger);

            builder.UseTokenProvider(provider);
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<AsyncTokenProviderStrategy>();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://a/");
            await strat.ApplyAsync(request, CancellationToken.None);
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization!.Parameter.Should().Be(token);
        }

        [Theory]
        [InlineData(null, "00:00:05")]
        [InlineData("valid", "00:00:00")]
        public void UseCachedProvider_InvalidArgs_Throws(
            string token,
            string marginStr)
        {
            var builder = new HttpAuthOptionsBuilder();
            var margin = TimeSpan.Parse(marginStr);

            var provider = token is null
                ? null
                : new Func<CancellationToken, ValueTask<(string, DateTimeOffset)>>(
                    ct => new ValueTask<(string, DateTimeOffset)>((token, DateTimeOffset.UtcNow.AddMinutes(1))));

            Action act;
            if (provider is null)
                act = () => builder.UseCachedProvider(null!, TimeSpan.FromSeconds(30));
            else
                act = () => builder.UseCachedProvider(provider, margin);

            act.Should().Throw<ArgumentException>() // could be ArgumentNullException or ArgumentOutOfRange
               .Where(e => e is ArgumentNullException || e is ArgumentOutOfRangeException);
        }

        [Fact]
        public void UseCachedProvider_ValidParams_SetsFactoryToCachedStrategy()
        {
            Func<CancellationToken, ValueTask<(string, DateTimeOffset)>> provider =
                ct => new ValueTask<(string, DateTimeOffset)>(("tok", DateTimeOffset.UtcNow.AddMinutes(1)));

            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var cachedLogger = Mock.Of<ILogger<CachedTokenProvider>>();
            var authLogger = Mock.Of<ILogger<CachedTokenAuthStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource))).Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<CachedTokenProvider>))).Returns(cachedLogger);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<CachedTokenAuthStrategy>))).Returns(authLogger);

            builder.UseCachedProvider(provider, TimeSpan.FromSeconds(10));
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<CachedTokenAuthStrategy>();
        }

        [Fact]
        public void UseComposite_NullOrEmpty_ThrowsArgumentException()
        {
            var builder = new HttpAuthOptionsBuilder();
            Action act1 = () => builder.UseComposite(null!);
            Action act2 = () => builder.UseComposite(Array.Empty<IAuthenticationStrategy>());

            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UseComposite_ValidStrategies_SetsFactoryToCompositeStrategy()
        {
            var strat1 = Mock.Of<IAuthenticationStrategy>();
            var strat2 = Mock.Of<IAuthenticationStrategy>();
            var builder = new HttpAuthOptionsBuilder();
            var spMock = new Mock<IServiceProvider>();
            var activitySource = new ActivitySource("Test");
            var compositeLogger = Mock.Of<ILogger<CompositeAuthStrategy>>();
            spMock.Setup(sp => sp.GetService(typeof(ActivitySource))).Returns(activitySource);
            spMock.Setup(sp => sp.GetService(typeof(ILogger<CompositeAuthStrategy>))).Returns(compositeLogger);

            builder.UseComposite(strat1, strat2);
            builder.Options.Enabled.Should().BeTrue();
            var strat = builder.Options.StrategyFactory!(spMock.Object);
            strat.Should().BeOfType<CompositeAuthStrategy>();
        }

        [Fact]
        public void Build_WhenEnabledWithoutStrategyOrPreset_ThrowsInvalidOperationException()
        {
            var builder = new HttpAuthOptionsBuilder().Enable();
            var dummyRegistry = Mock.Of<IAuthenticationStrategyPresetRegistry>();
            Action act = () => builder.Build(dummyRegistry);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Authentication enabled but no strategy factory or preset key configured.");
        }

        [Fact]
        public void Build_WithPresetKeyOrFactory_ReturnsOptions()
        {
            var builder1 = new HttpAuthOptionsBuilder().Enable().UsePreset("abc");
            var builder2 = new HttpAuthOptionsBuilder()
                .Enable()
                .UseStrategy(_ => Mock.Of<IAuthenticationStrategy>());

            var dummyRegistry = Mock.Of<IAuthenticationStrategyPresetRegistry>();

            var opts1 = builder1.Build(dummyRegistry);
            var opts2 = builder2.Build(dummyRegistry);

            opts1.Should().NotBeNull();
            opts2.Should().NotBeNull();
        }
    }
}
