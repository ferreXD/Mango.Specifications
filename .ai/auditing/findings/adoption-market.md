# Raw Audit Result — Adoption Readiness & Market Positioning

## Executive Verdict

**Do not adopt before v1.** The library contains a genuine, narrow differentiation over Ardalis.Specification (parentheses-aware composition with explicit policies + first-class GroupBy parity). The feature implementation is real and non-trivial. However, the repository is missing nearly every trust signal a prospective adopter checks before wiring a library into production: no published NuGet package, no CI, no CHANGELOG, no standalone docs, no samples, no comparison section, hardcoded developer-machine connection strings in test infrastructure, and placeholder text in the README build command. The idea is credible; the packaging is not.

---

## Scope Inspected

| Area | Files / Evidence | Notes |
|---|---|---|
| README | README.md | Present, structured, feature-complete headers |
| License | LICENSE | MIT, present, 2025 ferreXD |
| CI/Workflows | `.github/workflows/` | **Not found** in workspace |
| NuGet metadata | `*.csproj` package fields | Not observed; internal name is `Ferreimavi.Specification` |
| CHANGELOG | `CHANGELOG.md` | **Does not exist**; README links to it as "(TODO)" |
| Docs / Cookbook | `docs/COOKBOOK.md` | **Does not exist**; README says "(TODO)" |
| Samples | `samples/` | **Does not exist**; README says "(TODO)" |
| Unit tests | `Ferreimavi.Specification.Tests/` | Present — xUnit + FluentAssertions |
| Integration tests (EF Core) | `Ferreimavi.Specification.EntityFrameworkCore.Tests/` | Present — tied to developer SQL Server |
| Core API | Specification.cs, builders, evaluators | Inspected |
| Composition API | `CompositionParser`, `CompositionHelpers`, builders | Inspected |
| Grouping API | `GroupingSpecification`, `IGroupingSpecification` | Inspected |
| Ardalis.Specification (external) | nuget.org, github.com/ardalis/Specification | Fetched — v9.3.1, 16.8M downloads |

---

## Strengths

1. **Parentheses-aware composition is a real gap.** `OpenGroup/CloseGroup` with `ChainingType` solves explicit operator precedence in boolean spec composition. Ardalis.Specification has no equivalent mechanism. This is the strongest reason for Mango to exist.

2. **Policy-driven merge semantics are original.** The three-dimensional policy table (`OrderingEvaluationPolicy`, `PaginationEvaluationPolicy`, `ProjectionEvaluationPolicy`) with options like `ThrowOnConflict`, `BothLeftPriority`, and `None` provides deterministic, debuggable behaviour when composing specs that independently configure these concerns. This is not in the ecosystem.

3. **First-class `GroupingSpecification` with in-memory parity.** `GroupingSpecification<T, TKey, TResult>` works identically in LINQ and EF. Ardalis.Specification has no grouping abstraction. This eliminates a common source of spec parity gaps.

4. **README quality is above average for pre-v1.** Quickstart is realistic (`RecentHighValueOrdersSpec` covers filters, includes, ordering, pagination, projection in one example). The composition policy table is clear. Gotchas are mentioned.

5. **MIT license present.** No ambiguity about use rights.

6. **XML doc coverage** on public types is consistent.

---

## Findings

### Finding 1 — No CI Workflow

- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Trust / Build reliability
- **Evidence:** No `.github/workflows/` directory found. README has no CI badge (only a "last commit" badge). README build URL contains placeholder text: `git clone https://github.com/your-org/Mango.Specifications.git`.
- **Observation:** There is no automated build, no automated test run, and no green-check badge. Any contributor or adopter has no confidence that the codebase compiles on a clean machine.
- **Why it matters:** A library with no CI is effectively unverifiable by an external party. The README build URL being a placeholder signals the repo has never been fully exercised as a public artifact.
- **Suggested remediation:** Add a `.github/workflows/ci.yml` that runs `dotnet build` and `dotnet test` on push/PR. Fix the clone URL. Add a CI status badge.
- **Orchestrator notes:** This is the single highest-priority blocker. Nothing else matters to a first-time adopter if the build is unverified.

---

### Finding 2 — NuGet Package Not Published; Internal Name Mismatch

- **Severity:** Critical
- **Confidence:** High
- **Go-Live Relevance:** Blocker
- **Category:** Distribution / Package identity
- **Evidence:** README install section: `# Coming soon on NuGet / For now: project reference or local feed.` Internal `.csproj` names observed are `Ferreimavi.Specification` and `Ferreimavi.Specification.EntityFrameworkCore`, while the public brand is `Mango.Specifications`.
- **Observation:** The library is unavailable for standard `dotnet add package` installation. The package identity shown to consumers does not match the public name. This is a functional and branding blocker.
- **Why it matters:** Adoption requires zero-friction installation. The internal csproj naming also creates confusion if it is ever published as-is — a developer searching "Mango.Specifications" on NuGet.org will find nothing.
- **Suggested remediation:** Align `PackageId` properties in all `.csproj` files to `Mango.Specifications` and `Mango.Specifications.EntityFrameworkCore`. Publish at least a pre-release package to NuGet. Add NuGet badges to README.
- **Orchestrator notes:** Package identity should be resolved before any external promotion.

