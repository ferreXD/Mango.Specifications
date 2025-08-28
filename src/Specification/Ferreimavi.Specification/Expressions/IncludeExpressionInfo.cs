// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    using System.Linq.Expressions;

    public class IncludeExpressionInfo
    {
        /// <summary>
        /// Creates an 'Include' expression (for T -> TProperty).
        /// </summary>
        public IncludeExpressionInfo(
            LambdaExpression expression,
            Type entityType,
            Type propertyType)
        {
            LambdaExpression = expression ?? throw new ArgumentNullException(nameof(expression));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            Type = IncludeTypeEnum.Include;
        }

        /// <summary>
        /// Creates a 'ThenInclude' expression (for TPrev -> TProperty),
        /// referencing the parent property type if needed.
        /// </summary>
        public IncludeExpressionInfo(
            LambdaExpression expression,
            Type entityType,
            Type propertyType,
            Type previousPropertyType)
        {
            LambdaExpression = expression ?? throw new ArgumentNullException(nameof(expression));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            PreviousPropertyType = previousPropertyType ?? throw new ArgumentNullException(nameof(previousPropertyType));
            Type = IncludeTypeEnum.ThenInclude;
        }

        /// <summary>
        /// The actual expression: e.g. x => x.Orders, or x => x.Items
        /// </summary>
        public LambdaExpression LambdaExpression { get; }

        /// <summary>
        /// The type of the root entity or the "starting" entity for a ThenInclude.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// The property type that is being included.
        /// Could be a single navigation property or a collection.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// The type of the previously included entity (only for ThenInclude).
        /// </summary>
        public Type? PreviousPropertyType { get; }

        /// <summary>
        /// Whether this is an Include or ThenInclude.
        /// </summary>
        public IncludeTypeEnum Type { get; }
    }
}