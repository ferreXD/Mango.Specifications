// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;

    /// <summary>
    /// Options for configuring authentication in Mango HTTP clients.
    /// Use this class to enable authentication, specify preset keys, conditions, and strategy factories.
    /// </summary>
    public sealed class HttpAuthOptions
    {
        /// <summary>
        /// Master switch for client-level authentication.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets the list of preset keys used for authentication configuration.
        /// </summary>
        public List<string> PresetKeys { get; set; } = new();

        /// <summary>
        /// Optional predicate to skip authentication on certain requests.
        /// </summary>
        public Func<HttpRequestMessage, bool>? Condition { get; set; }

        /// <summary>
        /// Factory to resolve the <see cref="IAuthenticationStrategy"/> from dependency injection.
        /// </summary>
        public Func<IServiceProvider, IAuthenticationStrategy>? StrategyFactory { get; set; }
    }
}
