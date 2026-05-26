namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Defines a specification with a result projection.
    /// Inherits from the non-result specification.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ISpecification<T, TResult> : ISpecification<T>
    {
        /// <summary>
        /// Gets the query builder for creating and modifying the specification query. Overrides the base query builder.
        /// </summary>
        new ISpecificationBuilder<T, TResult> Query { get; }

        /// <summary>
        /// Gets the selector expression to project from the entity to the result.
        /// </summary>
        Expression<Func<T, TResult>>? Selector { get; }

        /// <summary>
        /// Gets the selector expression to project from the entity to a collection of results.
        /// </summary>
        Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; }


        /// <summary>
        /// Gets the action to perform on the entities after the query is executed. Overrides the base post-processing action.
        /// </summary>
        new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; }

        /// <summary>
        /// Evaluates the specification on the given collection of entities and returns the projected results.
        /// </summary>
        /// <param name="entities">The collection of entities to evaluate.</param>
        /// <returns>An enumerable of projected results.</returns>
        new IEnumerable<TResult> Evaluate(IEnumerable<T> entities);
    }

    /// <summary>
    /// Defines a specification that encapsulates query conditions and options.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Gets the query builder for constructing the specification.
        /// </summary>
        ISpecificationBuilder<T> Query { get; }

        /// <summary>
        /// Gets the collection of where expressions used to filter the entities.
        /// </summary>
        IReadOnlyCollection<WhereExpressionInfo<T>> WhereExpressions { get; }

        /// <summary>
        /// Gets the collection of order by expressions used to sort the entities.
        /// </summary>
        IReadOnlyCollection<OrderByExpressionInfo<T>> OrderByExpressions { get; }

        /// <summary>
        /// Gets the collection of include expressions used to specify related entities to include.
        /// </summary>
        IReadOnlyCollection<IncludeExpressionInfo> IncludeExpressions { get; }

        /// <summary>
        /// Gets the action to perform on the entities after the query is executed.
        /// </summary>
        Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; }

        /// <summary>
        /// Gets or sets the number of entities to skip.
        /// </summary>
        int? Skip { get; }

        /// <summary>
        /// Gets or sets the maximum number of entities to take.
        /// </summary>
        int? Take { get; }

        /// <summary>
        /// Gets a value indicating whether the query is executed as a split query.
        /// EF Core only. Default: <c>false</c>.
        /// </summary>
        bool AsSplitQuery => false;

        /// <summary>
        /// Gets a value indicating whether the query is executed as a single query.
        /// EF Core only. Default: <c>false</c>.
        /// </summary>
        bool AsSingleQuery => false;
        
        /// <summary>
        /// Gets a value indicating whether query filters should be ignored when executing queries.
        /// EF Core only. Default: <c>false</c>.
        /// </summary>
        bool IgnoreQueryFilters => false;
        
        /// <summary>
        /// Gets a value indicating whether queries are executed with no tracking but with identity resolution enabled.
        /// EF Core only. Default: <c>false</c>.
        /// </summary>
        bool AsNoTrackingWithIdentityResolution => false;

        /// <summary>
        /// Gets the tag to be applied to queries for this specification.
        /// EF Core only. Default: <c>""</c>.
        /// </summary>
        string TagWith => string.Empty;

        /// <summary>
        /// Gets the collection of string-based include paths used to specify related entities to include.
        /// EF Core only. Default: empty.
        /// </summary>
        IEnumerable<string> StringIncludes => Array.Empty<string>();

        /// <summary>
        /// Evaluates the specification on the given collection of entities.
        /// </summary>
        /// <param name="entities">The collection of entities to evaluate.</param>
        /// <returns>An enumerable of entities that satisfy the specification.</returns>
        IEnumerable<T> Evaluate(IEnumerable<T> entities);

        /// <summary>
        /// Determines whether the given entity satisfies the specification.
        /// </summary>
        /// <param name="entity">The entity to test.</param>
        /// <returns><c>true</c> if the entity satisfies the specification; otherwise, <c>false</c>.</returns>
        bool IsSatisfiedBy(T entity);
    }
}