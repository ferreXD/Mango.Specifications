# Raw Audit Result — Developer Experience & API Design

## Executive Verdict

**Not ready for v1.0 go-live.** The core specification authoring path is clean and discoverable, and the README is above average for a pre-release library. However, a silent `NullReferenceException` trap in `AsComposable()`, a composition API that forces a non-obvious `ReturnRoot()` escape hatch to re-access policy setters, leaked implementation types, missing documentation artifacts (COOKBOOK, samples, CHANGELOG), and several enum naming violations collectively prevent confident adoption. Fix the runtime-safety issue and the fluent chain break before cutting a stable release.

---

## Scope Inspected

| Area | Files / Evidence | Notes |
| --- | --- | --- |
| README | README.md | Complete pass |
| Public interfaces | ISpecification.cs, IGroupingSpecification.cs, IComposableSpecificationBuilder.cs, IBaseComposableSpecificationBuilder.cs | Full coverage |
| Base types | Specification.cs, GroupingSpecification.cs | Full coverage |
| Fluent builder extensions | SpecificationBuilderExtensions.cs, ProjectableSpecificationBuilderExtensions.cs, GroupingSpecificationBuilderExtensions.cs | Full coverage |
| Composition API | ComposableSpecificationBuilder.cs, ComposedGroupOperationBuilder.cs, SpecificationCompositionExtensions.cs | Full coverage |
| Composition internals | CompositionParser.cs, AndSpecification.cs, NotSpecification.cs | Full coverage |
| EF Core adapter | DbSetExtensions.cs | Full coverage |
| Enums | ChainingType.cs, IncludeTypeEnum.cs, OrderTypeEnum.cs, OperationType.cs | Notable naming issues |
| Tests (usage patterns) | ComposableSpecificationReadRepositoryTests.cs, ComposableProjectableSpecificationTests.cs, NotSpecificationTests.cs | Full coverage |
| Docs / samples | README docs section, filesystem | COOKBOOK, samples, CHANGELOG all absent |
| NuGet metadata | README install section | Not published |

---

## Strengths

1. **Clean base type.** `Specification<T>` and `Specification<T, TResult>` are straightforward to subclass. The constructor injecting a default evaluator and validator is a good "just works" default.
2. **Quickstart in README is accurate and realistic.** The `RecentHighValueOrdersSpec` example maps 1:1 to actual public API.
3. **Policy table is clear.** The `OrderingEvaluationPolicy` / `PaginationEvaluationPolicy` / `ProjectionEvaluationPolicy` table with defaults in the README reduces surprise.
4. **In-memory + EF parity** is a credible selling point and the README explains where it can break.
5. **XML docs on public types.** The public `ISpecification<T>`, builders, and most extension methods carry XML summaries.
6. **`ThrowOnConflict` for pagination** is a notably good safety-net default.
7. **Real EF Core integration tests** using a full `AdventureWorks` schema give confidence that the evaluator pipeline is solid.
8. **`SpecificationCompositionException`** is thrown with a message when group counts mismatch—better than a cryptic `InvalidOperationException`.

---

## Findings

---

### Finding 1 — `AsComposable()` will silently `NullReferenceException` on custom `ISpecification<T>` implementations
- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Runtime-safety / API contract
- **Evidence:**
  SpecificationCompositionExtensions.cs
  ```cs
  public static IComposableSpecificationBuilder<T> AsComposable<T>(this ISpecification<T> specification)
      => new ComposableSpecificationBuilder<T>((specification as Specification<T>)!);
  ```
  The same pattern appears for the `<T, TResult>` overload.
- **Observation:** The extension accepts `ISpecification<T>` but the constructor requires `Specification<T>`. The `as` cast returns `null` when the caller passes any custom `ISpecification<T>` that does not extend the base class. The `!` null-forgiveness suppresses the compiler warning, so the null is passed into the constructor, causing a `NullReferenceException` at the first property access inside `ComposableSpecificationBuilder<T>`—not at the call site. The stack trace will not point to `AsComposable`.
- **Why it matters:** Any developer who wraps or decorates `ISpecification<T>` (e.g., for caching, logging, or test fakes) will hit a confusing runtime crash.
- **Suggested remediation:** Guard the cast explicitly:
  ```cs
  if (specification is not Specification<T> concreteSpec)
      throw new ArgumentException(
          $"AsComposable() requires a Specification<T> instance. " +
          $"Got: {specification.GetType().Name}.", nameof(specification));
  return new ComposableSpecificationBuilder<T>(concreteSpec);
  ```
