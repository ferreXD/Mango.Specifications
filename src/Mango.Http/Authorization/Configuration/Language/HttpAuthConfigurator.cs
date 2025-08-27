// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Fluent configurator for building <see cref="HttpAuthOptions"/> for Mango HTTP client authentication.
    /// Use this class to enable authentication, specify strategies, presets, and conditions for HTTP clients.
    /// </summary>
    /// <example>
    /// <code>
    /// var configurator = new HttpAuthConfigurator()
    ///     .Enable()
    ///     .UseBearerToken("my-token")
    ///     .When(request => request.RequestUri.Host == "api.example.com");
    /// var options = configurator.Build(registry);
    /// </code>
    /// </example>
    public sealed class HttpAuthConfigurator
    {
        private readonly List<string> _presets = new();
        private readonly List<Action<HttpAuthOptionsBuilder>> _actions = new();

        /// <summary>
        /// Gets the list of preset names to apply.
        /// </summary>
        public IReadOnlyList<string> Presets => _presets;
        /// <summary>
        /// Gets the list of configuration actions to apply to the builder.
        /// </summary>
        public IReadOnlyList<Action<HttpAuthOptionsBuilder>> Actions => _actions;

        /// <summary>
        /// Enables authentication for the HTTP client.
        /// </summary>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator Enable()
        {
            Fluent(b => b.Enable());
            return this;
        }

        /// <summary>
        /// Sets a predicate to conditionally apply authentication based on the request.
        /// </summary>
        /// <param name="cond">A function that returns true if authentication should be applied.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator When(Func<HttpRequestMessage, bool> cond)
        {
            Fluent(b => b.When(cond));
            return this;
        }

        /// <summary>
        /// Adds a named authentication preset to the configuration.
        /// </summary>
        /// <param name="key">The preset key.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator WithPreset(string key)
        {
            _presets.Add(key);
            Fluent(b => b.UsePreset(key));
            return this;
        }

        /// <summary>
        /// Uses a custom authentication strategy resolved from DI.
        /// </summary>
        /// <typeparam name="TStrategy">The type of authentication strategy.</typeparam>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseStrategy<TStrategy>()
            where TStrategy : class, IAuthenticationStrategy
        {
            Fluent(b => b.UseStrategy(sp => sp.GetRequiredService<TStrategy>()));
            return this;
        }

        /// <summary>
        /// Uses a custom authentication strategy factory.
        /// </summary>
        /// <param name="factory">A factory function to create the authentication strategy.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseStrategy(Func<IServiceProvider, IAuthenticationStrategy> factory)
        {
            Fluent(b => b.UseStrategy(factory));
            return this;
        }

        /// <summary>
        /// Uses a bearer token authentication strategy.
        /// </summary>
        /// <param name="token">The bearer token.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseBearerToken(string token)
        {
            Fluent(b => b.UseBearerToken(token));
            return this;
        }

        /// <summary>
        /// Uses basic authentication strategy.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseBasicAuth(string username, string password)
        {
            Fluent(b => b.UseBasic(username, password));
            return this;
        }

        /// <summary>
        /// Uses a header-based authentication strategy.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="valueFactory">A factory to produce the header value.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseHeader(string name, Func<string> valueFactory)
        {
            Fluent(b => b.UseHeader(name, valueFactory));
            return this;
        }

        /// <summary>
        /// Uses an asynchronous token provider for authentication.
        /// </summary>
        /// <param name="provider">A function that asynchronously provides a token.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseTokenProvider(Func<CancellationToken, ValueTask<string>> provider)
        {
            Fluent(b => b.UseTokenProvider(provider));
            return this;
        }

        /// <summary>
        /// Uses a cached token provider for authentication, with a refresh margin.
        /// </summary>
        /// <param name="provider">A function that asynchronously provides a token and its expiration.</param>
        /// <param name="refreshMargin">The time before expiration to refresh the token.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseCachedProvider(
            Func<CancellationToken, ValueTask<(string Token, DateTimeOffset ExpiresAt)>> provider,
            TimeSpan refreshMargin)
        {
            Fluent(b => b.UseCachedProvider(provider, refreshMargin));
            return this;
        }

        /// <summary>
        /// Uses a composite authentication strategy composed of multiple strategies.
        /// </summary>
        /// <param name="strategies">The authentication strategies to compose.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpAuthConfigurator UseComposite(params IAuthenticationStrategy[] strategies)
        {
            Fluent(b => b.UseComposite(strategies));
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="HttpAuthOptions"/> using the provided preset registry.
        /// </summary>
        /// <param name="registry">The authentication strategy preset registry.</param>
        /// <returns>The built and validated <see cref="HttpAuthOptions"/>.</returns>
        internal HttpAuthOptions Build(IAuthenticationStrategyPresetRegistry registry)
        {
            var builder = new HttpAuthOptionsBuilder();

            // 1) Apply presets
            foreach (var preset in _presets.Select(registry.Get))
                preset.Configure(builder);

            // 2) Apply user actions
            foreach (var act in _actions)
                act(builder);

            return builder.Build(registry);
        }
        private HttpAuthConfigurator Fluent(Action<HttpAuthOptionsBuilder> act)
        {
            _actions.Add(act);
            return this;
        }
    }
}
