// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Abstract authentication strategy that provides OpenTelemetry instrumentation and logging for Mango HTTP clients.
    /// Inherit from this class to implement custom authentication logic with diagnostics and activity tracing.
    /// </summary>
    /// <remarks>
    /// This base class wraps authentication logic with activity tracing, logging, and error handling for diagnostics.
    /// </remarks>
    public abstract class InstrumentedAuthStrategy : IAuthenticationStrategy
    {
        /// <summary>
        /// Gets the logger used for diagnostics and telemetry.
        /// </summary>
        protected readonly ILogger Logger;
        private readonly string _strategyName;
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentedAuthStrategy"/> class.
        /// </summary>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if logger or activitySource is null.</exception>
        protected InstrumentedAuthStrategy(ActivitySource activitySource, ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
            _strategyName = GetType().Name;
        }

        /// <summary>
        /// Applies authentication to the specified HTTP request, with instrumentation and logging.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation (optional).</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="MangoAuthenticationException">Thrown if authentication fails.</exception>
        public async ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            using var activity = _activitySource.StartActivity($"{_strategyName}.Apply", ActivityKind.Internal)
                ?.AddTag("auth.strategy", _strategyName);
            Logger.LogDebug("Starting {Strategy}", _strategyName);

            var sw = Stopwatch.StartNew();
            try
            {
                await InnerApplyAsync(request, ct).ConfigureAwait(false);
                sw.Stop();
                Logger.LogInformation(
                    "{Strategy} applied in {Elapsed}ms",
                    _strategyName, sw.ElapsedMilliseconds);

                if (activity?.IsAllDataRequested == true)
                {
                    activity.SetTag("auth.strategy.elapsed", sw.ElapsedMilliseconds);
                    activity.SetTag("auth.strategy.success", true);
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogWarning("{Strategy} canceled", _strategyName);
                if (activity?.IsAllDataRequested == true)
                {
                    activity.SetTag("auth.strategy.error", ex.GetType().Name);
                    activity.SetTag("auth.strategy.error.message", ex.Message);
                    activity.SetTag("auth.strategy.success", false);
                }
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Strategy} failed", _strategyName);
                if (activity?.IsAllDataRequested == true)
                {
                    activity.SetTag("auth.strategy.error", ex.GetType().Name);
                    activity.SetTag("auth.strategy.error.message", ex.Message);
                    activity.SetTag("auth.strategy.success", false);
                }

                throw new MangoAuthenticationException(
                    $"{_strategyName} failed to apply authentication.", ex);
            }
        }

        /// <summary>
        /// When implemented in a derived class, applies authentication logic to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        protected abstract ValueTask InnerApplyAsync(HttpRequestMessage request, CancellationToken ct);
    }
}
