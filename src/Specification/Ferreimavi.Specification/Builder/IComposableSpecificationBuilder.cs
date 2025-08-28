// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IComposableSpecificationBuilder<T> : IBaseComposableSpecificationBuilder<T>
    {
        public IComposableSpecificationBuilder<T> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy);

        public IComposableSpecificationBuilder<T> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy);

        public ISpecification<T> Build();
    }

    public interface IComposableSpecificationBuilder<T, TResult> : IBaseComposableSpecificationBuilder<T, TResult>
    {
        public IComposableSpecificationBuilder<T, TResult> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy);

        public IComposableSpecificationBuilder<T, TResult> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy);

        public IComposableSpecificationBuilder<T, TResult> WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy policy);

        public ISpecification<T, TResult> Build();
    }
}