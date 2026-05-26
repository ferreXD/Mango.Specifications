// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Base fluent builder for composing specifications. Supports operand chaining (<c>And</c>, <c>Or</c>,
    /// <c>OpenGroup</c>, <c>CloseGroup</c>), policy configuration, and final construction via <c>Build()</c>.
    /// All methods are available on every builder in the chain — no type-escape is required.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IBaseComposableSpecificationBuilder<T>
    {
        IBaseComposableSpecificationBuilder<T> And(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T> Or(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T> Not(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T> CloseGroup();

        IBaseComposableSpecificationBuilder<T> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy);
        IBaseComposableSpecificationBuilder<T> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy);

        ISpecification<T> Build();
    }

    /// <summary>
    /// Base fluent builder for composing projectable specifications. Supports operand chaining,
    /// policy configuration, and final construction via <c>Build()</c>.
    /// All methods are available on every builder in the chain — no type-escape is required.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    public interface IBaseComposableSpecificationBuilder<T, TResult>
    {
        IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T, TResult> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T, TResult> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Not(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Not(ISpecification<T, TResult> spec);
        IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T, TResult> CloseGroup();

        IBaseComposableSpecificationBuilder<T, TResult> WithOrderingEvaluationPolicy(OrderingEvaluationPolicy policy);
        IBaseComposableSpecificationBuilder<T, TResult> WithPaginationEvaluationPolicy(PaginationEvaluationPolicy policy);
        IBaseComposableSpecificationBuilder<T, TResult> WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy policy);

        ISpecification<T, TResult> Build();
    }
}