# Raw Audit Result — Architecture & Maintainability

## Executive Verdict

The architecture is **directionally sound but not go-live ready**. The package split between `Mango.Specifications` (core) and `Mango.Specifications.EntityFrameworkCore` (EF adapter) is structurally correct, and the builder/evaluator/composition pipeline shows genuine design intent. However, **three critical issues block a credible v1 release**: an ORM-specific interface (`IQueryEvaluator`, `ISpecificationEvaluator`) living in the core package; an unsafe downcast in the composition entry point; and an unredacted production-style connection string embedded in the test project. Several high-severity API hygiene issues compound the risk.

---

## Scope Inspected

| Area | Files / Evidence | Notes |
| --- | --- | --- |
| Package boundary | `src/Specification/…`, `src/Specification.EntityFrameworkCore/…` | Two logical packages confirmed |
| Core interfaces | ISpecification.cs, ISpecificationEvaluator.cs, IQueryEvaluator.cs | Provider leakage confirmed |
| Evaluator pipeline (core) | InMemorySpecificationEvaluator.cs, OrderEvaluator.cs, PaginationEvaluator.cs, WhereEvaluator.cs | Visibility inconsistency confirmed |
| Evaluator pipeline (EF) | SpecificationEvaluator.cs, IncludeQueryEvaluator.cs, PaginationQueryEvaluator.cs | EF-specific pagination materializes the query |
| Builder hierarchy | ISpecificationBuilder.cs, IComposableSpecificationBuilder.cs, IBaseComposableSpecificationBuilder.cs, ComposableSpecificationBuilder.cs | Unsafe cast in extension; builder is public concrete class |
| Composition entry point | SpecificationCompositionExtensions.cs | Unsafe `as` cast with `!` |
| Specification state | Specification.cs | `internal set` on mutable properties; `new` hides |
| Extension methods | SpecificationBuilderExtensions.cs, ProjectableSpecificationBuilderExtensions.cs, GroupingSpecificationBuilderExtensions.cs | Tripled tracking extension duplication |
| Repository abstraction | ReadRepositoryBase.cs, IReadRepositoryBase.cs | Exists in EF package; reasonable placement |
| EF extension surface | DbSetExtensions.cs | `WithSpecification` is clean; `ToListAsync`/`ToEnumerableAsync` are redundant with each other |
| Test infrastructure | TestDbContext.cs | Hardcoded connection string in committed source |
| Enums / policies | PaginationEvaluationPolicy.cs, OrderingEvaluationPolicy.cs, ProjectionEvaluationPolicy.cs, IncludeTypeEnum.cs | All public; all in core — correct |
| Stale comment | IInMemorySpecificationEvaluator.cs | "ORM evaluators are not our concern yet" — contradicted by existing files |

---

## Strengths

- **Package split is real.** The core package compiles without EF Core; the EF package takes a project reference to core and adds the ORM glue. The conceptual boundary exists.
- **Evaluator pipelines are open by construction.** Both `InMemorySpecificationEvaluator` and `SpecificationEvaluator` accept custom `IEnumerable<IXEvaluator>` in their constructors, enabling third-party extension without subclassing.
- **Composition is parenthesis-aware.** The `OpenGroup`/`CloseGroup`/`ReturnRoot` API and shunting-yard-style `CompositionParser` handle non-trivial composition that the industry baseline (Ardalis) does not.
- **Conflict-resolution policies** (`OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy`) show explicit thinking about composition edge cases; most alternatives silently pick one side.
- **Grouping support** with separate `IGroupingSpecification` and matching `GroupingSpecification<T,TKey,TResult>` is a meaningful differentiator; it is rare in this category.
- **Internal evaluator singletons are mostly correct.** `OrderEvaluator` and `WhereEvaluator` are `internal sealed`; the singleton pattern avoids allocation per spec evaluation.
- **`WithSpecification` extension on `IQueryable<T>`** is the cleanest EF integration point; it accepts a custom evaluator, which allows testing.

---

## Findings

