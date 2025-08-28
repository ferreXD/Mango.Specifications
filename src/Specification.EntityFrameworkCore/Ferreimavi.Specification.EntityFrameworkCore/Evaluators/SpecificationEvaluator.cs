// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections;
    using System.Linq.Expressions;

    /// <summary>
    /// Evaluates specifications and applies them to Entity Framework Core queries.
    /// </summary>
    public class SpecificationEvaluator : ISpecificationEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationEvaluator" /> class with default evaluators.
        /// </summary>
        public SpecificationEvaluator()
        {
            Evaluators.AddRange(new IQueryEvaluator[]
            {
                AsNoTrackingQueryEvaluator.Instance,
                AsTrackingQueryEvaluator.Instance,
                IncludeQueryEvaluator.Instance,
                WhereQueryEvaluator.Instance,
                OrderQueryEvaluator.Instance,
                PaginationQueryEvaluator.Instance
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationEvaluator" /> class with custom evaluators.
        /// </summary>
        /// <param name="evaluators">The collection of query evaluators to use.</param>
        public SpecificationEvaluator(IEnumerable<IQueryEvaluator> evaluators)
        {
            Evaluators.AddRange(evaluators);
        }

        /// <summary>
        /// Gets the default instance of the specification evaluator.
        /// </summary>
        public static SpecificationEvaluator Default { get; } = new();

        /// <summary>
        /// Gets the list of query evaluators used by this specification evaluator.
        /// </summary>
        protected List<IQueryEvaluator> Evaluators { get; } = new();

        /// <summary>
        /// Gets a query for a projectable specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result after projection.</typeparam>
        /// <param name="query">The initial query.</param>
        /// <param name="specification">The specification to apply.</param>
        /// <returns>A query with the specification applied and the projection set up.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the specification is null.</exception>
        /// <exception cref="SelectorNotFoundException">Thrown when no selector is defined in the specification.</exception>
        /// <exception cref="ConcurrentSelectorsException">
        /// Thrown when both Selector and SelectorMany are defined in the
        /// specification.
        /// </exception>
        public IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query, ISpecification<T, TResult> specification)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

            if (specification.Selector is null && specification.SelectorMany is null) throw new SelectorNotFoundException();
            if (specification.Selector is not null && specification.SelectorMany is not null) throw new ConcurrentSelectorsException();

            query = GetQuery(query, (ISpecification<T>)specification);

            return specification.Selector is not null ? query.Select(specification.Selector) : query.SelectMany(specification.SelectorMany!);
        }

        /// <summary>
        /// Gets a query for a standard specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="query">The initial query.</param>
        /// <param name="specification">The specification to apply.</param>
        /// <param name="evaluateCriteriaOnly">Whether to only evaluate criteria (where expressions).</param>
        /// <returns>A query with the specification applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the specification is null.</exception>
        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification, bool evaluateCriteriaOnly = false) where T : class
        {
            ArgumentNullException.ThrowIfNull(specification, nameof(specification));

            var evaluators = evaluateCriteriaOnly ? Evaluators.Where(x => x.IsCriteriaEvaluator) : Evaluators;
            return Evaluate(query, specification, evaluators);
        }

        /// <summary>
        /// Gets a grouped query for a grouping specification.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key to group by.</typeparam>
        /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
        /// <param name="query">The initial query.</param>
        /// <param name="specification">The grouping specification to apply.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A queryable collection of groupings.</returns>
        /// <exception cref="SelectorNotFoundException">Thrown when GroupResultSelector or GroupBySelector is not defined.</exception>
        public async Task<IQueryable<IGrouping<TKey, TResult>>> GetQuery<T, TKey, TResult>(IQueryable<T> query, IGroupingSpecification<T, TKey, TResult> specification, CancellationToken cancellationToken)
            where T : class
        {
            if (specification is { GroupResultSelector: null }) throw new SelectorNotFoundException();
            if (specification is { GroupBySelector: null }) throw new SelectorNotFoundException();

            var evaluators = Evaluators.Where(evaluator => evaluator is not PaginationQueryEvaluator).ToList();
            var baseQuery = Evaluate(query, specification, evaluators);

            var effectiveSelector = IsIdentitySelector(specification.GroupResultSelector) ? CreateShallowSelector<T, TResult>() : specification.GroupResultSelector;

            // TODO: If performance is a real concern in the future, consider using a custom implementation of GroupBy that does not materialize the query.
            // As of right now, we'll keep it this way, in order to avoid the 'unable to translate' exception. Problem is with pagination after grouping.

            // Apply grouping: group the base query using the compiled GroupBySelector.
            // For each group, project each element using the compiled GroupResultSelector.
            // This yields an IEnumerable<IGrouping<TKey, TResult>>.
            var groupedResults = await baseQuery
                .GroupBy(specification.GroupBySelector, effectiveSelector)
                .ToListAsync(cancellationToken);

            // Apply pagination if specified.
            var skip = specification.Skip ?? 0;
            var take = specification.Take ?? groupedResults.Count;
            return groupedResults
                .Skip(skip)
                .Take(take)
                .AsQueryable();
        }

        /// <summary>
        /// Applies the evaluators to the source query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="specification">The specification to apply.</param>
        /// <param name="evaluators">The collection of evaluators to use.</param>
        /// <returns>A query with the evaluators applied in sequence.</returns>
        public virtual IQueryable<T> Evaluate<T>(IQueryable<T> source, ISpecification<T> specification, IEnumerable<IQueryEvaluator> evaluators)
            where T : class =>
            evaluators.Aggregate(source, (current, evaluator) => evaluator.GetQuery(current, specification));

        /// <summary>
        /// Determines whether the selector expression is an identity function (returns its input parameter unchanged).
        /// </summary>
        /// <typeparam name="T">The type of the input to the selector.</typeparam>
        /// <typeparam name="TResult">The type of the output from the selector.</typeparam>
        /// <param name="selector">The selector expression to check.</param>
        /// <returns><c>true</c> if the selector is an identity function; otherwise, <c>false</c>.</returns>
        private bool IsIdentitySelector<T, TResult>(Expression<Func<T, TResult>> selector) =>
            selector.Body is ParameterExpression parameter &&
            parameter == selector.Parameters[0];

        /// <summary>
        /// Creates a shallow copy selector expression that maps non-collection properties from the source type to the result type.
        /// </summary>
        /// <typeparam name="T">The source entity type.</typeparam>
        /// <typeparam name="TResult">The result type to create.</typeparam>
        /// <returns>A lambda expression that creates a shallow copy of an entity, excluding collection properties.</returns>
        /// <remarks>
        /// This method is used when no explicit selector is provided but projection is needed.
        /// It only copies simple properties and string properties, ignoring collections.
        /// </remarks>
        private Expression<Func<T, TResult>> CreateShallowSelector<T, TResult>()
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var bindings = typeof(T)
                .GetProperties()
                .Where(p => !typeof(IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType == typeof(string))
                .Select(p => Expression.Bind(p, Expression.Property(parameter, p)));

            var memberInit = Expression.MemberInit(Expression.New(typeof(TResult)), bindings);
            return Expression.Lambda<Func<T, TResult>>(memberInit, parameter);
        }
    }
}