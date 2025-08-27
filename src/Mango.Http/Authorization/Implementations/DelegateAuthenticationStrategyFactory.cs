// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Implementation of <see cref="IAuthenticationStrategyFactory"/> that creates authentication strategies using a delegate.
    /// Uses a provided factory function to resolve <see cref="IAuthenticationStrategy"/> instances from the service provider.
    /// </summary>
    public sealed class DelegateAuthenticationStrategyFactory : IAuthenticationStrategyFactory
    {
        private readonly Func<IServiceProvider, IAuthenticationStrategy> _factory;
        private readonly ILogger<DelegateAuthenticationStrategyFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateAuthenticationStrategyFactory"/> class.
        /// </summary>
        /// <param name="factory">A delegate that creates an authentication strategy from the service provider.</param>
        /// <param name="logger">The logger used for diagnostics and telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown if factory or logger is null.</exception>
        public DelegateAuthenticationStrategyFactory(
            Func<IServiceProvider, IAuthenticationStrategy> factory,
            ILogger<DelegateAuthenticationStrategyFactory> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates an <see cref="IAuthenticationStrategy"/> using the provided service provider.
        /// Logs the creation process for diagnostics.
        /// </summary>
        /// <param name="sp">The service provider for resolving dependencies.</param>
        /// <returns>An instance of <see cref="IAuthenticationStrategy"/>.</returns>
        public IAuthenticationStrategy Create(IServiceProvider sp)
        {
            _logger.LogDebug("Creating strategy via delegate");
            var strategy = _factory(sp);
            _logger.LogInformation("Created strategy {Strategy}", strategy.GetType().Name);
            return strategy;
        }
    }
}
