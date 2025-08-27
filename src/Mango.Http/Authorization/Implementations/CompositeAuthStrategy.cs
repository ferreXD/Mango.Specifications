// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication strategy that composes multiple <see cref="IAuthenticationStrategy"/> instances for Mango HTTP clients.
    /// Applies each strategy in order, providing OpenTelemetry instrumentation and logging for each.
    /// </summary>
    /// <remarks>
    /// This strategy is useful for scenarios where multiple authentication mechanisms must be applied to a single request.
    /// </remarks>
    public sealed class CompositeAuthStrategy : IAuthenticationStrategy
    {
        private readonly IReadOnlyList<IAuthenticationStrategy> _strategies;
        private readonly ILogger<CompositeAuthStrategy> _logger;
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeAuthStrategy"/> class.
        /// </summary>
        /// <param name="activitySource">The activity source for OpenTelemetry instrumentation.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <param name="strategies">The collection of authentication strategies to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown if logger, activitySource, or strategies is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no strategies are provided.</exception>
        public CompositeAuthStrategy(
            ActivitySource activitySource,
            ILogger<CompositeAuthStrategy> logger,
            IEnumerable<IAuthenticationStrategy> strategies)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (strategies == null)
                throw new ArgumentNullException(nameof(strategies));

            // Ensure no nulls and make read-only snapshot
            var list = strategies.Where(s => s != null).ToList();
            if (list.Count == 0)
                throw new ArgumentException("At least one authentication strategy must be provided.", nameof(strategies));

            _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
            _strategies = list.AsReadOnly();
        }

        /// <summary>
        /// Applies all configured authentication strategies to the HTTP request in order.
        /// Each strategy is instrumented and logged individually.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation (optional).</param>
        /// <returns>A ValueTask representing the asynchronous authentication operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="MangoAuthenticationException">Thrown if any strategy fails to apply authentication.</exception>
        public async ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            foreach (var strategy in _strategies)
            {
                var strategyName = strategy.GetType().Name;

                using var activity = _activitySource.StartActivity("CompositeAuth.Apply", ActivityKind.Internal)?
                    .AddTag("auth.strategy", strategyName);

                var sw = Stopwatch.StartNew();
                try
                {
                    _logger.LogDebug("Applying auth strategy {Strategy}", strategyName);
                    await strategy.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
                    sw.Stop();
                    _logger.LogInformation(
                        "Auth strategy {Strategy} applied in {ElapsedMilliseconds}ms",
                        strategyName, sw.ElapsedMilliseconds);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning(
                        "Auth strategy {Strategy} canceled",
                        strategyName);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Auth strategy {Strategy} failed",
                        strategyName);
                    throw new MangoAuthenticationException(
                        $"Strategy {strategyName} failed to apply authentication.", ex);
                }
            }
        }
    }
}
