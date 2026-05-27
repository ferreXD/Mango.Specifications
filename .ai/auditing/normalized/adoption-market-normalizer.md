---
audit-id:        AMP-2026-05-14
perspective:     Adoption Readiness & Market Positioning
source-branch:   develop
default-branch:  main
audit-date:      2026-05-14
auditor:         GitHub Copilot (Claude Sonnet 4.6)
schema-version:  1.0
---

# Normalized Audit Result — Adoption Readiness & Market Positioning

## Audit Header

| Field | Value |
|---|---|
| Repository | ferreXD/Mango.Specifications |
| Branch audited | `develop` |
| Default branch | `main` (state unknown — audit did not inspect `main`) |
| Verdict | **Do not adopt before v1.** Differentiation is real; trust signals are absent. |
| Total findings | 10 |
| Critical | 2 |
| High | 3 |
| Medium | 4 |
| Low | 1 |

---

## Scope Table

| Area | Evidence Examined | Evidence Present? |
|---|---|---|
| README | README.md | Yes |
| License | LICENSE | Yes — MIT, 2025 ferreXD |
| CI / Workflows | `.github/workflows/` | **No** — directory not found |
| NuGet package metadata | `*.csproj` `PackageId`, `Version` fields | **Not found** |
| CHANGELOG | `CHANGELOG.md` | **No** — file does not exist |
| Docs / Cookbook | `docs/COOKBOOK.md` | **No** — file does not exist |
| Samples | `samples/` directory | **No** — directory does not exist |
| Unit tests | `Ferreimavi.Specification.Tests/` | Yes — xUnit + FluentAssertions |
| EF integration tests | `Ferreimavi.Specification.EntityFrameworkCore.Tests/` | Yes — SQL Server–dependent |
| Core API | Specification.cs, builders, evaluators | Yes |
| Composition API | `CompositionParser`, `CompositionHelpers`, builders | Yes |
| Grouping API | `GroupingSpecification`, `IGroupingSpecification` | Yes |
| Ardalis.Specification (external) | nuget.org, github.com/ardalis/Specification | Yes — fetched; v9.3.1, 16.8M downloads |
| `main` branch state | Not inspected | **Unknown** |
| `.csproj` `PackageId` alignment | Not found in inspected files | **Unknown** |

---

## Confirmed Strengths

> These are Observed claims (direct evidence from files or fetched sources).

| # | Claim | Claim Type | Evidence |
|---|---|---|---|
| S1 | Parentheses-aware composition via `OpenGroup`/`CloseGroup` is absent from Ardalis.Specification | Observed | `IBaseComposableSpecificationBuilder<T>` in Builder/IBaseComposableSpecificationBuilder.cs; no equivalent found in Ardalis.Specification v9.3.1 repo |
| S2 | Three-dimension merge policy enums (`OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy`) are original | Observed | Common/PaginationEvaluationPolicy.cs, Common/ProjectionEvaluationPolicy.cs |
| S3 | `GroupingSpecification<T,TKey,TResult>` with in-memory parity exists and is functional | Observed | GroupingSpecification.cs, InMemorySpecificationEvaluator.cs |
| S4 | README Quickstart is realistic and non-trivial | Observed | README.md L82–L111 |
| S5 | MIT license present | Observed | LICENSE |
| S6 | XML doc coverage on public types is consistent | Observed | Inspected across multiple public API files |

---

## Findings

---

## Finding AMP-001 — No CI Workflow

| Field | Value |
|---|---|
| finding-id | AMP-001 |
| severity | Critical |
| confidence | High |
| go-live-relevance | Blocker |
| category | Trust / Build reliability |
| claim-type | Observed |
| blocks | AMP-003 (CI is prerequisite to unblocking EF test portability) |
| blocked-by | — |

### Raw Evidence

```
- No .github/workflows/ directory found in workspace.
- README.md (lines 195–210): build section contains placeholder clone URL:
  git clone https://github.com/your-org/Mango.Specifications.git
- README badge row: only "Last commit" badge present; no CI status badge.
```

### Interpretation

