// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    // As of right now we only have in-memory evaluators (ORM evaluators are not our concern yet, they'll need a different interface and implementation will depend on the ORM)
    public interface IInMemorySpecificationEvaluator
    {
        IEnumerable<IGrouping<TKey, TResult>> Evaluate<T, TKey, TResult>(IEnumerable<T> source, IGroupingSpecification<T, TKey, TResult> specification);
        IEnumerable<TResult> Evaluate<T, TResult>(IEnumerable<T> source, ISpecification<T, TResult> specification);
        IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification);
    }
}