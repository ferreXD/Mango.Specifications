# Normalized Audit Result — Architecture & Package Boundaries

## 1. Audit Metadata

| Field | Value |
| --- | --- |
| Audit Perspective | Architecture & Package Boundaries |
| Source Quality | Strong |
| Normalization Confidence | High |
| Main Risk Area | Provider leakage into core + unsafe public API contracts |
| Go-Live Sensitivity | High |

---

## 2. Executive Verdict

The architecture is directionally sound but not go-live ready as of this audit.

### Verdict Level

Mixed

### Summary

The two-package split between `Mango.Specifications` (core) and `Mango.Specifications.EntityFrameworkCore` (EF adapter) exists at the file system and namespace level and is a genuine structural achievement. The evaluator pipeline is open for extension by construction, and the composition subsystem (parenthesis-aware grouping, conflict-resolution policies) represents a meaningful differentiator over comparable libraries.

However, the package boundary is broken in practice. `ISpecificationEvaluator` and `IQueryEvaluator` — both defined in the core package — carry `IQueryable<T>` signatures, which are inherently ORM-dependent contracts. EF-specific semantics (`AsTracking`, `AsNoTracking`) are also members of the provider-agnostic `ISpecification<T>` interface, polluting the core abstraction. These two issues are co-located in the core package's public surface and represent a structural contradiction that will not be fixable without breaking changes after NuGet publication.

Three issues are classified as blockers: the ORM interface leakage described above; a silent null-dereference trap in the `AsComposable` entry point caused by an unsafe downcast; and a hardcoded production SQL Server connection string committed to the test source tree. Additionally, the `ISpecificationBuilder<T>` interface exposing the concrete `Specification<T>` class locks future immutability or custom-builder scenarios into a guaranteed breaking change.

The audit found no evidence of an inconsistency between the evaluated code and the stated design intent; rather, the stated intent (provider-agnostic core) is simply contradicted by the actual code. All blockers are fixable; none require architectural rewrites.

---

## 3. Strengths

| Strength | Evidence / Basis | Go-Live Impact |
| --- | --- | --- |
| Package split is real and physically enforced | Core package compiles without EF Core; EF package takes a project reference to core | Positive baseline for fixing leakage |
| Evaluator pipelines are open for extension by construction | Both `InMemorySpecificationEvaluator` and `SpecificationEvaluator` accept `IEnumerable<IXEvaluator>` in constructors | Reduces need for breaking changes when adding custom evaluators |
| Composition is parenthesis-aware | `OpenGroup`/`CloseGroup`/`ReturnRoot` API backed by shunting-yard-style `CompositionParser` | Differentiator; not present in Ardalis baseline |
| Conflict-resolution policies are explicit | `OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy` — all public enums in core | Avoids silent composition conflicts that plague alternatives |
| Grouping support is a rare capability | Separate `IGroupingSpecification<T,TKey,TResult>` with matching concrete classes | Material differentiator over category peers |
| Internal evaluator singletons are mostly correct | `OrderEvaluator` is `internal sealed`; `WhereEvaluator` is `internal` | Good default — avoids allocation per spec evaluation |
| `WithSpecification` is the cleanest EF integration surface | `DbSetExtensions.WithSpecification` accepts an optional custom `ISpecificationEvaluator`, enabling test substitution | Promotes testability |

---

## 4. Findings

### Finding 1 — `ISpecificationEvaluator` and `IQueryEvaluator` are ORM-specific contracts living in the core package

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Architecture |

#### Observation

`ISpecificationEvaluator` and `IQueryEvaluator` are declared in namespace `Mango.Specifications` (core package). Both carry `IQueryable<T>` signatures; `ISpecificationEvaluator` also references `IGroupingSpecification` and uses `Task`/`CancellationToken`. The in-memory evaluator (`InMemorySpecificationEvaluator`) implements `IInMemorySpecificationEvaluator`, not `ISpecificationEvaluator`. The two evaluator hierarchies are therefore incoherent: the core publishes a queryable-based interface that its own evaluator does not implement.

#### Evidence

