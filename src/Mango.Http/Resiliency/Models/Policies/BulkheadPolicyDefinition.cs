// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using Polly.Timeout;
    using System.Threading.Tasks;

    public sealed record BulkheadPolicyDefinition(int order = (int)DefaultPolicyOrder.Bulkhead) : ResiliencyPolicyDefinition(order), IMergeablePolicyDefinition<BulkheadPolicyDefinition>
    {
        public int MaxParallelization { get; init; } = 64;
        public int MaxQueuing { get; init; } = 0;                 // fail-fast by default
        public TimeSpan? QueueTimeout { get; init; } = null;      // off by default

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
        {
            var bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: MaxParallelization,
                maxQueuingActions: MaxQueuing,
                onBulkheadRejectedAsync: ctx =>
                {
                    if (diagnostics != null && ctx.TryGetRequest(out var req))
                        diagnostics.OnBulkheadRejected(req!, exception: null);
                    return Task.CompletedTask;
                });

            if (QueueTimeout is { } qt && qt > TimeSpan.Zero)
            {
                var queueTimeout = Policy.TimeoutAsync<HttpResponseMessage>(
                    qt,
                    TimeoutStrategy.Optimistic,
                    onTimeoutAsync: (ctx, ts, task, ex) =>
                    {
                        // Treat as “queue timeout” in your diagnostics. Reuse OnTimeout if you don’t want a new method.
                        if (diagnostics != null && ctx.TryGetRequest(out var req))
                            diagnostics.OnTimeout(req!, ts);
                        return Task.CompletedTask;
                    });

                // OUTER timeout bounds time spent waiting to enter the bulkhead
                return Policy.WrapAsync(queueTimeout, bulkhead);
            }

            return bulkhead;
        }

        public static readonly BulkheadPolicyDefinition Default = new();
        public static BulkheadPolicyDefinition TransitentHttpDefaults => new()
        {
            Order = (int)DefaultPolicyOrder.Bulkhead,
            MaxParallelization = 64,
            MaxQueuing = 0,
            QueueTimeout = null
        };

        public BulkheadPolicyDefinition Merge(BulkheadPolicyDefinition preset) => this with
        {
            Order = Order == Default.Order ? preset.Order : Order,
            MaxParallelization = MaxParallelization == Default.MaxParallelization ? preset.MaxParallelization : MaxParallelization,
            MaxQueuing = MaxQueuing == Default.MaxQueuing ? preset.MaxQueuing : MaxQueuing,
            QueueTimeout = QueueTimeout ?? preset.QueueTimeout
        };
    }
}
