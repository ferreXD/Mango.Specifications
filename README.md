# Mango.Specifications

> **Specification++ for .NET/EF Core** — parentheses-aware composition (`And/Or/Not` with groups), **policy-driven merges** (ordering/pagination/projection), and **first-class GroupBy**. In-memory and EF Core parity.

[![CI](https://github.com/ferreXD/Mango.Specifications/actions/workflows/ci.yml/badge.svg)](https://github.com/ferreXD/Mango.Specifications/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Mango.Specifications.svg?label=Mango.Specifications)](https://www.nuget.org/packages/Mango.Specifications)
[![NuGet](https://img.shields.io/nuget/v/Mango.Specifications.EntityFrameworkCore.svg?label=Mango.Specifications.EntityFrameworkCore)](https://www.nuget.org/packages/Mango.Specifications.EntityFrameworkCore)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Last commit](https://img.shields.io/github/last-commit/ferreXD/Mango.Specifications.svg)](./)

> **Status:** WIP (pre-v1). API surface may change. Targeting `v1.0.0` after docs polish & EF ergonomics.

---

## Table of Contents

- [About](#about)
- [When to Choose Mango](#when-to-choose-mango)
- [What's New](#whats-new)
- [Features](#features)
- [Install](#install)
- [Quickstart](#quickstart)
- [Composition & Policies](#composition--policies)
- [Grouping Example](#grouping-example)
- [EF Toggles](#ef-toggles)
- [In-Memory vs EF Parity](#in-memory-vs-ef-parity)
- [Documentation](#documentation)
- [Known Limitations](#known-limitations)
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

Builds on the ideas of **Ardalis.Specification**, adding composition policies and first-class grouping.

---

## When to Choose Mango

| | [Ardalis.Specification](https://github.com/ardalis/Specification) | **Mango.Specifications** |
|---|---|---|
| **Maturity** | Stable, production-proven, widely adopted | Pre-v1, active development |
| **NuGet** | ✅ Published | 🔜 Coming soon |
| **Basic fluent builder** | ✅ | ✅ |
| **EF Core adapter** | ✅ | ✅ |
| **In-memory evaluator** | ✅ | ✅ |
| **Spec composition** | Manual (new class or extension) | ✅ `AsComposable()` fluent chain |
| **Precedence groups** | ❌ | ✅ `OpenGroup/CloseGroup` |
| **Merge policies** | ❌ | ✅ Ordering, pagination, projection |
| **First-class GroupBy** | ❌ | ✅ `GroupingSpecification<T,TKey,TResult>` |
| **Not() in chain** | ❌ | ✅ `.Not(spec)` |

**Choose Ardalis.Specification when:**
- You need a stable, NuGet-published library today.
- Your specs are self-contained and never need runtime composition.
- You want the broadest community and ecosystem support.

**Choose Mango when:**
- You compose specs at runtime (e.g. dynamic filters, search policies, A/B rule sets).
- You need `(A AND (B OR C))` with explicit parentheses, not just flat `And`/`Or`.
- You want a single `Build()` call to produce a fully-resolved spec with deterministic ordering and pagination.
- You use `GroupBy` with result projections and need identical in-memory and EF behaviour.

---

## What's New

> **Pre-release summary** (update per tag)

- Composition API: `AsComposable()`, `OpenGroup/CloseGroup`, `.Build()`.
- Merge policies: `OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy`.
- Grouping: `GroupingSpecification<T,TKey,TResult>` with result selectors and parity evaluators.
- EF adapter: `DbSetExtensions.WithSpecification(...)`, evaluator pipeline, includes, tracking.

_See [CHANGELOG](./CHANGELOG.md) for details._

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
dotnet add package Mango.Specifications
dotnet add package Mango.Specifications.EntityFrameworkCore
```

> Pre-release packages are on NuGet. Add `--prerelease` if the stable badge does not yet appear:
> ```bash
> dotnet add package Mango.Specifications --prerelease
> ```

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

See the [full Specification & Policy Cookbook](./docs/COOKBOOK.md).

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

> If parity breaks, you're likely using an EF-only construct — move it behind a projection or adjust the expression.

---

## Known Limitations

### GroupBy — full in-memory materialization before pagination

When `Skip`/`Take` are set on a `GroupingSpecification<T,TKey,TResult>`, the **in-memory evaluator** materialises **all** matching groups into a list first, then applies the offset and limit over that list.

```
entities → Where → GroupBy → Select → ToList() → Skip/Take
                                       ^^^^^^^^
                                       full materialization here
```

**Impact:** for large collections this can cause significant memory pressure and negate the purpose of pagination.

**EF Core is unaffected** — the database engine emits `OFFSET`/`FETCH` and no in-process buffering occurs.

**Workaround:** when testing grouping specs with pagination, keep the seed data small. If you need server-side cursor pagination over grouped results, issue a separate `COUNT` query and page the raw entity query before grouping.

---

## Building

``` bash 
git clone https://github.com/ferreXD/Mango.Specifications.git
cd Mango.Specifications
dotnet restore
dotnet build
```

> Target frameworks: .NET 8 (primary). Wider TFMs may be added if there's demand.

---

## Running Tests

``` bash 
# Unit tests only (no database required)
dotnet test --filter "Category!=Integration"

# All tests including SQL Server integration tests (requires AdventureWorks2022)
# Set MANGO_TEST_CONNECTION_STRING before running
dotnet test
```

---

## Documentation

- Cookbook: [docs/COOKBOOK.md](./docs/COOKBOOK.md)
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
- The core `ISpecification<T>` interface shape and fluent-builder pattern draw from [Ardalis.Specification](https://github.com/ardalis/Specification) (MIT). The composition engine (`And/Or/Not` with precedence groups), all merge policies (ordering, pagination, projection), and `GroupingSpecification<T,TKey,TResult>` are original contributions of this project.