The source audit directly states:
- ISpecificationEvaluator.cs in namespace `Mango.Specifications` — methods return `IQueryable<T>`, use `Task`/`CancellationToken`, reference `IGroupingSpecification`.
- IQueryEvaluator.cs in namespace `Mango.Specifications` — method signature is `IQueryable<T> GetQuery<T>(IQueryable<T> …)`.
- IInMemorySpecificationEvaluator.cs contains a stale comment: "ORM evaluators are not our concern yet" — directly contradicted by the existence of both interfaces in the same package.

#### Why It Matters

Any consumer importing `Mango.Specifications` for provider-agnostic code is exposed to interfaces they cannot implement without taking an ORM dependency. The incoherence between the two evaluator hierarchies creates ambiguity about which interface is the intended extension point. This will generate incorrect implementations and support issues from day one.

#### Suggested Remediation

Move `ISpecificationEvaluator` and `IQueryEvaluator` to the `Mango.Specifications.EntityFrameworkCore` package. If a provider-agnostic query-pipeline interface is genuinely needed in core, redesign it to remove `IQueryable<T>` from its signature. Remove the stale comment from `IInMemorySpecificationEvaluator`.

#### Orchestrator Notes

This cannot be silently renamed post-publication. It requires a namespace migration that is a binary and source breaking change for any early adopters. Must be resolved before the first NuGet release.

---

### Finding 2 — Unsafe downcast in `AsComposable` is a silent null-dereference trap at the composition entry point

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Correctness |

#### Observation

Both overloads of `AsComposable` in `SpecificationCompositionExtensions` accept `ISpecification<T>` / `ISpecification<T, TResult>` but internally cast to `Specification<T>` / `Specification<T, TResult>` via `as` followed by the null-forgiving `!` operator. If the caller passes any implementation of `ISpecification<T>` that does not inherit from `Specification<T>`, the `as` returns `null` and the `!` suppresses the null check, producing a `NullReferenceException` inside the `ComposableSpecificationBuilder` constructor with no diagnostic context.

#### Evidence

The source audit directly states the pattern from SpecificationCompositionExtensions.cs:
```csharp
new ComposableSpecificationBuilder<T>((specification as Specification<T>)!)
new ComposableSpecificationBuilder<T, TResult>((specification as Specification<T, TResult>)!)
```
Both lines were identified as the public entry point to the composition subsystem.

#### Why It Matters

`AsComposable` is the entry point for the library's headline feature. The method signature advertises acceptance of `ISpecification<T>` but only functions correctly if the runtime type is `Specification<T>`. This is an interface lie. Any decorator or wrapper pattern applied to a spec (a common DDD pattern) will produce an unexplained crash at runtime, not a compile-time error.

#### Suggested Remediation

Change the parameter type to accept `Specification<T>` directly, making the concrete-type requirement visible at the call site. Alternatively, introduce a marker interface or sealed abstract method that only `Specification<T>` can satisfy, and enforce it at compile time. At minimum, replace the null-forgiving `!` with an `ArgumentException` that names the incompatibility.

#### Orchestrator Notes

This is a correctness bug, not a design smell. It is triggered by a realistic usage pattern (wrapping specifications). Must be resolved before NuGet publication. The source audit confirms no test coverage for this failure path exists.

---

### Finding 3 — Production SQL Server connection string hardcoded in committed test source

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Security |

#### Observation

`TestDbContext.OnConfiguring` in the integration test project contains a hardcoded, fully-qualified SQL Server connection string targeting a named developer machine (`DESKTOP-SB85G0U`) and a specific database (`AdventureWorks2022`) with Windows Integrated Security.

#### Evidence

The source audit directly states the following from TestDbContext.cs:
```csharp
optionsBuilder.UseSqlServer("Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;…");
```

#### Why It Matters

This breaks CI pipelines on all environments that are not the developer's local machine. It leaks internal network topology. It sets a poor precedent for test configuration hygiene. Every contributor or fork will encounter non-obvious test failures before understanding the cause. While no plaintext credential is present, the pattern is a known path toward credential leakage if a future change adds authentication details.

#### Suggested Remediation

Replace with environment-variable or `appsettings.json` / `user-secrets` driven configuration in test setup. For portability, migrate integration tests to use an in-memory provider or `TestContainers` with SQL Server.

#### Orchestrator Notes

Must be removed before this branch is made publicly accessible or before any CI pipeline is configured. Classified as Critical because it will cause every external contributor's test run to fail and because the pattern directly contradicts stated contribution guidelines ("PRs welcome").