### Finding 1 — `ISpecificationEvaluator` and `IQueryEvaluator` belong to the core package but are ORM-specific contracts

- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Provider leakage into core
- **Evidence:**
  - ISpecificationEvaluator.cs in namespace `Mango.Specifications` — methods return `IQueryable<T>`, use `Task`/`CancellationToken`, and reference `IGroupingSpecification`.
  - IQueryEvaluator.cs in namespace `Mango.Specifications` — method signature is `IQueryable<T> GetQuery<T>(IQueryable<T> …)`.
- **Observation:** `IQueryable<T>` is a LINQ provider abstraction; it is provider-agnostic at the type level, but every meaningful concrete implementation requires an ORM (or at minimum a queryable LINQ provider). The interfaces sit in the same assembly as the in-memory evaluator, yet the in-memory evaluator implements `IInMemorySpecificationEvaluator`, not `ISpecificationEvaluator`. The two hierarchies are therefore incoherent: core publishes an interface that its own evaluator does not implement.
- **Why it matters:** Any consumer who imports `Mango.Specifications` to write provider-agnostic code is exposed to an interface they cannot implement without taking an ORM dependency. The stale comment in IInMemorySpecificationEvaluator.cs ("ORM evaluators are not our concern yet") confirms the intent was different, but the current state violates it. This will be a source of confusion and incorrect implementation attempts from day one.
- **Suggested remediation:** Move `ISpecificationEvaluator` and `IQueryEvaluator` to `Mango.Specifications.EntityFrameworkCore`. If a provider-agnostic `IQueryEvaluator`-like interface is genuinely needed in core, remove `IQueryable<T>` from its signature and redesign.
- **Orchestrator notes:** This cannot be silently renamed; it requires a namespace migration that will break any consumer who has already implemented these interfaces.

---

### Finding 2 — Unsafe downcast in `AsComposable` extension is a silent runtime failure trap

- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Public abstractions with unclear contracts
- **Evidence:** SpecificationCompositionExtensions.cs lines 38 and 48:
  ```csharp
  new ComposableSpecificationBuilder<T>((specification as Specification<T>)!)
  new ComposableSpecificationBuilder<T, TResult>((specification as Specification<T, TResult>)!)
  ```
- **Observation:** Both overloads accept `ISpecification<T>` / `ISpecification<T, TResult>`, then silently attempt to downcast to the concrete `Specification<T>` class via `as` + null-forgiving `!`. If the caller passes any custom implementation of `ISpecification<T>` that does not inherit from `Specification<T>`, the `as` returns `null` and the `!` operator suppresses the null check, causing a `NullReferenceException` inside `ComposableSpecificationBuilder`'s constructor — with no indication to the caller that their type is incompatible.
- **Why it matters:** This is the public entry point for the composition API, the library's headline feature. It makes the interface lie: it claims to accept `ISpecification<T>` but only works if the underlying type is `Specification<T>`. Any user who tries to compose a specification that wraps or decorates the core class will hit an unexplained crash.
- **Suggested remediation:** Change the signature to accept `Specification<T>` directly, or introduce a marker interface/method that expresses the constraint explicitly, or expose the builder constructor accepting the concrete class.
- **Orchestrator notes:** This is technically a correctness bug, not just a design smell. It must be resolved before any NuGet publication.

---

### Finding 3 — Production connection string committed in test source

- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Security / operational safety
- **Evidence:** TestDbContext.cs:
  ```csharp
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
      optionsBuilder.UseSqlServer("Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;Integrated Security=True;…");
  ```
- **Observation:** A fully-qualified SQL Server connection string pointing to a named machine (`DESKTOP-SB85G0U`) and database (`AdventureWorks2022`) is hardcoded in a committed test file. Even though this is an integration test class and the credentials use Windows integrated auth (no plaintext password), the presence of a hardcoded machine-targeted connection string in source control is a maintenance problem and a CI failure in any non-Windows/non-local environment.
- **Why it matters:** This breaks CI pipelines by default, leaks network topology, and sets a poor precedent for test configuration hygiene. Any fork or contributor will encounter confusing failures.
- **Suggested remediation:** Replace with an environment-variable or `appsettings.json` / `user-secrets` backed configuration in test setup. Use an in-memory database or `TestContainers` for portable integration tests.
- **Orchestrator notes:** This must be removed before any public repository open-sourcing if the branch is made public.

