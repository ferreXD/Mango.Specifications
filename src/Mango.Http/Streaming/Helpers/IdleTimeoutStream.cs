// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using System;
    using System.Threading.Tasks;

    public sealed class IdleTimeoutStream : Stream
    {
        private readonly Stream _inner;
        private readonly TimeSpan _idle;
        private readonly CancellationTokenSource _idleCts;
        private readonly CancellationTokenSource _linked;

        public IdleTimeoutStream(Stream inner, TimeSpan idle, CancellationToken outer)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _idle = idle;

            _idleCts = new CancellationTokenSource();
            _linked = CancellationTokenSource.CreateLinkedTokenSource(outer, _idleCts.Token);
            _idleCts.CancelAfter(_idle); // arm immediately
        }

        private void BumpTimer() { if (!_idleCts.IsCancellationRequested) _idleCts.CancelAfter(_idle); }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _linked.Token);
            var n = await _inner.ReadAsync(buffer, linked.Token).ConfigureAwait(false);
            if (n > 0) BumpTimer(); else await DisposeAsync(); // EOF: stop watchdog
            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            => await ReadAsync(new Memory<byte>(buffer, offset, count), ct).ConfigureAwait(false);

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _idleCts.Cancel();
                _idleCts.Dispose();
                _linked.Cancel();
                _linked.Dispose();
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }

        // delegate rest
        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(long.MaxValue);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
    }
}