---

### Finding 4 — `PaginationEvaluator` is `public` while sibling evaluators are `internal sealed`; type-check used as grouping exclusion mechanism

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`PaginationEvaluator` is declared `public class` with a `private` constructor and a static `Instance` singleton. `OrderEvaluator` is `internal sealed`; `WhereEvaluator` is `internal`. The inconsistency is not cosmetic: `InMemorySpecificationEvaluator.Evaluate` for grouping uses `evaluator is not PaginationEvaluator` as the mechanism to exclude pagination from grouping queries. The same pattern appears in the EF `SpecificationEvaluator` with `evaluator is not PaginationQueryEvaluator`.

#### Evidence

The source audit directly references:
- PaginationEvaluator.cs: `public class PaginationEvaluator`
- OrderEvaluator.cs: `internal sealed class OrderEvaluator`
- WhereEvaluator.cs: `internal class WhereEvaluator`
- `InMemorySpecificationEvaluator`: `evaluators.Where(evaluator => evaluator is not PaginationEvaluator)`

#### Why It Matters

Making `PaginationEvaluator` public while its constructor is private provides no extension point — it is visible but not substitutable. The `is not PaginationEvaluator` type check couples the grouping pipeline to the concrete implementation type, not to a contract. A user who substitutes a custom pagination evaluator to change skip/take behavior will silently bypass the exclusion guard, causing incorrect grouping pagination.

#### Suggested Remediation

Make `PaginationEvaluator` `internal sealed`. Expose the exclusion behavior through a named contract — a `bool IsGroupingCompatible` property on `IInMemoryEvaluator`, or a separate `IPaginationEvaluator` marker interface — so the grouping pipeline checks for the role, not the type.

#### Orchestrator Notes

The type-check exclusion mechanism is the more significant problem. The visibility inconsistency is a symptom. Both should be resolved together.

---

### Finding 5 — `ISpecificationBuilder<T>` exposes the concrete `Specification<T>` class in its interface contract

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Category | Architecture |

#### Observation

`ISpecificationBuilder<T>` declares `Specification<T> Specification { get; }` — a concrete class, not an interface. Every extension method in the three builder extension classes mutates the spec state directly via `builder.Specification.<Property> = value`. Because `Specification<T>` uses `internal set` on its mutable properties, external builder implementations cannot satisfy the mutation contract at all — they compile but produce empty specs silently.

#### Evidence

The source audit directly states from ISpecificationBuilder.cs:
```csharp
public interface ISpecificationBuilder<T> { Specification<T> Specification { get; } }
public interface ISpecificationBuilder<T, TResult> : ISpecificationBuilder<T> { new Specification<T, TResult> Specification { get; } }
```
And from SpecificationBuilderExtensions.cs:
```csharp
builder.Specification.AsTracking = true;
```

#### Why It Matters

Any future attempt to make `Specification<T>` immutable, add frozen-spec semantics, or introduce an alternative mutable state container requires changing this interface — a binary breaking change. External custom builders silently fail to set spec state because of the `internal set` gate. The interface appears open but is effectively closed to external extension.

#### Suggested Remediation

Introduce a write-side abstraction (e.g., `ISpecificationWriter<T>`) with explicit mutation methods, decoupled from the read-side `ISpecification<T>`. Alternatively, document the concrete-class-in-interface constraint as a deliberate, permanent design decision with an explicit statement that external builder implementations are not a supported scenario.

#### Orchestrator Notes

This is the highest-impact architectural decision in the library. Deferring it past v1 guarantees a breaking change at some future version. Deciding to keep the concrete class in the interface is a valid choice but must be explicit and documented.

---

### Finding 6 — `AsTracking` / `AsNoTracking` are EF-ORM semantics baked into the provider-agnostic `ISpecification<T>` interface

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`bool AsTracking { get; }` and `bool AsNoTracking { get; }` are members of `ISpecification<T>` in the core package. They are consumed exclusively by `AsTrackingQueryEvaluator` and `AsNoTrackingQueryEvaluator` in the EF package. All core in-memory evaluators silently ignore them.

#### Evidence

The source audit directly states from ISpecification.cs and confirmed by AsTrackingQueryEvaluator.cs and AsNoTrackingQueryEvaluator.cs.

#### Why It Matters

