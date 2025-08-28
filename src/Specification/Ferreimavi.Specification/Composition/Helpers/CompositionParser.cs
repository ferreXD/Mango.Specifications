// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    /// <summary>
    /// Provides methods to parse composition operations into specifications.
    /// </summary>
    internal static class CompositionParser
    {
        /// <summary>
        /// Parses a list of composition operations into a non-projectable specification.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <param name="operations">The list of composition operations to parse.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        /// <returns>A specification resulting from the composed operations.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an operation has an unsupported type.</exception>
        /// <exception cref="SpecificationCompositionException">Thrown when the groups in the operations are not balanced.</exception>
        public static ISpecification<T> Parse<T>(
            List<CompositionOperation<T>> operations,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy)
        {
            ValidateGroups(operations);

            if (operations.Count == 0) return new Specification<T>();

            var stack = new Stack<CompositionOperation<T>>();

            foreach (var operation in operations)
                switch (operation.Type)
                {
                    case OperationType.GroupOpen:
                    case OperationType.And:
                    case OperationType.Or:
                        stack.Push(operation);
                        break;
                    case OperationType.GroupClose:
                        ComposeGroupOperation(stack, orderingPolicy, paginationPolicy);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            var list = stack.ToList();
            list.Reverse();
            return ComposeOperations(list, orderingPolicy, paginationPolicy);
        }

        /// <summary>
        /// Parses a list of composition operations into a projectable specification.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <typeparam name="TResult">The result type after projection.</typeparam>
        /// <param name="operations">The list of composition operations to parse.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        /// <param name="projectionPolicy">The policy determining how to combine projection expressions.</param>
        /// <returns>A projectable specification resulting from the composed operations.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an operation has an unsupported type.</exception>
        /// <exception cref="SpecificationCompositionException">Thrown when the groups in the operations are not balanced.</exception>
        public static ISpecification<T, TResult> Parse<T, TResult>(
            List<CompositionOperation<T, TResult>> operations,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy,
            ProjectionEvaluationPolicy projectionPolicy)
        {
            ValidateGroups(operations);

            if (operations.Count == 0) return new Specification<T, TResult>();

            var stack = new Stack<CompositionOperation<T, TResult>>();

            foreach (var operation in operations)
                switch (operation.Type)
                {
                    case OperationType.GroupOpen:
                    case OperationType.And:
                    case OperationType.Or:
                        stack.Push(operation);
                        break;
                    case OperationType.GroupClose:
                        ComposeGroupOperation(stack, orderingPolicy, paginationPolicy, projectionPolicy);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            var list = stack.ToList();
            list.Reverse();
            var spec = ComposeOperations(list, orderingPolicy, paginationPolicy, projectionPolicy);

            if (spec.Selector is null && spec.SelectorMany is null) throw new SpecificationCompositionException("The projection policy requires a projection selector.");

            return spec;
        }

        /// <summary>
        /// Validates that the grouping operations (open/close) are properly balanced.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <param name="operations">The list of composition operations to validate.</param>
        /// <exception cref="SpecificationCompositionException">
        /// Thrown when the groups are not balanced or when a closing group
        /// appears before an opening one.
        /// </exception>
        private static void ValidateGroups<T>(IReadOnlyCollection<CompositionOperation<T>> operations)
        {
            var openCount = operations.Count(op => op.Type == OperationType.GroupOpen);
            var closeCount = operations.Count(op => op.Type == OperationType.GroupClose);
            if (openCount != closeCount) throw new SpecificationCompositionException("The number of open and close group operations must be equal.");
        }

        /// <summary>
        /// Validates that the grouping operations (open/close) are properly balanced for projectable specifications.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <typeparam name="TResult">The result type after projection.</typeparam>
        /// <param name="operations">The list of composition operations to validate.</param>
        /// <exception cref="SpecificationCompositionException">
        /// Thrown when the groups are not balanced or when a closing group
        /// appears before an opening one.
        /// </exception>
        private static void ValidateGroups<T, TResult>(IReadOnlyCollection<CompositionOperation<T, TResult>> operations)
        {
            var openCount = operations.Count(op => op.Type == OperationType.GroupOpen);
            var closeCount = operations.Count(op => op.Type == OperationType.GroupClose);
            if (openCount != closeCount) throw new SpecificationCompositionException("The number of open and close group operations must be equal.");
        }

        #region Non-Projectable Specification Helpers

        /// <summary>
        /// Processes a group of operations from the stack until finding the matching opening group.
        /// Composes those operations into a new specification and pushes it back onto the stack.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <param name="stack">The stack of operations being processed.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        private static void ComposeGroupOperation<T>(
            Stack<CompositionOperation<T>> stack,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy)
        {
            var chainingType = ChainingType.And;
            var groupOps = new List<CompositionOperation<T>>();

            while (stack.Count > 0)
            {
                var op = stack.Pop();
                groupOps.Add(op);
                if (op.Type == OperationType.GroupOpen)
                {
                    chainingType = op.ChainingType ?? ChainingType.And;
                    break;
                }
            }

            groupOps.Reverse();
            var spec = ComposeOperations(groupOps, orderingPolicy, paginationPolicy);
            var groupOperation = new CompositionOperation<T>(
                chainingType == ChainingType.And ? OperationType.And : OperationType.Or,
                spec);
            stack.Push(groupOperation);
        }

        /// <summary>
        /// Composes a list of operations into a single specification by applying AND and OR operations sequentially.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <param name="operations">The list of operations to compose.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        /// <returns>A specification resulting from the composed operations.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an operation has an unsupported type.</exception>
        private static ISpecification<T> ComposeOperations<T>(
            IReadOnlyCollection<CompositionOperation<T>> operations,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy)
        {
            if (operations.Count == 0 || operations.ElementAt(0).Spec == null) return new Specification<T>();

            var spec = operations.ElementAt(0).Spec!;
            for (var i = 1; i < operations.Count; i++)
            {
                var op = operations.ElementAt(i);
                spec = op.Type switch
                {
                    OperationType.And => new AndSpecification<T>(spec, op.Spec ?? new Specification<T>(), orderingPolicy, paginationPolicy),
                    OperationType.Or => new OrSpecification<T>(spec, op.Spec ?? new Specification<T>(), orderingPolicy, paginationPolicy),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return spec;
        }

        #endregion

        #region Projectable Specification Helpers

        /// <summary>
        /// Processes a group of operations from the stack until finding the matching opening group.
        /// Composes those operations into a new projectable specification and pushes it back onto the stack.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <typeparam name="TResult">The result type after projection.</typeparam>
        /// <param name="stack">The stack of operations being processed.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        /// <param name="projectionPolicy">The policy determining how to combine projection expressions.</param>
        private static void ComposeGroupOperation<T, TResult>(
            Stack<CompositionOperation<T, TResult>> stack,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy,
            ProjectionEvaluationPolicy projectionPolicy)
        {
            var chainingType = ChainingType.And;
            var groupOps = new List<CompositionOperation<T, TResult>>();

            while (stack.Count > 0)
            {
                var op = stack.Pop();
                groupOps.Add(op);
                if (op.Type == OperationType.GroupOpen)
                {
                    chainingType = op.ChainingType ?? ChainingType.And;
                    break;
                }
            }

            groupOps.Reverse();
            var spec = ComposeOperations(groupOps, orderingPolicy, paginationPolicy, projectionPolicy);
            var groupOperation = new CompositionOperation<T, TResult>(
                chainingType == ChainingType.And ? OperationType.And : OperationType.Or,
                spec);
            stack.Push(groupOperation);
        }

        /// <summary>
        /// Composes a list of operations into a single projectable specification by applying AND and OR operations sequentially.
        /// </summary>
        /// <typeparam name="T">The entity type of the specification.</typeparam>
        /// <typeparam name="TResult">The result type after projection.</typeparam>
        /// <param name="operations">The list of operations to compose.</param>
        /// <param name="orderingPolicy">The policy determining how to combine ordering expressions.</param>
        /// <param name="paginationPolicy">The policy determining how to combine pagination settings.</param>
        /// <param name="projectionPolicy">The policy determining how to combine projection expressions.</param>
        /// <returns>A projectable specification resulting from the composed operations.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an operation has an unsupported type.</exception>
        private static ISpecification<T, TResult> ComposeOperations<T, TResult>(
            IReadOnlyCollection<CompositionOperation<T, TResult>> operations,
            OrderingEvaluationPolicy orderingPolicy,
            PaginationEvaluationPolicy paginationPolicy,
            ProjectionEvaluationPolicy projectionPolicy)
        {
            if (operations.Count == 0 || operations.ElementAt(0).Spec == null) return new Specification<T, TResult>();

            var spec = operations.ElementAt(0).Spec!;
            for (var i = 1; i < operations.Count; i++)
            {
                var op = operations.ElementAt(i);
                spec = op.Type switch
                {
                    OperationType.And => new AndSpecification<T, TResult>(spec, op.Spec ?? new Specification<T, TResult>(), orderingPolicy, paginationPolicy, projectionPolicy),
                    OperationType.Or => new OrSpecification<T, TResult>(spec, op.Spec ?? new Specification<T, TResult>(), orderingPolicy, paginationPolicy, projectionPolicy),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return spec;
        }

        #endregion
    }
}