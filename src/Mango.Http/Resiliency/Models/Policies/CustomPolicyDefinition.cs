// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;

    public sealed record CustomPolicyDefinition(int order, Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> Policy)
        : ResiliencyPolicyDefinition(order)
    {
        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics)
            => Policy.Invoke(diagnostics);
    }
}
