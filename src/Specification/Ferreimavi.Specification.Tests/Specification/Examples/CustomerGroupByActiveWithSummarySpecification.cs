namespace Mango.Specifications.Tests.Specification.Examples
{
    using Models;

    /// <summary>
    /// Groups customers by their active status and projects each customer to a <see cref="CustomerActiveSummary"/>.
    /// This spec deliberately uses <c>TResult ≠ T</c> to exercise the explicit-selector path in the
    /// <see cref="GroupingSpecification{T,TKey,TResult}"/> constructor.
    /// </summary>
    internal sealed class CustomerGroupByActiveWithSummarySpecification
        : GroupingSpecification<Customer, bool, CustomerActiveSummary>
    {
        public CustomerGroupByActiveWithSummarySpecification()
        {
            Query
                .GroupBy(c => c.IsActive)
                .Select(c => new CustomerActiveSummary { Name = c.Name, IsActive = c.IsActive });
        }
    }
}
