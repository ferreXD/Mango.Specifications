// ReSharper disable once CheckNamespace
namespace Mango.Http.Hooks
{
    using Polly;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that executes configured HTTP request and response hooks for Mango HTTP clients.
    /// Allows asynchronous actions to run before requests and after responses, with error telemetry support.
    /// </summary>
    /// <remarks>
    /// This handler should be added to the HTTP client pipeline to enable custom hook execution.
    /// </remarks>
    public sealed class MangoHttpHookHandler : DelegatingHandler
    {
        private readonly HttpRequestHookOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MangoHttpHookHandler"/> class.
        /// </summary>
        /// <param name="options">The hook options containing pre-request and post-response actions.</param>
        public MangoHttpHookHandler(HttpRequestHookOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Sends the HTTP request and executes pre-request and post-response hooks.
        /// Errors in hooks are captured in the context for telemetry.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            var context = new Context();
            context["Request"] = request;

            try
            {
                await options.PreRequestAsync(request, context, ct);
            }
            catch (Exception ex)
            {
                context[MangoHttpHooksTelemetryKeys.PreRequestError] = ex;
            }

            var response = await base.SendAsync(request, ct);

            try
            {
                await options.PostResponseAsync.Invoke(response, context, ct);
            }
            catch (Exception ex)
            {
                context[MangoHttpHooksTelemetryKeys.PostResponseError] = ex;
            }

            return response;
        }
    }
}
