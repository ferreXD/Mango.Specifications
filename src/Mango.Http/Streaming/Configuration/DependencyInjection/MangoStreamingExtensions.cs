// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using Common;
    using Common.Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Options;
    using System;

    public static class MangoStreamingExtensions
    {
        public static IMangoHttpClientBuilder WithStreaming(
            this IMangoHttpClientBuilder b,
            Action<StreamingOptionsBuilder>? configure)
        {
            var clientName = b.Name ?? throw new InvalidOperationException("MangoHttpClient must have a name to enable logging.");

            if (configure == null) return RegisterHandler(b, clientName, StreamingOptions.TransientHttpDefaults);

            var configurator = new StreamingOptionsBuilder();
            configure!(configurator);
            var opts = configurator.Build();

            // Add named options for logging
            return RegisterHandler(b, clientName, opts);
        }

        private static IMangoHttpClientBuilder RegisterHandler(IMangoHttpClientBuilder b, string clientName, StreamingOptions opts)
        {
            b.Services.AddOptions<StreamingOptions>(clientName)
                .Configure(o =>
                {
                    o.EnableIdleTimeout = opts.EnableIdleTimeout;
                    o.IdleReadTimeout = opts.IdleReadTimeout;
                })
                .Validate(o => o is { EnableIdleTimeout: true, IdleReadTimeout: not null },
                    "If EnableIdleTimeout is true, IdleReadTimeout must be set to a non-null TimeSpan value.")
                .ValidateOnStart();

            b.Services.Configure<HttpClientFactoryOptions>(clientName, o =>
            {
                // Insert the handler at the specified position in the pipeline
                o.HttpMessageHandlerBuilderActions.Add(hb =>
                {
                    var sp = hb.Services;
                    var monitor = sp.GetRequiredService<IOptionsMonitor<StreamingOptions>>();
                    hb.AdditionalHandlers.InsertByOrder(new IdleReadTimeoutHandler(monitor, clientName), (int)MangoHttpHandlerOrder.Logging + 1);
                });
            });

            return b;
        }
    }
}
