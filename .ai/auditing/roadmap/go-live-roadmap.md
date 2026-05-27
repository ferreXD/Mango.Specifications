# Mango.Specification Go-Live Readiness Synthesis

## 1. Overall Verdict

| Field | Value |
| --- | --- |
| Verdict | Not Ready |
| Confidence | High |
| Main Release Risk | Multiple confirmed correctness bugs producing silently wrong query results; core package boundary violated by ORM interface leakage; the library's primary README composition example does not compile |
| Recommended Release Target | v0.x after Phase 1–2 complete; v1 Preview after Phase 1–3 complete; v1 Stable after all phases |

The library has genuine, verified technical differentiation: parenthesis-aware boolean composition, deterministic policy-controlled merge semantics, and a `GroupingSpecification` abstraction with in-memory parity — none of which exist in Ardalis.Specification v9.3.1. The evaluator pipeline order is correct, parameter unification is sound, and the physical package split is real. These are meaningful positives that make a future v1 viable.

However, the correctness story is not acceptable for a public release in its current state. `ExpressionFlattener.Flatten` produces silently wrong predicate trees whenever either operand carries more than one `Where` clause — exactly the pattern used in base/derived specification hierarchies. The test suite is systematically blind to this case. `.Not()` hard-crashes on any filter-less specification, which is a valid and natural call pattern. Both failures ship as silent wrong-result or unexplained `InvalidOperationException`, with no consumer-visible signal.

Beyond correctness, the public API contains three breaking-change timebombs: `ISpecificationEvaluator` and `IQueryEvaluator` are ORM-specific contracts committed to the provider-agnostic core package (cannot be renamed after NuGet publication), `ISpecificationBuilder<T>` exposes `Specification<T>` as a concrete class in its interface contract (locks in a future breaking change if deferred), and `AsTracking`/`AsNoTracking` are EF Core concepts on the universal `ISpecification<T>` interface. These decisions must be made — and made explicitly — before the first NuGet release.

The distribution and trust infrastructure is absent in full: no CI pipeline, no published NuGet package, a hardcoded production connection string blocking every external contributor's test run, and the library's canonical composition code example in the README will not compile. A developer who finds this library today cannot install it, cannot run its tests, and cannot follow its own documentation. The technical work required before go-live is significant but bounded — none of the blockers require an architectural rewrite.

---

## 2. Cross-Audit Signal Summary

| Area | Signal | Confidence | Go-Live Sensitivity |
| --- | --- | --- | --- |
| Correctness | Weak — two confirmed runtime crashes, one silent wrong-result bug in the headline feature, one Expression construction defect in the EF grouping path; test suite is structurally blind to the primary composition correctness bug | High | Critical |
| Architecture | Mixed — physical package split is genuine; logical boundary is broken by ORM interface leakage into core; `ISpecificationBuilder` concrete-class exposure and type-check-based exclusion mechanisms guarantee future breaking changes | High | High |
| DX | Not Ready — the primary README composition example does not compile; `AsComposable` crashes on non-concrete implementations; policy setters require an undocumented `ReturnRoot()` escape; `Not()` advertised in composition layer but absent from it | High | High |
| Adoption / Market | Not Ready — no CI, no NuGet package, no CHANGELOG, no COOKBOOK, no samples; differentiation is real and externally verified but entirely inaccessible to first-time evaluators | High | High |

---

## 3. Consolidated Findings

### Finding 1 — `AsComposable` Silent Null-Dereference on Non-Concrete `ISpecification<T>` Implementations

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Architecture (Finding 2), Code Quality (Finding 4), DX (DX-001) |

#### Consolidated Evidence

All three audits independently observe the same defect. `SpecificationCompositionExtensions.AsComposable` accepts `ISpecification<T>` but internally performs `(specification as Specification<T>)!`. The `as` returns `null` for any non-`Specification<T>` implementation; the null-forgiveness operator `!` suppresses the compiler warning; the `null` is passed to the `ComposableSpecificationBuilder<T>` constructor, which crashes at the first property access with a `NullReferenceException` pointing to the wrong location. The same pattern recurs in projectable builder extension methods beyond the two cited examples.

#### Decision

**Conflict resolution:** Code Quality rates this Medium/Pre-Go-Live; Architecture and DX rate it Critical/Blocker. Architecture and DX are correct — this is a public API contract lie, not a code smell. The method signature advertises `ISpecification<T>` but silently requires the concrete type. The Critical/Blocker rating stands.

#### Required Action

Replace all `as X!` cast sites with explicit `ArgumentException` guards. Enumerate every occurrence across all builder extension files before patching — the Code Quality audit notes the pattern recurs beyond the two explicitly cited examples. Add a test that passes a non-`Specification<T>` implementation and asserts the exception message identifies the cause.

---

### Finding 2 — `NOT` Throws `InvalidOperationException` on Any Zero-Filter Specification

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Code Quality (Finding 1), DX (DX-010) |

