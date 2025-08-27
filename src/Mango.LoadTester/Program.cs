using Mango.Http.Defaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using Serilog;
using System.Net;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// --- ThreadPool priming (avoid ramp starvation)
ThreadPool.GetMinThreads(out var wt, out var ct);
ThreadPool.SetMinThreads(Math.Max(wt, 200), Math.Max(ct, 200));

// Hard reset OTel env overrides that can silently redirect/override your code options:
string[] vars =
[
    "OTEL_EXPORTER_OTLP_ENDPOINT",
    "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT",
    "OTEL_EXPORTER_OTLP_PROTOCOL",
    "OTEL_EXPORTER_OTLP_TRACES_PROTOCOL",
    "OTEL_EXPORTER_OTLP_HEADERS",
    "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "NO_PROXY"
];
foreach (var v in vars) Environment.SetEnvironmentVariable(v, null);

//var serviceName = "Mango.LoadTest";
//var serviceVersion = "0.0.0";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .WriteTo.Async(a => a.File(
        @"C:\Users\pabloff\source\repos\MangoLoadTester\Logs\Tester.log",
        rollingInterval: RollingInterval.Day,
        buffered: true,                 // let StreamWriter buffer
        shared: false,                   // safe across processes if needed
        flushToDiskInterval: TimeSpan.FromSeconds(2)))
    .CreateLogger();

using var host = Host.CreateDefaultBuilder()
    .UseSerilog()
    .ConfigureServices((_, services) =>
    {
        // 1) OpenTelemetry (one exporter → OTLP → Collector)
        //services.AddOpenTelemetry()
        //    .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        //    .WithTracing(t => t
        //        .AddSource("MangoHttp") // Mango.Http ActivitySource (if present)
        //        .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.1)))
        //        .AddOtlpExporter(o =>
        //        {
        //            o.Protocol = OtlpExportProtocol.Grpc;
        //            o.Endpoint = new Uri("http://127.0.0.1:14317"); // explicit path
        //            o.ExportProcessorType = ExportProcessorType.Batch;
        //            o.BatchExportProcessorOptions = new()
        //            {
        //                MaxQueueSize = 65536,
        //                MaxExportBatchSize = 2048,          // a little bigger = fewer flushes
        //                ScheduledDelayMilliseconds = 200,   // 200–500ms is a nice middle ground
        //                ExporterTimeoutMilliseconds = 10000
        //            };
        //        }))
        //    .WithMetrics(m => m
        //        .AddMeter("Mango.Http.Client") // Mango.Http Meter (if present)
        //        .AddOtlpExporter(o =>
        //        {
        //            // OTLP/HTTP to the collector (base endpoint, let the exporter append /v1/metrics)
        //            o.Protocol = OtlpExportProtocol.Grpc;
        //            o.Endpoint = new Uri("http://127.0.0.1:14317");
        //        }));
    })
    .Build();

var opts = ParseArgs(args);
var hostOnly = new Uri($"{opts.TargetUrl.Scheme}://{opts.TargetUrl.Authority}");
var path = opts.TargetUrl.IsAbsoluteUri ? opts.TargetUrl.PathAndQuery : opts.TargetUrl.ToString();

Console.WriteLine($"Target: {opts.TargetUrl}");
Console.WriteLine($"Mode: {opts.Mode}, Copies: {opts.Concurrency}, Duration: {opts.DurationSeconds}s\n");

await host.StartAsync();

// Decide which modes to run
var modes = ResolveModes(opts.Mode);
foreach (var mode in modes)
{
    Console.WriteLine($"\n=== Running mode: {mode} ===\n");

    var handler = CreateHandler();                // NEW handler per mode
    var client = BuildClientForMode(mode, hostOnly, handler);

    var scenario = BuildScenario(mode, client, path, opts.Concurrency, opts.DurationSeconds);

    NBomberRunner
        .RegisterScenarios(scenario)
        .WithReportFolder($"./nbomber-reports/{mode}")
        .WithReportFormats(ReportFormat.Md, ReportFormat.Html)
        .Run();

    client.Dispose();                              // dispose after the run
    handler.Dispose();
}