Change tracking is a concept specific to EF Core's identity map. It has no equivalent in Dapper, in-memory LINQ, or any other provider. Every non-EF implementation of `ISpecification<T>` is forced to expose meaningless properties. This is an API-level contradiction of the stated provider-agnostic design goal.

#### Suggested Remediation

Move `AsTracking` / `AsNoTracking` to an EF-specific interface (e.g., `IEFSpecification<T> : ISpecification<T>`) declared in the EF package. Scope the builder tracking extensions to builders whose spec implements `IEFSpecification<T>`.

#### Orchestrator Notes

This is a breaking API surface change if existing consumers have taken a dependency on `ISpecification<T>.AsTracking`. Decide before v1 whether this is a planned break or a deferred fix.

---

### Finding 7 — `new` hiding instead of `override` on `Query` and `Evaluate` creates a behavioral cliff for consumers using base interface types

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`ISpecification<T,TResult>` declares `new ISpecificationBuilder<T,TResult> Query` and `new IEnumerable<TResult> Evaluate(…)`, hiding base members. `Specification<T>` and `Specification<T,TResult>` use `new virtual` rather than `override`. A caller holding `ISpecification<T,TResult>` stored in an `ISpecification<T>` variable will invoke the base `Evaluate` and receive raw entities, not projections.

#### Evidence

The source audit directly states this pattern across ISpecification.cs and Specification.cs. The hiding is intentional (covariant return type), but the behavioral consequence is confirmed by the language specification.

#### Why It Matters

A user who stores `ISpecification<Customer, CustomerDto>` in an `ISpecification<Customer>` variable — a completely natural and common substitution — will silently receive unfiltered entities from `Evaluate` rather than DTO projections. The error produces no compile-time signal and may produce incorrect results rather than an exception, making it extremely difficult to diagnose.

#### Suggested Remediation

At minimum, document the hiding behavior prominently with a concrete example showing the behavioral cliff. For stronger safety: evaluate whether a two-interface split (`ISpecification<T>` for filtering, `IProjectableSpecification<T,TResult> : ISpecification<T>` for projection) eliminates the ambiguity. Ensure all internal pipeline paths use the strongly-typed overload and never accept the base type where the projection type is required.

#### Orchestrator Notes

Covariant hiding via `new` is idiomatic C# but is among the most commonly misunderstood patterns in public library design. Risk scales with adoption volume.

---

### Finding 8 — `PaginationQueryEvaluator` and `PaginationEvaluator` both call `query.Count()` unconditionally when `Take` is null

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Correctness |

#### Observation

Both `PaginationQueryEvaluator` (EF) and `PaginationEvaluator` (in-memory) use `specification.Take ?? query.Count()`. When `Take` is null this executes a full count before applying `.Skip().Take()`. For a specification with neither `Skip` nor `Take` set, the evaluator counts the entire source and then takes that count — resulting in two database round-trips where zero should occur.

#### Evidence

The source audit directly states from PaginationQueryEvaluator.cs and PaginationEvaluator.cs:
```csharp
var take = specification.Take ?? query.Count();
return query.Skip(skip).Take(take);
```
The audit notes the `COUNT(*)` query is unconditional whenever `Take` is not set.

#### Why It Matters

A specification with no pagination set is the default state for all non-paginated queries. This means every such query silently issues a count before fetching, doubling database round-trips without user awareness. This is both a correctness issue (semantically, "no pagination" should mean "return everything") and a performance issue (doubled query cost for the common case).

#### Suggested Remediation

Add an early return guard: `if (specification.Skip is null && specification.Take is null) return query;`

#### Orchestrator Notes

This is the simplest fix of all Critical/High findings — one guard clause in two files. The source audit confirms the EF integration test infrastructure exists and integration tests could be extended to verify the fix.

---

### Finding 9 — `DbSetExtensions` is in a sub-namespace that impedes discoverability of `WithSpecification`

| Field | Value |
| --- | --- |
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`DbSetExtensions` (which exposes `WithSpecification`) is in namespace `Mango.Specifications.EntityFrameworkCore.Extensions`. All builder-side extension methods (`SpecificationBuilderExtensions`, `ProjectableSpecificationBuilderExtensions`, `GroupingSpecificationBuilderExtensions`, `SpecificationCompositionExtensions`) are in the root `Mango.Specifications` namespace.