#### Consolidated Evidence

`NotSpecificationHelper.ComposeNotCriteria` uses `Enumerable.Aggregate` with no seed on `spec.WhereExpressions`. When the sequence is empty, this throws `InvalidOperationException: Sequence contains no elements` at a call site unrelated to the user's action. The internal `CompositionParser` uses `new Specification<T>()` as a fallback for null specs, meaning the crash surface extends to internal composition paths — not just direct consumer `.Not()` calls. No test covers the zero-filter path in any audit.

#### Decision

**Conflict resolution:** Code Quality rates this Critical/Blocker; DX rates it Medium/Pre-Go-Live citing lack of a reproducing test. The Code Quality rating is correct — the crash path is directly visible in source via static analysis and is reachable from both consumer and internal composition code. Medium confidence in the DX audit refers only to test evidence, not to the defect's existence.

#### Required Action

Guard `ComposeNotCriteria` for the empty-expressions case. The correct semantic is `_ => false` (negation of "match all" is "match none"). This decision must be documented. Add a test for `emptySpec.Not()` that must fail before the fix and pass after.

---

### Finding 3 — `ExpressionFlattener.Flatten` Produces Incorrect Predicate Semantics for Multi-Filter Operands; Test Suite is Systematically Blind to This

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Code Quality (Findings 2 and 9) |

#### Consolidated Evidence

`ExpressionFlattener.Flatten` concatenates all `WhereExpressions` from both operands into a flat list and applies the combiner uniformly. For OR: if `left` has `[f1, f2]` and `right` has `[f3, f4]`, the intended semantics are `(f1 AND f2) OR (f3 AND f4)`; the code produces `f1 OR f2 OR f3 OR f4`, returning more rows than intended with no error. The zero-filter-on-one-side short-circuit is also incorrect. Every existing AND/OR composition test uses specifications with exactly one `WhereExpression` — precisely the shape that prevents the bug from manifesting. The test suite is not accidentally incomplete; it is structurally blind to the failure mode by design of the test specifications.

#### Decision

These two findings are kept as a single consolidated entry because they describe a co-dependent root cause (logic defect) and a co-dependent diagnostic gap (test gap that made the defect invisible). Resolving the logic without adding multi-filter tests leaves the fix unguarded. Both must ship together.

#### Required Action

Rewrite `Flatten` to reduce each operand to a single expression via `AndAlso` before combining with the target combiner. Add regression tests with at least one two-filter operand, asserting correct predicate grouping. Tests must fail before the fix and pass after. Both the EF and in-memory evaluation paths are affected and must be covered.

---

### Finding 4 — ORM-Specific Interfaces Committed to the Provider-Agnostic Core Package

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Architecture (Findings 1 and 11) |

#### Consolidated Evidence

`ISpecificationEvaluator` and `IQueryEvaluator` are declared in namespace `Mango.Specifications` (core package) with `IQueryable<T>` signatures — an inherently ORM-dependent contract. `IInMemorySpecificationEvaluator` does not implement `ISpecificationEvaluator`, making the two evaluator hierarchies incoherent. A stale comment in `IInMemorySpecificationEvaluator` — "ORM evaluators are not our concern yet" — directly contradicts their presence in the same package. Additionally, `ISpecificationEvaluator.GetQuery<T,TKey,TResult>` returns `Task<IQueryable<IGrouping<TKey,TResult>>>`, which is semantically misleading: the EF implementation materializes via `ToListAsync` then wraps in `AsQueryable()`, so the returned `IQueryable` is backed by an in-memory list, not a deferred database query. This is a correctness trap for consumers who compose on the result.

#### Decision

The misleading `Task<IQueryable<...>>` return type (Architecture Finding 11) is consolidated here because its fix is in the same migration window. Moving `ISpecificationEvaluator` to the EF package is the opportunity to correct the return type to `Task<IReadOnlyList<IGrouping<TKey, TResult>>>` simultaneously, avoiding a second breaking change window.

#### Required Action

Move `ISpecificationEvaluator` and `IQueryEvaluator` to namespace `Mango.Specifications.EntityFrameworkCore`. Correct the grouping `GetQuery` return type to `Task<IReadOnlyList<IGrouping<TKey, TResult>>>` in the same operation. Remove the stale comment. Plan this as a deliberate breaking change with a changelog entry. This must be completed before the first NuGet release.

---

### Finding 5 — Hardcoded Production SQL Server Connection String in Committed Test Source

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Architecture (Finding 3), Adoption/Market (AMP-003) |

#### Consolidated Evidence

`TestDbContext.OnConfiguring` hardcodes `Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;Integrated Security=True;...`. `DbContextFactory` passes empty options; `OnConfiguring` overrides them unconditionally. Every external contributor's `dotnet test` run will fail with a network/connection error with no explanation. The hostname `DESKTOP-SB85G0U` is exposed in public source. No credentials are present (Integrated Security), but the pattern is OWASP A05 (Security Misconfiguration) — embedding environment-specific configuration in committed source is a direct path toward future credential leakage if the pattern is extended.

