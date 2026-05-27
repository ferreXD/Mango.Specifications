
Use the `audit-result-normalizer` skill.

Normalize the following free-form audit report into the strict orchestrator-ready format defined by the skill.

Hard rules:

- Do not invent findings.
- Do not create the final roadmap.
- Do not globally prioritize across audits.
- Preserve negative conclusions.
- Separate evidence from interpretation.
- Mark unsupported claims as low-confidence.
- If evidence is missing, say so explicitly.
- Keep recommendations local to each finding.

Audit perspective:

[Code Quality & Correctness / Architecture & Package Boundaries / Developer Experience & API Design / Adoption Readiness & Market Positioning]

Raw audit report will be provided as an attached markdown file.