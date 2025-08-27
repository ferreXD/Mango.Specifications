// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using Polly.CircuitBreaker;
    using System;
    using System.Threading.Tasks;

    public sealed record FallbackOnBreakPolicyDefinition(
        Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> onBreakAction,
        int order = (int)DefaultPolicyOrder.FallbackOnBreak) : ResiliencyPolicyDefinition(order)
    {
        /// <summary>Runs when the circuit breaker is open.</summary>
        private Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> _onBreakAction { get; init; }
            = onBreakAction ?? throw new InvalidOperationException("onBreakAction must be set.");

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
            => Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackAction: (outcome, context, ct) =>
                    {
                        context.TryGetCancellationToken(out var token);
                        return _onBreakAction(outcome, context, token ?? ct);
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        if (diagnostics != null && context.TryGetRequest(out var request))
                            diagnostics.OnFallback(request, outcome);

                        return Task.CompletedTask;
                    });
    }
}
