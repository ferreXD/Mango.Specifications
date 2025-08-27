// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using Polly.Timeout;
    using System;

    public sealed record OperationTimeoutPolicyDefinition(
        int order = (int)DefaultPolicyOrder.TimeoutOverall)
        : ResiliencyPolicyDefinition(order), IMergeablePolicyDefinition<OperationTimeoutPolicyDefinition>
    {
        /// <summary>
        /// Recommended: max(AttemptTimeout * (RetryCount+1) + jitter, 15s)
        /// </summary>
        public TimeSpan Budget { get; init; } = TimeSpan.FromSeconds(15);
        public TimeoutStrategy Strategy { get; init; } = TimeoutStrategy.Optimistic;

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null) =>
            Policy.TimeoutAsync<HttpResponseMessage>(
                Budget,
                Strategy,
                onTimeoutAsync: async (ctx, ts, task, ex) =>
                {
                    if (diagnostics != null && ctx.TryGetRequest(out var req))
                        diagnostics.OnTimeout(req!, ts);

                    await Task.CompletedTask;
                }
            );

        public static readonly OperationTimeoutPolicyDefinition Default = new();
        public static OperationTimeoutPolicyDefinition TransientHttpDefaults => new()
        {
            Order = (int)DefaultPolicyOrder.TimeoutOverall,
            Budget = TimeSpan.FromSeconds(15),
            Strategy = TimeoutStrategy.Optimistic
        };

        public OperationTimeoutPolicyDefinition Merge(OperationTimeoutPolicyDefinition preset) => this with
        {
            Order = Order == Default.Order ? preset.Order : Order,
            Budget = Budget == Default.Budget ? preset.Budget : Budget,
            Strategy = Strategy == Default.Strategy ? preset.Strategy : Strategy
        };
    }
}
