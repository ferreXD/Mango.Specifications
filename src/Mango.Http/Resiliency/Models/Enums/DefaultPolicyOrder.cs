// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    public enum DefaultPolicyOrder
    {
        TimeoutOverall = 0,
        Retry = 100,  // leave gaps for custom insertions
        CircuitBreaker = 200,
        TimeoutPerAttempt = 300,
        Bulkhead = 300,
        FallbackOnBreak = 400,
        Fallback = 500
    }
}
