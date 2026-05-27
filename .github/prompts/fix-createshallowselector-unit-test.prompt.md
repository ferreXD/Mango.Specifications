---
mode: agent
description: Add a non-integration unit test for the CreateShallowSelector<T,TResult> code path in SpecificationEvaluator.
tools: [changes, codebase, editFiles, problems, runCommands]
---

Add a unit test that exercises the `CreateShallowSelector` private method inside
`SpecificationEvaluator` (file
`src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore/Evaluators/SpecificationEvaluator.cs`)
without requiring a live SQL Server connection.

## Background

`CreateShallowSelector<T, TResult>` is reached when
`IsIdentitySelector(specification.GroupResultSelector)` returns `true` — i.e. when
the caller passes a spec whose `GroupResultSelector` body is just the input parameter
(`x => x`).  This path generates `MemberBinding` objects by iterating
`typeof(TResult).GetProperties()` and matching each by name and type against
`typeof(T)`.  The existing integration tests
(`GroupingSpecificationReadRepositoryTests`) already exercise this path via
`GroupByBusinessEntityIdSpecification` (T = TResult = `Person`), but those tests are
`[Trait("Category","Integration")]` and require AdventureWorks.  There is no fast,
offline unit test for this path.

## What to add

In `src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore.Tests/`
add a new test class `CreateShallowSelectorTests` (place it next to the other test
files, not inside a sub-folder).  Use the EF Core In-Memory provider
(`Microsoft.EntityFrameworkCore.InMemory`) so the test does not need `[Trait("Category",
"Integration")]`.

The test must:

1. Seed a small `DbContext` backed by `UseInMemoryDatabase` with at least three entities
   of type `Person` (use the existing `Person` entity from
   `src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore.Tests/Data/`).
2. Create a `GroupByBusinessEntityIdSpecification` (or an equivalent inline
   `GroupingSpecification<Person, int, Person>` where `GroupResultSelector = x => x`)
   so `IsIdentitySelector` returns `true` and `CreateShallowSelector` is invoked.
3. Call `SpecificationEvaluator.Default.GetQuery(dbSet.AsQueryable(), spec, CancellationToken.None)`
   and `await` the result.
4. Assert that each returned group contains a `Person` whose `BusinessEntityId` matches
   the group key and whose properties match the seeded values — confirming that
   `CreateShallowSelector` correctly bound at least `BusinessEntityId` and one other
   writable scalar property.

Do **not** mark this test `[Trait("Category","Integration")]`.

After adding the test, run `dotnet test --filter "FullyQualifiedName~CreateShallowSelectorTests"` from the solution root and confirm it passes.