#### Required Action

Replace with environment variable (`TEST_DB_CONNECTION`) with a `null` fallback. Guard `OnConfiguring` to only apply when the connection string is available. Annotate SQL Server–specific tests with `[Trait("Category", "Integration")]` and exclude them from CI runs via `--filter "Category!=Integration"` until a portable database solution (TestContainers or equivalent) is in place.

---

### Finding 6 — No CI Workflow

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Adoption/Market (AMP-001) |

#### Consolidated Evidence

No `.github/workflows/` directory exists. The README build section contains a placeholder clone URL (`your-org`), confirming the repository has never been treated as a fully public artifact. No automated regression gate exists; every other trust signal (test results, coverage, CI badge) depends on this prerequisite.

#### Required Action

Add `.github/workflows/ci.yml` running `dotnet build` and `dotnet test --filter "Category!=Integration"` on `push` and `pull_request` to `develop` and `main`. Correct the clone URL to `https://github.com/ferreXD/Mango.Specifications.git`. Add CI status badge to README header. This must land before the connection string fix is meaningful.

---

### Finding 7 — NuGet Packages Unpublished; Internal Project Name Conflicts with Public Brand

| Field | Value |
| --- | --- |
| Severity | Critical |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Adoption/Market (AMP-002), DX (DX-011) |

#### Consolidated Evidence

The README installation instructions read `dotnet add package Mango.Specifications` / `dotnet add package Mango.Specifications.EntityFrameworkCore`, with a "(Coming soon on NuGet)" comment. The internal project/assembly names use the `Ferreimavi.Specification` prefix. No `<PackageId>` elements were confirmed in `.csproj` files (Inferred/Medium confidence per AMP audit; direct `.csproj` inspection is required to confirm). If published without explicit `<PackageId>`, packages would appear under `Ferreimavi.Specification`, making them undiscoverable under the `Mango.Specifications` brand.

#### Decision

The AMP audit notes the `PackageId` absence is Inferred/Medium confidence. The finding is retained as a Blocker because the NuGet package is confirmed absent, and the name mismatch risk is real until `.csproj` files are directly inspected. This must be resolved — confirmed or corrected — before publication.

#### Required Action

Inspect all `.csproj` files and add `<PackageId>Mango.Specifications</PackageId>` and `<PackageId>Mango.Specifications.EntityFrameworkCore</PackageId>` if absent. Complete Phase 1–3 work before publication. Publish a pre-release package (e.g., `0.1.0-alpha.1`) as the first distribution milestone. Add NuGet version and download badges to README.

---

### Finding 8 — `ISpecificationBuilder<T>` Exposes Concrete `Specification<T>` in Its Interface Contract

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | Architecture (Finding 5) |

#### Consolidated Evidence

`ISpecificationBuilder<T>` declares `Specification<T> Specification { get; }`. Every builder extension method mutates spec state directly via `builder.Specification.<Property> = value`. Because `Specification<T>` uses `internal set` on mutable properties, external builder implementations cannot satisfy the mutation contract — they compile but produce empty specs silently. Any future attempt to introduce immutability, a frozen-spec pattern, or an alternative mutable state container requires changing this interface — a binary breaking change.

#### Decision

This is the highest-impact API design decision in the library. Deferring it past v1 is itself the risk. The resolution is a choice, not necessarily an implementation: either (a) introduce a write-side abstraction (`ISpecificationWriter<T>`) with explicit mutation methods, or (b) explicitly document the concrete-class-in-interface constraint as a permanent design decision with a clear statement that external builder implementations are not a supported scenario. Both are valid. Neither can be left ambiguous at v1.

#### Required Action

Make the decision before NuGet publication and encode it explicitly: either refactor toward a write-side abstraction, or add `<remarks>` XML documentation to `ISpecificationBuilder<T>` stating the concrete-class constraint is permanent. If option (b) is chosen, add `[EditorBrowsable(EditorBrowsableState.Never)]` guidance to discourage extension.

---

### Finding 9 — Policy Setters Unreachable After `And()`/`Or()` Without Undocumented `ReturnRoot()`; README Example Does Not Compile

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Blocker |
| Source Audits | DX (DX-002, DX-007) |

#### Consolidated Evidence

`And()`, `Or()`, and `CloseGroup()` all return `IBaseComposableSpecificationBuilder<T>`. `WithOrderingEvaluationPolicy()`, `WithPaginationEvaluationPolicy()`, and `Build()` exist only on `IComposableSpecificationBuilder<T>` (the stronger interface). The only escape is `ReturnRoot()`, which is not documented, is named after an internal tree concept, and returns `this` unchanged on the root builder (it is a pure type-cast no-op). The README's canonical composition example calls policy setters *before* `ReturnRoot()` — in the wrong order — and will produce a compile error. Test code works around this by using an `as` cast with `!`, which is the same undocumented workaround required of all consumers.

