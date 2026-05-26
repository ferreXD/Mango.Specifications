# Mango.Specifications — Cookbook

Two end-to-end examples that show real-world composition and policy decisions.

---

## Example 1 — Ordering + Pagination Policy

### Problem

You have two independent specifications:

- `OrdersByCustomerSpec` — filters by customer, orders by `CreatedAt DESC`, paginates to first 10.
- `HighValueOrdersSpec` — filters by minimum total, orders by `Total DESC`, paginates to first 5.

You want to compose them with AND, keep **both** orderings (customer date wins tie), and **fail fast** if both paginations are set instead of silently dropping one.

### Setup

```csharp
public sealed class OrdersByCustomerSpec : Specification<Order>
{
    public OrdersByCustomerSpec(Guid customerId)
    {
        Query
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(0).Take(10);
    }
}

public sealed class HighValueOrdersSpec : Specification<Order>
{
    public HighValueOrdersSpec(decimal minTotal)
    {
        Query
            .Where(o => o.Items.Sum(i => i.UnitPrice * i.Quantity) >= minTotal)
            .OrderByDescending(o => o.Items.Sum(i => i.UnitPrice * i.Quantity))
            .Skip(0).Take(5);
    }
}
```

### Composition

```csharp
var byCustomer = new OrdersByCustomerSpec(customerId);
var highValue  = new HighValueOrdersSpec(250m);

var composed = byCustomer.AsComposable()
    .And(highValue)
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.ThrowOnConflict)
    .Build();
```

> **What happens:**
> - Filter: `WHERE CustomerId = @id AND Total >= 250`.
> - Ordering: `ORDER BY CreatedAt DESC, Total DESC` — left (`CreatedAt`) keys win.
> - Pagination: **both specs define `Skip`/`Take`**, so `ThrowOnConflict` throws `InvalidOperationException` at `Build()` time. You must resolve the conflict explicitly by removing pagination from one of the input specs or choosing `Left` / `Right` policy.

### Resolving the Conflict

Option A — discard both paginations and apply a single page at the call site:

```csharp
var composed = byCustomer.AsComposable()
    .And(highValue)
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.None)
    .Build();

// Apply paging on the IQueryable directly:
var page = await db.Orders
    .WithSpecification(composed)
    .Skip(0).Take(20)
    .ToListAsync(ct);
```

Option B — keep only the left spec's pagination:

```csharp
var composed = byCustomer.AsComposable()
    .And(highValue)
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.Left)
    .Build();
// Result is paged Skip(0)/Take(10) from OrdersByCustomerSpec.
```

### Evaluation

```csharp
// EF Core
var orders = await db.Orders.WithSpecification(composed).ToListAsync(ct);

// In-memory (tests)
var orders = composed.Evaluate(inMemoryOrders).ToList();
```

---

## Example 2 — Projection Policy + Grouped OR

### Problem

You are building a search that must return `OrderDto` projections. Two specs each carry a `Select`:

- `OrdersByCustomerSpec<OrderDto>` — projects with customer context (includes customer name).
- `RecentOrdersSpec<OrderDto>` — projects with recency context (includes age in days).

You want: `(ByCustomer) OR (Recent AND HighValue)` — a grouped disjunction — using the **left** spec's projection (customer-context DTO).

### Setup

```csharp
public sealed class OrdersByCustomerSpec : Specification<Order, OrderDto>
{
    public OrdersByCustomerSpec(Guid customerId)
    {
        Query
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id           = o.Id,
                CustomerName = o.Customer.Name,   // customer-context
                Total        = o.Items.Sum(i => i.UnitPrice * i.Quantity),
                Date         = o.CreatedAt
            });
    }
}

public sealed class RecentOrdersSpec : Specification<Order, OrderDto>
{
    public RecentOrdersSpec(DateTime since)
    {
        Query
            .Where(o => o.CreatedAt >= since)
            .Select(o => new OrderDto
            {
                Id      = o.Id,
                AgeDays = (int)(DateTime.UtcNow - o.CreatedAt).TotalDays,  // recency-context
                Total   = o.Items.Sum(i => i.UnitPrice * i.Quantity),
                Date    = o.CreatedAt
            });
    }
}

public sealed class HighValueOrdersSpec : Specification<Order>
{
    public HighValueOrdersSpec(decimal minTotal)
    {
        Query.Where(o => o.Items.Sum(i => i.UnitPrice * i.Quantity) >= minTotal);
    }
}
```

### Composition

```csharp
var byCustomer = new OrdersByCustomerSpec(customerId);
var recent     = new RecentOrdersSpec(DateTime.UtcNow.AddDays(-30));
var highValue  = new HighValueOrdersSpec(500m);

//  byCustomer OR (recent AND highValue)
var composed = byCustomer.AsComposable()
    .OpenGroup(recent, ChainingType.Or)   // OR (
        .And(highValue)                   //       recent AND highValue
    .CloseGroup()                         //    )
    .WithProjectionEvaluationPolicy(ProjectionEvaluationPolicy.Left)
    .Build();
```

> **What happens:**
> - Filter: `WHERE CustomerId = @id OR (CreatedAt >= @since AND Total >= 500)`.
> - The group `(recent AND highValue)` is evaluated as a unit before being OR-ed with `byCustomer`.
> - Projection: `Left` policy picks `byCustomer`'s selector — `OrderDto` with `CustomerName`.
> - `AgeDays` from the right spec's selector is discarded because the left projection wins.

### Evaluation

```csharp
// EF Core — returns IQueryable<OrderDto>
var dtos = await db.Orders.WithSpecification(composed).ToListAsync(ct);

// In-memory (tests)
var dtos = composed.Evaluate(inMemoryOrders).ToList();
```

### Policy Note

If you want the **recency** projection instead, switch to `ProjectionEvaluationPolicy.Right`.  
If both specs must contribute different fields to the same DTO, compose the projection manually outside the spec and use a non-projectable composed spec + a final `Select` on the `IQueryable`.

---

## Quick Reference

| Goal | API |
|---|---|
| Simple AND | `specA.AsComposable().And(specB).Build()` |
| Simple OR | `specA.AsComposable().Or(specB).Build()` |
| Negate a spec | `spec.Not()` or `.Not(spec)` in a chain |
| Group precedence | `.OpenGroup(spec).Or(other).CloseGroup()` |
| Keep both orderings | `WithOrderingEvaluationPolicy(BothLeftPriority)` |
| Fail on pagination conflict | `WithPaginationEvaluationPolicy(ThrowOnConflict)` |
| Choose projection winner | `WithProjectionEvaluationPolicy(Left \| Right)` |
