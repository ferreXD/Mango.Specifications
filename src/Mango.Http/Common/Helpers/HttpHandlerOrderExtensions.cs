namespace Mango.Http.Common.Helpers
{
    using Authorization;
    using Headers;
    using Hooks;
    using Logging;
    using Metrics;
    using Resiliency;
    using System.Collections.Generic;

    internal static class HttpHandlerOrderExtensions
    {
        /// <summary>
        /// Inserts the handler into the list so that it ends up *before* any handler
        /// whose order in the MangoHttpHandlerOrder enum is strictly greater.
        /// Existing handlers at or after that spot are automatically shifted right.
        /// </summary>
        internal static void InsertByOrder(
            this IList<DelegatingHandler> handlers,
            DelegatingHandler handlerToInsert,
            MangoHttpHandlerOrder order) => handlers.InsertByOrder(handlerToInsert, (int)order);

        internal static void InsertByOrder(
            this IList<DelegatingHandler> handlers,
            DelegatingHandler handlerToInsert,
            int order)
        {
            var insertAt = 0;
            // scan until we find a handler whose enum-order > ours
            for (; insertAt < handlers.Count; insertAt++)
            {
                var existingOrder = (int)GetOrder(handlers[insertAt]);
                if (existingOrder > order)
                    break;
            }

            handlers.Insert(insertAt, handlerToInsert);
        }

        /// <summary>
        /// Map a handler instance back to its enum position.
        /// </summary>
        private static MangoHttpHandlerOrder GetOrder(DelegatingHandler handler)
            => handler switch
            {
                HttpAuthenticationHandler _ => MangoHttpHandlerOrder.Authentication,
                HttpHeadersInjectionHandler _ => MangoHttpHandlerOrder.Headers,
                HttpLoggingHandler _ => MangoHttpHandlerOrder.Logging,
                MetricsHandler _ => MangoHttpHandlerOrder.Metrics,
                MangoPolicyHandler _ => MangoHttpHandlerOrder.Resiliency,
                MangoHttpHookHandler _ => MangoHttpHandlerOrder.Hooks,
                _ => MangoHttpHandlerOrder.Default
            };
    }
}
