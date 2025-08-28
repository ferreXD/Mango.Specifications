namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a grouping specification that encapsulates query conditions including grouping and projection.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    /// <typeparam name="TResult">The type of the result of grouping.</typeparam>
    public class GroupingSpecification<T, TKey, TResult> : Specification<T>, IGroupingSpecification<T, TKey, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingSpecification{T, TKey, TResult}" /> class using the default
        /// in-memory evaluator.
        /// </summary>
        public GroupingSpecification() : this(InMemorySpecificationEvaluator.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingSpecification{T, TKey, TResult}" /> class with the specified
        /// in-memory specification evaluator.
        /// </summary>
        /// <param name="inMemorySpecificationEvaluator">The in-memory specification evaluator.</param>
        public GroupingSpecification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator) : base(inMemorySpecificationEvaluator)
        {
            Query = new GroupingSpecificationBuilder<T, TKey, TResult>(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingSpecification{T, TKey, TResult}" /> class with a group result
        /// selector.
        /// </summary>
        /// <param name="groupSelector">The expression to define the group result selector.</param>
        protected GroupingSpecification(Expression<Func<T, TResult>> groupSelector) : this(InMemorySpecificationEvaluator.Default)
        {
            GroupResultSelector = groupSelector;
        }

        /// <inheritdoc />
        public new virtual IGroupingSpecificationBuilder<T, TKey, TResult> Query { get; }

        /// <inheritdoc />
        public new Func<IEnumerable<IGrouping<TKey, TResult>>, IEnumerable<IGrouping<TKey, TResult>>>? PostProcessingAction { get; internal set; }

        /// <inheritdoc />
        public Expression<Func<T, TKey>>? GroupBySelector { get; internal set; }

        /// <inheritdoc />
        public Expression<Func<T, TResult>>? GroupResultSelector { get; internal set; }

        /// <inheritdoc />
        public new virtual IEnumerable<IGrouping<TKey, TResult>> Evaluate(IEnumerable<T> entities) => Evaluator.Evaluate(entities, this);
    }

    /// <summary>
    /// Represents a grouping specification where the result type is the same as the entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity and the result.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    public class GroupingSpecification<T, TKey> : GroupingSpecification<T, TKey, T>, IGroupingSpecification<T, TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingSpecification{T, TKey}" /> class using the default grouping
        /// behavior.
        /// </summary>
        public GroupingSpecification() : base(x => x)
        {
        }
    }
}