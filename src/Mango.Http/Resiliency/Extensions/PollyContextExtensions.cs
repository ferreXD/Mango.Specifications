// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;

    /// <summary>
    /// Provides keys for storing and retrieving values in Polly <see cref="Context"/>.
    /// </summary>
    public static class PollyContextKeys
    {
        /// <summary>
        /// The key for storing the <see cref="HttpRequestMessage"/> in the Polly context.
        /// </summary>
        public const string Request = nameof(Request);
        /// <summary>
        /// The key for storing the <see cref="CancellationToken"/> in the Polly context.
        /// </summary>
        public const string CancellationToken = nameof(CancellationToken);
    }

    /// <summary>
    /// Extension methods for storing and retrieving HTTP request and cancellation token in Polly <see cref="Context"/>.
    /// </summary>
    public static class PollyContextExtensions
    {
        /// <summary>
        /// Stores the HTTP request message in the Polly context.
        /// </summary>
        /// <param name="ctx">The Polly context.</param>
        /// <param name="req">The HTTP request message to store.</param>
        public static void SetRequest(this Context ctx, HttpRequestMessage req)
            => ctx[PollyContextKeys.Request] = req;

        /// <summary>
        /// Attempts to retrieve the HTTP request message from the Polly context.
        /// </summary>
        /// <param name="ctx">The Polly context.</param>
        /// <param name="req">When this method returns, contains the HTTP request message if found; otherwise, null.</param>
        /// <returns>True if the request was found; otherwise, false.</returns>
        public static bool TryGetRequest(this Context ctx, out HttpRequestMessage? req)
        {
            req = null;
            return ctx.TryGetValue(PollyContextKeys.Request, out var obj) && (req = obj as HttpRequestMessage) != null;
        }

        /// <summary>
        /// Stores the cancellation token in the Polly context.
        /// </summary>
        /// <param name="ctx">The Polly context.</param>
        /// <param name="ct">The cancellation token to store.</param>
        public static void SetCancellation(this Context ctx, CancellationToken ct)
            => ctx[PollyContextKeys.CancellationToken] = ct;

        /// <summary>
        /// Attempts to retrieve the cancellation token from the Polly context.
        /// </summary>
        /// <param name="ctx">The Polly context.</param>
        /// <param name="token">When this method returns, contains the cancellation token if found; otherwise, null.</param>
        /// <returns>True if the cancellation token was found; otherwise, false.</returns>
        public static bool TryGetCancellationToken(this Context ctx, out CancellationToken? token)
        {
            token = null;
            if (!ctx.TryGetValue(PollyContextKeys.CancellationToken, out var obj) || obj is not CancellationToken ct) return false;

            token = ct;
            return true;
        }

        public static bool TryGetBoolean(this Context ctx, string key, out bool value)
        {
            value = false;
            if (!ctx.TryGetValue(key, out var obj) || obj is not bool b) return false;
            value = b;
            return true;
        }
    }
}