- **Orchestrator notes:** This should block the v1 tag.

---

### Finding 2 — Fluent chain breaks after `And()`/`Or()`: policies are unreachable without `ReturnRoot()`
- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Fluent API discoverability / ergonomics
- **Evidence:**
  `IBaseComposableSpecificationBuilder<T>` returns `IBaseComposableSpecificationBuilder<T>` from `And()` / `Or()`.
  `WithOrderingEvaluationPolicy` and `WithPaginationEvaluationPolicy` live only on `IComposableSpecificationBuilder<T>`.
  
  The test workaround in ComposableProjectableSpecificationTests.cs:
  ```cs
  var builder = new ComposableSpecificationBuilder<Customer, string>(fullNameSpecification)
      .And(customerByNameSpecification) as IComposableSpecificationBuilder<Customer, string>;
  
  var spec = builder!
      .WithOrderingEvaluationPolicy(OrderingEvaluationPolicy.None)
  ```
  This requires an `as` cast and `!`, because `And()` returns `IBaseComposableSpecificationBuilder`, not the root.
  
  The typical README-suggested path (`AsComposable().And(...).WithOrderingEvaluationPolicy(...)`) does not compile.
- **Observation:** A first-time user will call `AsComposable().And(b).WithOrderingEvaluationPolicy(...)` and get a compilation error. The required escape hatch is `.ReturnRoot()`, but it is not documented in the README and its name ("return root") is not obvious to someone who has not read the source.
- **Why it matters:** Policy setters are a key differentiator of this library; making them hard to reach undermines the value proposition.
- **Suggested remediation:** Option A — Make `And()` / `Or()` return `IComposableSpecificationBuilder<T>` directly when called at the root level. Option B — Promote the `ReturnRoot()` pattern prominently in the README quickstart and rename it to something like `.ConfigurePolicies()` or simply make the policy methods available on `IBaseComposableSpecificationBuilder<T>`.
- **Orchestrator notes:** This is the most likely source of confusion for new adopters.

---

### Finding 3 — `ComposableSpecificationBuilder<T>` and `ComposedGroupOperationBuilder<T>` are public concrete types
- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** API surface / stability
- **Evidence:**
  - ComposableSpecificationBuilder.cs: `public class ComposableSpecificationBuilder<T>`
  - ComposedGroupOperationBuilder.cs: `public class ComposedGroupOperationBuilder<T>`
  - Tests instantiate `new ComposableSpecificationBuilder<Customer, string>(...)` directly.
- **Observation:** Exposing concrete builder types invites callers to reference implementation details. `RecurseToRoot` in `ComposedGroupOperationBuilder` uses a hard cast to `ComposedGroupOperationBuilder<T>`, which fails at runtime if any other `IBaseComposableSpecificationBuilder<T>` implementation is substituted—a fragility invisible at the interface level.
- **Why it matters:** Once v1 ships, these types become a compatibility promise. Sealing or internalizing them is cheaper before release than after.
- **Suggested remediation:** Make both builders `internal` and expose them only through the `AsComposable()` extension and the `IComposableSpecificationBuilder<T>` interface. Remove `new ComposableSpecificationBuilder<T>` from tests; replace with `spec.AsComposable()`.
- **Orchestrator notes:** Paired with Finding 1—fixing both requires a single coordinated change.

---

### Finding 4 — `Not()` lives in a different API layer than `And()`/`Or()`
- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** API consistency / discoverability
- **Evidence:**
  - `And()` / `Or()` are on `IBaseComposableSpecificationBuilder<T>` (builder-level, composing specs).
  - `Not()` is on `ISpecification<T>` itself (SpecificationCompositionExtensions.cs).
  - There is also a `Query.Not()` via `SpecificationBuilderExtensions`, which negates the *current* spec's own WHERE clause, not the composed result.
  - A `Not()` for grouping specs (SpecificationCompositionExtensions.cs) is a separate overload on `IGroupingSpecification`.
- **Observation:** Three different `Not` surfaces exist:
  1. `spec.Not()` — returns a new negated spec (spec-level extension).
  2. `builder.Query.Not()` — negates the WHERE clauses in the current spec being built (builder extension).
  3. `IGroupingSpecification.Not()` — grouping variant.
  
  None of these are callable at the composition chain level (e.g., `.And(specA).Not(specB)` is not possible). The README Feature list claims "Parentheses-aware `And/Or/Not` composition" but the composition builder interface does not expose `Not`.
