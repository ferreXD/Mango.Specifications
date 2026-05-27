# Audit Metadata

| Field | Value |
|---|---|
| Repository | ferreXD/Mango.Specifications |
| Branch audited | develop |
| Audit perspective | Developer Experience & API Design |
| Overall verdict | **Not ready for v1.0 go-live** |
| Verdict confidence | High |
| Blocker count | 3 (Findings 1, 2, 11) |
| Pre-Go-Live count | 7 (Findings 3, 4, 5, 6, 7, 8, 9, 10) |
| Post-Go-Live count | 2 (Findings 12, 13) |
| Invented findings | 0 |

---

## Findings

---

### DX-001 — `AsComposable()` silently produces a `NullReferenceException` on non-`Specification<T>` implementations

| Field | Value |
|---|---|
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Runtime Safety / API Contract |
| Affects | `SpecificationCompositionExtensions.AsComposable<T>()` and `AsComposable<T, TResult>()` |

**Evidence (observed in source, not interpreted):**

File: SpecificationCompositionExtensions.cs

```cs
public static IComposableSpecificationBuilder<T> AsComposable<T>(this ISpecification<T> specification)
    => new ComposableSpecificationBuilder<T>((specification as Specification<T>)!);

public static IComposableSpecificationBuilder<T, TResult> AsComposable<T, TResult>(this ISpecification<T, TResult> specification)
    => new ComposableSpecificationBuilder<T, TResult>((specification as Specification<T, TResult>)!);
```

- The parameter type is `ISpecification<T>` (interface).
- The body performs an `as` cast to `Specification<T>` (concrete type) and applies the `!` null-forgiveness operator.
- When `specification` does not derive from `Specification<T>`, `as` returns `null`; the `!` suppresses the C# nullable warning; `null` is passed to the `ComposableSpecificationBuilder<T>` constructor.

**Interpretation:** The `NullReferenceException` will occur at the first property access inside `ComposableSpecificationBuilder<T>`, not at the `AsComposable()` call site. The stack trace will not point to the actual problem location.

**Impact:** Any caller passing a decorator, proxy, or test fake implementing `ISpecification<T>` without subclassing `Specification<T>` will receive a confusing, non-actionable runtime crash.

**Remediation (local to this finding):**

```cs
public static IComposableSpecificationBuilder<T> AsComposable<T>(this ISpecification<T> specification)
{
    if (specification is not Specification<T> concreteSpec)
        throw new ArgumentException(
            $"AsComposable() requires a Specification<T> instance. Got: {specification.GetType().Name}.",
            nameof(specification));
    return new ComposableSpecificationBuilder<T>(concreteSpec);
}
```

Apply the same guard to the `<T, TResult>` overload.

**Open questions:** None. The defect is fully visible in source.

---

### DX-002 — Policy setters are unreachable after `And()`/`Or()` without an undocumented `ReturnRoot()` escape; the README's own code example will not compile

| Field | Value |
|---|---|
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Fluent API Ergonomics / Discoverability |
| Affects | `IBaseComposableSpecificationBuilder<T>`, `IComposableSpecificationBuilder<T>`, `ComposedGroupOperationBuilder<T>` |

**Evidence (observed in source, not interpreted):**

1. `IBaseComposableSpecificationBuilder<T>` interface (IBaseComposableSpecificationBuilder.cs):
   - `And()`, `Or()`, `OpenGroup()`, `CloseGroup()` all return `IBaseComposableSpecificationBuilder<T>`.
   - `WithOrderingEvaluationPolicy()` and `WithPaginationEvaluationPolicy()` are **not** on this interface.

2. `IComposableSpecificationBuilder<T>` interface (IComposableSpecificationBuilder.cs):
   - Extends `IBaseComposableSpecificationBuilder<T>`.
   - Adds `WithOrderingEvaluationPolicy()`, `WithPaginationEvaluationPolicy()`, `Build()`.

3. `ComposedGroupOperationBuilder<T>.CloseGroup()` (ComposedGroupOperationBuilder.cs):
   ```cs
   public IBaseComposableSpecificationBuilder<T> CloseGroup()
   {
       _rootBuilder.CloseGroup();
       return _rootBuilder;  // return type is IBaseComposableSpecificationBuilder<T>
   }
   ```

