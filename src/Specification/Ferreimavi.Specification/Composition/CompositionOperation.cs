// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Represents a composition operation for non-projectable specifications.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public record CompositionOperation<T>(
        OperationType Type,
        ISpecification<T>? Spec = null,
        ChainingType? ChainingType = null);

    /// <summary>
    /// Represents a composition operation for projectable specifications.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public record CompositionOperation<T, TResult>(
        OperationType Type,
        ISpecification<T, TResult>? Spec = null,
        ChainingType? ChainingType = null);
}