---

### Finding 4 — `PaginationEvaluator` is `public` while sibling evaluators are `internal sealed`

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Types that should be internal but are public
- **Evidence:**
  - PaginationEvaluator.cs: `public class PaginationEvaluator`
  - OrderEvaluator.cs: `internal sealed class OrderEvaluator`
  - WhereEvaluator.cs: `internal class WhereEvaluator`
- **Observation:** The three core in-memory evaluators have inconsistent visibility. `PaginationEvaluator` is fully public with a private constructor that exposes only a static `Instance` singleton. Making the class public while making the constructor private is a contradiction: the type is visible but cannot be subclassed or substituted. It provides no extension point. The `internal` siblings are consistent with the fact that evaluator implementations are registered via the pipeline, not directly consumed.
- **Why it matters:** `PaginationEvaluator` is referenced by type in `InMemorySpecificationEvaluator.Evaluate` (`evaluator is not PaginationEvaluator`) to exclude it from grouping evaluation. If this type check is the mechanism, making it `public` exposes an implementation detail as API. Users who reference `PaginationEvaluator.Instance` in their own evaluator lists create a tight coupling to the concrete type.
- **Suggested remediation:** Make `PaginationEvaluator` `internal sealed` and expose pagination exclusion behavior through a named interface or marker (e.g., `IPaginationEvaluator` / a `bool IsGroupingCompatible` property on `IInMemoryEvaluator`).
- **Orchestrator notes:** This is also a design smell in the grouping pipeline — using an `is not PaginationEvaluator` type check to skip pagination in grouping is brittle and will break if the user substitutes a custom pagination evaluator.

---

### Finding 5 — `ISpecificationBuilder<T>` exposes the concrete `Specification<T>` in its contract

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Breaking-change trap / abstraction violation
- **Evidence:** ISpecificationBuilder.cs:
  ```csharp
  public interface ISpecificationBuilder<T> { Specification<T> Specification { get; } }
  public interface ISpecificationBuilder<T, TResult> : ISpecificationBuilder<T> { new Specification<T, TResult> Specification { get; } }
  ```
- **Observation:** The builder interface contract directly exposes the concrete `Specification<T>` class. Every extension method in `SpecificationBuilderExtensions`, `ProjectableSpecificationBuilderExtensions`, and `GroupingSpecificationBuilderExtensions` writes directly to `builder.Specification.<Property>` (e.g., `builder.Specification.AsTracking = true`). This means: (a) custom builder implementations must produce or wrap `Specification<T>` — they cannot use a different state container; (b) the internal-set properties on `Specification<T>` that are mutated from extension methods in the same assembly become a de-facto sealed mutation contract. The `internal set` on `Specification<T>` properties is the only gate.
- **Why it matters:** This is a v1 breaking-change trap. If the team ever wants to make `Specification<T>` immutable, add frozen specs, or support custom mutable state, the interface forces a breaking change. The `internal set` also means that external custom builder implementations silently cannot set these properties — they will compile but produce incorrect specs.
- **Suggested remediation:** Introduce a write-side interface (e.g., `ISpecificationWriter<T>`) with explicit mutation methods, separate from the read-side `ISpecification<T>`. This decouples state mutation from interface exposure and future-proofs immutability. Alternatively, document the concrete-class-in-interface constraint explicitly and consider it a deliberate design decision.
- **Orchestrator notes:** This is the most architecturally loaded finding. Deferring it past v1 guarantees a breaking change.

---

