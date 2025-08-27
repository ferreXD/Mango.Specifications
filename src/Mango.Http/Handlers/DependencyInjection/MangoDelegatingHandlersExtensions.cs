// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Http;
    using Mango.Http;
    using Mango.Http.Common.Helpers;
    using System;

    /// <summary>
    /// Extension methods for adding custom delegating handlers to Mango HTTP clients.
    /// Use these methods to insert handlers at specific positions in the HTTP client pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.WithHandler<MyCustomHandler>(order: 2);
    /// builder.WithHandler(order: 3, handlerType: typeof(MyCustomHandler));
    /// </code>
    /// </example>
    public static class MangoDelegatingHandlersExtensions
    {
        /// <summary>
        /// Adds a delegating handler of type <typeparamref name="THandler"/> to the Mango HTTP client pipeline at the specified order.
        /// </summary>
        /// <typeparam name="THandler">The type of delegating handler to add.</typeparam>
        /// <param name="builder">The Mango HTTP client builder.</param>
        /// <param name="order">The position in the handler pipeline to insert the handler.</param>
        /// <returns>The Mango HTTP client builder for chaining.</returns>
        public static IMangoHttpClientBuilder WithHandler<THandler>(
            this IMangoHttpClientBuilder builder,
            int order)
            where THandler : DelegatingHandler
        {
            builder.Services.TryAddTransient<THandler>();

            var clientName = builder.Name ?? "DefaultMangoHttpClient";
            builder.Services.Configure<HttpClientFactoryOptions>(clientName, o =>
            {
                // Insert the handler at the specified position in the pipeline
                o.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.AdditionalHandlers.InsertByOrder(builder.Services.GetRequiredService<THandler>(), order);
                });
            });

            return builder;
        }

        /// <summary>
        /// Adds a delegating handler of the specified type to the Mango HTTP client pipeline at the given order.
        /// </summary>
        /// <param name="clientBuilder">The Mango HTTP client builder.</param>
        /// <param name="order">The position in the handler pipeline to insert the handler.</param>
        /// <param name="handlerType">The type of delegating handler to add.</param>
        /// <returns>The Mango HTTP client builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="handlerType"/> does not inherit from <see cref="DelegatingHandler"/>.</exception>
        public static IMangoHttpClientBuilder WithHandler(
            this IMangoHttpClientBuilder clientBuilder,
            int order,
            Type handlerType)
        {
            if (!typeof(DelegatingHandler).IsAssignableFrom(handlerType))
                throw new ArgumentException($"Type {handlerType.Name} must inherit from DelegatingHandler.");

            clientBuilder.Services.TryAddTransient(handlerType);

            var clientName = clientBuilder.Name ?? "DefaultMangoHttpClient";
            clientBuilder.Services.Configure<HttpClientFactoryOptions>(clientName, o =>
            {
                // Insert the handler at the specified position in the pipeline
                o.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.AdditionalHandlers.InsertByOrder((DelegatingHandler)builder.Services.GetRequiredService(handlerType), order);
                });
            });

            return clientBuilder;
        }
    }
}