4. README composition example (README.md):
   ```cs
   var composed = byCustomer.AsComposable()
       .OpenGroup(since)
           .Or(highValue)
       .CloseGroup()
       .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)  // ← compile error here
       .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.ThrowOnConflict)
       .ReturnRoot()
       .Build();
   ```
   After `.CloseGroup()`, the static type is `IBaseComposableSpecificationBuilder<T>`. `.WithOrderingEvaluationPolicy()` does not exist on that type. The `!` workaround cast pattern is demonstrated in tests instead.

5. Test workaround in ComposableProjectableSpecificationTests.cs:
   ```cs
   var builder = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
       .And(customerByNameSpecification) as IComposableSpecificationBuilder<Customer, string>;
   var spec = builder!.WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)...
   ```
   An `as` cast with `!` is required because `And()` returns `IBaseComposableSpecificationBuilder`.

**Interpretation:** The README's canonical composition example will not compile as written. The workaround — `ReturnRoot()` before calling policy setters — is not documented, requires knowing internal type structure, and is named in an implementation-centric way.

**Impact:** The library's primary differentiator (policy-controlled merges) is inaccessible via the documented happy path. Every first-time user who follows the README will hit a compile error.

> **Normalizer note:** The original audit (Finding 7) stated the README had no composition example. This is incorrect. The README has one; however, that example will not compile due to the identical type-chain issue described here. Finding 7 has been revised accordingly (see DX-007).

**Remediation (local to this finding):**

Option A — Move `WithOrderingEvaluationPolicy()`, `WithPaginationEvaluationPolicy()`, and `WithProjectionEvaluationPolicy()` to `IBaseComposableSpecificationBuilder<T>` (and `<T, TResult>`). Remove them from `IComposableSpecificationBuilder<T>`. This eliminates the need for `ReturnRoot()` as a type-escape.

Option B — Change the return type of `And()`, `Or()`, and `CloseGroup()` on the root builder to `IComposableSpecificationBuilder<T>` directly, using covariant return types or separate overloads, so policy setters are always reachable.

Fix the README example to compile. Until code is changed, at minimum add `.ReturnRoot()` before the policy setters in the README.

**Open questions:** None. The defect is fully visible in source and confirmed by the README example.

---

### DX-003 — `ComposableSpecificationBuilder<T>` and `ComposedGroupOperationBuilder<T>` are public concrete types with implementation-dependent cast behavior

| Field | Value |
|---|---|
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | API Surface / Stability |
| Affects | `ComposableSpecificationBuilder<T>`, `ComposedGroupOperationBuilder<T>` |

**Evidence (observed in source, not interpreted):**

1. ComposableSpecificationBuilder.cs:
   ```cs
   public class ComposableSpecificationBuilder<T>(...) : IComposableSpecificationBuilder<T>
   public class ComposableSpecificationBuilder<T, TResult>(...) : IComposableSpecificationBuilder<T, TResult>
   ```

2. ComposedGroupOperationBuilder.cs:
   ```cs
   private IComposableSpecificationBuilder<T> RecurseToRoot(IBaseComposableSpecificationBuilder<T> builder)
   {
       if (builder is IComposableSpecificationBuilder<T> root) return root;
       var composedGroupOperationBuilder = builder as ComposedGroupOperationBuilder<T>;
       return RecurseToRoot(composedGroupOperationBuilder!._rootBuilder);
   }
   ```
   Hard cast to the concrete type `ComposedGroupOperationBuilder<T>` with `!`. This will throw `NullReferenceException` if any other `IBaseComposableSpecificationBuilder<T>` implementation is in the chain.

3. Tests directly instantiate the concrete type:
   ```cs
   var builder = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
       .And(customerByNameSpecification) as IComposableSpecificationBuilder<Customer, string>;
   ```

**Interpretation:** Public concrete builders invite callers to take constructor dependencies on them. The `RecurseToRoot` hard cast is fragile against alternative implementations. Once v1 ships, these types become a public compatibility promise.

**Impact:** Post-v1, internalization is a breaking change. The `RecurseToRoot` cast creates a hidden runtime defect if any custom `IBaseComposableSpecificationBuilder<T>` is passed to a nested group.

