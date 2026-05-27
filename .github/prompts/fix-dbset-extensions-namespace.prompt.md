---
mode: agent
description: Rename DbSetExtensions namespace from Mango.Specifications.EntityFrameworkCore.Extensions to Mango.Specifications.EntityFrameworkCore.
tools: [changes, codebase, editFiles, problems, runCommands]
---

Change the namespace of `DbSetExtensions` from
`Mango.Specifications.EntityFrameworkCore.Extensions` to
`Mango.Specifications.EntityFrameworkCore` so that consumers do not need a separate
`using` directive beyond the one already required for the EF package.

## Background

`DbSetExtensions` lives in
`src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore/Extensions/DbSetExtensions.cs`
under namespace `Mango.Specifications.EntityFrameworkCore.Extensions`.  All other
types in the EF package use the root namespace `Mango.Specifications.EntityFrameworkCore`.
A consumer who adds `using Mango.Specifications.EntityFrameworkCore;` still cannot call
`.WithSpecification(...)` without a second `using` directive — inconsistent with the
rest of the package.

This must be fixed before the first NuGet publication; it is a binary-compatible
rename at source level (the assembly-qualified namespace changes, but nothing has been
published yet).

## What to change

### 1. `DbSetExtensions.cs`

Change the namespace declaration from:
```csharp
namespace Mango.Specifications.EntityFrameworkCore.Extensions
```
to:
```csharp
namespace Mango.Specifications.EntityFrameworkCore
```

### 2. Any `using` directives referencing the old namespace

Search the entire repository for `using Mango.Specifications.EntityFrameworkCore.Extensions;`.
Update every occurrence to `using Mango.Specifications.EntityFrameworkCore;` (or remove
the directive if the new namespace is already imported in the same file).

Files likely to be affected:
- Test files under
  `src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore.Tests/`
- Any example or sample files that reference `DbSetExtensions`

### 3. `docs/COOKBOOK.md`

Search for `Mango.Specifications.EntityFrameworkCore.Extensions` in `docs/COOKBOOK.md`
and update to `Mango.Specifications.EntityFrameworkCore` if present.

### Verification

After editing, run:
```
dotnet build src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore/Mango.Specifications.EntityFrameworkCore.csproj
dotnet build src/Specification.EntityFrameworkCore/Ferreimavi.Specification.EntityFrameworkCore.Tests/Mango.Specifications.EntityFrameworkCore.Tests.csproj
```

Confirm zero errors.  Then run
`grep -r "EntityFrameworkCore.Extensions" src/` and confirm no remaining references to
the old namespace exist.
