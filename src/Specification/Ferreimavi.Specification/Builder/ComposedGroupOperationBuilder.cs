// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class ComposedGroupOperationBuilder<T>(IBaseComposableSpecificationBuilder<T> rootBuilder, List<CompositionOperation<T>> operations) : IComposedGroupOperationBuilder<T>
    {
        private readonly IBaseComposableSpecificationBuilder<T> _rootBuilder = rootBuilder;

        public IBaseComposableSpecificationBuilder<T> OpenGroup(ISpecification<T> initialSpec, ChainingType chainingType = ChainingType.And)
        {
            operations.Add(new CompositionOperation<T>(OperationType.GroupOpen, initialSpec, chainingType));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T> And(ISpecification<T> spec)
        {
            operations.Add(new CompositionOperation<T>(OperationType.And, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T> Or(ISpecification<T> spec)
        {
            operations.Add(new CompositionOperation<T>(OperationType.Or, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T> CloseGroup()
        {
            // We notify the parent builder that a group is closed
            _rootBuilder.CloseGroup();
            return _rootBuilder;
        }

        public IComposableSpecificationBuilder<T> ReturnRoot()
        {
            if (_rootBuilder is IComposableSpecificationBuilder<T> root) return root;

            return RecurseToRoot(_rootBuilder);
        }

        private IComposableSpecificationBuilder<T> RecurseToRoot(IBaseComposableSpecificationBuilder<T> builder)
        {
            if (builder is IComposableSpecificationBuilder<T> root) return root;

            var composedGroupOperationBuilder = builder as ComposedGroupOperationBuilder<T>;
            return RecurseToRoot(composedGroupOperationBuilder!._rootBuilder);
        }
    }

    public class ComposedGroupOperationBuilder<T, TResult>(IBaseComposableSpecificationBuilder<T, TResult> rootBuilder, List<CompositionOperation<T, TResult>> operations) : IComposedGroupOperationBuilder<T, TResult>
    {
        private readonly IBaseComposableSpecificationBuilder<T, TResult> _rootBuilder = rootBuilder;

        public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And)
        {
            var projectionSpec = BuildProjectableSpecification(initialSpec);
            operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, projectionSpec, type));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType chainingType = ChainingType.And)
        {
            operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, initialSpec, chainingType));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T> spec)
        {
            var projectionSpec = BuildProjectableSpecification(spec);

            operations.Add(new CompositionOperation<T, TResult>(OperationType.And, projectionSpec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T, TResult> spec)
        {
            operations.Add(new CompositionOperation<T, TResult>(OperationType.And, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T> spec)
        {
            var projectionSpec = BuildProjectableSpecification(spec);

            operations.Add(new CompositionOperation<T, TResult>(OperationType.Or, projectionSpec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T, TResult> spec)
        {
            operations.Add(new CompositionOperation<T, TResult>(OperationType.Or, spec));
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> CloseGroup()
        {
            // We notify the parent builder that a group is closed
            _rootBuilder.CloseGroup();
            return _rootBuilder;
        }

        public IComposableSpecificationBuilder<T, TResult> ReturnRoot()
        {
            if (_rootBuilder is IComposableSpecificationBuilder<T, TResult> root) return root;

            return RecurseToRoot(_rootBuilder);
        }

        private IComposableSpecificationBuilder<T, TResult> RecurseToRoot(IBaseComposableSpecificationBuilder<T, TResult> builder)
        {
            if (builder is IComposableSpecificationBuilder<T, TResult> root) return root;

            var composedGroupOperationBuilder = builder as ComposedGroupOperationBuilder<T, TResult>;
            return RecurseToRoot(composedGroupOperationBuilder!._rootBuilder);
        }

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