---

### Finding 3 — EF Integration Tests Require a Specific Developer Machine

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Blocker (for CI) / Pre-Go-Live (for adopters)
- **Category:** Test infrastructure / Portability
- **Evidence:** TestDbContext.cs: `optionsBuilder.UseSqlServer("Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;Integrated Security=True;...")`
- **Observation:** The `OnConfiguring` override hard-codes a connection string pointing to a specific machine (`DESKTOP-SB85G0U`) with `AdventureWorks2022`. These tests cannot run on any other machine, including CI. The `DbContextFactory` creates the context with empty options but `OnConfiguring` overrides them unconditionally.
- **Why it matters:** Any adopter attempting `dotnet test` will see integration test failures. CI will fail unless the string is replaced. It also exposes the author's internal machine name in the repository.
- **Suggested remediation:** Move the connection string to an environment variable or `appsettings.json` excluded from source control. Use SQLite or SQL Server LocalDB for CI. Alternatively, skip integration tests in environments without the database using `[Trait]` + a custom test filter.
- **Orchestrator notes:** This is also a minor information disclosure (OWASP A05 — Security Misconfiguration) — machine hostnames in source are low severity but best avoided. No credentials are exposed because Integrated Security is used.

---

### Finding 4 — CHANGELOG, Docs, and Samples Do Not Exist

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Documentation
- **Evidence:** README links `./CHANGELOG.md` with "(TODO)"; `docs/COOKBOOK.md` listed as reference with "(TODO)"; `samples/Basic`, `samples/Composition`, `samples/Grouping` listed with "(TODO)".
- **Observation:** Three out of four documentation artifacts referenced in the README are stubs. The README itself acknowledges this. There is no version history, no upgrade guide, no cookbook for composition policies, and no runnable samples.
- **Why it matters:** The composition and policy API is the core differentiator. Without a cookbook, the value proposition of `OpenGroup/CloseGroup` and `ThrowOnConflict` pagination policy is not accessible to new users. The absence of a CHANGELOG makes it impossible to assess stability over time.
- **Suggested remediation:** Prioritize the composition policy cookbook (`docs/COOKBOOK.md`) over new features. Add at minimum one runnable sample project for composition. Create a minimal CHANGELOG with the current feature set as `v0.x.0`.
- **Orchestrator notes:** The README value proposition is present but the proof-of-concept docs that would convert a curious reader into an adopter are missing.

---

### Finding 5 — GroupBy EF Path Materializes Entire Result Set Before Pagination

- **Severity:** High
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Known limitation / Performance
- **Evidence:** SpecificationEvaluator.cs: `var groupedResults = await baseQuery.GroupBy(...).ToListAsync(...)` followed by in-memory pagination. Code comment: `// TODO: If performance is a real concern in the future, consider using a custom implementation...`
- **Observation:** `GroupingSpecification` with EF Core materializes the full grouped dataset into memory before applying `Skip`/`Take`. For large tables this is a silent correctness issue masquerading as functionality.
- **Why it matters:** Users will get correct results on small datasets but silently wrong performance at scale. This limitation is not documented in the README. It qualifies as an undocumented breaking limitation for production use.
- **Suggested remediation:** Document this limitation explicitly in the README and in the class-level XML doc. Add a note in the grouping example: "pagination on grouped results is applied in-memory." Long-term: investigate whether EF can translate group-then-paginate for simple cases.
- **Orchestrator notes:** This finding also reveals that the README's grouping example (`OrdersByMonthSpec`) does not warn about the materialization cost.

---

### Finding 6 — No "When to Use / When Not to Use" Section

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Positioning / Documentation
- **Evidence:** README contains "About", "Features", "Quickstart" but no section explaining who should choose Mango over Ardalis, or when raw LINQ is the right answer.
- **Observation:** The README says "Heavily inspired by Ardalis.Specification" in the credits section but has no explicit comparison or positioning statement. A developer evaluating both cannot find the answer to the core adoption question from within the repository.
- **Why it matters:** Without explicit differentiation guidance, developers who don't need composition policies (the majority of CRUD services) have no reason to pick Mango over Ardalis's 16.8M-download library. The value proposition gets buried.
- **Suggested remediation:** Add a "When to choose Mango vs Ardalis" section to the README. Suggested framing: choose Mango when you need (1) parentheses-aware boolean composition across multiple specs, (2) deterministic policy resolution for ordering/pagination/projection conflicts, or (3) a first-class GroupBy abstraction with in-memory parity. Use Ardalis when you want a battle-tested, heavily adopted, well-documented base with no opinion on composition.
- **Orchestrator notes:** This is the single most impactful documentation change for converting a skeptical developer.

---

