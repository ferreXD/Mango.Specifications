// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class ComposableSpecificationBuilder<T>(
        Specification<T> specification,
        OrderingEvaluationPolicy orderingEvaluationPolicy,
        PaginationEvaluationPolicy paginationEvaluationPolicy) : IComposableSpecificationBuilder<T>
    {
        private readonly List<CompositionOperation<T>> _operations = [new CompositionOperation<T>(OperationType.And, specification)];

        private OrderingEvaluationPolicy _orderingPolicy = orderingEvaluationPolicy;
        private PaginationEvaluationPolicy _paginationPolicy = paginationEvaluationPolicy;

        public ComposableSpecificationBuilder(Specification<T> specification) : this(specification, OrderingEvaluationPolicy.None, PaginationEvaluationPolicy.None)
        {
        }

        public IBaseComposableSpecificationBuilder<T> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And)
        {
            _operations.Add(new CompositionOperation<T>(OperationType.GroupOpen, initialSpec, type));
            return new ComposedGroupOperationBuilder<T>(this, _operations);
        }

        public IBaseComposableSpecificationBuilder<T> And(ISpecification<T> spec)
        {
            _operations.Add(new CompositionOperation<T>(OperationType.And, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T> Or(ISpecification<T> spec)
        {
            _operations.Add(new CompositionOperation<T>(OperationType.Or, spec));
            return this;
        }

        public IComposableSpecificationBuilder<T> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy)
        {
            _orderingPolicy = policy;
            return this;
        }

        public IComposableSpecificationBuilder<T> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy)
        {
            _paginationPolicy = policy;
            return this;
        }

        public ISpecification<T> Build() => BuildInternal();

        public IBaseComposableSpecificationBuilder<T> CloseGroup()
        {
            _operations.Add(new CompositionOperation<T>(OperationType.GroupClose));
            return this;
        }

        public IComposableSpecificationBuilder<T> ReturnRoot() => this;

        // parse Operations in order, unify them into a single spec
        // using your “stack-based” or “shunting-yard” approach.
        // Then apply _orderingPolicy, _paginationPolicy at the final step.
        internal ISpecification<T> BuildInternal() =>
            CompositionParser.Parse(_operations, _orderingPolicy, _paginationPolicy);
    }


    public class ComposableSpecificationBuilder<T, TResult>(
        Specification<T, TResult> specification,
        OrderingEvaluationPolicy orderingEvaluationPolicy,
        PaginationEvaluationPolicy paginationEvaluationPolicy,
        ProjectionEvaluationPolicy projectionEvaluationPolicy) : IComposableSpecificationBuilder<T, TResult>
    {
        private readonly List<CompositionOperation<T, TResult>> _operations = [new CompositionOperation<T, TResult>(OperationType.And, specification)];

        private OrderingEvaluationPolicy _orderingPolicy = orderingEvaluationPolicy;
        private PaginationEvaluationPolicy _paginationPolicy = paginationEvaluationPolicy;
        private ProjectionEvaluationPolicy _projectionPolicy = projectionEvaluationPolicy;

        public ComposableSpecificationBuilder(Specification<T, TResult> specification) : this(specification, OrderingEvaluationPolicy.None, PaginationEvaluationPolicy.None, ProjectionEvaluationPolicy.Left)
        {
        }

        public ComposableSpecificationBuilder(Specification<T> specification) : this(BuildProjectableSpecification(specification), OrderingEvaluationPolicy.None, PaginationEvaluationPolicy.None, ProjectionEvaluationPolicy.Left)
        {
        }

        public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And)
        {
            var projectionSpec = BuildProjectableSpecification(initialSpec);
            _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, projectionSpec, type));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType type = ChainingType.And)
        {
            _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, initialSpec, type));
            return new ComposedGroupOperationBuilder<T, TResult>(this, _operations);
        }

        public IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T> spec)
        {
            var projectionSpec = BuildProjectableSpecification(spec);

            _operations.Add(new CompositionOperation<T, TResult>(OperationType.And, projectionSpec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T, TResult> spec)
        {
            _operations.Add(new CompositionOperation<T, TResult>(OperationType.And, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T> spec)
        {
            var projectionSpec = BuildProjectableSpecification(spec);

            _operations.Add(new CompositionOperation<T, TResult>(OperationType.Or, projectionSpec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T, TResult> spec)
        {
            _operations.Add(new CompositionOperation<T, TResult>(OperationType.Or, spec));
            return this;
        }

        public IComposableSpecificationBuilder<T, TResult> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy)
        {
            _orderingPolicy = policy;
            return this;
        }

        public IComposableSpecificationBuilder<T, TResult> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy)
        {
            _paginationPolicy = policy;
            return this;
        }

        public IComposableSpecificationBuilder<T, TResult> WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy policy)
        {
            _projectionPolicy = policy;
            return this;
        }

        public ISpecification<T, TResult> Build() => BuildInternal();

        public IBaseComposableSpecificationBuilder<T, TResult> CloseGroup()
        {
            _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupClose));
            return this;
        }

        public IComposableSpecificationBuilder<T, TResult> ReturnRoot() => this;

        // parse Operations in order, unify them into a single spec
        // using your “stack-based” or “shunting-yard” approach.
        // Then apply _orderingPolicy, _paginationPolicy at the final step.
        internal ISpecification<T, TResult> BuildInternal() =>
            CompositionParser.Parse(_operations, _orderingPolicy, _paginationPolicy, _projectionPolicy);

        private static Specification<T, TResult> BuildProjectableSpecification(ISpecification<T> spec)
        {
            var projectionSpec = new Specification<T, TResult>();

            ((List<WhereExpressionInfo<T>>)projectionSpec.WhereExpressions).AddRange(spec.WhereExpressions);
            ((List<OrderByExpressionInfo<T>>)projectionSpec.OrderByExpressions).AddRange(spec.OrderByExpressions);
            ((List<IncludeExpressionInfo>)projectionSpec.IncludeExpressions).AddRange(spec.IncludeExpressions);

            projectionSpec.Skip = spec.Skip;
            projectionSpec.Take = spec.Take;
            projectionSpec.AsTracking = spec.AsTracking;
            projectionSpec.AsNoTracking = spec.AsNoTracking;

            return projectionSpec;
        }
    }
}