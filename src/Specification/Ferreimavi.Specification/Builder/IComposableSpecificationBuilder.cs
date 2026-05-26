// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Root composition builder. Extends <see cref="IBaseComposableSpecificationBuilder{T}"/>; all
    /// operand chaining, policy configuration, and <c>Build()</c> are inherited from the base interface.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IComposableSpecificationBuilder<T> : IBaseComposableSpecificationBuilder<T>
    {
    }

    /// <summary>
    /// Root composition builder for projectable specifications. Extends
    /// <see cref="IBaseComposableSpecificationBuilder{T, TResult}"/>; all operand chaining,
    /// policy configuration, and <c>Build()</c> are inherited from the base interface.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    public interface IComposableSpecificationBuilder<T, TResult> : IBaseComposableSpecificationBuilder<T, TResult>
    {
    }
}