// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    internal class ComposedGroupOperationBuilder<T>(IComposableSpecificationBuilder<T> rootBuilder, List<CompositionOperation<T>> operations) : IComposedGroupOperationBuilder<T>
    {
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

        public IBaseComposableSpecificationBuilder<T> Not(ISpecification<T> spec)
            => And(new NotSpecification<T>(spec));

        public IBaseComposableSpecificationBuilder<T> CloseGroup()
        {
            rootBuilder.CloseGroup();
            return rootBuilder;
        }

        public IBaseComposableSpecificationBuilder<T> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy)
        {
            rootBuilder.WithOrderingEvaluationPolicy(policy);
            return this;
        }

        public IBaseComposableSpecificationBuilder<T> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy)
        {
            rootBuilder.WithPaginationEvaluationPolicy(policy);
            return this;
        }

        public ISpecification<T> Build() => rootBuilder.Build();
    }

    public class ComposedGroupOperationBuilder<T, TResult>(IComposableSpecificationBuilder<T, TResult> rootBuilder, List<CompositionOperation<T, TResult>> operations) : IComposedGroupOperationBuilder<T, TResult>
    {
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

        public IBaseComposableSpecificationBuilder<T, TResult> Not(ISpecification<T> spec)
            => And(new NotSpecification<T>(spec));

        public IBaseComposableSpecificationBuilder<T, TResult> Not(ISpecification<T, TResult> spec)
            => And(new NotSpecification<T, TResult>(spec));

        public IBaseComposableSpecificationBuilder<T, TResult> CloseGroup()
        {
            rootBuilder.CloseGroup();
            return rootBuilder;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy)
        {
            rootBuilder.WithOrderingEvaluationPolicy(policy);
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy)
        {
            rootBuilder.WithPaginationEvaluationPolicy(policy);
            return this;
        }

        public IBaseComposableSpecificationBuilder<T, TResult> WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy policy)
        {
            rootBuilder.WithProjectionEvaluationPolicy(policy);
            return this;
        }

        public ISpecification<T, TResult> Build() => rootBuilder.Build();

        private static Specification<T, TResult> BuildProjectableSpecification(ISpecification<T> spec)
        {
            var projectionSpec = new Specification<T, TResult>();

            ((List<WhereExpressionInfo<T>>)projectionSpec.WhereExpressions).AddRange(spec.WhereExpressions);
            ((List<OrderByExpressionInfo<T>>)projectionSpec.OrderByExpressions).AddRange(spec.OrderByExpressions);
            ((List<IncludeExpressionInfo>)projectionSpec.IncludeExpressions).AddRange(spec.IncludeExpressions);

            projectionSpec.Skip = spec.Skip;
            projectionSpec.Take = spec.Take;

            if (spec is Specification<T> trackable)
            {
                projectionSpec.AsTracking = trackable.AsTracking;
                projectionSpec.AsNoTracking = trackable.AsNoTracking;
            }

            return projectionSpec;
        }
    }
}