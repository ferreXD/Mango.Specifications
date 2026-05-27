---
mode: agent
description: Apply [Experimental] to provisional composition API surfaces and add a v1 stability boundary to the README Roadmap.
tools: [changes, codebase, editFiles, problems, runCommands]
---

Apply `[System.Diagnostics.CodeAnalysis.Experimental]` to the provisional composition
API surfaces and document the v1 stability boundary in the README.

## Background

The composition builder chain (`AsComposable`, `IBaseComposableSpecificationBuilder`,
`IComposableSpecificationBuilder`, `IComposedGroupOperationBuilder`, merge policy
enums) is the library's primary differentiator but has not reached a stable contract.
No public type carries an `[Experimental]` attribute.  Consumers who enable
`<WarningsAsErrors>` will not be warned that these surfaces may change before v1.

All projects target `net8.0`, so `System.Diagnostics.CodeAnalysis.ExperimentalAttribute`
is available without additional packages.

## What to change

### 1. Apply `[Experimental]` to provisional composition surfaces

Add `[Experimental("MANGO001")]` to the following **public** types in
`src/Specification/Ferreimavi.Specification/`:

| Type | File |
|---|---|
| `IBaseComposableSpecificationBuilder<T>` and `<T,TResult>` | `Builder/IBaseComposableSpecificationBuilder.cs` |
| `IComposableSpecificationBuilder<T>` and `<T,TResult>` | `Builder/IComposableSpecificationBuilder.cs` |
| `IComposedGroupOperationBuilder<T>` and `<T,TResult>` | `Builder/IComposedGroupOperationBuilder.cs` |
| `SpecificationCompositionExtensions` | `Extensions/Composition/SpecificationCompositionExtensions.cs` |
| `OrderingEvaluationPolicy` | `Common/` (find exact file with codebase search) |
| `PaginationEvaluationPolicy` | `Common/` (find exact file with codebase search) |
| `ProjectionEvaluationPolicy` | `Common/` (find exact file with codebase search) |

Use the diagnostic ID `"MANGO001"` consistently across all attributed types.

Add `using System.Diagnostics.CodeAnalysis;` to each file that requires it.

Do **not** apply `[Experimental]` to:
- `ISpecification<T>` / `ISpecification<T,TResult>`
- `Specification<T>` / `Specification<T,TResult>`
- `GroupingSpecification<T,TKey,TResult>`
- `IGroupingSpecification<T,TKey,TResult>`
- Any evaluator type
- Any EF Core package type

### 2. Suppress the diagnostic in test projects

In both test `.csproj` files, add the following so tests do not fail to build:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);MANGO001</NoWarn>
</PropertyGroup>
```

### 3. Add v1 stability boundary to README

In `README.md`, find or create a `## Roadmap` section (search for "Roadmap" heading).
Add a clearly labelled sub-section:

```markdown
### API Stability

| Surface | Status |
|---|---|
| `ISpecification<T>` / `Specification<T>` | Stable from v1 |
| `GroupingSpecification<T,TKey,TResult>` | Stable from v1 |
| EF Core evaluators and `ReadRepositoryBase` | Stable from v1 |
| Composition builder chain (`AsComposable`, `And`/`Or`/`Not`, `OpenGroup`, merge policies) | **Experimental** — may change before v1; decorated with `[Experimental("MANGO001")]` |

Experimental surfaces emit a compile-time `MANGO001` warning (suppressible with
`#pragma warning disable MANGO001` or `<NoWarn>MANGO001</NoWarn>`).
```

### Verification

After editing, run `dotnet build` from the solution root and confirm:
- Zero errors.
- Both test projects build without `MANGO001` warnings (suppressed via `<NoWarn>`).
- A consumer project that references `Mango.Specifications` and calls
  `.AsComposable()` without suppressing `MANGO001` emits exactly one `MANGO001` warning.
