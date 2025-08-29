# Mango.Specifications

> **Specification++ for .NET/EF Core** — parentheses-aware composition (`And/Or/Not` with groups), **policy-driven merges** (ordering/pagination/projection), and **first-class GroupBy**. In-memory and EF Core parity.

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Status:** WIP (pre-v1). API surface may change. Targeting `v1.0.0` after docs polish & EF ergonomics.

---

## Table of Contents

- [About](#about)
- [What’s New](#whats-new)
- [Features](#features)
- [Install](#install)
- [Quickstart](#quickstart)
- [Composition & Policies](#composition--policies)
- [Grouping Example](#grouping-example)
- [EF Toggles](#ef-toggles)
- [In-Memory vs EF Parity](#in-memory-vs-ef-parity)
- [Documentation](#documentation)
- [Building](#building)
- [Running Tests](#running-tests)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License & Credits](#license--credits)
  
---

## About

**Mango.Specifications** implements the Specification pattern for .NET with a focus on:
- **Expressive composition**: `(A AND (B OR C))` with explicit precedence.
- **Policy-controlled merges**: pick winners for ordering/pagination/projection.
- **EF ergonomics**: includes, tracking modes, (planned) split queries, query filter controls.
- **Parity**: evaluate the same spec **in-memory** (tests) and **in EF** (production).

Heavily inspired by **Ardalis.Specification**, extended with composition policies and first-class grouping.

---

## What’s New

> **Pre-release summary** (update per tag)

- Composition API: `AsComposable()`, `OpenGroup/CloseGroup`, `ReturnRoot`, `.Build()`.
- Merge policies: `OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy`.
- Grouping: `GroupingSpecification<T,TKey,TResult>` with result selectors and parity evaluators.
- EF adapter: `DbSetExtensions.WithSpecification(...)`, evaluator pipeline, includes, tracking.

_See [CHANGELOG](./CHANGELOG.md) for details (TODO)._

---

## Features

- ✅ Fluent builders: `Where`, `Include/ThenInclude`, `OrderBy/ThenBy`, `Skip/Take`, `Select`.
- ✅ **Parentheses-aware** `And/Or/Not` composition.
- ✅ **Policies** to resolve conflicts in ordering/pagination/projection.
- ✅ **Grouping** with result selectors (EF + in-memory).
- ✅ EF Core adapter + in-memory evaluator.
- 🧭 Clear **gotchas** documented (negation scope, pagination conflicts).

---

## Install

```bash
# Coming soon on NuGet
dotnet add package Mango.Specifications
dotnet add package Mango.Specifications.EntityFrameworkCore
```

> For now: project reference or local feed.

---

## Quickstart

``` cs
// A real-world spec: filters, includes, ordering, pagination, projection.
public sealed class RecentHighValueOrdersSpec : Specification<Order, OrderDto>
{
    public RecentHighValueOrdersSpec(Guid customerId, DateTime since, decimal minTotal)
    {
        Query
            .Where(o => o.CustomerId == customerId && o.CreatedAt >= since)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(0).Take(20)            // pagination
            .AsNoTracking()              // EF toggle
            .Select(o => new OrderDto    // projection
            {
                Id    = o.Id,
                Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
                Date  = o.CreatedAt
            });
    }
}

// EF usage: IQueryable<OrderDto>
var page = await db.Orders
    .WithSpecification(new RecentHighValueOrdersSpec(customerId, since, 100m))
    .ToListAsync(ct);

// In-memory (tests)
var list = new RecentHighValueOrdersSpec(customerId, since, 100m)
    .Evaluate(inMemoryOrders);
```

---

## Composition & Policies

``` cs 
// (ByCustomer AND (Since OR HighValue)) with explicit merge policies
var byCustomer = new OrdersByCustomerSpec(customerId);
var since      = new OrdersSinceSpec(sinceDate);
var highValue  = new OrdersMinTotalSpec(250m);

var composed = byCustomer.AsComposable()
    .OpenGroup(since)                         // (
        .Or(highValue)                        //   since OR highValue
    .CloseGroup()                             // )
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.ThrowOnConflict)
    .ReturnRoot()
    .Build();

var result = await db.Orders.WithSpecification(composed).ToListAsync(ct);
```

#### Policy Defaults

| Concern        | Options                                                          | Default            | Meaning                                              |
| -------------- | ---------------------------------------------------------------- | ------------------ | ---------------------------------------------------- |
| **Ordering**   | `Left`, `Right`, `BothLeftPriority`, `BothRightPriority`, `None` | `BothLeftPriority` | Combine orderings; left keys win ties                |
| **Pagination** | `Left`, `Right`, `None`, `ThrowOnConflict`                       | `None`             | Keep left/right; drop both; or **throw** on conflict |
| **Projection** | `Left`, `Right`                                                  | `Left`             | Which selector wins when both project                |

> Tip: prefer ThrowOnConflict for pagination to avoid silent surprises.

See the full Specification & Policy Cookbook.

---

## Grouping Example

``` cs 
public sealed class OrdersByMonthSpec
  : GroupingSpecification<Order, int /* Month */, MonthlyTotal>
{
    public OrdersByMonthSpec(Guid customerId)
    {
        Query
            .Where(o => o.CustomerId == customerId)
            .GroupBy(o => o.CreatedAt.Month)
            .Select(g => new MonthlyTotal
            {
                Month  = g.Key,
                Count  = g.Count(),
                Amount = g.Sum(o => o.Items.Sum(i => i.UnitPrice * i.Quantity))
            })
            .OrderByDescending(x => x.Month);
    }
}

var monthly = await db.Orders.WithSpecification(new OrdersByMonthSpec(customerId)).ToListAsync(ct);

// In-memory parity
var monthlyMem = new OrdersByMonthSpec(customerId).Evaluate(inMemoryOrders).ToList();
```

---

## EF Toggles

#### Supported today

- AsNoTracking() / AsTracking()
- Include(...) / ThenInclude(...)
- OrderBy(...), ThenBy(...), Skip(...), Take(...)

#### Planned

- AsSplitQuery() / AsSingleQuery()
- AsNoTrackingWithIdentityResolution()
- IgnoreQueryFilters()
- TagWith("...")

---

## In-Memory vs EF Parity

``` cs 
var spec = new RecentHighValueOrdersSpec(customerId, since, 100m);

var ef  = await db.Orders.WithSpecification(spec).ToListAsync(ct);
var mem = spec.Evaluate(inMemoryOrders).ToList();
// Assert parity in tests (shape + count + key order)
```

> If parity breaks, you’re likely using an EF-only construct — move it behind a projection or adjust the expression.

---

## Building

``` bash 
git clone https://github.com/your-org/Mango.Specifications.git
cd Mango.Specifications
dotnet restore
dotnet build
```

> Target frameworks: .NET 8 (primary). Wider TFMs may be added if there’s demand.

---

## Running Tests

``` bash 
## From Mango.Specifications root directory
dotnet test
```

---

## Documentation

- Cookbook: docs/COOKBOOK.md
- Samples: samples/Basic, samples/Composition, samples/Grouping (TODO)
- API Reference: XML docs in packages (TODO)

---

## Roadmap

- Publish NuGet packages
- Composition policy cookbook & full samples
- EF extras: Split/Single query, IdentityResolution, IgnoreQueryFilters, TagWith
- Spec immutability / Freeze() semantics
- Benchmarks vs vanilla LINQ (baseline)

---

## Contributing

PRs welcome. Keep PRs focused and covered by tests. For bigger API changes, open an issue first.
- Style: nullable enabled, warnings as errors.
- Commit messages: conventional commits preferred (e.g., feat:, fix:).

---
## License & Credits

- MIT — see [LICENSE](LICENSE).
- Credits: Heavily inspired by Ardalis.Specification (MIT). Portions may conceptually derive from that work.
