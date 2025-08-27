// ReSharper disable once CheckNamespace
namespace Mango.Http.Hooks
{
    using Polly;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Builder for configuring <see cref="HttpRequestHookOptions"/> for Mango HTTP client request/response hooks.
    /// Use this class to specify asynchronous actions to run before requests and after responses.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new HttpRequestHookOptionsBuilder()
    ///     .WithPreRequestAsync((req, ctx, ct) => LogPreRequest(req))
    ///     .WithPostResponseAsync((resp, ctx, ct) => LogPostResponse(resp));
    /// var options = builder.Build();
    /// </code>
    /// </example>
    public sealed class HttpRequestHookOptionsBuilder
    {
        private readonly HttpRequestHookOptions _options = new();

        /// <summary>
        /// Sets the asynchronous action to execute before sending an HTTP request.
        /// </summary>
        /// <param name="hook">The pre-request hook action.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpRequestHookOptionsBuilder WithPreRequestAsync(Func<HttpRequestMessage, Context, CancellationToken, Task> hook)
        {
            _options.PreRequestAsync = hook;
            return this;
        }

        /// <summary>
        /// Sets the asynchronous action to execute after receiving an HTTP response.
        /// </summary>
        /// <param name="hook">The post-response hook action.</param>
        /// <returns>The builder for chaining.</returns>
        public HttpRequestHookOptionsBuilder WithPostResponseAsync(Func<HttpResponseMessage, Context, CancellationToken, Task> hook)
        {
            _options.PostResponseAsync = hook;
            return this;
        }

        /// <summary>
        /// Validates that both pre-request and post-response hooks are set.
        /// Throws if either hook is not set.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a required hook is not set.</exception>
        public HttpRequestHookOptionsBuilder Validate()
        {
            if (_options.PreRequestAsync is null) throw new InvalidOperationException("Pre-request async hook is not set.");
            if (_options.PostResponseAsync is null) throw new InvalidOperationException("Post-response async hook is not set.");

            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="HttpRequestHookOptions"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="HttpRequestHookOptions"/>.</returns>
        public HttpRequestHookOptions Build()
        {
            Validate();
            return _options;
        }
    }
}
