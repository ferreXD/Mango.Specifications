// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Polly;

    /// <summary>
    /// Configurator for the fallback resiliency policy in Mango HTTP clients.
    /// Use this class to set the fallback action and its order in the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var fallback = new FallbackPolicyConfigurator()
    ///     .SetFallbackAction((result, context, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
    ///     .SetOrder(700);
    /// </code>
    /// </example>
    public sealed class FallbackPolicyConfigurator() : BasePolicyConfigurator((int)DefaultPolicyOrder.Fallback)
    {
        /// <summary>
        /// Gets or sets the action to execute when a fallback is needed.
        /// </summary>
        internal Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> FallbackAction { get; private set; } = null!;

        /// <summary>
        /// Sets the action to execute when a fallback is needed.
        /// </summary>
        /// <param name="fallbackAction">The fallback action to execute.</param>
        /// <returns>The configurator for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if fallbackAction is null.</exception>
        public FallbackPolicyConfigurator SetFallbackAction(Func<DelegateResult<HttpResponseMessage>, Context, CancellationToken, Task<HttpResponseMessage>> fallbackAction)
        {
            FallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
            return this;
        }

        /// <summary>
        /// Sets the order of the fallback policy in the pipeline.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The configurator for chaining.</returns>
        public FallbackPolicyConfigurator SetOrder(int order)
        {
            Order = order;
            return this;
        }

        /// <summary>
        /// Validates the fallback policy configuration.
        /// Throws an exception if the fallback action is not provided.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if FallbackAction is null.</exception>
        internal override void Validate()
        {
            if (FallbackAction == null)
            {
                throw new ArgumentNullException(nameof(FallbackAction), "Fallback action must be provided.");
            }
        }
    }
}
