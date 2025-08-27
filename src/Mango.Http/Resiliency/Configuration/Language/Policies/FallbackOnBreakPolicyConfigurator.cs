// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Configurator for the fallback-on-break resiliency policy in Mango HTTP clients.
    /// Use this class to set the fallback action and its order in the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var fallbackOnBreak = new FallbackOnBreakPolicyConfigurator()
    ///     .SetOnBreak((result, context, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)))
    ///     .SetOrder(650);
    /// </code>
    /// </example>
    public sealed class FallbackOnBreakPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.FallbackOnBreak)
    {
        /// <summary>
        /// Gets or sets the action to execute when the circuit is broken and a fallback is needed.
        /// </summary>
        internal Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> OnBreak { get; private set; } = null!;

        /// <summary>
        /// Sets the action to execute when the circuit is broken and a fallback is needed.
        /// </summary>
        /// <param name="onBreakAction">The fallback action to execute.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if onBreakAction is null.</exception>
        public FallbackOnBreakPolicyConfigurator SetOnBreak(Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> onBreakAction)
        {
            OnBreak = onBreakAction ?? throw new ArgumentNullException(nameof(onBreakAction));
            return this;
        }

        /// <summary>
        /// Sets the order of the fallback-on-break policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public FallbackOnBreakPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the fallback-on-break policy configuration.
        /// Throws an exception if the fallback action is not provided.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if OnBreak is null.</exception>
        internal override void Validate()
        {
            if (OnBreak == null)
            {
                throw new ArgumentNullException(nameof(OnBreak), "Fallback action must be provided.");
            }
        }
    }
}
