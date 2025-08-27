// ReSharper disable once CheckNamespace
namespace Mango.Http.Tracing
{
    using System.Diagnostics;

    public static class MangoTracingConstants
    {
        public static readonly HttpRequestOptionsKey<Activity> ActivityKey = new("MangoHttp.Activity");
    }
}
