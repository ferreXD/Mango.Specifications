// ReSharper disable once CheckNamespace
namespace Mango.Http.Diagnostics
{
    using Polly;
    using System;

    internal sealed class NoOpDiagnostics : IResiliencyDiagnostics
    {
        public void OnRetry(HttpRequestMessage _, int __, Exception? ___) { }
        public void OnTimeout(HttpRequestMessage _, TimeSpan __) { }
        public void OnCircuitBreak(HttpRequestMessage _, Exception? ___) { }
        public void OnCircuitReset(HttpRequestMessage _) { }
        public void OnBulkheadRejected(HttpRequestMessage _, Exception? __) { }
        public void OnFallback(HttpRequestMessage? _, DelegateResult<HttpResponseMessage> __) { }
    }
}