DX-007 (README compile error) is the consumer-visible symptom of DX-002 (API type chain design flaw). They share the same root cause and required action.

#### Required Action

Option A (preferred): Move `WithOrderingEvaluationPolicy()`, `WithPaginationEvaluationPolicy()`, and `WithProjectionEvaluationPolicy()` to `IBaseComposableSpecificationBuilder<T>`, eliminating the need for `ReturnRoot()` as a type-escape. Option B: Change `And()`/`Or()`/`CloseGroup()` return types to `IComposableSpecificationBuilder<T>` directly on the root builder. Regardless of which option is chosen, fix the README example to compile. Rename `ReturnRoot()` if it remains (see DX-009).

---

### Finding 10 — `CreateShallowSelector<T, TResult>` Binds `T` Property Infos to a `TResult` `MemberInit` Expression

| Field | Value |
| --- | --- |
| Severity | Medium |
| Confidence | High |
| Go-Live Relevance | Blocker (scoped to EF Core grouping path when `T ≠ TResult`) |
| Source Audits | Code Quality (Finding 5) |

#### Consolidated Evidence

`CreateShallowSelector<T, TResult>` in `SpecificationEvaluator` iterates `typeof(T).GetProperties()` to build `MemberBinding` objects but passes them to `Expression.MemberInit(Expression.New(typeof(TResult)), bindings)`. `Expression.MemberInit` requires that each `MemberInfo` belongs to `TResult`. When `T ≠ TResult` — the common case for grouping — `Expression.Bind` throws `ArgumentException` at expression construction time. The `IsIdentitySelector` guard gates this path, but its exact condition was not confirmed in any audit. Until `IsIdentitySelector` is inspected, the crash risk for real-world grouping usage is not bounded.

#### Required Action

Inspect `IsIdentitySelector` to determine whether the guard reliably prevents `T ≠ TResult` from reaching `CreateShallowSelector`. Fix the binding regardless: iterate `typeof(TResult).GetProperties()` and resolve matching source properties from `typeof(T)`. Add a targeted test for a grouping specification where `TResult ≠ T` to confirm the fix and guard against regression.

---

### Finding 11 — Pagination Evaluators Issue an Unconditional `COUNT(*)` for Every Unpaginated Specification

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Source Audits | Code Quality (Finding 3), Architecture (Finding 8) |

#### Consolidated Evidence

Both `PaginationQueryEvaluator` (EF) and `PaginationEvaluator` (in-memory) use `specification.Take ?? query.Count()`. When `Take` is null, this executes a full `COUNT(*)` against the database (or fully enumerates the in-memory source) before applying `.Skip().Take()`. Because `PaginationQueryEvaluator` is unconditionally registered in the default evaluator list, this penalty applies to every non-paginated query — the majority of real-world usage. The fix is a single early-return guard in two files.

#### Required Action

Add `if (specification.Skip is null && specification.Take is null) return query;` to both `PaginationQueryEvaluator.GetQuery` and `PaginationEvaluator.Evaluate`. Add a test that asserts a spec with no `Skip`/`Take` executes exactly one database round-trip.

---

### Finding 12 — EF-Specific Tracking Semantics Baked into the Provider-Agnostic `ISpecification<T>` Interface

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Source Audits | Architecture (Finding 6) |

#### Consolidated Evidence

`bool AsTracking { get; }` and `bool AsNoTracking { get; }` are members of `ISpecification<T>` in the core package. They are consumed exclusively by `AsTrackingQueryEvaluator` and `AsNoTrackingQueryEvaluator` in the EF package. Every non-EF implementation of `ISpecification<T>` is forced to expose meaningless properties. This is a different surface than Finding 4 (evaluator interfaces) — it pollutes the core data contract, not just the evaluation contract.

#### Required Action

Move `AsTracking`/`AsNoTracking` to an EF-specific interface (`IEFSpecification<T> : ISpecification<T>`) declared in the EF package. Scope the builder tracking extensions to builders whose spec implements `IEFSpecification<T>`. This is a breaking API change; decide and document before v1.

---

### Finding 13 — `Not()` Advertised as Composition-Level Feature; Absent from Composition API

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Source Audits | DX (DX-004) |

#### Consolidated Evidence

The README Features section states "✅ Parentheses-aware `And/Or/Not` composition." `Not` is not a member of `IBaseComposableSpecificationBuilder<T>`. Three separate `Not` surfaces exist at different abstraction levels with different semantics, but none is a composition-chain operator equivalent to `And`/`Or`. A developer expecting `.And(specA).Not(specB)` will find no such method.

#### Required Action

