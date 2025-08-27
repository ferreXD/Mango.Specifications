// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using System;

    public static class MangoStreamingConstants
    {
        public static readonly HttpRequestOptionsKey<TimeSpan> IdleReadTimeoutKey
            = new("MangoHttp.IdleReadTimeout");
    }
}
