// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating handler that applies configured Mango resiliency policies to outgoing HTTP requests.
    /// Supports custom and composite policies using Polly.
    /// </summary>
    public class MangoPolicyHandler : DelegatingHandler
    {
        private readonly IResiliencyDiagnostics? _diagnostics;
        private readonly IList<ResiliencyPolicyDefinition> _definitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MangoPolicyHandler"/> class.
        /// </summary>
        /// <param name="definitions">The ordered list of resiliency policy definitions to apply.</param>
        /// <param name="diagnostics">Optional diagnostics for policy events.</param>
        public MangoPolicyHandler(
            IEnumerable<ResiliencyPolicyDefinition> definitions,
            IResiliencyDiagnostics? diagnostics)
        {
            _definitions = definitions.OrderBy(d => d).ToList();
            _diagnostics = diagnostics ?? throw new InvalidOperationException(
                "IResiliencyDiagnostics missing. Call .WithDiagnostics() or register your own.");
        }

        /// <summary>
        /// Sends the HTTP request through the configured resiliency policies.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The HTTP response message after applying resiliency policies.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            if (!_definitions.Any())
            {
                // If no policies are defined, just call the inner handler
                return base.SendAsync(request, ct);
            }

            // Seed the context with the request
            var context = new Context();
            context.SetRequest(request);
            context.SetCancellation(ct);

            // For non-idempotent methods, filter out retry policies
            var defs = _definitions;
            if (!IsIdempotent(request.Method))
                defs = defs.Where(d => d is RetryPolicyDefinition { ApplyToNonIdempotentMethods: true }).ToList();

            var customPolicy = defs
                .OfType<CustomPolicyDefinition>()
                .FirstOrDefault();

            if (customPolicy != null)
            {
                // If a custom policy is defined, execute it directly
                return customPolicy.BuildPolicy(_diagnostics).ExecuteAsync(
                    (ctx, token) => base.SendAsync(request, token), context, ct);
            }

            // Build policies, injecting diagnostics
            var policies = defs.Select(d => d.BuildPolicy(_diagnostics)).ToArray();
            if (policies.Length == 0) return base.SendAsync(request, ct);
            if (policies.Length == 1) return policies[0].ExecuteAsync((ctx, token) => base.SendAsync(request, token), context, ct);

            return Policy.WrapAsync(policies)
                .ExecuteAsync(async (ctx, token) => await ExecuteHandlingPessimisticTimeout(request, ctx, token), context, ct);
        }

        private async Task<HttpResponseMessage> ExecuteHandlingPessimisticTimeout(HttpRequestMessage request, Context ctx, CancellationToken token)
        {
            _ = ctx.TryGetCancellationToken(out var originalCt);
            // If no original token, just use the passed one
            if (originalCt == null)
            {
                return await base.SendAsync(request, token).ConfigureAwait(false);
            }

            // Create a linked CTS so either Polly’s token or our total token cancels the inner call
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, originalCt.Value);
            try
            {
                return await base.SendAsync(request, linkedCts.Token).ConfigureAwait(false);
            }
            finally
            {
                // If a pessimistic timeout triggered, Polly’s token is not cancelled automatically.
                // But our onTimeoutAsync can set a marker in Context; if set, cancel underlying.
                if (linkedCts.Token.IsCancellationRequested == false
                    && ctx.TryGetBoolean(ResiliencyConstants.PessimisticTimeoutFiredKey, out var fired) && fired)
                {
                    try { await linkedCts.CancelAsync(); } catch { /* ignore */ }
                }
            }
        }

        // For conservatism, only retry GET/HEAD/OPTIONS by default
        private static bool IsIdempotent(HttpMethod m)
            => m == HttpMethod.Get
               || m == HttpMethod.Head
               || m == HttpMethod.Options;
    }
}