Either implement `Not(ISpecification<T>)` on `IBaseComposableSpecificationBuilder<T>` (and `<T, TResult>`) before v1, or remove the `Not` claim from the README Features section and qualify it accurately. The design decision for `Not` semantics in composition context (negate operand vs. negate accumulated left side) must be made explicitly.

---

### Finding 14 — `GroupBy` EF Path Materializes Full Result Set Before Pagination; Undocumented Production Risk

| Field | Value |
| --- | --- |
| Severity | High |
| Confidence | High |
| Go-Live Relevance | Pre-Go-Live |
| Source Audits | Adoption/Market (AMP-005) |

#### Consolidated Evidence

`SpecificationEvaluator.GetQuery<T,TKey,TResult>` calls `ToListAsync()` on the full grouped dataset before applying `Skip`/`Take` in memory. A code comment in the same method reads "TODO: If performance is a real concern in the future, consider using a custom implementation of GroupBy that does not materialize the query." The limitation is acknowledged internally but not surfaced to consumers. The README grouping example does not include a warning.

#### Required Action

Add a `<remarks>` warning to `GroupingSpecification<T,TKey,TResult>` XML docs and a "Known Limitations" section to the README stating that pagination on grouped results is applied in-memory after full materialization. This is a documentation-only action for v1; server-side group pagination is out of scope.

---

## 4. Release Blockers

| Blocker | Why It Blocks | Required Exit Criteria |
| --- | --- | --- |
| ExpressionFlattener incorrect semantics (Finding 3) | Silently returns wrong query results for multi-filter OR composition — the library's headline feature | Fixed + regression tests using multi-filter operands passing |
| NOT crash on zero-filter spec (Finding 2) | Hard crash on a naturally reachable call pattern; also triggered by internal composition fallbacks | Guard implemented; `emptySpec.Not()` test passes |
| AsComposable null-dereference (Finding 1) | Public API contract lie; silent crash on decorator/wrapper patterns common in DDD | All `as X!` cast sites replaced with `ArgumentException` guards; test for non-concrete implementation passes |
| ORM interfaces in core package (Finding 4) | Cannot rename post-NuGet without binary breaking change; incoherent evaluator hierarchies | `ISpecificationEvaluator`/`IQueryEvaluator` in EF namespace; grouping return type corrected; stale comment removed |
| ISpecificationBuilder concrete class exposure (Finding 8) | Decision must be made before v1 or a breaking change is guaranteed post-v1 | Explicit decision implemented or documented; no ambiguity in public contract |
| Hardcoded SQL Server connection string (Finding 5) | Breaks every external contributor's test run; CI cannot pass | Connection string externalized via environment variable; CI passes on a clean machine |
| No CI workflow (Finding 6) | No automated regression gate; no trust signal for any consumer | CI workflow running `dotnet build` + `dotnet test --filter "Category!=Integration"` on PRs; badge in README |
| NuGet not published / name mismatch (Finding 7) | Library is currently uninstallable via `dotnet add package`; brand mismatch may persist post-publication | `PackageId` confirmed in `.csproj`; pre-release package published to NuGet.org |
| Policy setters unreachable / README example won't compile (Finding 9) | Library's primary differentiator (merge policies) is inaccessible via the documented happy path; 100% of first-time users who follow the README hit a compile error | Policy setters accessible without undocumented escape; README example compiles and is copy-paste runnable |
| CreateShallowSelector binds wrong type (Finding 10) | Crashes the EF grouping path when `T ≠ TResult` — the common case | Fix confirmed by `IsIdentitySelector` inspection; dedicated grouping test where `TResult ≠ T` passes |

---

## 5. Pre-Go-Live Refinement Roadmap

### Phase 1 — Correctness & API Stability

