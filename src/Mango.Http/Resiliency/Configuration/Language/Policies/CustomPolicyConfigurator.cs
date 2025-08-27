// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;
    using System;

    /// <summary>
    /// Configurator for custom resiliency policies in Mango HTTP clients.
    /// Use this class to provide a custom policy factory and set its order in the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var customPolicy = new CustomPolicyConfigurator()
    ///     .SetPolicyFactory(diag => Policy.NoOpAsync<HttpResponseMessage>())
    ///     .SetOrder(999);
    /// </code>
    /// </example>
    public sealed class CustomPolicyConfigurator : BasePolicyConfigurator
    {
        /// <summary>
        /// Gets or sets the factory function that creates the custom async policy.
        /// </summary>
        internal Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> PolicyFactory { get; private set; } = _ => throw new NotImplementedException();

        /// <summary>
        /// Sets the factory function for the custom async policy.
        /// </summary>
        /// <param name="policyFactory">A function that takes diagnostics and returns an async policy.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if policyFactory is null.</exception>
        public CustomPolicyConfigurator SetPolicyFactory(Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        {
            PolicyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
            return this;
        }

        /// <summary>
        /// Sets the order of the custom policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public CustomPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the custom policy configuration.
        /// Throws an exception if the policy factory is not provided.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if PolicyFactory is null.</exception>
        internal override void Validate()
        {
            if (PolicyFactory == null)
            {
                throw new ArgumentNullException(nameof(PolicyFactory), "Policy factory must be provided.");
            }
        }
    }
}