#### Evidence

The source audit directly states:
- `SpecificationCompositionExtensions` → `Mango.Specifications`
- `DbSetExtensions` → `Mango.Specifications.EntityFrameworkCore.Extensions`
- All builder extension classes → `Mango.Specifications`

#### Why It Matters

`WithSpecification` is the primary EF integration surface and the recommended way to apply a spec to an EF query. A developer importing `Mango.Specifications.EntityFrameworkCore` will not find `WithSpecification` without explicitly knowing the sub-namespace. This creates first-use friction and increases the likelihood of users calling `SpecificationEvaluator.Default.GetQuery` directly — bypassing the injected evaluator and breaking testability.

#### Suggested Remediation

Move `DbSetExtensions` to namespace `Mango.Specifications.EntityFrameworkCore`. This matches the pattern used by `Microsoft.EntityFrameworkCore` extension methods and eliminates the additional `using` directive.

#### Orchestrator Notes

Low risk to implement; high discoverability impact. If `DbSetExtensions` is already referenced externally by test code in the sub-namespace, this is a minor breaking change.

---

### Finding 10 — Tracking extension methods are tripled across builder types and use unchecked casts for delegation

| Field | Value |
| --- | --- |
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`AsTracking` and `AsNoTracking` are defined three times — in `SpecificationBuilderExtensions`, `ProjectableSpecificationBuilderExtensions`, and `GroupingSpecificationBuilderExtensions`. The projectable and grouping variants delegate to the core variant via explicit casts back to their stronger interface types: `(ISpecificationBuilder<T, TResult>)SpecificationBuilderExtensions.AsTracking(builder, condition)`.

#### Evidence

The source audit directly states from ProjectableSpecificationBuilderExtensions.cs and GroupingSpecificationBuilderExtensions.cs. The cast is safe only because the current builder implementation returns `this` from these methods.

#### Why It Matters

The cast is invisible to the compiler and unchecked at runtime. Any builder implementation that does not return `this` (e.g., an immutable builder that returns a new instance, a decorator, a proxy) will produce an `InvalidCastException` at the extension method delegation point with no diagnostic context. This is future-break bait for anyone extending the builder hierarchy.

#### Suggested Remediation

Accept the redundancy and document it as a known consequence of C# interface covariance limitations. Alternatively, investigate a single constrained generic extension method pattern. Do not leave the cast pattern undocumented.

#### Orchestrator Notes

This is a symptom of Finding 5 (concrete class in interface). Resolving Finding 5 would also resolve the structural cause of this duplication.

---

### Finding 11 — `GetQuery` for grouping returns `Task<IQueryable<…>>` — a semantically misleading return type

| Field | Value |
| --- | --- |
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Category | Architecture |

#### Observation

`ISpecificationEvaluator.GetQuery<T,TKey,TResult>` returns `Task<IQueryable<IGrouping<TKey,TResult>>>`. The EF implementation materializes the result via `ToListAsync`, then wraps the materialized list in `AsQueryable()`. The returned `IQueryable` is backed by an in-memory list, not a deferred database query.

#### Evidence

The source audit directly states from ISpecificationEvaluator.cs and SpecificationEvaluator.cs (confirmed by presence of `ToListAsync` followed by `AsQueryable`).

#### Why It Matters

`Task<IQueryable<T>>` signals "the queryable itself is computed asynchronously, and further LINQ composition will be server-evaluated." The reality is the opposite: the collection is already materialized. Any LINQ operator added to the returned `IQueryable` will execute in-memory with no warning. This is a correctness trap for consumers who compose on the result.

#### Suggested Remediation

Change the return type to `Task<IReadOnlyList<IGrouping<TKey, TResult>>>` or `IAsyncEnumerable<IGrouping<TKey, TResult>>`. Remove the misleading `AsQueryable()` wrapping in the EF implementation.

#### Orchestrator Notes

This is also implicitly connected to Finding 1 — if `ISpecificationEvaluator` is moved to the EF package, the return type change can be made at the same time without a separate breaking change window.

---

### Finding 12 — `ReadRepositoryBase` is a public class with a full virtual repository API committed at v1

| Field | Value |
| --- | --- |
| Severity | Medium |
| Confidence | Medium |
| Go-Live Relevance | Post-Go-Live |
| Category | Architecture |

