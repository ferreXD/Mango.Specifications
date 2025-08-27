// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Diagnostics;
    using Polly;

    /// <summary>
    /// Builder for configuring a custom resiliency policy for Mango HTTP clients.
    /// Use this class to provide a custom policy factory and set its order in the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new CustomPolicyBuilder()
    ///     .SetPolicyFactory(diag => Policy.NoOpAsync<HttpResponseMessage>())
    ///     .SetOrder(1000);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class CustomPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the custom policy in the pipeline.
        /// </summary>
        private int _order = 1000;
        /// <summary>
        /// Gets or sets the factory function that creates the custom async policy.
        /// </summary>
        private Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> _policyFactory { get; set; } = _ => throw new NotImplementedException();

        /// <summary>
        /// Sets the factory function for the custom async policy.
        /// </summary>
        /// <param name="policyFactory">A function that takes diagnostics and returns an async policy.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if policyFactory is null.</exception>
        public CustomPolicyBuilder SetPolicyFactory(Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        {
            _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
            return this;
        }

        /// <summary>
        /// Sets the order of the custom policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        public CustomPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="CustomPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="CustomPolicyDefinition"/>.</returns>
        internal CustomPolicyDefinition Build()
        {
            Validate();
            return new CustomPolicyDefinition(_order, _policyFactory);
        }

        /// <summary>
        /// Validates the custom policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown if policyFactory is not provided.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_policyFactory == null)
            {
                throw new ArgumentNullException(nameof(_policyFactory), "Policy factory must be provided.");
            }
        }
    }
}