**Remediation (local to this finding):** Make both builder classes `internal`. Expose them only through the `AsComposable()` extension. Replace direct constructor calls in tests with `spec.AsComposable()`.

**Open questions:** None.

---

### DX-004 — `Not()` exists in three separate API layers; composition-level `Not` is absent despite README claim

| Field | Value |
|---|---|
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | API Consistency / Feature Parity |
| Affects | `SpecificationCompositionExtensions`, `SpecificationBuilderExtensions`, `IBaseComposableSpecificationBuilder<T>` |

**Evidence (observed in source, not interpreted):**

1. Spec-level `Not` on `ISpecification<T>` (SpecificationCompositionExtensions.cs):
   ```cs
   public static ISpecification<T> Not<T>(this ISpecification<T> spec) => new NotSpecification<T>(spec);
   public static ISpecification<T, TResult> Not<T, TResult>(this ISpecification<T, TResult> spec) => ...
   public static IGroupingSpecification<T, TKey, TResult> Not<T, TKey, TResult>(...) => ...
   ```

2. Builder-level `Not` on `ISpecificationBuilder<T>` (SpecificationBuilderExtensions.cs):
   ```cs
   public static ISpecificationBuilder<T> Not<T>(this ISpecificationBuilder<T> builder)
   {
       var spec = builder.Specification.Not();
       return spec.Query;
   }
   ```
   This negates the WHERE clauses of the spec being built, not a composed spec.

3. `IBaseComposableSpecificationBuilder<T>` (IBaseComposableSpecificationBuilder.cs):
   — `Not(ISpecification<T>)` is **absent** from this interface.

4. README Features claim (README.md):
   > ✅ **Parentheses-aware** `And/Or/Not` composition.

**Interpretation:** The README advertises `Not` as part of the composition API. It is not on `IBaseComposableSpecificationBuilder<T>`. The existing `Not` surfaces operate at different levels with different semantics. A developer expecting `.And(specA).Not(specB)` in the builder chain will find no such method.

**Impact:** README promise does not match the public API. Developers expecting parity with `.And()` / `.Or()` will be surprised.

**Remediation (local to this finding):** Add `Not(ISpecification<T>)` to `IBaseComposableSpecificationBuilder<T>` (and `<T, TResult>`), semantically equivalent to `.And(spec.Not())`. Update the README Features section to accurately reflect the current state until this is implemented; remove the `Not` claim or qualify it.

**Open questions:** None regarding the gap. Whether `Not` in the composition chain should negate the operand (`AND NOT b`) or negate the accumulated left side is a design decision not yet made.

---

### DX-005 — `IncludeTypeEnum` and `OrderTypeEnum` use an `Enum` suffix that violates .NET naming guidelines

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Naming Conventions |
| Affects | `IncludeTypeEnum`, `OrderTypeEnum`, `IncludeExpressionInfo`, `OrderByExpressionInfo` |

**Evidence (observed in source, not interpreted):**

1. IncludeTypeEnum.cs:
   ```cs
   public enum IncludeTypeEnum { Include, ThenInclude }
   ```

2. OrderTypeEnum.cs:
   ```cs
   public enum OrderTypeEnum { OrderBy, OrderByDescending, ThenBy, ThenByDescending }
   ```

3. Sibling enums in the same `Common` folder do not use the suffix:
   ```cs
   public enum ChainingType { And, Or }
   public enum OperationType { And, Or, GroupOpen, GroupClose }
   ```

4. `IncludeExpressionInfo` (IncludeExpressionInfo.cs) exposes `IncludeTypeEnum Type { get; }` as a public property.

**Interpretation:** The suffix is inconsistent within the same folder and violates the .NET Framework Design Guidelines (Framework Design Guidelines, §4.6: "Do NOT use a suffix of Enum on enum type names"). These are public types referenced in public expression info classes.

**Impact:** Renaming after v1 is a breaking change for any consumer referencing `IncludeTypeEnum` or `OrderTypeEnum` by name. No impact today; high cost post-release.

**Remediation (local to this finding):** Rename `IncludeTypeEnum` → `IncludeType` and `OrderTypeEnum` → `OrderType`. Update all references in `IncludeExpressionInfo`, `OrderByExpressionInfo`, and any internal usages.