#### Observation

`ReadRepositoryBase<T>` in the EF package is a `public class` with `virtual` methods covering `GetByIdAsync`, `FirstOrDefaultAsync`, `ListAsync`, `CountAsync`, `AnyAsync`, `AsAsyncEnumerable`, and grouping variants.

#### Evidence

The source audit directly states from ReadRepositoryBase.cs. The audit notes: "Every `virtual` method on a public base is a potential override surface." The `IReadRepositoryBase` interface member list was not fully inspected.

#### Why It Matters

Publishing a repository base class at v1 adds a second compatibility axis. Repository implementations are highly opinionated; the library will receive issues from users who want slightly different behavior (e.g., different cancellation token handling, different async enumerable semantics). Every `virtual` method is a maintenance commitment.

#### Suggested Remediation

Consider shipping `ReadRepositoryBase` in a separate package (`Mango.Specifications.EntityFrameworkCore.Repositories`) or as an explicit sample. If kept in the main EF package, reduce `virtual` surface to only the methods users are expected to override, and `seal` the rest.

#### Orchestrator Notes

The source audit confidence on this finding is Medium because `IReadRepositoryBase` was not fully inspected. Confirm the interface member list before deciding on scope reduction. This is post-go-live and not a blocker.

---

## 5. Duplicated / Merged Findings

| Original Theme | Merged Into | Reason |
| --- | --- | --- |
| No merges performed | — | All 12 findings describe distinct problems. Findings 1 and 6 are related (both are EF leakage into core) but describe different artifacts (`ISpecificationEvaluator` vs. `ISpecification<T>` properties) and require independent remediation. Finding 10 is a symptom of Finding 5 but is retained as a separate finding because it has independent evidence and a distinct surface area. |

---

## 6. Open Questions

| Question | Why It Matters | Suggested Follow-Up |
| --- | --- | --- |
| Is `InternalsVisibleTo` configured for test projects? | If yes, `PaginationEvaluator` being `public` may be a compensatory workaround for test access, not an intentional design decision | Inspect `.csproj` files and any `AssemblyInfo.cs` / `GlobalUsings.cs` |
| Is the `AsComposable` unsafe-cast failure path covered by any test? | If untested, the blocker may be latent in production use patterns | Review composition test suite for coverage of external `ISpecification<T>` implementations passed to `AsComposable` |
| Does `ExpressionFlattener` have correctness issues under nested `Not` + grouped composition? | The shunting-yard parser is the highest-complexity area and the most likely source of silent expression bugs | Inspect ExpressionFlattener.cs and add targeted tests for `Not(And(a, Or(b, c)))` patterns |
| What is the full member list of `IReadRepositoryBase`? | Determines whether `ReadRepositoryBase` scope reduction is a breaking change or additive change | Inspect `IReadRepositoryBase.cs` directly |
| Is there an integration test that confirms `PaginationQueryEvaluator` double-query? | Without a test, the fix may regress | Add an integration test asserting that a spec with no `Skip`/`Take` executes exactly one database query |

---

## 7. Missing Evidence

| Missing Evidence | Impact on Confidence | Recommended Check |
| --- | --- | --- |
| `.csproj` files not directly inspected | Cannot confirm: exact `<PackageReference>` versions, EF Core not referenced in core `.csproj`, `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` | Inspect all four `.csproj` files directly |
| `IReadRepositoryBase` interface member list not inspected | Finding 12 confidence is Medium rather than High | Inspect IReadRepositoryBase.cs |
| ExpressionFlattener.cs not inspected | Cannot assess correctness of composed negation expressions | Inspect and add to a follow-up correctness audit |
| Composition test coverage for unsafe cast path | Cannot confirm whether Finding 2 is tested or latent | Review test files under `Ferreimavi.Specification.Tests/Specification/` for `AsComposable` with non-`Specification<T>` inputs |
| `GlobalUsings.cs` / `AssemblyInfo.cs` not visible | Cannot confirm `InternalsVisibleTo` usage, which affects interpretation of `PaginationEvaluator` visibility decision | Search for `InternalsVisibleTo` across all `.csproj` and assembly attribute files |

---

## 8. Local Recommendations

