// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Options for configuring HTTP logging in Mango HTTP clients.
    /// Use this class to control logging behavior, levels, body capture, excluded headers, and custom logger/handler types.
    /// </summary>
    public sealed class HttpLoggingOptions
    {
        /// <summary>
        /// Master switch. Disable to turn off all logging.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Only log when predicate returns true. Null = always.
        /// </summary>
        public Func<HttpRequestMessage, bool>? Condition { get; set; }

        /// <summary>
        /// Whether to treat 5xx responses as errors.
        /// </summary>
        public bool Treat5xxAsError { get; set; } = true;

        /// <summary>
        /// Whether to treat 4xx responses as errors.
        /// </summary>
        public bool Treat4xxAsError { get; set; } = false;

        /// <summary>
        /// Whether to treat 404 responses as informational (not errors).
        /// </summary>
        public bool Treat404AsInfo { get; set; } = true;

        /// <summary>
        /// Whether to set the OpenTelemetry error flag for 5xx responses.
        /// </summary>
        public bool OtelSetErrorOn4xx { get; set; } = false;

        /// <summary>
        /// LogLevel for request start events.
        /// </summary>
        public LogLevel RequestLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// LogLevel for failures.
        /// </summary>
        public LogLevel ErrorLevel { get; set; } = LogLevel.Error;

        /// <summary>
        /// LogLevel for successful requests.
        /// </summary>
        public LogLevel ResponseSuccessLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// LogLevel for client errors (4xx).
        /// </summary>
        public LogLevel ResponseClientErrorLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// LogLevel for server errors (5xx).
        /// </summary>
        public LogLevel ResponseServerErrorLevel { get; set; } = LogLevel.Error;

        /// <summary>
        /// Maximum number of characters to capture for request/response bodies.
        /// </summary>
        public int MaxBodyLength { get; set; } = 2048;

        /// <summary>
        /// Headers to exclude from logging.
        /// </summary>
        public ISet<string> ExcludedHeaders { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Whether to capture and log request body.
        /// </summary>
        public bool LogRequestBody { get; set; } = false;

        /// <summary>
        /// Whether to capture and log response body.
        /// </summary>
        public bool LogResponseBody { get; set; } = false;

        /// <summary>
        /// Custom logger implementation. If null, DefaultHttpLogger is used.
        /// </summary>
        public Type? LoggerType { get; set; }

        /// <summary>
        /// Use a custom DelegatingHandler instead of built-in HttpLoggingHandler.
        /// </summary>
        public bool UseCustomHandler { get; set; } = false;

        /// <summary>
        /// The type of custom handler to use if <see cref="UseCustomHandler"/> is true.
        /// </summary>
        public Type? CustomHandlerType { get; set; }

        /// <summary>
        /// Optional: custom classifier function to determine log level based on response.
        /// </summary>
        public Func<HttpResponseMessage, LogLevel?>? CustomClassifier { get; set; }

        /// <summary>
        /// Optional: override the event name prefix for ActivityEvents.
        /// </summary>
        public string ActivityEventPrefix { get; set; } = "http.client";
    }
}
