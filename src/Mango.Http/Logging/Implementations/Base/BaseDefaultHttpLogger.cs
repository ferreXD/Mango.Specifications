// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of <see cref="IMangoHttpLogger"/> for Mango HTTP clients.
    /// Provides common logic for logging requests, responses, and errors, including header and body formatting.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to customize logging behavior for HTTP events.
    /// </remarks>
    public abstract class BaseDefaultHttpLogger : IMangoHttpLogger
    {
        protected readonly HttpLoggingOptions Opts;
        protected readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDefaultHttpLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger used for HTTP events.</param>
        /// <param name="optionsMonitor">The options monitor for logging configuration.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <exception cref="ArgumentNullException">Thrown if options for the client are not found.</exception>
        public BaseDefaultHttpLogger(ILogger logger, IOptionsMonitor<HttpLoggingOptions> optionsMonitor, string clientName)
        {
            this.logger = logger;
            Opts = optionsMonitor.Get(clientName) ?? throw new ArgumentNullException(nameof(clientName), $"HttpLoggingOptions not found for client: {clientName}");
        }

        /// <summary>
        /// Logs the HTTP request event, including headers and optionally the body.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task LogRequestAsync(HttpRequestMessage request)
        {
            if (!ShouldLog(request, Opts.RequestLevel)) return Task.CompletedTask; ;
            var headers = FormatHeaders(request.Headers, Opts.ExcludedHeaders);
            var contentHeaders = request.Content is not null
                ? FormatHeaders(request.Content.Headers, Opts.ExcludedHeaders)
                : string.Empty;

            string? body = null;
            if (Opts.LogRequestBody && request.Content != null && IsText(request.Content))
            {
                request.Content = CloneContentWithBuffer(request.Content!, Opts.MaxBodyLength, out body);
            }

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                [MangoHttpLoggerTelemetryKeys.HttpMethod] = request.Method.Method,
                [MangoHttpLoggerTelemetryKeys.HttpUrl] = request.RequestUri?.ToString()
            }))
            {
                logger.Log(Opts.RequestLevel,
                    "Request {Method} {Url} | Headers: {Headers} {ContentHeaders} | Body: {Body}",
                    request.Method, request.RequestUri, headers, contentHeaders, body);

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Logs the HTTP response event, including headers and optionally the body.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task LogResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
        {
            var level = ResponseClassificationUtil.Classify(response, Opts);

            if (!ShouldLog(request, level)) return Task.CompletedTask;
            var headers = FormatHeaders(response.Headers, Opts.ExcludedHeaders);
            var contentHeaders = response.Content is not null
                ? FormatHeaders(response.Content.Headers, Opts.ExcludedHeaders)
                : string.Empty;

            string? body = null;
            if (Opts.LogResponseBody && response.Content is not null && IsText(response.Content))
            {
                response.Content = CloneContentWithBuffer(response.Content!, Opts.MaxBodyLength, out body);
            }

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                [MangoHttpLoggerTelemetryKeys.HttpMethod] = request.Method.Method,
                [MangoHttpLoggerTelemetryKeys.HttpUrl] = request.RequestUri?.ToString(),
                [MangoHttpLoggerTelemetryKeys.HttpResponseStatusCode] = (int)response.StatusCode,
                [MangoHttpLoggerTelemetryKeys.HttpResponseElapsed] = elapsed.TotalMilliseconds
            }))
            {
                logger.Log(level,
                    "HTTP {Method} {Uri} -> {Status} in {Elapsed}ms | Headers: {Headers} {ContentHeaders} | Body: {Body}",
                    request.Method.Method, request.RequestUri, response.StatusCode, elapsed.TotalMilliseconds, headers, contentHeaders, body);

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Logs an error event for the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task LogErrorAsync(HttpRequestMessage request, Exception ex, TimeSpan elapsed)
        {
            if (!ShouldLog(request, Opts.ErrorLevel)) return Task.CompletedTask;
            using (logger.BeginScope(new Dictionary<string, object?>
            {
                [MangoHttpLoggerTelemetryKeys.HttpMethod] = request.Method.Method,
                [MangoHttpLoggerTelemetryKeys.HttpUrl] = request.RequestUri?.ToString(),
                [MangoHttpLoggerTelemetryKeys.HttpFailureElapsed] = elapsed.TotalMilliseconds,
                [MangoHttpLoggerTelemetryKeys.HttpFailureReason] = ex.GetType().Name
            }))
            {
                logger.Log(Opts.ErrorLevel, ex,
                    "HTTP {Method} {Uri} -> after {Elapsed}ms: {Message}",
                    request.Method.Method, request.RequestUri, elapsed.TotalMilliseconds, ex.Message);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines whether logging should occur for the given request based on options.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="level">The log level to check against.</param>
        /// <returns>True if logging should occur; otherwise, false.</returns>
        private bool ShouldLog(HttpRequestMessage request, LogLevel level)
        {
            return Opts.Enabled && (Opts.Condition?.Invoke(request) ?? true) && logger.IsEnabled(level);
        }

        /// <summary>
        /// Formats HTTP headers for logging, excluding specified headers.
        /// </summary>
        /// <param name="headers">The HTTP headers to format.</param>
        /// <param name="excluded">The set of headers to exclude from logging.</param>
        /// <returns>A formatted string of headers.</returns>
        private static string FormatHeaders(HttpHeaders headers, ISet<string> excluded)
        {
            var list = headers
                .Where(h => !excluded.Contains(h.Key))
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");

            return string.Join("; ", list);
        }

        private static StreamContent CloneContentWithBuffer(HttpContent src, int maxBodyChars, out string? text)
        {
            var buffer = new MemoryStream();
            // Copy the original content into buffer
            src.CopyToAsync(buffer).GetAwaiter().GetResult(); // already on a background thread; if you prefer, make the caller async/await
            buffer.Position = 0;

            // Read for logging
            using var reader = new StreamReader(buffer, leaveOpen: true);
            text = reader.ReadToEnd();
            if (text.Length > maxBodyChars) text = text[..maxBodyChars] + "...";
            buffer.Position = 0;

            // Replace with buffered content and copy headers
            var copy = new StreamContent(buffer);
            foreach (var h in src.Headers)
                copy.Headers.TryAddWithoutValidation(h.Key, h.Value);

            return copy; // buffer will be disposed when copy is disposed
        }

        private bool IsText(HttpContent c) =>
            c.Headers.ContentType?.MediaType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true
            || c.Headers.ContentType?.MediaType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true;
    }
}
