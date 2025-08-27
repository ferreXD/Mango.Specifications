// ReSharper disable once CheckNamespace
namespace Mango.Http.Resiliency
{
    using Presets;

    /// <summary>
    /// Provides methods for building <see cref="ResiliencyOptions"/> from a MangoResiliencyPolicyConfigurator and preset registry.
    /// Use this class to merge preset and user-defined resiliency policies for HTTP clients.
    /// </summary>
    /// <example>
    /// <code>
    /// var options = MangoResiliencyPolicyBuilder.Build(configurator, registry);
    /// </code>
    /// </example>
    internal class MangoResiliencyPolicyBuilder
    {
        /// <summary>
        /// Builds and validates <see cref="ResiliencyOptions"/> by applying presets and user policy actions.
        /// </summary>
        /// <param name="cfg">The Mango resiliency policy configurator.</param>
        /// <param name="registry">The resiliency policy preset registry.</param>
        /// <returns>The merged and validated <see cref="ResiliencyOptions"/>.</returns>
        public static ResiliencyOptions Build(
            MangoResiliencyPolicyConfigurator cfg,
            IResiliencyPolicyPresetRegistry registry)
        {
            // 1) Start a fresh options‐builder
            var presetBuilder = new ResiliencyPolicyOptionsBuilder();

            // 2) Apply presets first, in declaration order
            foreach (var presetName in cfg.Presets)
            {
                var preset = registry.Get(presetName); // throws if not found
                preset.Configure(presetBuilder);
            }

            var optionsBuilder = new ResiliencyPolicyOptionsBuilder();

            // 3) Apply each recorded policy action
            foreach (var action in cfg.PolicyActions)
                action(optionsBuilder);

            var presetOptions = presetBuilder.Build();
            var userOptions = optionsBuilder.Build();
            var merged = MergePolicies(presetOptions, userOptions);

            var final = new ResiliencyOptions(merged);
            final.Validate(); // Validate the final merged options

            // 5) Return the final merged options
            return final;
        }

        /// <summary>
        /// Merges preset and user-defined resiliency policies, combining them according to merge logic.
        /// </summary>
        /// <param name="presetOptions">The options built from presets.</param>
        /// <param name="userOptions">The options built from user actions.</param>
        /// <returns>The merged list of resiliency policy definitions.</returns>
        private static IEnumerable<ResiliencyPolicyDefinition> MergePolicies(
            ResiliencyOptions presetOptions,
            ResiliencyOptions userOptions)
        {
            var merged = new List<ResiliencyPolicyDefinition>();

            // build a lookup of preset policies by their concrete type
            var presetDict = presetOptions.Policies
                .ToDictionary(p => p.GetType(), p => p);

            foreach (var userPolicy in userOptions.Policies)
            {
                var policyType = userPolicy.GetType();
                if (presetDict.TryGetValue(policyType, out var presetPolicy))
                {
                    // find IMergeablePolicyDefinition<policyType>
                    var mergeInterface = policyType.GetInterfaces()
                        .FirstOrDefault(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IMergeablePolicyDefinition<>) &&
                            i.GetGenericArguments()[0] == policyType);

                    if (mergeInterface != null)
                    {
                        // invoke the Merge(T preset) method
                        var mergeMethod = mergeInterface.GetMethod("Merge", new[] { policyType });
                        var mergedPolicy = (ResiliencyPolicyDefinition)mergeMethod!
                            .Invoke(userPolicy, new[] { presetPolicy })!;
                        merged.Add(mergedPolicy);
                        continue;
                    }
                }

                // no preset or not mergeable — just take the user policy
                merged.Add(userPolicy);
            }

            // include any presets the user didn't override
            foreach (var kv in presetDict)
            {
                if (userOptions.Policies.All(up => up.GetType() != kv.Key)) merged.Add(kv.Value);
            }

            return merged;
        }
    }
}