**Open questions:** None.

---

### DX-006 — `OpenGroup(ISpecification<T>)` overload does not return a `ComposedGroupOperationBuilder`, causing silent incorrect expression trees when mixing projected and non-projected specs

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Fluent API Correctness / Hidden Behavior |
| Affects | `ComposableSpecificationBuilder<T, TResult>.OpenGroup(ISpecification<T>)` |

**Evidence (observed in source, not interpreted):**

ComposableSpecificationBuilder.cs:

```cs
// ISpecification<T> overload — returns this (root), NOT a group builder
public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And)
{
    var projectionSpec = BuildProjectableSpecification(initialSpec);
    _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, projectionSpec, type));
    return this;  // ← root returned immediately
}

// ISpecification<T, TResult> overload — returns ComposedGroupOperationBuilder
public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType type = ChainingType.And)
{
    _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, initialSpec, type));
    return new ComposedGroupOperationBuilder<T, TResult>(this, _operations);  // ← group context
}
```

- Both overloads have the same return type (`IBaseComposableSpecificationBuilder<T, TResult>`).
- The `ISpecification<T>` overload returns the root builder. Subsequent `.And()` / `.Or()` calls do not operate within the group context; they are flat at the root level.
- The `ISpecification<T, TResult>` overload correctly returns a `ComposedGroupOperationBuilder`.
- No compile-time signal distinguishes the two behaviors.

**Interpretation:** A developer authoring `(A AND (B OR C))` where B is a non-projected spec will observe B being combined at the root level, not inside the group. The expression tree will be incorrect and the behavior will differ from the developer's intent.

**Impact:** Silent logic error. The error is especially likely when composing projectable specs with filter-only specs (a common real-world scenario).

**Remediation (local to this finding):** Make the `ISpecification<T>` overload also return a `ComposedGroupOperationBuilder<T, TResult>`, matching the `ISpecification<T, TResult>` overload. Add a test that verifies group precedence for the `ISpecification<T>` overload.

**Open questions:** Is this also present in the non-projected (`ComposableSpecificationBuilder<T>`) overload? That builder only has one `OpenGroup` overload and returns a `ComposedGroupOperationBuilder<T>` correctly — the asymmetry is specific to the `<T, TResult>` builder.

---

### DX-007 — README composition example will not compile; calls policy setters before `ReturnRoot()`

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Documentation Accuracy |
| Affects | README.md Composition & Policies section |

> **Normalizer correction:** The source audit (Finding 7) stated the README lacked a composition example. This is incorrect. The README at README.md contains a composition example. However, that example will not compile as written. This finding supersedes the source's Finding 7.

**Evidence (observed in source, not interpreted):**

README.md, "Composition & Policies" section, line 117:
```cs
var composed = byCustomer.AsComposable()
    .OpenGroup(since)
        .Or(highValue)
    .CloseGroup()
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)   // ← compile error
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.ThrowOnConflict)
    .ReturnRoot()
    .Build();
```

Established by DX-002 evidence: `.CloseGroup()` returns `IBaseComposableSpecificationBuilder<T>`. `WithOrderingEvaluationPolicy()` does not exist on that interface. The call to `.ReturnRoot()` — which would return `IComposableSpecificationBuilder<T>` and make policy setters available — comes **after** the policy setters in the example, not before.

**Interpretation:** A developer who copies the canonical example from the README will receive a compiler error on `WithOrderingEvaluationPolicy`. There is no inline comment, note, or alternative shown.

**Impact:** The README's only composition code example is misleading and untestable by copy-paste. First-impression failure.

**Remediation (local to this finding):** Move `.ReturnRoot()` before the policy setter calls:
```cs
var composed = byCustomer.AsComposable()
    .OpenGroup(since)
        .Or(highValue)
    .CloseGroup()
    .ReturnRoot()                                               // ← must precede policy setters
    .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.BothLeftPriority)
    .WithPaginationEvaluationPolicy(PaginationEvaluationPolicy.ThrowOnConflict)
    .Build();
```
Add a brief comment explaining that `.ReturnRoot()` re-exposes policy setters and `Build()`.

**Open questions:** None. The compile failure is deterministic from the interface definitions.