### Finding 7 — README Build Clone URL Is a Placeholder

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Documentation / Trust
- **Evidence:** README.md: `git clone https://github.com/your-org/Mango.Specifications.git`
- **Observation:** The actual repository is `github.com/ferreXD/Mango.Specifications` (per the attachment metadata), but the README instructs contributors to clone a non-existent `your-org` URL.
- **Why it matters:** Minor but disproportionately damaging to credibility. It signals the README was never proofread end-to-end.
- **Suggested remediation:** Replace with the actual URL.
- **Orchestrator notes:** Low effort, high signal for trust.

---

### Finding 8 — No Comparison with Ardalis.Specification; Potential Legal Ambiguity

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** Positioning / Legal
- **Evidence:** README credits section: "Heavily inspired by Ardalis.Specification (MIT). Portions may conceptually derive from that work."
- **Observation:** The phrase "portions may conceptually derive" is vague. MIT allows derivative works freely, but teams with IP policies may pause at ambiguous language. There is no explicit statement about which parts are original.
- **Why it matters:** Enterprise adopters routinely screen library licenses and attribution. Vague attribution can trigger legal review delays.
- **Suggested remediation:** Replace with a crisp statement: "Architecture and interface design inspired by Ardalis.Specification (MIT). No source code copied. Original work: composition engine, merge policies, grouping abstraction."
- **Orchestrator notes:** This also serves as a positioning statement that clarifies what is genuinely new.

---

### Finding 9 — API Surface Is Unstable and Explicitly Flagged

- **Severity:** Medium
- **Confidence:** High
- **Go-Live Relevance:** Pre-Go-Live
- **Category:** API stability
- **Evidence:** README status banner: "WIP (pre-v1). API surface may change. Targeting v1.0.0 after docs polish & EF ergonomics."
- **Observation:** The status warning is visible and honest. However, the library has no Experimental/Preview attributes or obsoletion guards on any public APIs to guide consumers about what is stable vs. provisional. Breaking changes would not be warned at compile time.
- **Why it matters:** Pre-v1 is acceptable for an exploratory adopter but not for a team evaluating production adoption.
- **Suggested remediation:** Identify the core stable API surface (`Specification<T>`, `Specification<T, TResult>`, `GroupingSpecification`) and mark provisional APIs with `[Experimental]` (C# 12) or XML doc warnings. Define the v1.0.0 scope boundary explicitly in the README Roadmap.
- **Orchestrator notes:** The roadmap exists but does not define what is IN or OUT of scope for v1.

---

### Finding 10 — No Coverage Signal

- **Severity:** Low
- **Confidence:** High
- **Go-Live Relevance:** Post-Go-Live
- **Category:** Test quality
- **Evidence:** No coverage report, badge, or configuration found. No `.coveragerc`, no Coverlet config in `.csproj` files, no CodeCov or similar badge in README.
- **Observation:** Tests exist and are reasonably structured. In-memory unit tests (`ComposableProjectableSpecificationTests`, `OrSpecificationTests`) and EF integration tests (blocked by connection string issue above) are both present. But there is no measurable coverage signal.
- **Why it matters:** Low confidence adopters cannot assess coverage without running it themselves.
- **Suggested remediation:** Add Coverlet and a coverage report step to CI once CI exists. Add a badge.
- **Orchestrator notes:** Lower priority than CI itself.

---

## Missing Evidence / Open Questions

| Item | Status |
|---|---|
| `CHANGELOG.md` | Does not exist |
| `docs/COOKBOOK.md` | Does not exist |
| `samples/` directory | Does not exist |
| `.github/workflows/` | Not found in workspace |
| `PackageId` / `Version` in `.csproj` | Not found — package naming appears unresolved |
| `CONTRIBUTING.md` (standalone) | Not found — only a section in README |
| docs folder content beyond COOKBOOK stub | Unknown |
| Does `DbContextFactory` override fix the hardcoded string in tests? | Factory creates context with empty options but `OnConfiguring` overrides unconditionally — EF tests require the specific SQL Server |
| What is the current `develop` vs `main` diff? | Unknown — audit is on `develop` branch |
| Are there GitHub Issues or PRs tracking v1 scope? | Cannot determine from workspace alone |

---

## Recommended Next Checks

1. **Verify `main` branch state** — the audit was run on `develop`. Confirm whether `main` is behind or ahead and whether any of the blockers above are already resolved there.
2. **Run `dotnet test` on a clean machine** to confirm exactly which tests fail and what the failure mode is for the hardcoded connection string.
3. **Search for any `.csproj` `PackageId` property** — if it exists and resolves the `Ferreimavi.Specification` vs `Mango.Specifications` naming, that removes Finding 2 partially.
4. **Check whether `docs/COOKBOOK.md` exists in any branch or stash** — it may be in-progress.
5. **Fetch Ardalis.Specification documentation** on composition (specifically: does v9+ add any `And/Or` composition or policy features?) to confirm Mango's differentiation is still valid vs. the current Ardalis release.
6. **Evaluate whether the grouping materialization issue** (Finding 5) is acceptable for the target use cases, or whether a known-limitation disclaimer is sufficient for v1.