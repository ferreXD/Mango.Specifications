// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using System;

    public sealed record CircuitBreakerPolicyDefinition(int order = (int)DefaultPolicyOrder.CircuitBreaker) : ResiliencyPolicyDefinition(order), IMergeablePolicyDefinition<CircuitBreakerPolicyDefinition>
    {
        public int FailureThreshold { get; init; } = 5;
        public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);
        public Func<DelegateResult<HttpResponseMessage>, bool>? ShouldBreak { get; init; }

        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
            => Policy<HttpResponseMessage>
                .HandleResult(r => ShouldBreak?.Invoke(new DelegateResult<HttpResponseMessage>(r)) ?? !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: FailureThreshold,
                    durationOfBreak: BreakDuration,
                    onBreak: (outcome, ts, context) =>
                    {
                        if (diagnostics != null && context.TryGetRequest(out var request))
                            diagnostics.OnCircuitBreak(request!, outcome.Exception);
                    },
                    onReset: context =>
                    {
                        if (diagnostics != null && context.TryGetRequest(out var request))
                            diagnostics.OnCircuitReset(request!);
                    });

        public static readonly CircuitBreakerPolicyDefinition Default = new();

        public CircuitBreakerPolicyDefinition Merge(
            CircuitBreakerPolicyDefinition preset)
        {
            return this with
            {
                Order = Order == Default.Order ? preset.Order : Order,
                FailureThreshold = FailureThreshold == Default.FailureThreshold ? preset.FailureThreshold : FailureThreshold,
                BreakDuration = BreakDuration == Default.BreakDuration ? preset.BreakDuration : BreakDuration,
                ShouldBreak = ShouldBreak ?? preset.ShouldBreak
            };
        }
    }
}