| Recommendation | Related Finding | Urgency | Notes |
| --- | --- | --- | --- |
| Move `ISpecificationEvaluator` and `IQueryEvaluator` to the EF package | Finding 1 | Critical | Plan namespace migration as a deliberate breaking change with a changelog entry |
| Replace `as … !` in `AsComposable` with a typed parameter or explicit `ArgumentException` guard | Finding 2 | Critical | Add a test that passes a custom `ISpecification<T>` implementation to confirm the guard fires |
| Externalize `TestDbContext` connection string via environment variable or `user-secrets` | Finding 3 | Critical | Use `TestContainers` for SQL Server if portable CI is required |
| Add `if (specification.Skip is null && specification.Take is null) return query;` to both pagination evaluators | Finding 8 | High | Add an integration test asserting single-query execution for non-paginated specs |
| Make `PaginationEvaluator` `internal sealed`; introduce `IPaginationEvaluator` marker or `IsGroupingCompatible` property on `IInMemoryEvaluator` | Finding 4 | High | Update grouping exclusion logic to check the contract, not the type |
| Move `AsTracking` / `AsNoTracking` to `IEFSpecification<T>` in the EF package | Finding 6 | High | Scope builder tracking extensions accordingly; this is a breaking API change — plan with versioning |
| Document or eliminate the `ISpecificationBuilder<T>` concrete-class-in-interface constraint | Finding 5 | High | If kept, add `[Obsolete]` guidance to discourage external builder implementations |
| Change `GetQuery` grouping return type to `Task<IReadOnlyList<IGrouping<TKey, TResult>>>` | Finding 11 | Medium | Coordinate with Finding 1 migration — same breaking change window |
| Move `DbSetExtensions` to namespace `Mango.Specifications.EntityFrameworkCore` | Finding 9 | Medium | Minor breaking change for consumers using the sub-namespace |
| Document `new`-hiding behavior in `ISpecification<T,TResult>` with a concrete misuse example | Finding 7 | Medium | Add to XML docs and README |
| Evaluate whether `ReadRepositoryBase` belongs in a separate package | Finding 12 | Low | Decision required before NuGet publication; not a blocking concern |

---

## 9. Orchestrator Input Summary

### Top Signals

- The core package is not provider-agnostic: `ISpecificationEvaluator`, `IQueryEvaluator`, and the `AsTracking`/`AsNoTracking` members of `ISpecification<T>` all embed ORM semantics in the wrong package layer.
- The composition entry point (`AsComposable`) contains a correctness bug (unsafe null-forgiving cast) that silently crashes on the most common extension pattern (wrapping a spec).
- A hardcoded production connection string in committed test source will break every CI pipeline and every external contributor by default.

### Potential Blockers

- `ISpecificationEvaluator` / `IQueryEvaluator` in the core package (Finding 1) — namespace migration required before publication.
- `AsComposable` unsafe downcast (Finding 2) — correctness bug at the composition entry point.
- Hardcoded SQL Server connection string in `TestDbContext` (Finding 3) — CI and security blocker.
- `ISpecificationBuilder<T>` exposing concrete `Specification<T>` (Finding 5) — guarantees a v1 breaking change if deferred.

### Pre-Go-Live Candidates

- `PaginationEvaluator` visibility inconsistency and `is not PaginationEvaluator` type-check mechanism (Finding 4).
- `AsTracking`/`AsNoTracking` in core `ISpecification<T>` (Finding 6).
- `new`-hiding behavioral cliff on `Evaluate` and `Query` (Finding 7).
- `PaginationQueryEvaluator` unconditional `Count()` double-query (Finding 8).
- `DbSetExtensions` sub-namespace discoverability issue (Finding 9).
- Tracking extension method triplication with unchecked cast delegation (Finding 10).
- `Task<IQueryable<…>>` semantically incorrect return type for grouping (Finding 11).

### Post-Go-Live Candidates

- `ReadRepositoryBase` API scope and package placement decision (Finding 12).

### Do Not Overreact To

- The `new`-hiding pattern (Finding 7) is idiomatic C# and does not require a rewrite — documentation and a clear example of the misuse scenario are sufficient for v1.
- The tracking extension triplication (Finding 10) is a code hygiene issue, not a correctness issue; the casts are safe under current implementations.
- The `ReadRepositoryBase` finding (Finding 12) reflects a strategic API scope question, not a defect — it can safely remain in the backlog after release.