### Finding 6 — `ISpecification<T>` contains `AsTracking`/`AsNoTracking` as EF-ORM semantics in a provider-agnostic interface

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** EF-specific behavior leaks into core abstractions
- **Evidence:** ISpecification.cs: `bool AsTracking { get; }` and `bool AsNoTracking { get; }` are members of the core `ISpecification<T>` interface, resolved by EF-only evaluators AsTrackingQueryEvaluator.cs and AsNoTrackingQueryEvaluator.cs.
- **Observation:** Tracking is a concept specific to EF Core's change tracker. It has no equivalent in in-memory LINQ evaluation, Dapper, or any other provider. The in-memory `WhereEvaluator`, `OrderEvaluator`, and `PaginationEvaluator` all silently ignore `AsTracking`/`AsNoTracking`. The core `ISpecification<T>` is therefore littered with properties that are semantically meaningless outside EF.
- **Why it matters:** Every non-EF consumer who implements `ISpecification<T>` is forced to expose tracking properties, even if they are irrelevant to their provider. This is a guaranteed source of confusion in provider-agnostic contexts and documentation.
- **Suggested remediation:** Move `AsTracking`/`AsNoTracking` to an EF-specific interface (e.g., `IEFSpecification<T> : ISpecification<T>`) in the EF package. The builder extensions for tracking should only be available when the builder's specification implements `IEFSpecification<T>`.
- **Orchestrator notes:** This change is a breaking API surface change. Decide before v1.

---

### Finding 7 — `new` keyword hides rather than overrides; return-type covariance creates a fragile interface hierarchy

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Breaking-change traps / public abstractions with unclear contracts
- **Evidence:**
  - `ISpecification<T,TResult>` declares `new ISpecificationBuilder<T,TResult> Query` — hides the base `Query` property.
  - `IGroupingSpecification<T,TKey,TResult>` declares `new IGroupingSpecificationBuilder<T,TKey,TResult> Query`.
  - `Specification<T>` and `Specification<T,TResult>` both use `new virtual` on `Query` and `Evaluate`, rather than `override`.
  - `ISpecification<T,TResult>` declares `new IEnumerable<TResult> Evaluate(…)`, hiding the base `IEnumerable<T> Evaluate(…)`.
- **Observation:** The `new` hiding pattern is intentional (covariant return), but it means that an `ISpecification<T,TResult>` stored as `ISpecification<T>` will call the wrong `Evaluate` and return `IEnumerable<T>` (raw entities) instead of `IEnumerable<TResult>` (projected). This is not a language bug; it is a design contract ambiguity that is invisible at the call site unless the consumer knows to cast.
- **Why it matters:** A user who writes `ISpecification<Customer, CustomerDto> spec = …` and then passes it to a method accepting `ISpecification<Customer>` will get unfiltered entities back from `Evaluate`, not a DTO projection. The hiding creates a behavioral cliff that is silent at compile time.
- **Suggested remediation:** Document the hiding behavior prominently and consider whether a two-interface split (`ISpecification<T>` for filtering, `IProjectableSpecification<T,TResult>` for projection) would be cleaner. At minimum, seal `Evaluate` in derived specifications and ensure all pipeline paths use the strongly-typed overload.
- **Orchestrator notes:** Covariant hiding is idiomatic in C# but remains one of the most commonly misunderstood patterns in library design. The risk is proportional to public adoption.

---

### Finding 8 — `PaginationQueryEvaluator` calls `query.Count()` unconditionally when `Take` is null

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Correctness / performance
- **Evidence:** PaginationQueryEvaluator.cs:
  ```csharp
  var take = specification.Take ?? query.Count();
  ```
  Identical pattern in PaginationEvaluator.cs for in-memory.