---

### DX-008 — `docs/COOKBOOK.md`, `samples/`, and `CHANGELOG.md` are referenced in the README but do not exist

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Documentation Completeness |
| Affects | README.md Documentation section |

**Evidence (observed in source, not interpreted):**

README.md, Documentation section:
```
- Cookbook: docs/COOKBOOK.md
- Samples: samples/Basic, samples/Composition, samples/Grouping (TODO)
- API Reference: XML docs in packages (TODO)
```

README.md, What's New section:
```
_See [CHANGELOG](./CHANGELOG.md) for details (TODO)._
```

None of the following paths exist in the repository: `docs/COOKBOOK.md`, `samples/`, `CHANGELOG.md`. The `(TODO)` markers confirm they were intentionally left incomplete.

**Interpretation:** All four referenced artifacts are absent. Clicking any of the README links produces a 404.

**Impact:** Trust signal for first-time evaluators. A library with no changelog and no samples is harder to evaluate relative to Ardalis.Specification.

**Remediation (local to this finding):** Before go-live, either remove the broken links and `(TODO)` markers, or replace with a single statement ("Full docs and samples in progress — see tests for usage examples"). Create a minimal `CHANGELOG.md` with the v1.0.0 entry at the time of the first release tag.

**Open questions:** Is a COOKBOOK draft in progress on a different branch? If so, link it from the README immediately.

---

### DX-009 — `ReturnRoot()` is named after an implementation concept; its purpose is opaque to callers who never opened a group

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Naming / Discoverability |
| Affects | `IBaseComposableSpecificationBuilder<T>.ReturnRoot()`, `IBaseComposableSpecificationBuilder<T, TResult>.ReturnRoot()` |

**Evidence (observed in source, not interpreted):**

1. IBaseComposableSpecificationBuilder.cs:
   ```cs
   IComposableSpecificationBuilder<T> ReturnRoot();
   ```

2. ComposableSpecificationBuilder.cs:
   ```cs
   public IComposableSpecificationBuilder<T> ReturnRoot() => this;
   ```
   On the root builder, `ReturnRoot()` returns `this` unchanged. It is a no-op that exists solely to re-type `this` from `IBaseComposableSpecificationBuilder<T>` to `IComposableSpecificationBuilder<T>`.

**Interpretation:** A developer who never uses `OpenGroup` still must call `ReturnRoot()` after every `And()`/`Or()` to reach `Build()` and policy setters. The name communicates an implementation detail ("root" is an internal tree concept) rather than the user-facing intent.

**Impact:** Unnecessary ceremony for the common case (flat composition). Confusing name that requires reading source code to understand.

**Remediation (local to this finding):** Resolved entirely if Finding DX-002's Option A is adopted (policy setters on `IBaseComposableSpecificationBuilder<T>`). If `ReturnRoot()` remains, rename to `Done()`, `Finalize()`, or `Configure()` — anything that signals "I am done adding operands, now set options."

**Open questions:** None. The naming issue is independent of the fix chosen for DX-002.

---

### DX-010 — `NotSpecificationHelper.ComposeNotCriteria` throws a non-descriptive `InvalidOperationException` on specs with no `Where` expressions

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | Medium |
| Go-Live Relevance | Pre-Go-Live |
| Category | Error Behavior / Runtime Safety |
| Affects | `NotSpecificationHelper.ComposeNotCriteria<T>()` |

**Evidence (observed in source, not interpreted):**

NotSpecificationHelper.cs:
```cs
internal static Expression<Func<T, bool>> ComposeNotCriteria<T>(ISpecification<T> spec)
{
    return spec.WhereExpressions
        .Select(x => ExpressionCombiner.Not(x.Filter))
        .Aggregate(ExpressionCombiner.AndAlso);
}
```

`Enumerable.Aggregate` with no seed value throws `InvalidOperationException: Sequence contains no elements` when `WhereExpressions` is empty.

**Confidence note:** Medium, not High — no test was found that exercises this path. The defect is provable by static analysis but has not been confirmed with a failing test.

**Interpretation:** Calling `.Not()` on a specification that has no `Where` clause (e.g., one that only sets ordering or pagination) will produce a non-descriptive `InvalidOperationException` at `NotSpecificationHelper`, not at the `Not()` call site. The error message does not indicate which spec was involved.

