// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using System;
    using System.Linq;

    public sealed record RetryPolicyDefinition(int order = (int)DefaultPolicyOrder.Retry) : ResiliencyPolicyDefinition(order), IMergeablePolicyDefinition<RetryPolicyDefinition>
    {
        public int RetryCount { get; init; } = 3;
        public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(100);
        public bool? UseJitter { get; init; } = true; // on by default

        public bool ApplyToNonIdempotentMethods { get; init; } = false; // off by default
        public bool RetryOn500 { get; init; } = false;               // off by default
        public bool RespectRetryAfter { get; init; } = true;          // on by default
        public TimeSpan? MaxRetryAfter { get; init; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Optional user override. If null, we use DefaultShouldRetry(outcome, RetryOn500).
        /// NOTE: idempotency gating is enforced in the handler, not here.
        /// </summary>
        public Func<DelegateResult<HttpResponseMessage>, bool>? ShouldRetry { get; init; }


        public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null)
        {
            // Build backoff sequence once
            var backoff = BuildRetryBackoff().ToArray();

            return Policy<HttpResponseMessage>
                .HandleResult(r => (ShouldRetry?.Invoke(new DelegateResult<HttpResponseMessage>(r)))
                                   ?? DefaultShouldRetry(new DelegateResult<HttpResponseMessage>(r), RetryOn500))
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<OperationCanceledException>()
                // Use outcome-aware delay so we can honor Retry-After
                .WaitAndRetryAsync(
                    retryCount: RetryCount,
                    sleepDurationProvider: (attempt, outcome, ctx) => ComputeDelay(attempt, outcome, backoff),
                    onRetryAsync: async (outcome, delay, attempt, context) =>
                    {
                        if (diagnostics != null && context.TryGetRequest(out var req))
                            diagnostics.OnRetry(req!, attempt, outcome.Exception);
                        await Task.CompletedTask;
                    });
        }

        private static bool DefaultShouldRetry(DelegateResult<HttpResponseMessage> outcome, bool retryOn500)
        {
            if (outcome.Exception is not null)
                return true; // network/timeout/etc. (we gate idempotency in the handler)

            var r = outcome.Result;
            if (r is null) return false;

            var code = (int)r.StatusCode;
            return code is 408 or 429 or 502 or 503 or 504
                   || (retryOn500 && code == 500);
        }

        private TimeSpan ComputeDelay(
            int attempt,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan[] backoff)
        {
            if (RespectRetryAfter)
            {
                var ra = outcome.Result?.Headers?.RetryAfter;
                if (ra is not null)
                {
                    var delta = ra.Delta
                                ?? (ra.Date.HasValue ? ra.Date.Value - DateTimeOffset.UtcNow : (TimeSpan?)null);

                    if (delta is { } d && d > TimeSpan.Zero)
                    {
                        if (MaxRetryAfter is { } max && d > max) d = max;
                        return d;
                    }
                }
            }

            // Fallback to configured backoff (jitter or fixed)
            var idx = Math.Clamp(attempt - 1, 0, backoff.Length - 1);
            return backoff[idx];
        }

        private IEnumerable<TimeSpan> BuildRetryBackoff()
            => (UseJitter ?? false)
                ? Backoff.DecorrelatedJitterBackoffV2(RetryDelay, RetryCount)
                : Enumerable.Repeat(RetryDelay, RetryCount);

        public static readonly RetryPolicyDefinition Default = new();
        public static RetryPolicyDefinition TransientHttpDefaults => new()
        {
            RetryCount = 3,
            RetryDelay = TimeSpan.FromMilliseconds(100),
            UseJitter = true,
            ApplyToNonIdempotentMethods = false,
            RetryOn500 = false,
            RespectRetryAfter = true,
            MaxRetryAfter = TimeSpan.FromSeconds(60)
        };

        public RetryPolicyDefinition Merge(RetryPolicyDefinition preset) => this with
        {
            Order = Order == Default.Order ? preset.Order : Order,
            RetryCount = RetryCount == Default.RetryCount ? preset.RetryCount : RetryCount,
            RetryDelay = RetryDelay == Default.RetryDelay ? preset.RetryDelay : RetryDelay,
            UseJitter = UseJitter ?? preset.UseJitter ?? false,
            RetryOn500 = RetryOn500 || preset.RetryOn500,
            RespectRetryAfter = RespectRetryAfter && preset.RespectRetryAfter,
            MaxRetryAfter = MaxRetryAfter ?? preset.MaxRetryAfter,
            ShouldRetry = ShouldRetry ?? preset.ShouldRetry
        };
    }
}