| Work Item | Source Finding | Exit Criteria |
| --- | --- | --- |
| Guard `NotSpecificationHelper.ComposeNotCriteria` for empty `WhereExpressions`; return `_ => false` | Finding 2 (CQ-1) | `emptySpec.Not()` returns `_ => false`; no crash |
| Rewrite `ExpressionFlattener.Flatten` to reduce each operand before combining | Finding 3 (CQ-2) | Multi-filter OR returns correct predicate grouping |
| Replace all `as X!` cast sites in builder/composition extensions with explicit `ArgumentException` guards; enumerate all occurrences first | Finding 1 (ARCH-2/CQ-4/DX-001) | `AsComposable` on non-`Specification<T>` throws descriptive `ArgumentException` |
| Move `ISpecificationEvaluator` / `IQueryEvaluator` to EF namespace; correct grouping return type to `Task<IReadOnlyList<...>>`; remove stale comment | Finding 4 (ARCH-1/ARCH-11) | Core package compiles without any `IQueryable` reference in evaluator interfaces |
| Make explicit decision on `ISpecificationBuilder<T>` concrete class exposure; implement write-side abstraction or document constraint permanently | Finding 8 (ARCH-5) | Public contract is unambiguous; no deferred breaking change risk |
| Move `AsTracking`/`AsNoTracking` to `IEFSpecification<T>` in EF package | Finding 12 (ARCH-6) | Core `ISpecification<T>` has no EF-specific members |
| Move policy setter methods to `IBaseComposableSpecificationBuilder<T>`; remove or rename `ReturnRoot()` | Finding 9 (DX-002/DX-009) | Policy setters reachable after every `And()`/`Or()`/`CloseGroup()` without cast escape |
| Inspect `IsIdentitySelector`; fix `CreateShallowSelector` to bind `TResult` properties | Finding 10 (CQ-5) | No `ArgumentException` in EF grouping path when `T ≠ TResult` |
| Add `if (specification.Skip is null && specification.Take is null) return query;` to both pagination evaluators | Finding 11 (CQ-3/ARCH-8) | Unpaginated spec executes one DB query, not two |
| Externalize `TestDbContext` connection string via environment variable; guard `OnConfiguring` | Finding 5 (ARCH-3/AMP-003) | `dotnet test --filter "Category!=Integration"` passes on a clean machine with no SQL Server |

### Phase 2 — Tests & Behavioral Proof

| Work Item | Source Finding | Exit Criteria |
| --- | --- | --- |
| Add multi-filter OR regression tests (two-filter spec OR one-filter spec, asserting correct predicate grouping) | Finding 3 (CQ-9) | Tests fail before Phase 1 ExpressionFlattener fix; pass after |
| Add `emptySpec.Not()` test | Finding 2 (CQ-1) | Test fails before guard; passes after |
| Add test for `AsComposable` on non-`Specification<T>` implementation | Finding 1 (DX-001) | `ArgumentException` with correct message is asserted |
| Add test for `CreateShallowSelector` where `TResult ≠ T` | Finding 10 (CQ-5) | No exception thrown; projection is correct |
| Add test for unpaginated spec asserting single DB round-trip | Finding 11 (CQ-3/ARCH-8) | COUNT(*) is not issued for specs with no `Skip`/`Take` |
| Add CI workflow (`.github/workflows/ci.yml`) with `dotnet build` + `dotnet test --filter "Category!=Integration"` | Finding 6 (AMP-001) | CI badge shows passing status on `develop` and `main` |
| Correct README clone URL to `https://github.com/ferreXD/Mango.Specifications.git` | AMP-007 | Link returns HTTP 200 |

### Phase 3 — Documentation & DX Trust

| Work Item | Source Finding | Exit Criteria |
| --- | --- | --- |
| Fix README composition example (move `.ReturnRoot()` or remove it per Phase 1 API fix) | Finding 9 (DX-007) | Example compiles and runs as copy-paste |
| Implement `Not(ISpecification<T>)` on `IBaseComposableSpecificationBuilder<T>` or remove `Not` from README Features | Finding 13 (DX-004) | README accurately reflects the API |
| Rename `IncludeTypeEnum` → `IncludeType`; `OrderTypeEnum` → `OrderType`; update all references | DX-005 | No `Enum`-suffixed public types in the codebase |
| Fix `OpenGroup(ISpecification<T>)` overload to return `ComposedGroupOperationBuilder` in `<T, TResult>` builder | DX-006 | Group context is consistent across both `OpenGroup` overloads |
| Make `ComposableSpecificationBuilder<T>` and `ComposedGroupOperationBuilder<T>` `internal`; expose only via `AsComposable()` | DX-003 | No public concrete builder types; tests use `spec.AsComposable()` |
| Document `new`-hiding behavioral cliff on `ISpecification<T,TResult>.Evaluate`/`Query` with a concrete misuse example in XML docs and README | ARCH-7 | XML doc on hiding members includes a `/// <remarks>` warning with a behavioral example |
| Add `<remarks>` warning to `GroupingSpecification` re: in-memory pagination; add "Known Limitations" section to README | Finding 14 (AMP-005) | README and XML docs explicitly warn about full materialization before pagination |
| Document include parity gap in README and `InMemorySpecificationEvaluator` XML docs | CQ-6 | README's parity section accurately describes what is and is not mirrored in-memory |
| Move `DbSetExtensions` to namespace `Mango.Specifications.EntityFrameworkCore` | ARCH-9 | No additional `using` directive required to access `WithSpecification` |
| Create `CHANGELOG.md` with v0.x-alpha feature inventory | AMP-004/DX-008 | `CHANGELOG.md` exists at repo root; README changelog link resolves |
| Create `docs/COOKBOOK.md` with at minimum two end-to-end composition policy examples | AMP-004/DX-008 | Cookbook is reachable from README; examples compile and demonstrate `OpenGroup`/policy setters |
| Add "When to Choose Mango" vs. Ardalis section to README | AMP-006 | README has an explicit comparison section before the feature list |
| Clarify attribution statement (replace "may conceptually derive" with an explicit statement of original work) | AMP-008 | Attribution is unambiguous; no "may" language |
| Apply `[Experimental]` attributes to provisional API surfaces (composition builder chain); define v1 stability boundary | AMP-009 | README Roadmap explicitly lists v1 scope boundary; provisional APIs emit compile-time warnings |
| Document or eliminate tracking extension triplication; document cast safety assumption | ARCH-10 | Either redundancy is accepted and documented, or constrained generic extension reduces duplication |

