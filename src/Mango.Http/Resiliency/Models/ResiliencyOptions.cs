// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    /// <summary>
    /// Represents configuration options for resiliency policies in the HTTP pipeline.
    /// </summary>
    public sealed class ResiliencyOptions
    {
        /// <summary>
        /// Gets the ordered list of resiliency policy definitions.
        /// </summary>
        public List<ResiliencyPolicyDefinition> Policies { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ResiliencyOptions"/> with the specified policies.
        /// </summary>
        /// <param name="policies">The collection of resiliency policy definitions.</param>
        internal ResiliencyOptions(IEnumerable<ResiliencyPolicyDefinition> policies)
        {
            Policies = policies.OrderBy(x => x.Order).ToList();
            // Only validate when *not* empty OR when custom is present
            if (Policies.Any() && !Policies.OfType<CustomPolicyDefinition>().Any())
                Validate();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ResiliencyOptions"/> with no policies.
        /// </summary>
        public ResiliencyOptions() : this(Enumerable.Empty<ResiliencyPolicyDefinition>()) { }

        /// <summary>
        /// Returns a new <see cref="ResiliencyOptions"/> instance with the specified policy added.
        /// </summary>
        /// <param name="policy">The policy to add.</param>
        /// <returns>A new <see cref="ResiliencyOptions"/> instance with the policy appended.</returns>
        public ResiliencyOptions Add(ResiliencyPolicyDefinition policy)
            => new ResiliencyOptions(Policies.Append(policy));

        /// <summary>
        /// Validates the current set of resiliency policies for correctness and consistency.
        /// Throws <see cref="InvalidOperationException"/> if the configuration is invalid.
        /// </summary>
        public void Validate()
        {
            if (!Policies.Any())
                return; // No policies to validate

            if (Policies.OfType<CustomPolicyDefinition>().Any()
                && Policies.Count > 1)
                throw new InvalidOperationException(
                    "Cannot mix a custom policy with built-in policies.");

            EnsureNoDuplicateOrders();

            if (Policies.OfType<CustomPolicyDefinition>().Any())
                return; // OK if there’s at least one policy or a custom policy  

            ValidateFallbacks(Policies);
        }

        /// <summary>
        /// Ensures that no two policies have the same order value.
        /// Throws <see cref="InvalidOperationException"/> if duplicates are found.
        /// </summary>
        private void EnsureNoDuplicateOrders()
        {
            var duplicateOrders = Policies.GroupBy(p => p.Order)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateOrders.Any())
                throw new InvalidOperationException($"Duplicate orders: {string.Join(',', duplicateOrders)}");
        }

        /// <summary>
        /// Validates the placement and dependencies of fallback-related policies.
        /// Throws <see cref="InvalidOperationException"/> if the configuration is invalid.
        /// </summary>
        /// <param name="policies">The list of policies to validate.</param>
        private void ValidateFallbacks(IReadOnlyList<ResiliencyPolicyDefinition> policies)
        {
            var fb = policies.OfType<FallbackPolicyDefinition>().SingleOrDefault();
            var fbb = policies.OfType<FallbackOnBreakPolicyDefinition>().SingleOrDefault();
            var cb = policies.OfType<CircuitBreakerPolicyDefinition>().SingleOrDefault();
            var max = policies.Max(p => p.Order);

            if (fb != null && cb == null)
                throw new InvalidOperationException("Fallback requires CircuitBreaker first.");
            if (fbb != null && cb == null)
                throw new InvalidOperationException("FallbackOnBreak requires CircuitBreaker first.");

            if (fb?.Order != null && fb.Order != max)
                throw new InvalidOperationException("Fallback must be last.");

            if (fbb?.Order != null && (fbb.Order != max && fb == null
                                       || fb != null && fbb.Order != fb.Order - 1))
                throw new InvalidOperationException("FallbackOnBreak must immediately precede Fallback.");
        }
    }
}
