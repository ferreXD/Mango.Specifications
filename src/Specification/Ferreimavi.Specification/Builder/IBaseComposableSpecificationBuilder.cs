// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public interface IBaseComposableSpecificationBuilder<T>
    {
        IBaseComposableSpecificationBuilder<T> And(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T> Or(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T> CloseGroup();
        IComposableSpecificationBuilder<T> ReturnRoot();
    }


    public interface IBaseComposableSpecificationBuilder<T, TResult>
    {
        IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T, TResult> And(ISpecification<T, TResult> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T> spec);
        IBaseComposableSpecificationBuilder<T, TResult> Or(ISpecification<T, TResult> spec);
        IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType type = ChainingType.And);
        IBaseComposableSpecificationBuilder<T, TResult> CloseGroup();
        IComposableSpecificationBuilder<T, TResult> ReturnRoot();
    }
}