### Phase 4 — Adoption & Positioning

| Work Item | Source Finding | Exit Criteria |
| --- | --- | --- |
| Publish `Mango.Specifications` and `Mango.Specifications.EntityFrameworkCore` pre-release to NuGet.org | Finding 7 (AMP-002/DX-011) | `dotnet add package Mango.Specifications --prerelease` succeeds |
| Add NuGet version and download badges to README | AMP-002 | Badges visible in README header |
| Add Coverlet to test project `.csproj` files; publish coverage report via GitHub Actions | AMP-010 | Coverage badge visible in README; report accessible from CI run |
| Create one runnable sample project under `samples/Composition/` | AMP-004 | Sample builds and runs without SQL Server |
| Evaluate whether `ReadRepositoryBase` belongs in a separate package | ARCH-12 | Explicit decision documented; either separated or sealed surface reduced |

---

## 6. Post-Go-Live Backlog

| Item | Why Post-Go-Live | Notes |
| --- | --- | --- |
| `NotSpecification` silently drops ordering, pagination, and include expressions | Behavior is arguably defensible for a NOT operator; documentation clarifying it is the v1 answer | Add XML doc to `Not()` stating ordering/pagination/includes are not propagated; consider a `Not(propagateRest: true)` overload in v1.1 |
| `WhereEvaluator.IsCriteriaEvaluator` dead property | Low severity; no user-visible impact; safe cleanup | Remove the property or extend `IInMemoryEvaluator` to support criteria-only evaluation symmetrically with `IQueryEvaluator` |
| `ReadRepositoryBase` API surface and package placement | Strategy question, not a defect; `IReadRepositoryBase` member list was not fully inspected | Inspect `IReadRepositoryBase.cs`; if scope is broad, move to `Mango.Specifications.EntityFrameworkCore.Repositories` or a separate NuGet package |
| `ISpecification<T>` scalar property mutation contract documentation | `internal set` limits actual mutation to subclasses in the same assembly; external risk is lower than originally flagged | Document that specs are mutable until the Freeze() mechanism lands; revisit when implementing Freeze() |
| `GroupingSpecification.Query.Select()` semantic divergence from `Specification<T,TResult>.Query.Select()` | Low user risk if COOKBOOK distinguishes the two; naming refactor (`SelectResult()`) is a breaking API change | Address in v1.1 or document thoroughly in COOKBOOK |
| Server-side group pagination for `GroupingSpecification` | Acknowledged as a TODO in source; requires provider-specific query analysis | Out of scope for v1; track in GitHub Issues |
| Test coverage reporting | Dependent on CI (Phase 2) and portable integration tests | Coverage report after integration tests are made portable |

---

## 7. Recommended v1 Scope Boundary

### In Scope for v1

- `Specification<T>` and `Specification<T, TResult>` as the primary base classes
- `ISpecification<T>` and `ISpecification<T, TResult>` as public read interfaces (after removing EF-specific members)
- `InMemorySpecificationEvaluator` and its `IInMemoryEvaluator` extension point
- `ISpecificationEvaluator` and `IQueryEvaluator` (in EF package after Phase 1 migration)
- `SpecificationEvaluator` for EF Core
- `DbSetExtensions.WithSpecification` as the primary EF integration surface
- `AsComposable()` entry point + composition builder chain (after Phase 1 API fixes)
- `OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy` enums
- `GroupingSpecification<T,TKey,TResult>` with documented in-memory pagination limitation
- `ReadRepositoryBase` (with reduced `virtual` surface; or moved to a separate package)
- All builder extension methods (after Phase 1 tracking/EF cleanup)

### Explicitly Out of Scope for v1

- Dapper or any second ORM provider (the ORM leakage cleanup must be complete first)
- `Freeze()` / frozen-spec semantics (referenced in README roadmap; not implemented; out of scope)
- Server-side group pagination
- `IAsyncEnumerable<T>` evaluation pipeline
- A custom package for `ReadRepositoryBase` (post-go-live decision)
- Any new composition operators beyond `And`, `Or`, `Not` (get the existing three correct first)

---

## 8. Do-Not-Do List

These are things that may feel attractive but would damage the release focus.

