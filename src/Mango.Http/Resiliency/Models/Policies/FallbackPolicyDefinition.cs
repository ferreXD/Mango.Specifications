// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using System;
    using System.Threading.Tasks;

    public sealed record FallbackPolicyDefinition(Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> fallbackAction,
        int order = (int)DefaultPolicyOrder.Fallback) : ResiliencyPolicyDefinition(order)
    {
        private Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> _fallbackAction { get; init; }
            = fallbackAction ?? throw new InvalidOperationException("fallbackAction must be set.");

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
        {
            return Policy<HttpResponseMessage>
                .Handle<Exception>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .FallbackAsync(
                    fallbackAction: (outcome, context, ct) =>
                    {
                        context.TryGetCancellationToken(out var token);
                        return _fallbackAction(outcome, context, token ?? ct);
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        if (diagnostics != null && context.TryGetRequest(out var request))
                            diagnostics.OnFallback(request, outcome);

                        return Task.CompletedTask;
                    }
                );
        }
    }
}
