// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides cached authentication tokens for Mango HTTP clients, with automatic refresh and metrics tracking.
    /// Implements <see cref="ICachedTokenProvider"/>, <see cref="ITokenCacheMetrics"/>, <see cref="IAsyncDisposable"/>, and <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// This provider caches tokens and refreshes them before expiration, using a refresh margin and thread-safe locking.
    /// </remarks>
    public sealed class CachedTokenProvider : ICachedTokenProvider, ITokenCacheMetrics, IAsyncDisposable, IDisposable
    {
        private readonly ILogger<CachedTokenProvider> _logger;
        private readonly Func<CancellationToken, ValueTask<(string Token, DateTimeOffset ExpiresAt)>> _tokenFactory;
        private readonly TimeSpan _refreshMargin;
        private readonly SemaphoreSlim _lock = new(1, 1);

        // Store expiration as UTC ticks for volatile thread-safe reads
        private long _expiresAtUtcTicks;
        private volatile string? _cachedToken;

        private long _renewalCount;
        private long _failureCount;
        private long _cancelCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedTokenProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <param name="tokenFactory">A function that asynchronously provides the token and expiration.</param>
        /// <param name="refreshMargin">Optional margin before expiration to trigger refresh (default: 30 seconds).</param>
        /// <exception cref="ArgumentNullException">Thrown if tokenFactory is null.</exception>
        public CachedTokenProvider(
            ILogger<CachedTokenProvider> logger,
            Func<CancellationToken, ValueTask<(string Token, DateTimeOffset ExpiresAt)>> tokenFactory,
            TimeSpan? refreshMargin = null)
        {
            _logger = logger;
            _tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            _refreshMargin = refreshMargin ?? TimeSpan.FromSeconds(30);
        }

        /// <inheritdoc/>
        public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            if (IsValid())
                return _cachedToken!;

            try
            {
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref _cancelCount);
                _logger.LogWarning("Token refresh canceled waiting for lock.");
                throw;
            }

            try
            {
                if (IsValid())
                    return _cachedToken!;

                try
                {
                    var (token, expiresAt) = await _tokenFactory(cancellationToken).ConfigureAwait(false);

                    _cachedToken = token;
                    // use Ticks for atomic updates
                    Interlocked.Exchange(ref _expiresAtUtcTicks, expiresAt.ToUniversalTime().UtcTicks);
                    Interlocked.Increment(ref _renewalCount);

                    _logger.LogInformation("Token refreshed; expires at {ExpiresAt} UTC.", expiresAt);
                    return token;
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Increment(ref _cancelCount);
                    _logger.LogWarning("Token factory canceled.");
                    throw;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _failureCount);
                    _logger.LogError(ex, "Token refresh failed.");
                    throw;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Invalidates the cached token, forcing a refresh on the next request.
        /// </summary>
        public void Invalidate()
        {
            _cachedToken = null!;
            Interlocked.Exchange(ref _expiresAtUtcTicks, DateTimeOffset.MinValue.Ticks);
            _logger.LogInformation("Token cache invalidated.");
        }

        /// <inheritdoc/>
        public long RenewalCount => Interlocked.Read(ref _renewalCount);
        /// <inheritdoc/>
        public long FailureCount => Interlocked.Read(ref _failureCount);
        /// <summary>
        /// Gets the number of token refresh cancellations.
        /// </summary>
        public long CancelCount => Interlocked.Read(ref _cancelCount);

        /// <summary>
        /// Determines whether the cached token is valid and not expired, considering the refresh margin.
        /// </summary>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        private bool IsValid()
        {
            var token = _cachedToken;
            if (token is null)
            {
                _logger.LogDebug("Token cache is empty.");
                return false;
            }

            var ticks = Interlocked.Read(ref _expiresAtUtcTicks);
            var expiresAtUtc = new DateTimeOffset(ticks, TimeSpan.Zero);
            var valid = DateTimeOffset.UtcNow < expiresAtUtc - _refreshMargin;

            _logger.LogDebug("Token cache valid: {Valid}, Expires at: {ExpiresAt} UTC, Current time: {CurrentTime} UTC",
                valid, expiresAtUtc, DateTimeOffset.UtcNow);

            return valid;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _lock.Dispose();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            _lock.Dispose();
            return default;
        }
    }
}
