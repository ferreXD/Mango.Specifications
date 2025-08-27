// ReSharper disable once CheckNamespace
namespace Mango.Http.Hooks
{
    using Polly;
    using System;

    /// <summary>
    /// Options for configuring HTTP request and response hooks in Mango HTTP clients.
    /// Use this class to specify asynchronous actions to run before requests and after responses.
    /// </summary>
    public sealed class HttpRequestHookOptions
    {
        /// <summary>
        /// Gets or sets the asynchronous action to execute before sending an HTTP request.
        /// </summary>
        /// <remarks>
        /// The default action does nothing and completes immediately.
        /// </remarks>
        public Func<HttpRequestMessage, Context, CancellationToken, Task> PreRequestAsync { get; set; } = (_, _, _) => Task.CompletedTask;

        /// <summary>
        /// Gets or sets the asynchronous action to execute after receiving an HTTP response.
        /// </summary>
        /// <remarks>
        /// The default action does nothing and completes immediately.
        /// </remarks>
        public Func<HttpResponseMessage, Context, CancellationToken, Task> PostResponseAsync { get; set; } = (_, _, _) => Task.CompletedTask;
    }
}
