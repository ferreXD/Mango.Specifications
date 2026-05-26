// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// A specification builder for projectable specifications that produce a <typeparamref name="TResult"/>.
    /// Inherits from <see cref="ISpecificationBuilder{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Specification"/> property is intentionally typed as the concrete <see cref="Specification{T, TResult}"/>
    /// rather than the <see cref="ISpecification{T, TResult}"/> interface. This is a permanent design constraint: all library
    /// extension methods depend on direct access to the concrete type in order to call its <c>internal</c> mutation methods
    /// (<c>AddWhere</c>, <c>AddInclude</c>, <c>AddOrderBy</c>, <c>ClearOrdering</c>) and <c>internal set</c> properties.
    /// Custom implementations of this interface must therefore always back the <see cref="Specification"/> property with an
    /// instance of <see cref="Specification{T, TResult}"/> (or a subclass).
    /// </para>
    /// </remarks>
    public interface ISpecificationBuilder<T, TResult> : ISpecificationBuilder<T>
    {
        new Specification<T, TResult> Specification { get; }
    }

    /// <summary>
    /// A fluent builder that accumulates query clauses (filters, includes, ordering, pagination, tracking)
    /// onto a <see cref="Specification{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Specification"/> property is intentionally typed as the concrete <see cref="Specification{T}"/>
    /// rather than the <see cref="ISpecification{T}"/> interface. This is a permanent design constraint: all library
    /// extension methods depend on direct access to the concrete type in order to call its <c>internal</c> mutation methods
    /// (<c>AddWhere</c>, <c>AddInclude</c>, <c>AddOrderBy</c>, <c>ClearOrdering</c>) and <c>internal set</c> properties.
    /// Custom implementations of this interface must therefore always back the <see cref="Specification"/> property with an
    /// instance of <see cref="Specification{T}"/> (or a subclass).
    /// </para>
    /// </remarks>
    public interface ISpecificationBuilder<T>
    {
        Specification<T> Specification { get; }
    }
}