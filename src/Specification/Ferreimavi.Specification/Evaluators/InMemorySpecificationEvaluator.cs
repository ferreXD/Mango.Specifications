// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Default evaluator for specifications in memory.
    /// Evaluates specifications against in-memory collections using LINQ.
    /// </summary>
    public class InMemorySpecificationEvaluator : IInMemorySpecificationEvaluator
    {
        // Maintain the common evaluators in a static readonly array to avoid recreating them
        private static readonly IInMemoryEvaluator[] DefaultEvaluators =
        {
            WhereEvaluator.Instance,
            OrderEvaluator.Instance,
            PaginationEvaluator.Instance
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySpecificationEvaluator" /> class with default evaluators.
        /// </summary>
        public InMemorySpecificationEvaluator()
        {
            Evaluators.AddRange(DefaultEvaluators);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySpecificationEvaluator" /> class with the provided evaluators.
        /// </summary>
        /// <param name="evaluators">A collection of evaluators to use for processing specifications.</param>
        public InMemorySpecificationEvaluator(IEnumerable<IInMemoryEvaluator> evaluators)
        {
            Evaluators.AddRange(evaluators);
        }

        /// <summary>
        /// Gets the default instance of <see cref="InMemorySpecificationEvaluator" />.
        /// </summary>
        /// <remarks>
        /// Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided
        /// evaluators.
        /// </remarks>
        public static InMemorySpecificationEvaluator Default { get; } = new();

        /// <summary>
        /// Gets the list of evaluators used to process specifications.
        /// </summary>
        protected List<IInMemoryEvaluator> Evaluators { get; } = new();

        /// <summary>
        /// Evaluates a grouping specification against a source collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="source">The source collection to evaluate against.</param>
        /// <param name="specification">The grouping specification to apply.</param>
        /// <returns>A collection of grouped results.</returns>
        /// <exception cref="SelectorNotFoundException">Thrown when either the group by selector or group result selector is null.</exception>
        public virtual IEnumerable<IGrouping<TKey, TResult>> Evaluate<T, TKey, TResult>(IEnumerable<T> source, IGroupingSpecification<T, TKey, TResult> specification)
        {
            if (specification is { GroupResultSelector: null }) throw new SelectorNotFoundException();
            if (specification is { GroupBySelector: null }) throw new SelectorNotFoundException();

            var evaluators = Evaluators.Where(evaluator => evaluator is not PaginationEvaluator).ToList();
            var baseQuery = Evaluate(source, specification, evaluators);

            // Compile the grouping selectors once for performance.
            var groupByFunc = specification.GroupBySelector.Compile();
            var groupResultFunc = specification.GroupResultSelector.Compile();

            // Apply grouping: group the base query using the compiled GroupBySelector.
            // For each group, project each element using the compiled GroupResultSelector.
            // This yields an IEnumerable<IGrouping<TKey, TResult>>.
            var groupedResult = baseQuery
                .GroupBy(groupByFunc, x => groupResultFunc(x))
                .ToList();

            // If a post-processing action is defined, apply it to the grouped results.
            if (specification.PostProcessingAction is not null) groupedResult = specification.PostProcessingAction(groupedResult).ToList();

            // Finally, apply pagination to the groups.
            var skip = specification.Skip ?? 0;
            var take = specification.Take ?? groupedResult.Count;
            return groupedResult
                .Skip(skip)
                .Take(take);
        }

        /// <summary>
        /// Evaluates a result specification against a source collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source collection to evaluate against.</param>
        /// <param name="specification">The result specification to apply.</param>
        /// <returns>A collection of results.</returns>
        /// <exception cref="SelectorNotFoundException">Thrown when both Selector and SelectorMany are null.</exception>
        /// <exception cref="ConcurrentSelectorsException">Thrown when both Selector and SelectorMany are provided.</exception>
        public virtual IEnumerable<TResult> Evaluate<T, TResult>(IEnumerable<T> source, ISpecification<T, TResult> specification)
        {
            if (specification is { Selector: null, SelectorMany: null }) throw new SelectorNotFoundException();
            if (specification is { Selector: not null, SelectorMany: not null }) throw new ConcurrentSelectorsException();

            var baseQuery = Evaluate(source, (ISpecification<T>)specification);

            var resultQuery = specification.Selector is not null ? baseQuery.Select(specification.Selector.Compile()) : baseQuery.SelectMany(specification.SelectorMany!.Compile());

            if (specification.PostProcessingAction is not null)
                resultQuery = specification
                    .PostProcessingAction(resultQuery)
                    .ToList();

            return resultQuery;
        }

        /// <summary>
        /// Evaluates a basic specification against a source collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source collection to evaluate against.</param>
        /// <param name="specification">The specification to apply.</param>
        /// <returns>A filtered and processed collection based on the specification.</returns>
        public virtual IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification)
        {
            var result = Evaluate(source, specification, Evaluators).ToList();

            if (specification.PostProcessingAction is not null)
                result = specification
                    .PostProcessingAction(result)
                    .ToList();

            return result;
        }

        /// <summary>
        /// Evaluates a specification against a source collection using the specified evaluators.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source collection to evaluate against.</param>
        /// <param name="specification">The specification to apply.</param>
        /// <param name="evaluators">The evaluators to use for processing the specification.</param>
        /// <returns>A filtered and processed collection based on the specification and evaluators.</returns>
        public virtual IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification, IEnumerable<IInMemoryEvaluator> evaluators)
            => evaluators.Aggregate(source, (current, evaluator) => evaluator.Evaluate(current, specification));
    }
}