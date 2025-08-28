namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <inheritdoc cref="Specification{T,TResult}" />
    /// <summary>
    /// Represents a specification with a result projection.
    /// Inherits from <see cref="Specification{T}" /> and implements <see cref="ISpecification{T, TResult}" />.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class Specification<T, TResult> : Specification<T>, ISpecification<T, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T, TResult}" /> class using the default in-memory evaluator.
        /// </summary>
        public Specification() : this(InMemorySpecificationEvaluator.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T, TResult}" /> class with the specified in-memory
        /// specification evaluator.
        /// </summary>
        /// <param name="inMemorySpecificationEvaluator">The in-memory specification evaluator.</param>
        public Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator) : base(inMemorySpecificationEvaluator)
        {
            Query = new SpecificationBuilder<T, TResult>(this);
        }

        /// <inheritdoc />
        public new virtual ISpecificationBuilder<T, TResult> Query { get; }

        /// <inheritdoc />
        public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; internal set; }

        /// <inheritdoc />
        public new virtual IEnumerable<TResult> Evaluate(IEnumerable<T> entities) => Evaluator.Evaluate(entities, this);

        /// <inheritdoc />
        public Expression<Func<T, TResult>>? Selector { get; internal set; }

        /// <inheritdoc />
        public Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; internal set; }
    }

    /// <inheritdoc cref="Specification{T}" />
    /// <summary>
    /// Represents a specification that encapsulates query conditions and options.
    /// Implements the <see cref="ISpecification{T}" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of the entity to evaluate against the specification.</typeparam>
    public class Specification<T> : ISpecification<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T}" /> class with the specified in-memory evaluator and
        /// specification validator.
        /// </summary>
        /// <param name="inMemorySpecificationEvaluator">The in-memory specification evaluator.</param>
        /// <param name="specificationValidator">The specification validator.</param>
        public Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator, ISpecificationValidator specificationValidator)
        {
            Evaluator = inMemorySpecificationEvaluator;
            Validator = specificationValidator;
            Query = new SpecificationBuilder<T>(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T}" /> class using the default in-memory evaluator and
        /// default specification validator.
        /// </summary>
        public Specification() : this(InMemorySpecificationEvaluator.Default, SpecificationValidator.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T}" /> class with the specified in-memory evaluator.
        /// Uses the default specification validator.
        /// </summary>
        /// <param name="inMemorySpecificationEvaluator">The in-memory specification evaluator.</param>
        public Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator) : this(inMemorySpecificationEvaluator, SpecificationValidator.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification{T}" /> class with the specified specification validator.
        /// Uses the default in-memory evaluator.
        /// </summary>
        /// <param name="specificationValidator">The specification validator.</param>
        public Specification(ISpecificationValidator specificationValidator) : this(InMemorySpecificationEvaluator.Default, specificationValidator)
        {
        }

        /// <summary>
        /// Gets the in-memory specification evaluator.
        /// </summary>
        protected IInMemorySpecificationEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the specification validator.
        /// </summary>
        protected ISpecificationValidator Validator { get; }

        /// <inheritdoc />
        public virtual ISpecificationBuilder<T> Query { get; }

        /// <inheritdoc />
        public virtual IEnumerable<T> Evaluate(IEnumerable<T> entities) => Evaluator.Evaluate(entities, this);

        /// <inheritdoc />
        public virtual bool IsSatisfiedBy(T entity) => Validator.IsValid(entity, this);

        /// <inheritdoc />
        public IReadOnlyCollection<WhereExpressionInfo<T>> WhereExpressions => _whereExpressions;

        /// <inheritdoc />
        public IReadOnlyCollection<OrderByExpressionInfo<T>> OrderByExpressions => _orderByExpressions;

        /// <inheritdoc />
        public IReadOnlyCollection<IncludeExpressionInfo> IncludeExpressions => _includeExpressions;

        /// <inheritdoc />
        public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; }

        /// <inheritdoc />
        public int? Skip { get; internal set; }

        /// <inheritdoc />
        public int? Take { get; internal set; }

        /// <inheritdoc />
        public bool AsTracking { get; internal set; } = false;

        /// <inheritdoc />
        public bool AsNoTracking { get; internal set; } = false;

        #region Private Properties

        private readonly List<WhereExpressionInfo<T>> _whereExpressions = new();
        private readonly List<OrderByExpressionInfo<T>> _orderByExpressions = new();
        private readonly List<IncludeExpressionInfo> _includeExpressions = new();

        #endregion

        #region Internal Setters

        /// <summary>
        /// Adds a where expression to the specification.
        /// </summary>
        /// <param name="whereInfo">The where expression info to add.</param>
        internal void AddWhere(WhereExpressionInfo<T> whereInfo) => _whereExpressions.Add(whereInfo);

        /// <summary>
        /// Adds an order by expression to the specification.
        /// </summary>
        /// <param name="orderByInfo">The order by expression info to add.</param>
        internal void AddOrderBy(OrderByExpressionInfo<T> orderByInfo) => _orderByExpressions.Add(orderByInfo);

        /// <summary>
        /// Adds an include expression to the specification.
        /// </summary>
        /// <param name="includeInfo">The include expression info to add.</param>
        internal void AddInclude(IncludeExpressionInfo includeInfo) => _includeExpressions.Add(includeInfo);

        /// <summary>
        /// Clears all ordering expressions from the specification.
        /// </summary>
        internal void ClearOrdering() => _orderByExpressions.Clear();

        #endregion
    }
}