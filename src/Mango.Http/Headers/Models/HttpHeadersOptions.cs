// ReSharper disable once CheckNamespace
namespace Mango.Http.Headers
{
    using System;
    using System.Collections.Generic;

    public class HttpHeadersOptions
    {
        public Dictionary<string, Func<string?>> CustomHeaders { get; internal set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
