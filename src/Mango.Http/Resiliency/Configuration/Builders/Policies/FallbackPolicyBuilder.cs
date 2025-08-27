// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Builder for configuring a fallback resiliency policy for Mango HTTP clients.
    /// Use this class to set the fallback action and order for the policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new FallbackPolicyBuilder()
    ///     .SetFallbackAction((result, context, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
    ///     .SetOrder(700);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class FallbackPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the fallback policy in the pipeline.
        /// </summary>
        private int _order { get; set; } = (int)DefaultPolicyOrder.Fallback;
        /// <summary>
        /// Gets or sets the fallback action to execute when a fallback is needed.
        /// </summary>
        private Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> _fallbackAction { get; set; } = null!;

        /// <summary>
        /// Sets the fallback action to execute when a fallback is needed.
        /// </summary>
        /// <param name="fallbackAction">The fallback action to execute.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if fallbackAction is null.</exception>
        public FallbackPolicyBuilder SetFallbackAction(Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> fallbackAction)
        {
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
            return this;
        }

        /// <summary>
        /// Sets the order of the fallback policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        public FallbackPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="FallbackPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="FallbackPolicyDefinition"/>.</returns>
        internal FallbackPolicyDefinition Build()
        {
            Validate();
            return new FallbackPolicyDefinition(_fallbackAction, _order);
        }

        /// <summary>
        /// Validates the fallback policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown if fallbackAction is not provided.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_fallbackAction == null)
            {
                throw new ArgumentNullException(nameof(_fallbackAction), "Fallback action must be provided.");
            }
        }
    }
}