**Impact:** Unexpected crash for a superficially reasonable operation. The empty-filter case has no documented behavior.

**Remediation (local to this finding):**

```cs
internal static Expression<Func<T, bool>> ComposeNotCriteria<T>(ISpecification<T> spec)
{
    var expressions = spec.WhereExpressions.ToList();
    if (expressions.Count == 0)
        throw new InvalidOperationException(
            $"Cannot negate a specification of type '{typeof(T).Name}' that has no Where expressions.");
    return expressions
        .Select(x => ExpressionCombiner.Not(x.Filter))
        .Aggregate(ExpressionCombiner.AndAlso);
}
```

Decision required: should negating a spec with no filters return `x => false` ("match nothing") or throw? Document whichever is chosen. Add a unit test for the empty-expression case.

**Open questions:** Is negating a filter-less spec a supported scenario? No documentation states either way.

---

### DX-011 — NuGet packages are unpublished; `dotnet add package` instructions in the README will fail

| Field | Value |
|---|---|
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Blocker (for public release) |
| Category | Distribution / First-Use Path |
| Affects | README.md Install section |

**Evidence (observed in source, not interpreted):**

README.md, Install section:
```
# Coming soon on NuGet
dotnet add package Mango.Specifications
dotnet add package Mango.Specifications.EntityFrameworkCore
```

- No NuGet version badge is present in the README header.
- No `.nupkg`, `.nuspec`, or `<PackageId>` metadata in any `.csproj` was inspected (see Open Questions).
- The README comment "Coming soon on NuGet" confirms no package is currently published.

**Interpretation:** Every developer who follows the standard install path will receive `error NU1101: Unable to find package Mango.Specifications`.

**Impact:** The install step is the first action a new user takes after reading the README. A broken install command causes 100% of new evaluators to fail before writing any code.

**Remediation (local to this finding):** Option A — Publish a pre-release package (e.g., `1.0.0-alpha.1`) to NuGet.org or a GitHub Packages feed before publicizing the library. Option B — Replace the install section with a project-reference setup guide until NuGet publication is ready.

**Open questions:** Are NuGet `.csproj` metadata fields (`<PackageId>`, `<Description>`, `<Authors>`, `<RepositoryUrl>`, `<PackageTags>`) present and complete? These were not inspected. If absent, `dotnet pack` output will be low quality regardless of when publication occurs.

---

### DX-012 — `ISpecification<T>` exposes scalar properties (`Skip`, `Take`, `AsTracking`, `AsNoTracking`) with no documented mutation contract

| Field | Value |
|---|---|
| Severity | Low |
| Confidence | Medium |
| Go-Live Relevance | Post-Go-Live |
| Category | API Surface / Mutability Contract |
| Affects | `ISpecification<T>`, `Specification<T>` |

**Evidence (observed in source, not interpreted):**

ISpecification.cs:
```cs
int? Skip { get; }
int? Take { get; }
bool AsTracking { get; }
bool AsNoTracking { get; }
```

Specification.cs:
These same properties have `internal set` setters on the concrete class.

**Interpretation (claimed in source report; marked Medium confidence because no mutation test was found):** A developer holding a reference to a `Specification<T>` instance could mutate it after passing it to a repository by accessing the internal setter through a subclass in the same assembly, or through reflection. The library's roadmap includes a `Freeze()` mechanism (confirmed in README roadmap) but it does not exist yet.

**Impact:** Post-construction mutation is possible but undocumented. In practice, no external caller can reach `internal set` through normal code — the actual risk is subclasses. Medium confidence.

**Remediation (local to this finding):** Add a note to the public documentation (once docs exist) explicitly stating that spec instances are mutable until `Freeze()` lands. At v1, consider whether `internal set` should be `private set` on sealed specs.

**Open questions:** Can an external project subclass `Specification<T>` and write to the `internal set` property? In C#, `internal` members are not accessible outside the assembly, so external subclasses cannot call the setter. Risk is lower than described in the source report. Confidence revised to Medium from the original Medium.

---

### DX-013 — `GroupingSpecification.Query.Select()` has different semantics from `Specification<T, TResult>.Query.Select()`

