// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using System;

    public abstract record ResiliencyPolicyDefinition(int order) : IComparable<ResiliencyPolicyDefinition>
    {
        /// <summary>Controls pipeline order: lower runs closer to the HTTP call.</summary>
        public int Order { get; init; } = order;

        public abstract IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics = null);

        public int CompareTo(ResiliencyPolicyDefinition? other)
            => Order.CompareTo(other?.Order ?? 0);
    }
}