- **Observation:** When `Take` is null, this forces a full `COUNT(*)` query against the database (in the EF evaluator) before applying `.Skip().Take()`. This means that for any spec without an explicit `Take`, the pipeline executes two queries: a count and then the fetch. Since pagination is always applied (even when `Skip` is 0 and `Take` is "all"), a specification with no pagination set at all will count the entire table on every query.
- **Why it matters:** This is a silent performance regression that will surprise users who don't set explicit pagination. It is also semantically wrong: a specification with no `Skip` and no `Take` should return all results, not count then take-all. The correct guard is: if both are null, skip the `.Skip().Take()` entirely.
- **Suggested remediation:** Add a guard: `if (specification.Skip is null && specification.Take is null) return query;`
- **Orchestrator notes:** This has a direct user-visible impact (double queries) and is reproducible with any spec that omits pagination.

---

### Finding 9 — The `AsComposable` extension is in a different namespace (`Mango.Specifications`) than `DbSetExtensions` (`Mango.Specifications.EntityFrameworkCore.Extensions`)

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** API surface / namespace hygiene
- **Evidence:**
  - `SpecificationCompositionExtensions` → namespace `Mango.Specifications`
  - `DbSetExtensions` → namespace `Mango.Specifications.EntityFrameworkCore.Extensions`
  - `ProjectableSpecificationBuilderExtensions`, `SpecificationBuilderExtensions`, `GroupingSpecificationBuilderExtensions` → namespace `Mango.Specifications`
- **Observation:** Extension methods that belong conceptually to the builder surface are in the root `Mango.Specifications` namespace, which is consistent. But `DbSetExtensions` requires an explicit `using Mango.Specifications.EntityFrameworkCore.Extensions`. This is a minor but real discoverability friction — EF users will not find `WithSpecification` until they know the sub-namespace.
- **Why it matters:** First-use friction leads to support questions and incorrect patterns (users calling `SpecificationEvaluator.Default.GetQuery` directly instead of using `WithSpecification`).
- **Suggested remediation:** Move `DbSetExtensions` to namespace `Mango.Specifications.EntityFrameworkCore` (not a sub-namespace). This follows the pattern of major .NET libraries.
- **Orchestrator notes:** Low risk to implement; high discoverability impact.

---

### Finding 10 — Tripled tracking extension methods; grouping and projectable builders delegate via unchecked cast

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Duplicate responsibility / API surface bloat
- **Evidence:**
  - `SpecificationBuilderExtensions.AsTracking<T>` — core
  - `ProjectableSpecificationBuilderExtensions.AsTracking<T,TResult>` — delegates via `(ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.AsTracking(builder, condition)`
  - `GroupingSpecificationBuilderExtensions.AsTracking<T,TKey,TResult>` — delegates via `(IGroupingSpecificationBuilder<T,TKey,TResult>)SpecificationBuilderExtensions.AsTracking(builder, condition)`
- **Observation:** The delegation pattern relies on explicit casts. Because `ISpecificationBuilder<T,TResult>` inherits `ISpecificationBuilder<T>`, the `SpecificationBuilderExtensions` method returns `ISpecificationBuilder<T>`, which is then cast back to `ISpecificationBuilder<T,TResult>`. This cast is safe given the current implementation (the builder returns `this`), but it is invisible to the compiler and breaks if anyone extends the builder hierarchy without matching this pattern.
- **Why it matters:** This is future-break bait. Any attempt to intercept or wrap the builder (e.g., for an immutable spec variant) will produce a `InvalidCastException` at the extension method layer with no useful error message.
- **Suggested remediation:** The tracking extension method duplication is a symptom of the missing return-type covariance problem in C# interfaces. Either accept the redundancy and document it, or use a single generic extension method with a constraint.
- **Orchestrator notes:** Low immediate risk, but worth resolving before the public extension API is documented.

---

### Finding 11 — `GetQuery` for grouping returns `Task<IQueryable<…>>` — a semantically incorrect return type

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Public abstractions with unclear contracts
- **Evidence:** ISpecificationEvaluator.cs:
  ```csharp
  Task<IQueryable<IGrouping<TKey, TResult>>> GetQuery<T, TKey, TResult>(…)
  ```
  The EF implementation materializes to a `List<…>` then wraps it in `AsQueryable()`. The method returns a `Task<IQueryable<…>>` but the underlying collection has already been executed.
