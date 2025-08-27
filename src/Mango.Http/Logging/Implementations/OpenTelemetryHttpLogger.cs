// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Tracing;

    /// <summary>
    /// Implementation of <see cref="BaseDefaultHttpLogger"/> that records HTTP client logging events using OpenTelemetry.
    /// Provides activity-based tracing for requests, responses, and errors.
    /// </summary>
    public sealed class OpenTelemetryHttpLogger : BaseDefaultHttpLogger
    {
        private readonly ActivitySource _activitySource;
        private readonly string _prefix;
        private static readonly HttpRequestOptionsKey<bool> StartedByLoggerKey = new("MangoHttp.ActivityByLogger");

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTelemetryHttpLogger"/> class.
        /// </summary>
        /// <param name="activitySource">The activity source for OpenTelemetry tracing.</param>
        /// <param name="logger">The logger used for HTTP events.</param>
        /// <param name="optionsMonitor">The options monitor for logging configuration.</param>
        /// <param name="clientName">The name of the HTTP client.</param>
        public OpenTelemetryHttpLogger(ActivitySource activitySource, ILogger<OpenTelemetryHttpLogger> logger, IOptionsMonitor<HttpLoggingOptions> optionsMonitor, string clientName) : base(logger, optionsMonitor, clientName)
        {
            _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
            _prefix = Opts.ActivityEventPrefix;
        }

        /// <summary>
        /// Logs the HTTP request event and starts an OpenTelemetry activity.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task LogRequestAsync(HttpRequestMessage request)
        {
            if (!(Opts.Enabled && (Opts.Condition?.Invoke(request) ?? true)))
                return Task.CompletedTask;

            // If nobody is listening, don’t create an Activity
            if (_activitySource.HasListeners())
            {
                request.Options.TryGetValue(MangoTracingConstants.ActivityKey, out var activity);
                if (activity is null) activity = Activity.Current;

                if (activity?.IsAllDataRequested == true)
                {
                    var u = request.RequestUri!;

                    activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpMethod, request.Method.Method);
                    activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpUrl, request.RequestUri?.ToString());
                    activity.SetTag("url.scheme", u.Scheme);
                    activity.SetTag("server.address", u.Host);

                    if (!u.IsDefaultPort) activity.SetTag("server.port", u.Port);
                    activity.SetTag("url.path", u.AbsolutePath);
                }
            }

            return base.LogRequestAsync(request);
        }

        /// <summary>
        /// Logs the HTTP response event and records response details in the current OpenTelemetry activity.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task LogResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
        {
            _ = request.Options.TryGetValue(MangoTracingConstants.ActivityKey, out var activity);
            if (!(Opts.Enabled && (Opts.Condition?.Invoke(request) ?? true))) return;
            
            var code = (int)response.StatusCode;

            if (activity?.IsAllDataRequested == true)
            {
                activity.SetTag("http.status_code", code);
                if (code >= 500 || (Opts.OtelSetErrorOn4xx && code >= 400))
                {
                    activity.SetStatus(ActivityStatusCode.Error);
                    activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpFailure, $"ERROR - {code} {Enum.GetName(typeof(System.Net.HttpStatusCode), response.StatusCode)}");
                    activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpFailureReason, response.ReasonPhrase);
                }
                else
                {
                    // Ensure we override any earlier "Error" from misbehaving code
                    activity.SetStatus(ActivityStatusCode.Ok);
                }

                activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpResponseStatusCode, (int)response.StatusCode);
                activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpResponseElapsed, elapsed.TotalMilliseconds);
                if (Opts.LogResponseBody && response.Content != null)
                {
                    var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (text.Length > Opts.MaxBodyLength)
                        text = text.Substring(0, Opts.MaxBodyLength) + "...";
                    activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpResponseBody, text);
                }
            }

            await base.LogResponseAsync(request, response, elapsed);
        }

        /// <summary>
        /// Logs an error event and records error details in the current OpenTelemetry activity.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="elapsed">The elapsed time for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task LogErrorAsync(HttpRequestMessage request, Exception ex, TimeSpan elapsed)
        {
            _ = request.Options.TryGetValue(MangoTracingConstants.ActivityKey, out var activity);
            if (!(Opts.Enabled && (Opts.Condition?.Invoke(request) ?? true))) return Task.CompletedTask;

            if (activity?.IsAllDataRequested == true)
            {
                activity.SetStatus(ActivityStatusCode.Error);

                activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpFailure, $"ERROR - {ex.GetType().Name}");
                activity.SetTag(MangoHttpLoggerTelemetryKeys.HttpFailureReason, ex.Message);
            }

            return base.LogErrorAsync(request, ex, elapsed);
        }
    }
}