- **Why it matters:** The README promise and the actual API do not match. A developer expecting `.Not()` in the same chain as `.And()` will be confused. The two builder-level `Not` usages also conflate "negate this spec's filters" with "negate a composed spec".
- **Suggested remediation:** Add `Not(ISpecification<T>)` to `IBaseComposableSpecificationBuilder<T>`. Clarify in docs which `Not` is which and when to use each. The README feature claim should either match the interface or be amended.
- **Orchestrator notes:** The README claim is a pre-Go-Live documentation issue even if the full fix to the builder is deferred.

---

### Finding 5 — Enum naming: `IncludeTypeEnum` and `OrderTypeEnum` use `Enum` suffix
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Naming conventions
- **Evidence:**
  - IncludeTypeEnum.cs: `public enum IncludeTypeEnum`
  - OrderTypeEnum.cs: `public enum OrderTypeEnum`
  - Compare: `ChainingType`, `OperationType` (no suffix) — inconsistent within the same `Common` folder.
- **Observation:** .NET naming guidelines forbid `Enum` suffix on enum types (same principle as not appending `Class` to class names). The inconsistency inside a single folder suggests the suffix was not intentional policy.
- **Why it matters:** These are public types. Once v1 ships, renaming them is a breaking change.
- **Suggested remediation:** Rename to `IncludeType` and `OrderType` before v1. These types appear in `IncludeExpressionInfo` (public) and `OrderByExpressionInfo` (public), so both need to be updated.
- **Orchestrator notes:** Cheap fix before release, expensive after.

---

### Finding 6 — `OpenGroup()` with `ISpecification<T>` overload does not return a `ComposedGroupOperationBuilder`
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Fluent API consistency / hidden behavior
- **Evidence:**
  ComposableSpecificationBuilder.cs:
  ```cs
  public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T> initialSpec, ChainingType type = ChainingType.And)
  {
      var projectionSpec = BuildProjectableSpecification(initialSpec);
      _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, projectionSpec, type));
      return this;  // returns root, NOT ComposedGroupOperationBuilder
  }
  
  public IBaseComposableSpecificationBuilder<T, TResult> OpenGroup(ISpecification<T, TResult> initialSpec, ChainingType type = ChainingType.And)
  {
      _operations.Add(new CompositionOperation<T, TResult>(OperationType.GroupOpen, initialSpec, type));
      return new ComposedGroupOperationBuilder<T, TResult>(this, _operations);  // returns group builder
  }
  ```
- **Observation:** Calling `OpenGroup` with an `ISpecification<T>` (non-projected) returns `this` (the root builder), while calling it with an `ISpecification<T, TResult>` returns a `ComposedGroupOperationBuilder`. The return type is the same (`IBaseComposableSpecificationBuilder<T, TResult>`), so callers cannot distinguish the two cases at compile time. The group context is added to `_operations`, but the caller is back at the root. Subsequent `.And()` calls after the first overload proceed without semantic group nesting.
- **Why it matters:** A developer authoring `(A AND (B OR C))` using non-projected specs for B and C will get the wrong expression tree silently.
- **Suggested remediation:** Both overloads should return a `ComposedGroupOperationBuilder`. Align the behavior to the `ISpecification<T, TResult>` overload.
- **Orchestrator notes:** Needs a test that verifies group precedence when using the `ISpecification<T>` overload of `OpenGroup`.

---

### Finding 7 — No composition example in the README that shows `ReturnRoot().Build()`
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Documentation / discoverability
- **Evidence:**
  The README "Composition & Policies" section (README.md) describes the policy table but provides **no code example**. The "What's New" section lists `AsComposable()`, `ReturnRoot`, `.Build()` as bullet points only.
- **Observation:** A developer who wants to combine two specs with a non-default ordering policy has no working example to copy. The only end-to-end example is inside test code, which requires navigating to ComposableSpecificationReadRepositoryTests.cs to find the full `AsComposable().And(...).ReturnRoot().WithProjectionEvaluationPolicy(...).Build()` chain.
- **Why it matters:** The composition API is the primary differentiator. Without a code example in the README, most users will reach for `And()`/`Or()` in the spec constructor instead.
- **Suggested remediation:** Add a minimal 10-line composition example to the README that shows: `AsComposable().And(specB).ReturnRoot().WithPaginationEvaluationPolicy(ThrowOnConflict).Build()`.
- **Orchestrator notes:** Low cost, high DX value.

---

### Finding 8 — `docs/COOKBOOK.md`, `samples/`, and `CHANGELOG.md` are referenced but absent
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Documentation completeness
- **Evidence:**
  README.md:
  ```
  - Cookbook: docs/COOKBOOK.md
  - Samples: samples/Basic, samples/Composition, samples/Grouping (TODO)
  - API Reference: XML docs in packages (TODO)
  ```
  README.md: `_See [CHANGELOG](./CHANGELOG.md) for details (TODO)._`
  
  None of these files exist in the repository.