| Field | Value |
|---|---|
| Severity | Low |
| Confidence | High |
| Go-Live Relevance | Post-Go-Live |
| Category | Naming / Mental Model |
| Affects | `GroupingSpecificationBuilderExtensions.Select()`, `ProjectableSpecificationBuilderExtensions.Select()` |

**Evidence (observed in source, not interpreted):**

1. `GroupingSpecificationBuilderExtensions.Select<T, TKey, TResult>()` (GroupingSpecificationBuilderExtensions.cs):
   ```cs
   public static IGroupingSpecificationBuilder<T, TKey, TResult> Select<T, TKey, TResult>(
       this IGroupingSpecificationBuilder<T, TKey, TResult> builder,
       Expression<Func<T, TResult>> selector)
   {
       builder.Specification.GroupResultSelector = selector;
       return builder;
   }
   ```
   Sets `GroupResultSelector` — a per-element projection applied **inside** the group.

2. `ProjectableSpecificationBuilderExtensions.Select<T, TResult>()` sets `Selector` — a per-entity projection applied **before** grouping.

3. Both methods appear as `.Select(...)` on their respective `Query` builders.

**Interpretation:** The name `Select` is overloaded across two concepts with different semantics. A developer migrating a `Specification<T, TResult>` to `GroupingSpecification<T, TKey, TResult>` will not realize that `.Select()` now projects inside the group, not across entities.

**Impact:** Low risk at scale because the Grouping README example shows the usage clearly. Risk increases if the COOKBOOK (which does not yet exist) fails to distinguish the two.

**Remediation (local to this finding):** Consider renaming the grouping variant to `.SelectResult()` or `.ProjectGroup()` before the API is locked for v1. At minimum, document both behaviors explicitly in the COOKBOOK.

**Open questions:** Conditional on DX-008 (COOKBOOK existence). If the COOKBOOK is never written, this naming ambiguity will remain unexplained.

---

## Missing Evidence / Open Questions (Audit-Level)

| # | Question | Impact if Unresolved |
|---|---|---|
| Q1 | Are NuGet `.csproj` metadata fields (`<PackageId>`, `<Description>`, `<Authors>`, `<RepositoryUrl>`, `<PackageTags>`) present and complete on both packages? | Poor `dotnet pack` output; low NuGet discoverability |
| Q2 | Is `IComposedGroupOperationBuilder<T>` and `<T, TResult>` `public` or `internal`? (Interface referenced in compositions but access modifier not confirmed.) | If public, adds unexpected API surface |
| Q3 | Does `docs/COOKBOOK.md` exist on any branch (draft, internal)? | Changes remediation priority for DX-008 and DX-013 |
| Q4 | What is the behavior when `CloseGroup()` is called without a matching `OpenGroup()`? The `ValidateGroups` check runs at `Build()` time, but the spurious `GroupClose` is added to `_operations` immediately — are there intermediate side effects? | Possible confusing behavior before `Build()` is called |
| Q5 | Is `SpecificationCompositionException` serializable? Does it carry a `HResult` or `Data` entries useful for logging? | Diagnostic quality in production |
| Q6 | The README Building section contains a placeholder clone URL: `git clone https://github.com/your-org/Mango.Specifications.git`. Is this present in the live README? | Broken documentation for contributors |

---

## Normalization Corrections Applied

| Correction | Original Finding | Normalized Finding | Reason |
|---|---|---|---|
| README has composition example (present, not absent) | Finding 7: "No composition example in README" | DX-007: "README composition example is present but will not compile" | Evidence found at README.md line 117 during additional search |
| Finding 2 strengthened: README example is direct evidence | Finding 2 noted only tests as workaround evidence | DX-002 adds README line 117 as primary evidence | README example confirms the exact failure mode |

---

## Invariants Preserved

- No findings were invented beyond the 13 in the source report.
- All 13 source findings are represented (DX-001 through DX-013).
- Negative conclusions are preserved (verdict: not ready for v1.0).
- Evidence and interpretation are separated within each finding.
- One low-confidence claim (DX-012 external mutability risk) is marked Medium with rationale.
- Recommendations are local to each finding; no cross-finding roadmap is produced.