No automated build or test execution exists for this repository. The placeholder URL in the build section is direct evidence the repository has never been exercised as a fully public artifact. An external evaluator has no automated signal that the code compiles or tests pass on a neutral machine.

### Impact Statement

A library without CI cannot be trusted by any adopter. This is the minimum threshold for any open-source library evaluation. Its absence compounds every other finding because there is no automated regression gate.

### Local Remediation

1. Add `.github/workflows/ci.yml` running `dotnet build` and `dotnet test` on `push` and `pull_request`.
2. Correct the clone URL to `https://github.com/ferreXD/Mango.Specifications.git`.
3. Add a CI status badge to the README header.

### Orchestrator Notes

- All other trust findings are secondary to this one. CI is a prerequisite for AMP-003 resolution.
- No evidence of a local CI substitute (e.g., pre-commit hooks, local pipeline scripts) was found.

---

## Finding AMP-002 — NuGet Package Not Published; Internal Name Mismatch

| Field | Value |
|---|---|
| finding-id | AMP-002 |
| severity | Critical |
| confidence | High |
| go-live-relevance | Blocker |
| category | Distribution / Package identity |
| claim-type | Observed (not-published) / Observed (name mismatch) |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md (lines 62–67):
  # Coming soon on NuGet
  dotnet add package Mango.Specifications
  dotnet add package Mango.Specifications.EntityFrameworkCore
  > For now: project reference or local feed.

