---
mode: agent
description: Add a test asserting that a spec with no Skip/Take issues exactly one database round-trip (no COUNT query).
tools: [changes, codebase, editFiles, problems, runCommands]
---

Add a unit test that proves a non-paginated `ISpecification<T>` (one with `Skip = null`
and `Take = null`) does **not** trigger a `COUNT(*)` query when evaluated via
`PaginationQueryEvaluator`.

## Background

`PaginationQueryEvaluator.GetQuery` (file
`src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore/Evaluators/PaginationQueryEvaluator.cs`)
now contains the guard:

```csharp
if (specification.Skip is null && specification.Take is null) return query;
```

Before this guard the evaluator always called `query.Count()`, firing a second SQL
round-trip for every non-paginated query.  No test currently asserts the guard is
active; if the guard is accidentally removed it will go undetected.

## What to add

Add a new test class `PaginationQueryEvaluatorTests` inside
`src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore.Tests/`
(no `[Trait("Category","Integration")]`).  The test must not require a live SQL Server
connection.

Approach — use `IQueryable` expression-tree inspection:

1. Create a simple `IQueryable<T>` stub using `Enumerable.Empty<T>().AsQueryable()` (or
   any queryable whose expression tree can be inspected).
2. Create a `Specification<T>` with no `Skip`/`Take` set.
3. Call `PaginationQueryEvaluator.Instance.GetQuery(query, spec)`.
4. Assert that the returned object is **reference-equal** to the input `query` — the
   guard must return the original instance unchanged, proving no Count or Skip/Take was
   appended to the expression tree.

Add a second test with `Take = 10` set on the specification and assert the returned
queryable is **not** reference-equal to the input (proving the pagination path is still
taken when needed).

After adding the tests, run
`dotnet test --filter "FullyQualifiedName~PaginationQueryEvaluatorTests"` from the
solution root and confirm both pass.
