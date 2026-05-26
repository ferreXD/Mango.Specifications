# Changelog

All notable changes to **Mango.Specifications** are documented in this file.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).  
Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased] — v0.x (pre-release)

This section tracks everything shipped before the first stable release (`v1.0.0`).

### Added

#### Core Specification

- `Specification<T>` — base class with `Where`, `Include`/`ThenInclude`, `OrderBy`/`ThenBy`/`ThenByDescending`, `Skip`, `Take`, `AsNoTracking`, `AsTracking`.
- `Specification<T, TResult>` — extends base with `Select` (single) and `SelectMany` projection.
- `GroupingSpecification<T, TKey, TResult>` — first-class `GroupBy` + result-selector (`Select`) with full in-memory and EF Core parity.
- `GroupingSpecification<T, TKey>` — convenience variant where the result type equals the entity type.

#### Composition API

- `ISpecification<T>.AsComposable()` — entry point; wraps a concrete `Specification<T>` in a fluent builder.
- `ISpecification<T, TResult>.AsComposable()` — projectable overload.
- `IComposableSpecificationBuilder<T>` / `IComposableSpecificationBuilder<T, TResult>` — public interfaces for the composition chain.
- `And(ISpecification<T>)` / `And(ISpecification<T, TResult>)` — conjunction.
- `Or(ISpecification<T>)` / `Or(ISpecification<T, TResult>)` — disjunction.
- `Not(ISpecification<T>)` / `Not(ISpecification<T, TResult>)` — negation; delegates to `NotSpecification<T>`.
- `OpenGroup(ISpecification<T>, ChainingType)` / `OpenGroup(ISpecification<T, TResult>, ChainingType)` — opens a precedence group (parenthesised sub-expression); `ChainingType` defaults to `And`.
- `CloseGroup()` — closes the most-recently-opened precedence group.
- `Build()` — reduces the operation list to a single composed specification via `CompositionParser`.

#### Composition Operators (standalone extensions)

- `SpecificationCompositionExtensions.Not<T>(this ISpecification<T>)` — negation without builder.
- `SpecificationCompositionExtensions.Not<T, TResult>(this ISpecification<T, TResult>)` — projectable variant.
- `SpecificationCompositionExtensions.Not<T, TKey, TResult>(this IGroupingSpecification<T, TKey, TResult>)` — grouping variant.

#### Merge Policies

- `OrderingEvaluationPolicy` — controls how two specs' `OrderBy` chains are merged when composed:
  - `Left` — keep only the left spec's ordering.
  - `Right` — keep only the right spec's ordering.
  - `BothLeftPriority` *(default)* — concatenate both; left keys come first.
  - `BothRightPriority` — concatenate both; right keys come first.
  - `None` — discard all ordering.
- `PaginationEvaluationPolicy` — controls `Skip`/`Take` resolution on conflict:
  - `Left` — keep left spec's pagination.
  - `Right` — keep right spec's pagination.
  - `None` *(default)* — discard both when both are set.
  - `ThrowOnConflict` — throw `InvalidOperationException` when both specs supply pagination.
- `ProjectionEvaluationPolicy` — controls which `Select`/`SelectMany` wins when both projectable specs define one:
  - `Left` *(default)* — use the left spec's selector.
  - `Right` — use the right spec's selector.

#### EF Core Adapter (`Mango.Specifications.EntityFrameworkCore`)

- `DbSetExtensions.WithSpecification<T>(this DbSet<T>, ISpecification<T>)` — applies spec to a `DbSet`, returns `IQueryable<T>`.
- `DbSetExtensions.WithSpecification<T, TResult>(this DbSet<T>, ISpecification<T, TResult>)` — projectable overload, returns `IQueryable<TResult>`.
- Tracking toggles: `AsNoTracking()`, `AsTracking()` wired into the EF evaluator pipeline.
- Includes pipeline: `Include`/`ThenInclude` expressions applied automatically.
- Pagination: `Skip`/`Take` applied as `IQueryable` operators.

#### In-Memory Evaluator

- `InMemorySpecificationEvaluator` — default evaluator; applies `Where`, `OrderBy`, `Skip`/`Take`, `Select` / `SelectMany` in-memory for test parity.
- Grouping evaluator: applies `GroupBy` + `Select` in-memory to produce `IEnumerable<IGrouping<TKey, TResult>>`.

---

## Notes

- **Not yet on NuGet** — consume via project reference or local NuGet feed.
- **Pre-v1 API** — interface shapes may change before `v1.0.0`. See README Roadmap.
- **Planned for v1**: NuGet publish, EF extras (`AsSplitQuery`, `IgnoreQueryFilters`, `TagWith`), spec immutability / `Freeze()`, benchmarks.
