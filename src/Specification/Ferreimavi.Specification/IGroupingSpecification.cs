namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Defines a grouping specification that encapsulates query conditions and options,
    /// including group by and group result selectors. Inherits from <see cref="ISpecification{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key to group by.</typeparam>
    /// <typeparam name="TResult">The type of the result after grouping.</typeparam>
    public interface IGroupingSpecification<T, TKey, TResult> : ISpecification<T>
    {
        /// <summary>
        /// Gets the grouping specification query builder for constructing the specification. Overrides the base query builder.
        /// </summary>
        new IGroupingSpecificationBuilder<T, TKey, TResult> Query { get; }

        /// <summary>
        /// Gets the group by selector expression to specify the key for grouping.
        /// </summary>
        Expression<Func<T, TKey>>? GroupBySelector { get; }

        /// <summary>
        /// Gets the group result selector expression to project the grouped data.
        /// </summary>
        Expression<Func<T, TResult>>? GroupResultSelector { get; }

        /// <summary>
        /// Gets the action to perform on the entities after the query is executed. Overrides the base post-processing action.
        /// </summary>
        new Func<IEnumerable<IGrouping<TKey, TResult>>, IEnumerable<IGrouping<TKey, TResult>>>? PostProcessingAction { get; }

        /// <summary>
        /// Evaluates the grouping specification on the given collection of entities and returns the grouped results.
        /// </summary>
        /// <param name="entities">The collection of entities to evaluate.</param>
        /// <returns>An enumerable of groupings containing the group key and the grouped result.</returns>
        new IEnumerable<IGrouping<TKey, TResult>> Evaluate(IEnumerable<T> entities);
    }

    /// <summary>
    /// Defines a grouping specification with the result type set as the entity type.
    /// Inherits from <see cref="IGroupingSpecification{T, TKey, TResult}" /> with TResult set to T.
    /// </summary>
    /// <typeparam name="T">The type of the entity and the result.</typeparam>
    /// <typeparam name="TKey">The type of the key to group by.</typeparam>
    public interface IGroupingSpecification<T, TKey> : IGroupingSpecification<T, TKey, T>;
}