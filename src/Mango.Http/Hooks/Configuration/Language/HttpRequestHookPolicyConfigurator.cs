// ReSharper disable once CheckNamespace
namespace Mango.Http.Hooks
{
    using Polly;

    /// <summary>
    /// Fluent configurator for building <see cref="HttpRequestHookOptions"/> for Mango HTTP client request/response hooks.
    /// Use this class to specify asynchronous actions to run before requests and after responses.
    /// </summary>
    /// <example>
    /// <code>
    /// var configurator = new HttpRequestHookPolicyConfigurator()
    ///     .PreAsync((req, ctx, ct) => LogPreRequest(req))
    ///     .PostAsync((resp, ctx, ct) => LogPostResponse(resp));
    /// var options = configurator.Build();
    /// </code>
    /// </example>
    public sealed class HttpRequestHookPolicyConfigurator
    {
        private HttpRequestHookOptionsBuilder Builder { get; } = new();

        /// <summary>
        /// Sets the asynchronous action to execute before sending an HTTP request.
        /// </summary>
        /// <param name="hook">The pre-request hook action.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpRequestHookPolicyConfigurator PreAsync(Func<HttpRequestMessage, Context, CancellationToken, Task> hook)
        {
            Builder.WithPreRequestAsync(hook);
            return this;
        }

        /// <summary>
        /// Sets the asynchronous action to execute after receiving an HTTP response.
        /// </summary>
        /// <param name="hook">The post-response hook action.</param>
        /// <returns>The configurator for chaining.</returns>
        public HttpRequestHookPolicyConfigurator PostAsync(Func<HttpResponseMessage, Context, CancellationToken, Task> hook)
        {
            Builder.WithPostResponseAsync(hook);
            return this;
        }

        /// <summary>
        /// Builds the configured <see cref="HttpRequestHookOptions"/>.
        /// </summary>
        /// <returns>The built <see cref="HttpRequestHookOptions"/>.</returns>
        internal HttpRequestHookOptions Build() => Builder.Build();
    }
}
