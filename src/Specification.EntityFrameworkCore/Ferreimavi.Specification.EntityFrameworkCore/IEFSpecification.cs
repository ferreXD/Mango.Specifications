// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    using Mango.Specifications;

    /// <summary>
    /// Marks a specification as EF Core–aware by adding explicit change-tracking hints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ISpecification{T}"/> intentionally omits EF-specific properties such as
    /// <see cref="AsTracking"/> and <see cref="AsNoTracking"/> so that core in-memory specifications
    /// carry no ORM dependency. This interface is the contract for specifications that target an
    /// Entity Framework Core query pipeline.
    /// </para>
    /// <para>
    /// The built-in <see cref="Specification{T}"/> concrete class carries these properties directly
    /// (without implementing this interface, to avoid a circular package dependency).
    /// Custom specification types that do not extend <see cref="Specification{T}"/> should implement
    /// <see cref="IEFSpecification{T}"/> when they need to participate in the EF evaluator pipeline.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IEFSpecification<T> : ISpecification<T>
    {
        /// <summary>
        /// Gets a value indicating whether entities returned by the query should be tracked by the
        /// change tracker. When <c>true</c>, <see cref="AsNoTracking"/> must be <c>false</c>.
        /// </summary>
        bool AsTracking { get; }

        /// <summary>
        /// Gets a value indicating whether entities returned by the query should <em>not</em> be
        /// tracked by the change tracker. When <c>true</c>, <see cref="AsTracking"/> must be <c>false</c>.
        /// </summary>
        bool AsNoTracking { get; }
    }
}
