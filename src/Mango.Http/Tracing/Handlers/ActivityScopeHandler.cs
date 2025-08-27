// ReSharper disable once CheckNamespace
namespace Mango.Http.Tracing
{
    using Logging;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public sealed class ActivityScopeHandler(ActivitySource source) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Activity? act = null;

            if (source.HasListeners())
            {
                act = (req.Options.TryGetValue(MangoTracingConstants.ActivityKey, out var existing) ? existing : null)
                      ?? Activity.Current;

                if (act is null)
                {
                    var name = $"HTTP {req.Method} {req.RequestUri?.AbsolutePath}";
                    act = source.StartActivity(name, ActivityKind.Client);
                    if (act is not null)
                    {
                        req.Options.Set(MangoTracingConstants.ActivityKey, act);
                    }
                }
                else
                {
                    req.Options.Set(MangoTracingConstants.ActivityKey, act);
                }

                if (act?.IsAllDataRequested == true)
                {
                    var u = req.RequestUri!;
                    act.SetTag("http.request.method", req.Method.Method);
                    act.SetTag("url.scheme", u.Scheme);
                    act.SetTag("server.address", u.Host);
                    if (!u.IsDefaultPort) act.SetTag("server.port", u.Port);
                    act.SetTag("url.path", u.AbsolutePath);
                }
            }

            try
            {
                return await base.SendAsync(req, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (act?.IsAllDataRequested == true)
                {
                    ActivityRecordingHelpers.RecordException(
                        MangoHttpLoggerTelemetryKeys.HttpFailure,
                        ex,
                        req);
                }
                throw;
            }
            finally
            {
                act?.Stop(); // close span after pipeline finishes
            }
        }
    }

}