- **Observation:** The README points to four documentation artifacts that do not exist. A first-time visitor who follows any of these links encounters a 404. The `(TODO)` markers are honest but do not help a developer evaluating adoption.
- **Why it matters:** Missing docs are a trust signal. A library with no changelog and no samples is harder to evaluate than Ardalis.Specification, which this library aims to extend.
- **Suggested remediation:** Remove the broken links and `(TODO)` placeholders before go-live, or replace them with a single "Full docs coming" notice. A minimal `CHANGELOG.md` with the initial release entry should exist at v1.0 tag.
- **Orchestrator notes:** No code change required, editorial only.

---

### Finding 9 — `ReturnRoot()` is a confusing name for its role
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Naming / discoverability
- **Evidence:**
  IBaseComposableSpecificationBuilder.cs:
  ```cs
  IComposableSpecificationBuilder<T> ReturnRoot();
  ```
  On the root builder (ComposableSpecificationBuilder.cs):
  ```cs
  public IComposableSpecificationBuilder<T> ReturnRoot() => this;
  ```
- **Observation:** `ReturnRoot()` serves two purposes: (1) escaping from a `ComposedGroupOperationBuilder` back to the root, and (2) on the root builder, it is a no-op that merely re-types `this` to expose policy setters and `Build()`. Its name is implementation-centric ("root" is an internal concept). When called at the root level, it does nothing and is only needed to satisfy the type system.
- **Why it matters:** A developer who never opens a group will still need `ReturnRoot()` just to call `WithPaginationEvaluationPolicy()`, with no intuitive reason why (see Finding 2).
- **Suggested remediation:** Resolve through Finding 2's remediation: move policy setters to `IBaseComposableSpecificationBuilder<T>`. If `ReturnRoot()` remains, rename it to something like `CloseAllGroups()` or `Done()` that signals "I am finished adding specs."
- **Orchestrator notes:** Naming is a breaking change post-v1. Fix now.

---

### Finding 10 — `NotSpecificationHelper.ComposeNotCriteria` will throw on a spec with no `WhereExpressions`
- **Severity:** Medium
- **Confidence:** Medium
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Error behavior / runtime safety
- **Evidence:**
  NotSpecificationHelper.cs:
  ```cs
  internal static Expression<Func<T, bool>> ComposeNotCriteria<T>(ISpecification<T> spec)
  {
      return spec.WhereExpressions
          .Select(x => ExpressionCombiner.Not(x.Filter))
          .Aggregate(ExpressionCombiner.AndAlso);
  }
  ```
  `Enumerable.Aggregate` with no seed throws `InvalidOperationException: Sequence contains no elements` when the source is empty.
- **Observation:** Calling `.Not()` on a specification that has no `Where` clause (e.g., a spec that only sets ordering or pagination) will throw a non-descriptive `InvalidOperationException` instead of a domain-relevant error or a "match all" / "match none" semantic.
- **Why it matters:** A developer might reasonably negate a spec that only specifies ordering, expecting a no-op filter. The crash is confusing and undocumented.
- **Suggested remediation:** Guard the empty case:
  ```cs
  var expressions = spec.WhereExpressions.ToList();
  if (expressions.Count == 0) 
      throw new InvalidOperationException("Cannot negate a specification with no Where expressions.");
  return expressions.Select(x => ExpressionCombiner.Not(x.Filter)).Aggregate(ExpressionCombiner.AndAlso);
  ```
  Alternatively, decide whether negating an unconditional spec returns a "match nothing" expression (`x => false`).
- **Orchestrator notes:** Needs a unit test for the empty-expression case.

---

### Finding 11 — NuGet package is not published; installation instructions are broken
- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Blocker (for public release)
- **Category:** Distribution / first-use path
- **Evidence:**
  README.md:
  ```
  # Coming soon on NuGet
  dotnet add package Mango.Specifications
  dotnet add package Mango.Specifications.EntityFrameworkCore
  ```
- **Observation:** The install section shows `dotnet add package` commands that will fail. There is no NuGet badge. The fallback "project reference or local feed" is mentioned but not explained.
- **Why it matters:** The first-use path is the most important DX signal. A broken install instruction causes 100 % of new evaluators to abandon before writing a single line of code.
- **Suggested remediation:** Either publish a pre-release NuGet package (e.g., `1.0.0-alpha.1`) or replace the install section with honest project-reference instructions including a Git clone + project reference setup guide.
- **Orchestrator notes:** The library's quality does not justify staying unpublished. Publish an alpha.

