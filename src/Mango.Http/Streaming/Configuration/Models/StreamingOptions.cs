// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using System;

    public sealed record StreamingOptions
    {
        public bool EnableIdleTimeout { get; set; } = false;
        public TimeSpan? IdleReadTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public static StreamingOptions TransientHttpDefaults => new()
        {
            EnableIdleTimeout = false,
            IdleReadTimeout = TimeSpan.FromSeconds(30)
        };
    }
}