await host.StopAsync();

// ===================== helpers =====================
static SocketsHttpHandler CreateHandler() => new SocketsHttpHandler
{
    UseProxy = false,
    Proxy = null,
    AllowAutoRedirect = false,
    UseCookies = false,
    AutomaticDecompression = DecompressionMethods.None,
    MaxConnectionsPerServer = int.MaxValue,
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(10),
    ConnectTimeout = TimeSpan.FromSeconds(2)
};

static HttpClient BuildRawClient(Uri hostOnly, SocketsHttpHandler handler)
{
    return new HttpClient(handler, disposeHandler: false)
    {
        BaseAddress = hostOnly,
        DefaultRequestVersion = HttpVersion.Version11,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
    };
}

static HttpClient BuildMangoClientMinimal(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    var services = new ServiceCollection();

    // Register the named client with base address via Mango registration
    services
        .AddSerilog() // Serilog is required for Mango logging
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        // No policies: measure bare Mango pipeline glue
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    // Ensure our handler is used
    services.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    var sp = services.BuildServiceProvider();
    return sp.GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoClientPolicies(string mode, Uri hostOnly, SocketsHttpHandler handler, bool addMetrics)
{
    var services = new ServiceCollection();

    services
        .AddSerilog() // Serilog is required for Mango logging
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .WithResiliency(cfg => cfg
            .WithTimeout(t => t.SetTimeout(TimeSpan.FromSeconds(10)))
            .WithRetry(r => r.SetMaxRetryCount(1))                            // no retries in success path
            .WithBulkhead(b => b.SetMaxParallelization(10_000).SetMaxQueueLength(0))
            // keep circuit breaker inert for a success-only baseline
            .WithCircuitBreaker(cb => cb.SetFailureThreshold(int.MaxValue).SetBreakDuration(TimeSpan.FromSeconds(1)))
        )
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    if (addMetrics)
        services.PostConfigureAll<HttpClientFactoryOptions>(_ => { }); // placeholder to keep services non-empty

    if (addMetrics)
        services.AddMangoHttpClient(mode) // acquire builder again to append metrics to same named client
                .WithMetrics(m => m.Enable());

    services.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    var sp = services.BuildServiceProvider();
    return sp.GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoOtelClientWithExport(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    var services = new ServiceCollection();

    services.AddSerilog().AddMangoHttpClientWithOpenTelemetry(mode, c =>
    {
        c.BaseAddress = hostOnly;
        c.DefaultRequestVersion = HttpVersion.Version11;
        c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
    })
    .WithResiliency(cfg => cfg
        .WithTimeout(t => t.SetTimeout(TimeSpan.FromSeconds(10)))
        .WithRetry(r => r
            .SetMaxRetryCount(3)
            .SetUseJitter()
            .SetDelay(TimeSpan.FromMilliseconds(50))
            .SetShouldRetryCondition(dr =>
                dr.Exception is HttpRequestException ||
                (dr.Result is { } res && (
                   (int)res.StatusCode == 408 ||
                   (int)res.StatusCode == 429 ||
                   ((int)res.StatusCode >= 500 && res.StatusCode != HttpStatusCode.NotImplemented && res.StatusCode != HttpStatusCode.HttpVersionNotSupported)
                ))
            )
        )
        .WithCircuitBreaker(cb => cb
            .SetFailureThreshold(5)                    // open fast
            .SetBreakDuration(TimeSpan.FromSeconds(5)))
        .WithBulkhead(b => b.SetMaxParallelization(10_000).SetMaxQueueLength(0))
    )
    .WithTracingHandler()
    .WithDiagnostics()
    .WithLogging(l => l.WithDefaultOpenTelemetry().LogRequestBody(false).LogResponseBody(false))
    .WithMetrics(m => m.Enable());

    services.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    var sp = services.BuildServiceProvider();
    return sp.GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoError_Min(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    var sc = new ServiceCollection();

    sc
        .AddSerilog()
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        // diagnostics only, no resiliency => measure bare failures
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    sc.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    return sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoError_Retry(string mode, Uri hostOnly, SocketsHttpHandler handler, int retries)
{
    var sc = new ServiceCollection();
    sc
        .AddSerilog()
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .WithResiliency(cfg => cfg
            .WithTimeout(t => t.SetTimeout(TimeSpan.FromMilliseconds(300)))
            .WithRetry(r => r
                .SetMaxRetryCount(retries)
                .SetUseJitter()
                .SetDelay(TimeSpan.FromMilliseconds(50))
                .SetShouldRetryCondition(dr =>
                    dr.Exception is HttpRequestException ||
                    (dr.Result is { } res && (
                        (int)res.StatusCode == 408 ||
                        (int)res.StatusCode == 429 ||
                        ((int)res.StatusCode >= 500 && res.StatusCode != HttpStatusCode.NotImplemented && res.StatusCode != HttpStatusCode.HttpVersionNotSupported)
                    ))
                )
        ))
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    sc.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    return sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoError_Breaker(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    var sc = new ServiceCollection();
    sc
        .AddSerilog()
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .WithResiliency(cfg => cfg
            .WithTimeout(t => t.SetTimeout(TimeSpan.FromMilliseconds(300)))
            .WithCircuitBreaker(cb => cb
                .SetFailureThreshold(10)                    // open fast
                .SetBreakDuration(TimeSpan.FromSeconds(10)))
        // no retries: we want to observe “fast-fail” once breaker opens
        )
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    sc.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    return sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildMangoError_Bulkhead(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    var sc = new ServiceCollection();
    sc
        .AddSerilog()
        .AddMangoDefaultHttpLogger()
        .AddMangoHttpClient(mode, c =>
        {
            c.BaseAddress = hostOnly;
            c.DefaultRequestVersion = HttpVersion.Version11;
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .WithResiliency(cfg => cfg
            .WithTimeout(t => t.SetTimeout(TimeSpan.FromMilliseconds(300)))
            .WithBulkhead(b => b.SetMaxParallelization(256).SetMaxQueueLength(0)))
        .WithDiagnostics()
        .WithTracingHandler()
        .WithLogging(l => l.WithDefaultLogging().LogRequestBody(false).LogResponseBody(false));

    sc.Configure<HttpClientFactoryOptions>(mode, o =>
        o.HttpMessageHandlerBuilderActions.Add(hb => hb.PrimaryHandler = handler));

    return sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient(mode);
}

static HttpClient BuildClientForMode(string mode, Uri hostOnly, SocketsHttpHandler handler)
{
    mode = mode.ToLowerInvariant();
    return mode switch
    {
        "raw" => BuildRawClient(hostOnly, handler),
        "mango-min" => BuildMangoClientMinimal(mode, hostOnly, handler),
        "mango-policies" => BuildMangoClientPolicies(mode, hostOnly, handler, addMetrics: false),
        "mango-policies+metrics" => BuildMangoClientPolicies(mode, hostOnly, handler, addMetrics: true),
        "mango-otel+export" => BuildMangoOtelClientWithExport(mode, hostOnly, handler),

        // error profiles
        "raw-error" => BuildRawClient(hostOnly, handler),
        "mango-error-min" => BuildMangoError_Min(mode, hostOnly, handler),
        "mango-error-retry1" => BuildMangoError_Retry(mode, hostOnly, handler, retries: 1),
        "mango-error-retry3" => BuildMangoError_Retry(mode, hostOnly, handler, retries: 3),
        "mango-error-breaker" => BuildMangoError_Breaker(mode, hostOnly, handler),
        "mango-error-bulkhead" => BuildMangoError_Bulkhead(mode, hostOnly, handler),
        "mango-otel+export-error" => BuildMangoOtelClientWithExport(mode, hostOnly, handler),

        _ => throw new ArgumentException($"Unknown mode '{mode}'.")
    };
}


static IEnumerable<string> ResolveModes(string mode)
{
    mode = mode.ToLowerInvariant();

    if (mode is "error-matrix")
        return new[]
        {
            "raw-error",
            "mango-error-min",
            "mango-error-retry1",
            "mango-error-retry3",
            "mango-error-breaker",
            "mango-error-bulkhead",
            "mango-otel+export-error"
        };

    if (mode is "all" or "matrix")
        return new[] { "raw", "mango-min", "mango-policies", "mango-policies+metrics", "mango-otel", "mango-otel+export" };

    return new[] { mode };
}

static ScenarioProps BuildScenario(string name, HttpClient client, string path, int copies, int durationSeconds)
{
    // Expect failures if the URL path contains /error or /flaky, or if the mode name contains "error"
    var expectFailures =
        name.Contains("error", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/error", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/flaky", StringComparison.OrdinalIgnoreCase);

    return Scenario.Create(name, async _ =>
        {
            try
            {
                using var resp = await client.GetAsync(path, HttpCompletionOption.ResponseContentRead);
                var bytes = await resp.Content.ReadAsByteArrayAsync();

                if (expectFailures || resp.IsSuccessStatusCode)
                    return Response.Ok(statusCode: ((int)resp.StatusCode).ToString(), sizeBytes: bytes.LongLength);

                return Response.Fail(statusCode: ((int)resp.StatusCode).ToString(),
                    sizeBytes: bytes.LongLength,
                    message: resp.ReasonPhrase ?? "HTTP error");
            }
            catch (Exception ex)
            {
                // timeouts, BrokenCircuitException, etc. Still count them as OK in failure runs
                if (expectFailures)
                    return Response.Ok(statusCode: "EX", sizeBytes: 0, message: ex.GetType().Name);

                return Response.Fail(statusCode: "EX", sizeBytes: 0, message: ex.Message);
            }
        })
        .WithInit(async _ =>
        {
            var warms = Enumerable.Range(0, Math.Max(10, copies))
                .Select(_ => client.GetAsync(path, HttpCompletionOption.ResponseContentRead));
            try { await Task.WhenAll(warms); } catch { }
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(Simulation.KeepConstant(copies, TimeSpan.FromSeconds(durationSeconds)));
}

static LoadTestOptions ParseArgs(string[] args)
{
    if (args.Length == 0)
    {
        PrintUsage();
        Environment.Exit(1);
    }

    var opts = new LoadTestOptions();
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-u":
            case "--url":
                opts.TargetUrl = new Uri(args[++i]);
                break;
            case "-c":
            case "--concurrency":
                opts.Concurrency = int.Parse(args[++i]);
                break;
            case "-n":
            case "--requests":
                opts.TotalRequests = int.Parse(args[++i]);
                break;
            case "-m":
            case "--mode":
                opts.Mode = args[++i];
                break;
            case "-d":
            case "--duration":
                opts.DurationSeconds = int.Parse(args[++i]);
                break;
            default:
                Console.Error.WriteLine($"Unknown argument: {args[i]}");
                PrintUsage();
                Environment.Exit(1);
                break;
        }
    }
    return opts;
}

static void PrintUsage()
{
    Console.WriteLine(@"Usage:
  dotnet run -c Release -- \
    -u <target-url> \
    -c <copies (VUs)> \
    -d <duration-seconds> \
    -m <raw|mango-min|mango-policies|mango-policies+metrics|mango-otel|mango-otel+export|matrix>

Examples:
  dotnet run -c Release -- -u http://127.0.0.1:5000/echo -c 100 -d 30 -m matrix
  dotnet run -c Release -- -u http://127.0.0.1:5000/echo -c 100 -d 30 -m mango-otel+export   (requires collector)");
}

// --- CLI options
class LoadTestOptions
{
    public Uri TargetUrl { get; set; } = default!;               // e.g., http://127.0.0.1:5000/echo
    public int Concurrency { get; set; } = 50;                   // virtual users (closed model)
    public int TotalRequests { get; set; } = 0;                  // unused in closed model
    public string Mode { get; set; } = "matrix";                 // see modes above
    public int DurationSeconds { get; set; } = 30;               // steady duration
}