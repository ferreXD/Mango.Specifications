namespace Mango.Http.IntegrationTests
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using System;

    public static class MangoHttpClientHandlerExtensions
    {
        /// <summary>
        /// Overrides the primary HTTP message handler for the named Mango HTTP client.
        /// </summary>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <param name="handlerFactory">A factory that creates the primary <see cref="HttpMessageHandler"/>.</param>
        /// <returns>The same <see cref="IMangoHttpClientBuilder"/> for chaining.</returns>
        public static IMangoHttpClientBuilder ConfigurePrimaryHttpMessageHandler(
            this IMangoHttpClientBuilder builder,
            Func<HttpMessageHandler> handlerFactory)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (handlerFactory == null) throw new ArgumentNullException(nameof(handlerFactory));

            // Configure the underlying HttpClientFactoryOptions for this named client
            builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
            {
                // When the HttpMessageHandlerBuilder is created, set its PrimaryHandler
                options.HttpMessageHandlerBuilderActions.Add(msgBuilder =>
                {
                    msgBuilder.PrimaryHandler = handlerFactory();
                });
            });

            return builder;
        }
    }
}
