// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Mango.Http;
    using Mango.Http.Common;
    using Mango.Http.Tracing;
    using System.Diagnostics;

    public static class MangoHttpTracingExtensions
    {
        public static IServiceCollection AddMangoHttpTracing(this IServiceCollection services)
        {
            services.TryAddSingleton<ActivitySource>(sp => new ActivitySource("MangoHttp"));
            return services;
        }

        public static IMangoHttpClientBuilder WithTracingHandler(
            this IMangoHttpClientBuilder builder)
        {
            builder.Services.AddMangoHttpTracing();
            builder.WithHandler<ActivityScopeHandler>((int)MangoHttpHandlerOrder.ActivityScope);
            return builder;
        }
    }
}
