// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    /// <summary>
    /// Encapsulates data needed to perform sorting.
    /// </summary>
    /// <typeparam name="T">Type of the entity to apply sort on.</typeparam>
    public class OrderByExpressionInfo<T>(Expression<Func<T, object?>> keySelector, OrderTypeEnum orderType)
    {
        private readonly Lazy<Func<T, object?>> _keySelectorFunc = new(keySelector.Compile);

        /// <summary>
        /// A function to extract a key from an element.
        /// </summary>
        public Expression<Func<T, object?>> KeySelector { get; } = keySelector;

        /// <summary>
        /// Whether to (subsequently) sort ascending or descending.
        /// </summary>
        public OrderTypeEnum OrderType { get; } = orderType;

        /// <summary>
        /// Compiled <see cref="KeySelector" />.
        /// </summary>
        public Func<T, object?> KeySelectorFunc => _keySelectorFunc.Value;
    }
}