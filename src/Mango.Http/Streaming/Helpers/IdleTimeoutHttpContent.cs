// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Response HttpContent wrapper that enforces an *idle read* timeout:
    /// it cancels reads if no bytes are received for the configured window.
    /// </summary>
    public sealed class IdleTimeoutHttpContent : HttpContent
    {
        private readonly HttpContent _inner;
        private readonly TimeSpan _idle;
        private readonly CancellationToken _outerCt;

        public IdleTimeoutHttpContent(HttpContent inner, TimeSpan idle, CancellationToken outerCt)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _idle = idle <= TimeSpan.Zero
                ? throw new ArgumentOutOfRangeException(nameof(idle), "Idle timeout must be > 0.")
                : idle;
            _outerCt = outerCt;

            // Mirror headers so callers see the same metadata
            foreach (var h in inner.Headers)
                Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        /// <summary>
        /// Used when someone buffers the content. We delegate to the inner content
        /// but still pass the outer cancellation token.
        /// </summary>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => _inner.CopyToAsync(stream, _outerCt);

#if NET8_0_OR_GREATER
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
            => _inner.CopyToAsync(stream, cancellationToken);
#endif

        /// <summary>
        /// Try to compute length from the inner headers; if unknown, return false.
        /// </summary>
        protected override bool TryComputeLength(out long length)
        {
            if (_inner.Headers.ContentLength is long l)
            {
                length = l;
                return true;
            }
            length = -1;
            return false;
        }

        /// <summary>
        /// The core hook: when a stream is requested, wrap it with the idle watchdog.
        /// </summary>
        protected override async Task<Stream> CreateContentReadStreamAsync()
        {
            var s = await _inner.ReadAsStreamAsync(_outerCt).ConfigureAwait(false);
            return new IdleTimeoutStream(s, _idle, _outerCt);
        }

        /// <summary>
        /// Ensure any direct ReadAsStreamAsync(path) also goes through our wrapper.
        /// </summary>
        public new Task<Stream> ReadAsStreamAsync() => CreateContentReadStreamAsync();

#if NET6_0_OR_GREATER
        public new Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken)
            => CreateContentReadStreamAsync();
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose inner content as well
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
