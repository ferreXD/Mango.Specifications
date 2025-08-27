// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Builder for configuring <see cref="HttpAuthOptions"/> for Mango HTTP client authentication.
    /// Use this builder to enable authentication and specify strategies such as bearer token, basic, header, or custom providers.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new HttpAuthOptionsBuilder()
    ///     .Enable()
    ///     .UseBearerToken("my-token")
    ///     .When(request => request.RequestUri.Host == "api.example.com");
    /// var options = builder.Build(registry);
    /// </code>
    /// </example>
    public sealed class HttpAuthOptionsBuilder
    {
        /// <summary>
        /// Gets the options being configured.
        /// </summary>
        internal HttpAuthOptions Options { get; } = new();

        /// <summary>
        /// Enables authentication for the HTTP client.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public HttpAuthOptionsBuilder Enable()
        {
            Options.Enabled = true;
            return this;
        }

        /// <summary>
        /// Sets a predicate to conditionally apply authentication based on the request.
        /// </summary>
        /// <param name="predicate">A function that returns true if authentication should be applied.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpAuthOptionsBuilder When(Func<HttpRequestMessage, bool> predicate)
        {
            Options.Condition = predicate ?? throw new ArgumentNullException();
            return this;
        }

        /// <summary>
        /// Uses a named authentication preset strategy.
        /// </summary>
        /// <param name="key">The preset key.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the key is null or whitespace.</exception>
        public HttpAuthOptionsBuilder UsePreset(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Preset key cannot be null or whitespace.", nameof(key));
            Options.PresetKeys.Add(key);
            return this;
        }

        /// <summary>
        /// Uses a custom authentication strategy factory.
        /// </summary>
        /// <param name="factory">A factory function to create the authentication strategy.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the factory is null.</exception>
        public HttpAuthOptionsBuilder UseStrategy(Func<IServiceProvider, IAuthenticationStrategy> factory)
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));
            Options.Enabled = true;
            Options.StrategyFactory = factory;
            return this;
        }

        /// <summary>
        /// Uses a bearer token authentication strategy.
        /// </summary>
        /// <param name="token">The bearer token.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the token is null or whitespace.</exception>
        public HttpAuthOptionsBuilder UseBearerToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Bearer token cannot be null or whitespace.", nameof(token));

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
                new BearerTokenAuthStrategy(
                    token,
                    sp.GetRequiredService<ActivitySource>(),
                    sp.GetRequiredService<ILogger<BearerTokenAuthStrategy>>());
            return this;
        }

        /// <summary>
        /// Uses basic authentication strategy.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if username or password is null or whitespace.</exception>
        public HttpAuthOptionsBuilder UseBasic(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(nameof(password));

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
                new BasicAuthStrategy(
                    username,
                    password,
                    sp.GetRequiredService<ActivitySource>(),
                    sp.GetRequiredService<ILogger<BasicAuthStrategy>>());

            return this;
        }

        /// <summary>
        /// Uses a header-based authentication strategy.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="valueFactory">A factory to produce the header value.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the name is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if valueFactory is null.</exception>
        public HttpAuthOptionsBuilder UseHeader(string name, Func<string> valueFactory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Header name cannot be null or whitespace.", nameof(name));

            if (valueFactory is null)
                throw new ArgumentNullException(nameof(valueFactory));

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
                new HeaderAuthenticationStrategy(
                    name,
                    valueFactory(),
                    sp.GetRequiredService<ActivitySource>(),
                    sp.GetRequiredService<ILogger<HeaderAuthenticationStrategy>>());

            return this;
        }

        /// <summary>
        /// Uses an asynchronous token provider for authentication.
        /// </summary>
        /// <param name="provider">A function that asynchronously provides a token.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if provider is null.</exception>
        public HttpAuthOptionsBuilder UseTokenProvider(
            Func<CancellationToken, ValueTask<string>> provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
            {
                var activitySource = sp.GetRequiredService<ActivitySource>();
                var logger = sp.GetRequiredService<ILogger<AsyncTokenProviderStrategy>>();
                return new AsyncTokenProviderStrategy(provider, activitySource, logger);
            };

            return this;
        }

        /// <summary>
        /// Uses a cached token provider for authentication, with a refresh margin.
        /// </summary>
        /// <param name="provider">A function that asynchronously provides a token and its expiration.</param>
        /// <param name="refreshMargin">The time before expiration to refresh the token.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if provider is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if refreshMargin is not greater than zero.</exception>
        public HttpAuthOptionsBuilder UseCachedProvider(
            Func<CancellationToken, ValueTask<(string Token, DateTimeOffset ExpiresAt)>> provider,
            TimeSpan refreshMargin)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));
            if (refreshMargin <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshMargin), "Refresh margin duration must be greater than zero.");

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
            {
                var providerLogger = sp.GetRequiredService<ILogger<CachedTokenProvider>>();
                var activitySource = sp.GetRequiredService<ActivitySource>();
                var cachedProvider = new CachedTokenProvider(providerLogger, provider, refreshMargin);

                var authLogger = sp.GetRequiredService<ILogger<CachedTokenAuthStrategy>>();
                return new CachedTokenAuthStrategy(activitySource, cachedProvider, authLogger);
            };

            return this;
        }

        /// <summary>
        /// Uses a composite authentication strategy composed of multiple strategies.
        /// </summary>
        /// <param name="strategies">The authentication strategies to compose.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if no strategies are provided.</exception>
        public HttpAuthOptionsBuilder UseComposite(params IAuthenticationStrategy[] strategies)
        {
            if (strategies is null || strategies.Length == 0)
                throw new ArgumentException("At least one strategy is required.", nameof(strategies));

            Options.Enabled = true;
            Options.StrategyFactory = sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CompositeAuthStrategy>>();
                var activitySource = sp.GetRequiredService<ActivitySource>();
                return new CompositeAuthStrategy(activitySource, logger, strategies);
            };

            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="HttpAuthOptions"/>.
        /// </summary>
        /// <param name="registry">The authentication strategy preset registry.</param>
        /// <returns>The built and validated <see cref="HttpAuthOptions"/>.</returns>
        internal HttpAuthOptions Build(IAuthenticationStrategyPresetRegistry registry)
        {
            Validate();
            return Options;
        }

        /// <summary>
        /// Validates the current authentication options configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if authentication is enabled but no strategy factory or preset key is configured.</exception>
        private void Validate()
        {
            if (!Options.Enabled) return;
            if (Options.StrategyFactory is null && !Options.PresetKeys.Any())
                throw new InvalidOperationException("Authentication enabled but no strategy factory or preset key configured.");
        }
    }
}
