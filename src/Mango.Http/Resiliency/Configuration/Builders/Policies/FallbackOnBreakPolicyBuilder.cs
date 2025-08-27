// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;

    /// <summary>
    /// Builder for configuring a fallback-on-break resiliency policy for Mango HTTP clients.
    /// Use this class to set the fallback action and order for the policy when the circuit breaker is open.
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new FallbackOnBreakPolicyBuilder()
    ///     .SetOnBreak((result, context, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)))
    ///     .SetOrder(650);
    /// var policy = builder.Build();
    /// </code>
    /// </example>
    public class FallbackOnBreakPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the order of the fallback-on-break policy in the pipeline.
        /// </summary>
        private int _order = (int)DefaultPolicyOrder.FallbackOnBreak;
        /// <summary>
        /// Gets or sets the action to execute when the circuit breaker is open and a fallback is needed.
        /// </summary>
        private Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> _onBreak { get; set; } = null!;

        /// <summary>
        /// Sets the action to execute when the circuit breaker is open and a fallback is needed.
        /// </summary>
        /// <param name="onBreakAction">The fallback action to execute.</param>
        /// <returns>The builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if onBreakAction is null.</exception>
        public FallbackOnBreakPolicyBuilder SetOnBreak(Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> onBreakAction)
        {
            _onBreak = onBreakAction ?? throw new ArgumentNullException(nameof(onBreakAction));
            return this;
        }

        /// <summary>
        /// Sets the order of the fallback-on-break policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The builder for chaining.</returns>
        public FallbackOnBreakPolicyBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Builds and validates the configured <see cref="FallbackOnBreakPolicyDefinition"/>.
        /// </summary>
        /// <returns>The built and validated <see cref="FallbackOnBreakPolicyDefinition"/>.</returns>
        internal FallbackOnBreakPolicyDefinition Build()
        {
            Validate();
            return new FallbackOnBreakPolicyDefinition(_onBreak, _order);
        }

        /// <summary>
        /// Validates the fallback-on-break policy configuration.
        /// Throws an exception if configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown if onBreak action is not provided.</exception>
        private void Validate()
        {
            if (_order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_order), "Order must be a non-negative integer.");
            }
            if (_onBreak == null)
            {
                throw new ArgumentNullException(nameof(_onBreak), "Fallback action must be provided.");
            }
        }
    }
}
