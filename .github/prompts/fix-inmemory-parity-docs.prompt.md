---
mode: agent
description: Document which ISpecification<T> features are unsupported by InMemorySpecificationEvaluator via XML remarks and README.
tools: [changes, codebase, editFiles, problems]
---

Add explicit documentation of in-memory evaluation gaps to two locations.

## Background

`InMemorySpecificationEvaluator` (file
`src/Specification/Ferreimavi.Specification/Evaluators/InMemorySpecificationEvaluator.cs`)
registers only three evaluators:

```csharp
private static readonly IInMemoryEvaluator[] DefaultEvaluators =
{
    WhereEvaluator.Instance,
    OrderEvaluator.Instance,
    PaginationEvaluator.Instance
};
```

The following `ISpecification<T>` features are silently ignored in-memory (no
evaluator exists for them):

- `IncludeExpressions` / `StringIncludes` — navigation property loading
- `AsTracking` / `AsNoTracking` — EF tracking flags (on `IEFSpecification<T>`)
- `AsNoTrackingWithIdentityResolution`
- `AsSplitQuery` / `AsSingleQuery`
- `IgnoreQueryFilters`
- `TagWith`

Currently neither the class XML doc nor the README calls this out explicitly.  The
README's in-memory parity section contains only a vague sentence ("move it behind a
projection").

## What to change

### 1. `InMemorySpecificationEvaluator.cs`

Add a class-level `<remarks>` block immediately after the existing `<summary>` (or add
a `<summary>` + `<remarks>` if neither exists).  The remarks must enumerate every
unsupported feature with a brief explanation, e.g.:

```xml
/// <remarks>
/// <para>
/// The default in-memory evaluator applies only <c>Where</c>, <c>OrderBy</c>, and
/// <c>Skip</c>/<c>Take</c>.  The following <see cref="ISpecification{T}"/> features
/// are <b>silently ignored</b> because they have no in-memory equivalent:
/// </para>
/// <list type="bullet">
///   <item><term>Include / StringInclude</term><description>Navigation-property eager
///     loading is performed by EF Core, not by LINQ-to-objects.</description></item>
///   <item><term>AsTracking / AsNoTracking</term><description>Change-tracking is an
///     EF Core concept; in-memory collections are not tracked.</description></item>
///   <item><term>AsNoTrackingWithIdentityResolution</term><description>Same as
///     above.</description></item>
///   <item><term>AsSplitQuery / AsSingleQuery</term><description>SQL query-split
///     strategy; has no meaning for in-memory evaluation.</description></item>
///   <item><term>IgnoreQueryFilters</term><description>EF Core global query filters
///     are not applied during in-memory evaluation.</description></item>
///   <item><term>TagWith</term><description>SQL comment tags are emitted by EF Core
///     and have no effect in-memory.</description></item>
/// </list>
/// <para>
/// To support additional features, pass a custom evaluator list to the
/// <see cref="InMemorySpecificationEvaluator(IEnumerable{IInMemoryEvaluator})"/>
/// constructor.
/// </para>
/// </remarks>
```

### 2. `README.md`

In the existing "In-Memory vs EF Parity" section (search for the phrase
"move it behind a projection") replace the vague sentence with an explicit
unsupported-feature table, e.g.:

```markdown
The in-memory evaluator applies `Where`, `OrderBy`, and `Skip`/`Take` only.
The following features are **silently ignored** when evaluating in memory:

| Feature | Reason |
|---|---|
| `Include` / `StringInclude` | Navigation loading is an EF Core concern |
| `AsTracking` / `AsNoTracking` | Change-tracking has no in-memory meaning |
| `AsNoTrackingWithIdentityResolution` | Same |
| `AsSplitQuery` / `AsSingleQuery` | SQL-split strategy; not applicable |
| `IgnoreQueryFilters` | EF global filters are not applied |
| `TagWith` | SQL comment tags are EF-only |

If your specification relies on any of these features, move the logic into a
`Where` expression or a projection before testing in-memory.
```

Do **not** change any logic.  Only add documentation.  After editing, run
`dotnet build` from the solution root and confirm zero errors.
