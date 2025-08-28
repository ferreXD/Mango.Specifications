// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public class WhereExpressionInfo<T>(Expression<Func<T, bool>> filter)
    {
        private readonly Lazy<Func<T, bool>> _filterFunc = new(filter.Compile);

        public Expression<Func<T, bool>> Filter { get; } = filter;
        public Func<T, bool> FilterFunc => _filterFunc.Value;
    }
}