- **Do not publish to NuGet before the correctness blockers (Findings 1–3, 10) are fixed.** Shipping silently wrong query results under the `Mango.Specifications` brand will define the library's reputation permanently.
- **Do not add new composition operators or builder overloads before resolving the `ISpecificationBuilder<T>` concrete class decision (Finding 8).** Every new overload is another instance of the cast-delegation pattern that will need to be unwound.
- **Do not write `samples/` before fixing the README composition example.** Samples derived from a broken example will carry the same compile error into more visible code.
- **Do not begin a Dapper or alternate provider before the ORM interface migration (Finding 4) is complete.** Adding a second provider before `ISpecificationEvaluator` is correctly scoped to the EF package will deepen the architectural debt, not validate the provider-agnostic claim.
- **Do not attempt a `Freeze()` implementation during this release cycle.** It is referenced in the roadmap but not started; implementing it without first resolving the `ISpecificationBuilder<T>` mutability design question would force two simultaneous breaking changes.
- **Do not treat `ReadRepositoryBase` as a first-class v1 API commitment without first deciding its package placement.** Publishing it in the main EF package with a full `virtual` surface creates a maintenance surface you will not be able to reduce without a breaking change.
- **Do not use `Task<IQueryable<T>>` as a return type anywhere in the public API.** It is semantically misleading by definition. Fix the existing occurrence in the grouping `GetQuery` method (Finding 4) and do not introduce new usages.
- **Do not overhaul `ExpressionFlattener` beyond the two-operand-reduction fix.** The fix is a targeted guard clause, not a rewrite. Scope creep into expression caching or operator generalization would delay the correctness fix with no user benefit.

---

## 9. Implementation Prompt Queue

The following Copilot implementation prompts should be generated after this synthesis, in priority order:

1. **Fix `NotSpecificationHelper.ComposeNotCriteria` empty-expression guard** — add `if (expressions.Count == 0) return _ => false;` with a companion unit test for `emptySpec.Not()`.
2. **Rewrite `ExpressionFlattener.Flatten` to reduce operands before combining** — implement the two-step reduction pattern; add multi-filter OR regression tests that fail before and pass after.
3. **Enumerate and fix all `as X!` cast sites in builder and composition extensions** — grep for `as ` + `!` across all extension files; replace with `ArgumentException` guards; add a test for `AsComposable` on a non-`Specification<T>` implementation.
4. **Migrate `ISpecificationEvaluator` and `IQueryEvaluator` to the EF package; correct grouping return type to `Task<IReadOnlyList<IGrouping<TKey,TResult>>>`** — plan as a deliberate breaking change with a changelog entry.
5. **Resolve the `ISpecificationBuilder<T>` concrete class design decision** — implement `ISpecificationWriter<T>` or document the constraint as permanent; update all extension methods accordingly.
6. **Move `AsTracking`/`AsNoTracking` to `IEFSpecification<T>` in the EF package** — scope builder tracking extensions to the EF-specific interface.
7. **Fix composition builder type chain: move policy setters to `IBaseComposableSpecificationBuilder<T>`** — eliminate the need for `ReturnRoot()` as a type-escape; fix the README example; rename or remove `ReturnRoot()`.
8. **Inspect `IsIdentitySelector`; fix `CreateShallowSelector` to bind `TResult` properties** — add a grouping test where `TResult ≠ T`.
9. **Add early-return pagination guard** — `if (Skip is null && Take is null) return query;` in both `PaginationQueryEvaluator` and `PaginationEvaluator`; add no-pagination single-query test.
10. **Externalize `TestDbContext` connection string** — environment variable with null fallback; `[Trait("Category", "Integration")]` on SQL Server–dependent tests.
11. **Add CI workflow** — `.github/workflows/ci.yml`; `dotnet build` + `dotnet test --filter "Category!=Integration"`; CI badge in README; correct clone URL.
12. **Rename `IncludeTypeEnum` → `IncludeType` and `OrderTypeEnum` → `OrderType`** — update all references.
13. **Fix `OpenGroup(ISpecification<T>)` overload in `ComposableSpecificationBuilder<T, TResult>`** — return `ComposedGroupOperationBuilder` matching the `ISpecification<T, TResult>` overload; add group-precedence test.
14. **Make `ComposableSpecificationBuilder<T>` and `ComposedGroupOperationBuilder<T>` `internal`** — expose only via `AsComposable()`; update tests.
15. **Implement or disclaim `Not()` on `IBaseComposableSpecificationBuilder<T>`** — add to interface or remove claim from README Features section.
16. **Create `CHANGELOG.md` and `docs/COOKBOOK.md`** — CHANGELOG with v0.x feature inventory; COOKBOOK with two end-to-end composition policy examples.
17. **Add "When to Choose Mango" section and clarify attribution statement in README** — include explicit comparison vs. Ardalis.Specification; replace vague attribution with a positive statement of original contributions.
18. **Confirm `<PackageId>` in `.csproj` files; publish pre-release to NuGet.org** — add NuGet version badge to README.