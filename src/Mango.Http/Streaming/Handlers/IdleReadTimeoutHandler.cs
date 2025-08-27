// ReSharper disable once CheckNamespace
namespace Mango.Http.Streaming
{
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;

    public class IdleReadTimeoutHandler(IOptionsMonitor<StreamingOptions> monitor, string clientName) : DelegatingHandler
    {
        private readonly StreamingOptions _defaults = monitor.Get(clientName) ?? throw new ArgumentNullException(nameof(clientName), $"StreamingOptions not found for client: {clientName}");

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var resp = await base.SendAsync(req, ct).ConfigureAwait(false);

            // Determine if idle timeout is enabled (per request wins)
            var enabled = _defaults.EnableIdleTimeout;
            var timeout = _defaults.IdleReadTimeout;

            if (req.Options.TryGetValue(MangoStreamingConstants.IdleReadTimeoutKey, out var perReq))
            {
                enabled = true;
                timeout = perReq;
            }

            if (!enabled || resp.Content is null) return resp;

            // Wrap content lazily so normal buffered reads are cheap/no-op
            resp.Content = new IdleTimeoutHttpContent(resp.Content, timeout!.Value, ct);
            return resp;
        }
    }
}
