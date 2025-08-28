// ReSharper disable once CheckNamespace

namespace Mango.Specifications.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Evaluates and applies Include expressions from a specification to an Entity Framework Core query.
    /// </summary>
    internal class IncludeQueryEvaluator : IQueryEvaluator
    {
        /// <summary>
        /// Method info for EntityFrameworkQueryableExtensions.Include method used for the initial Include expression.
        /// </summary>
        private static readonly MethodInfo _includeMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
                .Single(mi => mi.GetGenericArguments().Length == 2
                              && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                              && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

        /// <summary>
        /// Method info for EntityFrameworkQueryableExtensions.ThenInclude method used after a reference navigation.
        /// </summary>
        private static readonly MethodInfo _thenIncludeAfterReferenceMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Single(mi => mi.GetGenericArguments().Length == 3
                              && mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter
                              && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IIncludableQueryable<,>)
                              && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

        /// <summary>
        /// Method info for EntityFrameworkQueryableExtensions.ThenInclude method used after a collection navigation.
        /// </summary>
        private static readonly MethodInfo _thenIncludeAfterEnumerableMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Where(mi => mi.GetGenericArguments().Length == 3)
                .Single(
                    mi =>
                    {
                        var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1];

                        return typeInfo.IsGenericType
                               && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                               && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IIncludableQueryable<,>)
                               && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>);
                    });

        /// <summary>
        /// Gets the singleton instance of the <see cref="IncludeQueryEvaluator" />.
        /// </summary>
        public static IncludeQueryEvaluator Instance { get; } = new();

        /// <summary>
        /// Gets a value indicating whether this evaluator evaluates criteria (where expressions).
        /// Always returns false for include evaluators.
        /// </summary>
        public bool IsCriteriaEvaluator => false;

        /// <summary>
        /// Applies the include expressions from the specification to the query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="query">The query to which the include expressions will be applied.</param>
        /// <param name="specification">The specification containing include expressions.</param>
        /// <returns>A query with include expressions applied.</returns>
        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            return specification.IncludeExpressions.Aggregate(query, (current, includeInfo) => includeInfo.Type switch
            {
                IncludeTypeEnum.Include => BuildInclude<T>(current, includeInfo),
                IncludeTypeEnum.ThenInclude => BuildThenInclude<T>(current, includeInfo),
                _ => current
            });
        }

        /// <summary>
        /// Builds an Include query from the provided include information.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="query">The query to which the include will be applied.</param>
        /// <param name="includeInfo">Information about the include expression.</param>
        /// <returns>A query with the include expression applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when includeInfo is null.</exception>
        /// <exception cref="TargetException">Thrown when the result of invoking the Include method is null.</exception>
        private IQueryable<T> BuildInclude<T>(IQueryable query, IncludeExpressionInfo includeInfo)
        {
            _ = includeInfo ?? throw new ArgumentNullException(nameof(includeInfo));
            var result = _includeMethodInfo.MakeGenericMethod(includeInfo.EntityType, includeInfo.PropertyType).Invoke(null, new object[] { query, includeInfo.LambdaExpression });

            _ = result ?? throw new TargetException();
            return (IQueryable<T>)result;
        }

        /// <summary>
        /// Builds a ThenInclude query from the provided include information.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="query">The query to which the then-include will be applied.</param>
        /// <param name="includeInfo">Information about the then-include expression.</param>
        /// <returns>A query with the then-include expression applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when includeInfo or previousPropertyType is null.</exception>
        /// <exception cref="TargetException">Thrown when the result of invoking the ThenInclude method is null.</exception>
        private IQueryable<T> BuildThenInclude<T>(IQueryable query, IncludeExpressionInfo includeInfo)
        {
            _ = includeInfo ?? throw new ArgumentNullException(nameof(includeInfo));
            _ = includeInfo.PreviousPropertyType ?? throw new ArgumentNullException(nameof(includeInfo.PreviousPropertyType));

            var result = (IsGenericEnumerable(includeInfo.PreviousPropertyType, out var previousPropertyType) ? _thenIncludeAfterEnumerableMethodInfo : _thenIncludeAfterReferenceMethodInfo).MakeGenericMethod(includeInfo.EntityType, previousPropertyType, includeInfo.PropertyType)
                .Invoke(null, new object[] { query, includeInfo.LambdaExpression });

            _ = result ?? throw new TargetException();
            return (IQueryable<T>)result;
        }

        /// <summary>
        /// Determines whether a type is a generic enumerable and extracts its element type if it is.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="propertyType">
        /// When this method returns, contains the element type if the input type is a generic
        /// enumerable; otherwise, contains the input type.
        /// </param>
        /// <returns><c>true</c> if the type is a generic enumerable; otherwise, <c>false</c>.</returns>
        private static bool IsGenericEnumerable(Type type, out Type propertyType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                propertyType = type.GenericTypeArguments[0];
                return true;
            }

            propertyType = type;
            return false;
        }
    }
}