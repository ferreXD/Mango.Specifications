---
name: audit-result-normalizer
description: Normalize free-form Mango.Specification audit reports into strict orchestrator-ready markdown without inventing findings.
---

# Audit Result Normalizer Skill

## Purpose

Normalize a free-form audit report into a strict, orchestrator-ready markdown structure.

This skill is used after an audit agent has produced a raw analysis for one perspective, such as:

- Code Quality & Correctness
- Architecture & Package Boundaries
- Developer Experience & API Design
- Adoption Readiness & Market Positioning

The output must be suitable for a later orchestrator synthesis step.

The skill must not invent findings, upgrade certainty, add unsupported claims, or perform roadmap synthesis.

---

## Core Responsibility

Transform messy audit output into a consistent structure:

1. Preserve all meaningful findings.
2. Deduplicate obvious repetitions.
3. Normalize severity, confidence, and go-live relevance.
4. Separate evidence from interpretation.
5. Surface missing evidence and open questions.
6. Keep recommendations local to each finding.
7. Avoid global prioritization across audits.

---

## Non-Goals

Do not:

- Produce the final roadmap.
- Rank findings across different audit perspectives.
- Decide the final go-live verdict for the whole project.
- Add new findings not present in the source audit.
- Rewrite weak evidence into strong evidence.
- Pretend something is confirmed if the audit only implies it.
- Merge unrelated findings just because they seem thematically similar.
- Remove uncomfortable or negative conclusions.

If the source audit is vague, preserve that vagueness as a confidence issue.

---

## Output Format

Produce exactly this structure:

```markdown
# Normalized Audit Result — [Audit Perspective]

## 1. Audit Metadata

| Field | Value |
| --- | --- |
| Audit Perspective |  |
| Source Quality | Strong / Moderate / Weak |
| Normalization Confidence | High / Medium / Low |
| Main Risk Area |  |
| Go-Live Sensitivity | High / Medium / Low |

---

## 2. Executive Verdict

[Short verdict based only on the provided audit.]

### Verdict Level

One of:

- Strong
- Mostly Strong
- Mixed
- Weak
- Not Enough Evidence

### Summary

[2-4 concise paragraphs explaining the audit’s overall signal.]

---

## 3. Strengths

| Strength | Evidence / Basis | Go-Live Impact |
| --- | --- | --- |
|  |  |  |

---

## 4. Findings

### Finding 1 — [Clear Finding Title]

| Field | Value |
| --- | --- |
| Severity | Critical / High / Medium / Low |
| Confidence | High / Medium / Low |
| Go-Live Relevance | Blocker / Pre-Go-Live / Post-Go-Live / Optional |
| Category | Correctness / Architecture / DX / Documentation / Testing / Market / Maintainability / Performance / Security / Other |

#### Observation

[What the audit observed.]

#### Evidence

[Concrete evidence from the audit. If the audit provides no concrete evidence, write: "No concrete evidence provided in source audit."]

#### Why It Matters

[Impact on correctness, maintainability, DX, adoption, or trust.]

#### Suggested Remediation

[Local remediation only. Do not create a global roadmap.]

#### Orchestrator Notes

[What the orchestrator should consider when prioritizing this finding.]

---

## 5. Duplicated / Merged Findings

| Original Theme | Merged Into | Reason |
| --- | --- | --- |
|  |  |  |

---

## 6. Open Questions

| Question | Why It Matters | Suggested Follow-Up |
| --- | --- | --- |
|  |  |  |

---

## 7. Missing Evidence

| Missing Evidence | Impact on Confidence | Recommended Check |
| --- | --- | --- |
|  |  |  |

---

## 8. Local Recommendations

These are recommendations from this audit perspective only. They are not the final roadmap.

| Recommendation | Related Finding | Urgency | Notes |
| --- | --- | --- | --- |
|  |  | Critical / High / Medium / Low |  |

---

## 9. Orchestrator Input Summary

### Top Signals

- 
- 
- 

### Potential Blockers

- 
- 
- 

### Pre-Go-Live Candidates

- 
- 
- 

### Post-Go-Live Candidates

- 
- 
- 

### Do Not Overreact To

- 
- 
- 
```

---

## Severity Rules

### Critical

Use when the finding may make the library unsafe, incorrect, misleading, or unfit for release.

Examples:

- incorrect query results
- broken expression composition
- EF evaluator behavior contradicts documented behavior
- tests giving false confidence
- public API design that would require breaking changes immediately after release
- security-sensitive bug if applicable

### High

Use when the finding strongly affects trust, maintainability, adoption, or correctness but does not obviously block all release.

Examples:

- weak test coverage around important behavior
- unclear package boundaries
- leaky EF-specific concepts in core
- confusing fluent API that can cause misuse
- missing documentation for critical behavior

### Medium

Use when the issue matters but can reasonably be handled after an initial release if documented.

Examples:

- incomplete examples
- rough naming
- limited samples
- missing convenience overloads
- minor API consistency issues

### Low

Use for polish, nice-to-have improvements, or non-urgent cleanup.

---

## Confidence Rules

### High Confidence

Use when the source audit provides concrete evidence, such as:

- code references
- test examples
- observed behavior
- API examples
- package structure references
- direct comparison points

### Medium Confidence

Use when the audit gives plausible reasoning but limited evidence.

### Low Confidence

Use when the audit is speculative, generic, or unsupported.

Do not raise confidence just because the finding sounds reasonable.

---

## Go-Live Relevance Rules

### Blocker

Use when shipping without addressing the issue would likely damage trust or require immediate breaking changes.

### Pre-Go-Live

Use when the issue should ideally be fixed before v1, but does not completely prevent release.

### Post-Go-Live

Use when the issue is valid but can safely become backlog after release.

### Optional

Use when the improvement is mostly polish or strategic experimentation.

---

## Evidence Handling

Always distinguish between:

- what the audit directly observed
- what the audit inferred
- what remains unknown

Use explicit wording:

- "The source audit directly states..."
- "The source audit implies..."
- "The source audit does not provide enough evidence to confirm..."
- "This should be verified before being treated as a blocker..."

Never turn assumptions into facts.

---

## Deduplication Rules

Merge findings only when they clearly describe the same underlying issue.

Do not merge findings just because they belong to the same theme.

Example:

These may be related but should usually remain separate:

- "Public API has too many overloads"
- "EF-specific options leak into core abstractions"
- "README does not explain evaluator behavior"

They all affect DX, but they are different problems.

---

## Recommendation Rules

Recommendations must be local and tied to findings.

Good:

> Add tests proving AND/OR/NOT grouping precedence across EF and in-memory evaluators.

Bad:

> Improve the whole testing strategy.

Good:

> Move EF-only flags behind an EF adapter-specific options mechanism.

Bad:

> Clean up architecture.

---

## Handling Weak Source Audits

If the source audit is too vague, do not fabricate structure.

Instead:

1. Set `Source Quality` to `Weak`.
2. Set `Normalization Confidence` to `Low`.
3. Preserve useful claims as low-confidence findings.
4. Add missing evidence entries.
5. Recommend a follow-up audit with stricter evidence requirements.

---

## Final Quality Bar

Before returning the normalized result, verify:

- Every finding has severity.
- Every finding has confidence.
- Every finding has go-live relevance.
- Every finding has evidence or explicitly says evidence is missing.
- No final roadmap has been created.
- No global prioritization across audits has been performed.
- No unsupported claim has been strengthened.
- The output can be pasted directly into an orchestrator synthesis prompt.