- Namespace in all source files: Mango.Specifications
- Project file names observed: Ferreimavi.Specification, Ferreimavi.Specification.EntityFrameworkCore
- No PackageId or Version elements found in any inspected .csproj content.
```

**Missing evidence:** No `.csproj` file content was retrieved that confirms or denies `PackageId` fields. The mismatch is inferred from project file naming conventions observable in file paths. The claim that `PackageId` is unset is `Inferred — Medium confidence`.

### Interpretation

The public-facing brand is `Mango.Specifications`. The internal project/assembly names use `Ferreimavi.Specification`. If published as-is without explicit `PackageId` properties, the NuGet packages would appear under `Ferreimavi.Specification`, making them undiscoverable to developers searching for `Mango.Specifications`. The library is currently not installable via `dotnet add package`.

### Impact Statement

Zero-friction installation is required for adoption consideration. A name mismatch is a branding and discoverability blocker even after publication.

### Local Remediation

1. Verify `.csproj` files for `PackageId` elements. If absent, add:
   - `<PackageId>Mango.Specifications</PackageId>`
   - `<PackageId>Mango.Specifications.EntityFrameworkCore</PackageId>`
2. Publish a pre-release package (e.g., `0.1.0-alpha.1`) to NuGet.org.
3. Add NuGet download and version badges to README.

### Orchestrator Notes

- The claim about `PackageId` being absent is **Inferred — Medium confidence**. A direct `.csproj` inspection could confirm or dismiss it. This is listed in Open Questions.
- Do not globally delay other remediations pending this check.

---

## Finding AMP-003 — EF Integration Tests Require a Specific Developer Machine

| Field | Value |
|---|---|
| finding-id | AMP-003 |
| severity | High |
| confidence | High |
| go-live-relevance | Blocker (for CI) / Pre-Go-Live (for external contributors) |
| category | Test infrastructure / Portability |
| claim-type | Observed |
| blocks | — |
| blocked-by | AMP-001 (must resolve CI before this is testable in automation) |

### Raw Evidence

```
- TestDbContext.cs (line 34):
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlServer(
         "Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;
          Integrated Security=True;Connect Timeout=30;Encrypt=True;
          Trust Server Certificate=True;Application Intent=ReadWrite;
          Multi Subnet Failover=False;");

- DbContextFactory.cs (lines 9–13):
  var options = new DbContextOptionsBuilder<TestDbContext>().Options;
  var context = new TestDbContext(options);
  // OnConfiguring overrides options unconditionally.
```

### Interpretation

`OnConfiguring` is called regardless of the options passed to the constructor. `DbContextFactory` passes empty options, but the `OnConfiguring` override replaces them with the hardcoded SQL Server string targeting `DESKTOP-SB85G0U`. These tests will fail on any machine that is not the author's workstation. The machine hostname `DESKTOP-SB85G0U` is exposed publicly in source.

**Security note (OWASP A05 — Security Misconfiguration, Low):** No credentials are exposed (Integrated Security is used). Machine hostname exposure is low severity but violates the principle of not embedding environment-specific configuration in source.

### Impact Statement

Any developer attempting `dotnet test` on a fresh clone will see integration test failures with a network/connection error. This silently signals broken tests rather than a portability issue. CI cannot pass without resolving this.

### Local Remediation

1. Move the connection string to an environment variable (`TEST_DB_CONNECTION`) with a fallback to `null`.
2. Guard the `OnConfiguring` override: only apply if the string is available. Otherwise skip (or use an in-memory provider for tests that don't require SQL Server–specific behaviour).
3. Alternatively: annotate SQL Server–specific tests with `[Trait("Category", "Integration")]` and skip them in CI via `dotnet test --filter "Category!=Integration"`.

### Orchestrator Notes

- The factory/context interaction was directly verified: the factory pattern does not override `OnConfiguring`. This finding is high confidence.
- Resolution of AMP-001 (CI) makes this finding immediately testable.

---

## Finding AMP-004 — CHANGELOG, Docs, and Samples Do Not Exist

| Field | Value |
|---|---|
| finding-id | AMP-004 |
| severity | High |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | Documentation |
| claim-type | Observed |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md (lines 44–49):
  _See [CHANGELOG](./CHANGELOG.md) for details (TODO)._

- README.md (lines 230–234):
  - Cookbook: docs/COOKBOOK.md
  - Samples: samples/Basic, samples/Composition, samples/Grouping (TODO)
  - API Reference: XML docs in packages (TODO)

- Workspace search: CHANGELOG.md — not found.
- Workspace search: docs/COOKBOOK.md — not found.
- Workspace search: samples/ directory — not found.
```

### Interpretation

Three of the four documentation artifacts cited in the README are stubs or absent. The README explicitly marks them as "(TODO)". There is no changelog, no policy cookbook, and no runnable sample.

### Impact Statement

The composition and policy API is the primary differentiator. Without a cookbook, the value of `OpenGroup/CloseGroup` and `ThrowOnConflict` is not accessible to a developer evaluating the library. The absence of a CHANGELOG makes version history assessment impossible, which is a standard due-diligence step for any library adoption.

### Local Remediation

1. Create `CHANGELOG.md` with a single entry documenting the current feature set under version `0.x.0-alpha` or equivalent.
2. Create `docs/COOKBOOK.md` with at minimum two end-to-end examples for the composition policy API.
3. Add one runnable console or test sample project under `samples/Composition/`.
4. Prioritize (2) over (3): the cookbook converts readers; samples are secondary.

### Orchestrator Notes

- This finding is independent of AMP-001/AMP-003 and can be resolved in parallel.
- The README value proposition is present; it is the supporting documentation that is absent.

---

## Finding AMP-005 — GroupBy EF Path Materializes Entire Result Set Before Pagination

| Field | Value |
|---|---|
| finding-id | AMP-005 |
| severity | High |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | Known limitation / Performance |
| claim-type | Observed |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- SpecificationEvaluator.cs (GetQuery<T,TKey,TResult>, line ~130):
  var groupedResults = await baseQuery
      .GroupBy(specification.GroupBySelector, effectiveSelector)
      .ToListAsync(cancellationToken);
  // Apply pagination if specified.
  var skip = specification.Skip ?? 0;
  [pagination applied in-memory to groupedResults]

- Code comment in same method:
  // TODO: If performance is a real concern in the future, consider using a
  // custom implementation of GroupBy that does not materialize the query.
```

### Interpretation

`GroupingSpecification` with EF Core executes `ToListAsync()` on the full grouped dataset before applying `Skip`/`Take`. Pagination is client-side. For large tables this produces: correct results, but O(n) memory usage and full table transfer regardless of page size.

**Separation:** The code comment acknowledges this as a known TODO, not an oversight. The finding is that this limitation is not surfaced to consumers in the README or XML docs.

### Impact Statement

Developers relying on `GroupingSpecification` with pagination in production against large datasets will experience silent performance degradation. The README grouping example (`OrdersByMonthSpec`) does not warn about this. This qualifies as an undocumented production risk.

### Local Remediation

1. Add a warning to the `GroupingSpecification<T,TKey,TResult>` XML doc:
   `/// <remarks>When used with EF Core, pagination (Skip/Take) is applied in-memory after full materialization. Avoid on large datasets until server-side pagination is implemented.</remarks>`
2. Add a "Known Limitations" section to the README with this callout.
3. Add the note inline in the README grouping example.

### Orchestrator Notes

- The performance impact is real but bounded by whether pagination is used. Teams using `GroupingSpecification` without `Skip`/`Take` are unaffected.
- Long-term fix (server-side group pagination) is out of scope for this finding's local remediation.

---

## Finding AMP-006 — No "When to Use / When Not to Use" Section

| Field | Value |
|---|---|
| finding-id | AMP-006 |
| severity | Medium |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | Positioning / Documentation |
| claim-type | Observed |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md table of contents: About, What's New, Features, Install, Quickstart,
  Composition & Policies, Grouping Example, EF Toggles, In-Memory vs EF Parity,
  Documentation, Building, Running Tests, Roadmap, Contributing, License & Credits.
  No "When to use" or "vs. Ardalis" section present.

- README.md (credits section, final lines):
  "Heavily inspired by Ardalis.Specification (MIT). Portions may conceptually derive
  from that work."
  [No comparison beyond this sentence]
```

### Interpretation

The README positions Mango relative to Ardalis only in a credits attribution line. There is no section explaining when to choose Mango over Ardalis, when raw LINQ is sufficient, or when neither library is appropriate. A developer evaluating both must infer this from the feature list.

**Separate claim (unverified):** The raw report asserts Ardalis.Specification v9.3.1 has no equivalent composition mechanism. This was corroborated by inspecting the Ardalis GitHub repo — no `OpenGroup`/`CloseGroup` API or policy enum equivalent was found. Confidence: **High**.

### Impact Statement

Without explicit differentiation guidance, the majority of developers who do not need composition policies have no reason to prefer Mango over a 16.8M-download library. The value proposition remains discoverable only to readers who study the full feature list.

### Local Remediation

Add a "When to Choose Mango" section to README. Suggested placement: after "About", before "Features". Suggested content:

- **Choose Mango when** you need: (1) parentheses-aware boolean composition across multiple specs, (2) deterministic policy resolution for ordering/pagination/projection conflicts, (3) a first-class `GroupBy` abstraction with in-memory parity.
- **Use Ardalis.Specification when** you want a mature, battle-tested, heavily adopted base with no composition opinion and extensive documentation.
- **Use raw EF LINQ when** your query logic is simple, per-feature, and does not require reuse or testability via in-memory evaluation.

### Orchestrator Notes

- This is the highest-leverage single documentation change for converting a skeptical evaluator.
- Do not fabricate download comparisons or NuGet ranking in the remediation text.

---

## Finding AMP-007 — README Build Clone URL Is a Placeholder

| Field | Value |
|---|---|
| finding-id | AMP-007 |
| severity | Medium |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | Documentation / Trust |
| claim-type | Observed |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md (Building section):
  git clone https://github.com/your-org/Mango.Specifications.git

- Repository metadata (attachment):
  Owner: ferreXD
  Repository: Mango.Specifications
  → Actual URL: https://github.com/ferreXD/Mango.Specifications
```

### Interpretation

The README clone URL points to a non-existent `your-org` organization. The correct URL is `https://github.com/ferreXD/Mango.Specifications`. This is a verbatim template placeholder that was never updated.

### Impact Statement

Minor individually. Disproportionately damaging to credibility because it signals the README was never proofread as a complete document. A developer following the build instructions will receive a 404 from `git clone`.

### Local Remediation

Replace `https://github.com/your-org/Mango.Specifications.git` with `https://github.com/ferreXD/Mango.Specifications.git` in the Building section.

### Orchestrator Notes

- Low effort. Zero ambiguity about correct value. Resolve alongside AMP-001.

---

## Finding AMP-008 — Vague Attribution Statement; Potential Legal Ambiguity

| Field | Value |
|---|---|
| finding-id | AMP-008 |
| severity | Medium |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | Positioning / Legal |
| claim-type | Observed (statement) / Inferred (legal risk) |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md (License & Credits section):
  "Credits: Heavily inspired by Ardalis.Specification (MIT). Portions may
  conceptually derive from that work."
```

### Interpretation

**Observed:** The attribution statement exists and uses the phrase "may conceptually derive." MIT license permits derivative works and does not require attribution beyond copyright notice preservation. However, the phrase "may conceptually derive" is ambiguous — it neither confirms nor denies copied source code.

**Inferred (Medium confidence):** Enterprise legal teams performing dependency screening may flag this ambiguity and request clarification before approving adoption. This is not a legal violation under MIT; it is a friction point in enterprise due-diligence workflows.

**Not claimed:** That any source code was copied. No such evidence was found.

### Impact Statement

Vague attribution creates unnecessary legal review overhead for enterprise adopters. A crisp, affirmative statement removes this friction.

### Local Remediation

Replace with: *"Architecture and interface design inspired by Ardalis.Specification (MIT). No source code was copied. Original work in this repository: composition engine (`CompositionParser`, `ComposableSpecificationBuilder`), merge policy enums and resolution logic, and `GroupingSpecification` abstraction."*

### Orchestrator Notes

- The legal risk claim is **Inferred**. There is no direct evidence of enterprise legal review being triggered. It is a plausible adoption barrier, not a confirmed one.
- The remediation also functions as a positioning statement that clarifies what is genuinely new in this library.

---

## Finding AMP-009 — API Surface Is Unstable Without Compile-Time Stability Markers

| Field | Value |
|---|---|
| finding-id | AMP-009 |
| severity | Medium |
| confidence | High |
| go-live-relevance | Pre-Go-Live |
| category | API stability |
| claim-type | Observed (status warning) / Observed (no stability attributes) |
| blocks | — |
| blocked-by | — |

### Raw Evidence

```
- README.md (status banner, line 8):
  > **Status:** WIP (pre-v1). API surface may change. Targeting `v1.0.0` after
  > docs polish & EF ergonomics.

- Inspected public API types: Specification<T>, Specification<T,TResult>,
  GroupingSpecification<T,TKey,TResult>, IComposableSpecificationBuilder<T>,
  IBaseComposableSpecificationBuilder<T,TResult>, all builder extensions.
  No [Experimental] attributes, no [Obsolete] guards, no stability tier markers found.
```

### Interpretation

The README correctly and honestly warns that the API may change. However, this warning exists only in prose. No public API types or members carry `[Experimental]` (C# 12) attributes or equivalent markers that would produce compile-time warnings when consumers call unstable APIs. Breaking changes would arrive silently.

**Not claimed:** That any specific API is unstable beyond the README's blanket warning. Which parts are considered stable vs. provisional is unknown.

### Impact Statement

Pre-v1 status is acceptable for exploratory adoption. Without compile-time stability markers, consumers cannot distinguish stable from provisional surfaces, and breaking changes cannot be graduated incrementally.

### Local Remediation

1. Define a public stability contract: identify which types/members are considered stable (e.g., `Specification<T>`, `ISpecification<T>`) vs. provisional (e.g., composition builder fluent chain).
2. Apply `[Experimental("MANGO001", UrlFormat = "...")]` to provisional APIs in .NET 8+.
3. Update the README Roadmap section to explicitly list what is IN and OUT of scope for v1.0.0.

### Orchestrator Notes

- The v1.0.0 scope boundary is absent from the Roadmap. The roadmap lists features but does not define a freeze boundary. This is a distinct gap from the stability attribute gap but is addressed by the same remediation step (3).
- Do not produce the final roadmap as part of this audit normalization.

---

## Finding AMP-010 — No Test Coverage Signal

| Field | Value |
|---|---|
| finding-id | AMP-010 |
| severity | Low |
| confidence | High |
| go-live-relevance | Post-Go-Live |
| category | Test quality |
| claim-type | Observed |
| blocks | — |
| blocked-by | AMP-001 (coverage reporting depends on CI) |

### Raw Evidence

```
- Workspace search: no .coveragerc, no Coverlet configuration in .csproj content,
  no codecov.yml or similar found.
- README.md: no coverage badge present.
- Test files observed: ComposableProjectableSpecificationTests.cs,
  OrSpecificationTests.cs, OrderBySpecificationTests.cs,
  SpecificationGroupOperationsTests.cs, GroupBySpecificationTests.cs,
  PaginationSpecificationTests.cs, ProjectableSpecificationReadRepositoryTests.cs,
  BaseSpecificationReadRepositoryTests.cs, ComposableSpecificationReadRepositoryTests.cs,
  GroupingSpecificationReadRepositoryTests.cs.
```

**Missing evidence:** Actual coverage percentage is unknown. No coverage report was run as part of this audit.

### Interpretation

Tests exist and are structurally reasonable. Both unit (in-memory) and integration (EF) test categories are present. However, no coverage report configuration or published coverage signal exists. External adopters cannot assess coverage without running it themselves.

### Impact Statement

Low severity individually; becomes relevant during due-diligence for library selection. An adopter cannot make a coverage-informed decision without running the test suite, which is itself blocked by AMP-003.

### Local Remediation

1. After AMP-001 is resolved, add Coverlet to test project `.csproj` files.
2. Generate and publish a coverage report (Codecov, Coveralls, or GitHub Actions artifact).
3. Add a coverage badge to the README.

### Orchestrator Notes

- This finding is dependent on AMP-001 and partially on AMP-003 (integration tests need a portable database to contribute coverage).
- Do not treat as a blocker. It is a post-go-live quality signal.

---

## Open Questions — Missing Evidence

| # | Item | Status | Impact on Findings |
|---|---|---|---|
| OQ-1 | `CHANGELOG.md` content | Does not exist | AMP-004 confirmed |
| OQ-2 | `docs/COOKBOOK.md` content | Does not exist | AMP-004 confirmed |
| OQ-3 | `samples/` directory content | Does not exist | AMP-004 confirmed |
| OQ-4 | `.github/workflows/` content | Not found | AMP-001 confirmed |
| OQ-5 | `PackageId` / `Version` in `.csproj` files | Not inspected directly | AMP-002 is **Inferred — Medium confidence** pending this check |
| OQ-6 | `main` branch state vs. `develop` | Not inspected | All findings may differ on `main` |
| OQ-7 | Ardalis.Specification v9.3.1 composition features | Fetched — no `OpenGroup`/`CloseGroup` found | S1 differentiation claim confidence: High |
| OQ-8 | Actual test coverage percentage | Not measured | AMP-010 scope limitation |
| OQ-9 | Whether `DbContextFactory` was designed to be overridden for portability | No evidence found | AMP-003 confidence remains High |
| OQ-10 | GitHub Issues / PRs tracking v1 scope | Cannot determine from workspace | AMP-009 remediation step (3) may already be in-progress |

---

## Recommended Verification Checks

> Ordered by confidence gain per effort. Not a global roadmap.

| Check | Finding(s) Affected | Confidence Gain |
|---|---|---|
| Inspect all `.csproj` files for `PackageId` elements | AMP-002 | Converts Inferred → Observed |
| Inspect `main` branch for CI, CHANGELOG, docs, samples | AMP-001, AMP-002, AMP-004, AMP-007 | May resolve multiple findings or confirm they apply to both branches |
| Run `dotnet test` on a clean machine without SQL Server | AMP-003 | Confirms failure mode and error message |
| Inspect Ardalis.Specification v9.3.1 source for composition APIs | AMP-006 (S1) | Confirms or weakens differentiation claim |
| Inspect docs directory on `develop` for any partial content | AMP-004 | May find in-progress docs not surfaced by search |