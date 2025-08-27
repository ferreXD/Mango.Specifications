// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using System;

    public class StreamingOptionsBuilder
    {
        private bool _enableIdleTimeout = false;
        private TimeSpan? _idleReadTimeout = null;

        public StreamingOptionsBuilder EnableIdleTimeout(bool enable = true)
        {
            _enableIdleTimeout = enable;
            return this;
        }

        public StreamingOptionsBuilder SetIdleReadTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("IdleReadTimeout must be greater than zero.", nameof(timeout));
            _idleReadTimeout = timeout;
            return this;
        }

        internal StreamingOptions Build()
        {
            Validate();

            return new StreamingOptions
            {
                EnableIdleTimeout = _enableIdleTimeout,
                IdleReadTimeout = _idleReadTimeout
            };
        }

        private void Validate()
        {
            if (_enableIdleTimeout && _idleReadTimeout == null)
                throw new InvalidOperationException("If EnableIdleTimeout is true, IdleReadTimeout must be set to a non-null TimeSpan value.");
        }
    }
}