- **Observation:** A `Task<IQueryable<T>>` signals "the queryable itself is computed asynchronously", which is unusual. The real intent is "the results are materialized asynchronously." This mixes the deferred execution contract of `IQueryable` with the eager execution reality of `ToListAsync`. Any consumer who tries to add further LINQ operators to the returned `IQueryable` (expecting server-side evaluation) will silently fall back to in-memory LINQ on the already-materialized list.
- **Why it matters:** This is a correctness trap. The type signature creates a false promise of composable deferred execution.
- **Suggested remediation:** Return `Task<IReadOnlyList<IGrouping<TKey, TResult>>>` or `IAsyncEnumerable<IGrouping<TKey, TResult>>`. Remove the misleading `IQueryable` wrapping.
- **Orchestrator notes:** This is also a blocker if the grouping API is part of the documented v1 surface.

---

### Finding 12 — `ReadRepositoryBase` is a public class exposing a full repository API in the EF package

- **Severity:** Medium
- **Confidence:** Medium
- **Go-Live Relevance:** Post-Go-Live
- **Category:** API surface size / overengineering
- **Evidence:** ReadRepositoryBase.cs — public class with `GetByIdAsync`, `FirstOrDefaultAsync`, `ListAsync`, `CountAsync`, `AnyAsync`, `AsAsyncEnumerable`, and grouping variants, all `virtual`.
- **Observation:** A repository base class is a significant API commitment. Every `virtual` method on a public base is a potential override surface. Publishing this at v1 locks the team into supporting the repository abstraction indefinitely. Many users will consume only the evaluator + `WithSpecification` pattern; forcing a repository as part of the package increases the surface without a corresponding benefit.
- **Why it matters:** Repository implementations are highly opinionated. Publishing one in a specification library adds a second axis of compatibility concern and will generate issues from users who want slightly different behavior.
- **Suggested remediation:** Consider shipping the repository as a `sealed` sample or moving it to a separate NuGet package (`Mango.Specifications.EntityFrameworkCore.Repositories`). If kept, remove unused `virtual` overrides.
- **Orchestrator notes:** Not a blocker, but a strategic decision. Decide before publishing the NuGet.

---

## Missing Evidence / Open Questions

1. **`.csproj` files were not directly inspected** — semantic search found sufficient evidence for package separation, but the exact `<PackageReference>` versions, `<TargetFramework>`, nullable settings, and `TreatWarningsAsErrors` state could not be confirmed. The README claims `.NET 8` target.
2. **No `GlobalUsings.cs` or `AssemblyInfo.cs` visible** — it is unclear whether `InternalsVisibleTo` is used to allow test projects to access `internal` members without needing to expose them as `public`.
3. **`IReadRepositoryBase` interface** — the interface corresponding to `ReadRepositoryBase` was referenced but its full member list was not inspected.
4. **Composition tests** — tests for `ComposableSpecificationBuilder` were visible by name but their coverage of the unsafe cast path and of grouping policy behavior could not be confirmed.
5. **`ExpressionFlattener`** — referenced but not inspected; unclear whether it has correctness issues with nested lambdas.

---

## Recommended Next Checks

1. **Inspect all `.csproj` files** to confirm: no EF Core `PackageReference` in the core project, TFM, nullable/warnings configuration, and packaging metadata.
2. **Audit `IReadRepositoryBase`** for method count and whether it is intended as a public v1 commitment.
3. **Check `InternalsVisibleTo`** — if absent, `PaginationEvaluator` being public may be a compensatory leak for test access.
4. **Confirm `ExpressionFlattener`** correctness under composed `Not` + nested group expressions — the shunting-yard parser is the highest-complexity area and the most likely source of silent correctness bugs.
5. **Verify test coverage** for the `AsComposable` + downcast scenario with a custom `ISpecification<T>` implementation — this is the most likely user-reported crash.
6. **Confirm `PaginationQueryEvaluator` double-query** is reproducible with a concrete integration test; if confirmed, fix is one guard clause.