---

### Finding 12 — `ISpecification<T>` exposes mutable-looking properties `AsTracking` / `AsNoTracking` / `Skip` / `Take` as raw getters with no mutation contract
- **Severity:** Low
- **Confidence:** Medium
- **Go-Live Relevance:** Post-Go-Live
- **Category:** API surface / mutability contract
- **Evidence:**
  ISpecification.cs: `bool AsNoTracking { get; }`, `int? Skip { get; }`, `int? Take { get; }`.
  Specification.cs: these properties have `internal set` setters.
- **Observation:** The properties are read-only on the interface but mutable internally. The library does not prevent post-construction mutation through the `Specification<T>` concrete type. There is no immutability / `Freeze()` mechanism (this is listed on the roadmap). A developer who holds a reference to the spec and mutates it after passing it to a repository will get undefined behavior.
- **Why it matters:** Pre-v1 this is acceptable. After v1, immutability semantics must be documented if not enforced.
- **Suggested remediation:** Add a note to the docs about spec mutability being intentionally allowed before `Freeze()` lands. At v1, consider making the mutation path require explicit builder calls only.
- **Orchestrator notes:** Roadmap item. Note the risk explicitly.

---

### Finding 13 — `GroupingSpecification`'s `Select()` on the builder is semantically different from `Specification<T, TResult>`'s `Select()`
- **Severity:** Low
- **Confidence:** High
- **Go-Live Relevance:** Post-Go-Live
- **Category:** Naming / mental model
- **Evidence:**
  - `GroupingSpecificationBuilderExtensions.Select()` sets `GroupResultSelector` — the per-element projection inside a group.
  - `ProjectableSpecificationBuilderExtensions.Select()` sets `Selector` — the per-entity projection.
  - Both appear as `.Select(...)` on their respective `Query` builders.
- **Observation:** The method name is the same but the semantics differ: one applies before grouping (per entity), the other applies inside a group. The Grouping README example shows the usage pattern clearly, but a developer migrating a `Specification<T, TResult>` to a `GroupingSpecification` will be surprised that `.Select()` now means something different.
- **Why it matters:** Low risk if the COOKBOOK documents the distinction clearly, but currently the COOKBOOK does not exist.
- **Suggested remediation:** Consider renaming the grouping variant to `.SelectResult()` or `.ProjectGroup()` to make the distinction clear. At minimum, document both in the COOKBOOK.
- **Orchestrator notes:** Minor, but worth resolving before the API is locked for v1.

---

## Missing Evidence / Open Questions

1. **Does `docs/COOKBOOK.md` exist anywhere** (draft branch, internal notes)? If so, link it. If not, the policy section in the README needs to include composition code examples.
2. **NuGet package metadata** (`.csproj` `<PackageId>`, `<Description>`, `<Authors>`, `<RepositoryUrl>`) — not inspected. If these are missing, `dotnet pack` output will be poor quality.
3. **`IComposedGroupOperationBuilder<T>` interface** — referenced in compositions but not shown in full; unclear if it is public or internal. If public, it is an additional type a user might encounter unexpectedly.
4. **What happens when `CloseGroup()` is called more times than `OpenGroup()`?** The validator only checks balanced counts at `Build()` time, but calling `CloseGroup()` without a matching open will silently add a spurious `GroupClose` operation that only errors at `Build()`.
5. **`SpecificationCompositionException`** — not inspected for message quality. Is it serializable? Does it have a `HResult`?
6. **The README.md clone URL** uses `your-org/Mango.Specifications.git` — a placeholder that should be replaced with `ferreXD/Mango.Specifications`.

---

## Recommended Next Checks

1. **Fix Finding 1** (unsafe cast in `AsComposable`) and add a test with a custom `ISpecification<T>` implementation.
2. **Fix Finding 2** (policy setters after `And()`/`Or()`) — decide whether to promote policies to the base interface or restructure the chain.
3. **Inspect `.csproj` NuGet metadata** on both packages for completeness (`<PackageId>`, `<Description>`, `<PackageTags>`, `<RepositoryUrl>`).
4. **Audit `IComposedGroupOperationBuilder<T>` access modifier** — should be `internal` unless intentionally public.
5. **Write a single failing test** for `Not()` on a zero-`Where` spec to confirm Finding 10.
6. **Check whether the README Building section clone URL** still says `your-org` and update it.