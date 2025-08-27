// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using Polly.Timeout;
    using System;
    using System.Threading.Tasks;

    public sealed record AttemptTimeoutPolicyDefinition(
        int order = (int)DefaultPolicyOrder.TimeoutPerAttempt
    ) : ResiliencyPolicyDefinition(order), IMergeablePolicyDefinition<AttemptTimeoutPolicyDefinition>
    {
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
        public TimeoutStrategy Strategy { get; init; } = TimeoutStrategy.Optimistic;

        /// <summary>
        /// When using Pessimistic strategy, this indicates whether the underlying operation should be cancelled when a timeout occurs.
        /// Advanced: if Pessimistic, we cancel the inner request via a linked CTS.
        /// </summary>
        public bool CancelUnderlyingOnPessimistic { get; init; } = true;

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                Timeout,
                Strategy,
                onTimeoutAsync: async (ctx, ts, task, ex) =>
                {
                    if (Strategy == TimeoutStrategy.Pessimistic) ctx[ResiliencyConstants.PessimisticTimeoutFiredKey] = true;

                    if (diagnostics != null && ctx.TryGetRequest(out var req))
                        diagnostics.OnTimeout(req!, ts);

                    await Task.CompletedTask;
                });
        }

        public static readonly AttemptTimeoutPolicyDefinition Default = new();
        public static AttemptTimeoutPolicyDefinition TransientHttpDefaults => new()
        {
            Order = (int)DefaultPolicyOrder.TimeoutPerAttempt,
            Timeout = TimeSpan.FromSeconds(10),
            Strategy = TimeoutStrategy.Optimistic,
            CancelUnderlyingOnPessimistic = true
        };

        public AttemptTimeoutPolicyDefinition Merge(AttemptTimeoutPolicyDefinition preset) => this with
        {
            Order = Order == Default.Order ? preset.Order : Order,
            Timeout = Timeout == Default.Timeout ? preset.Timeout : Timeout,
            Strategy = Strategy == Default.Strategy ? preset.Strategy : Strategy,
            CancelUnderlyingOnPessimistic = CancelUnderlyingOnPessimistic || preset.CancelUnderlyingOnPessimistic
        };
    }
}
