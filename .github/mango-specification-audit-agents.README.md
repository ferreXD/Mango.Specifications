# Mango.Specification Audit Agents

Drop this `.github` folder into the root of your Mango.Specification repository.

## Contents

```text
.github/
  agents/
    mango-code-quality-auditor.agent.md
    mango-architecture-auditor.agent.md
    mango-dx-auditor.agent.md
    mango-adoption-market-auditor.agent.md
    mango-go-live-orchestrator.agent.md
  skills/
    audit-result-normalizer/
      SKILL.md
```

## Intended Workflow

1. Run each auditor agent independently:
   - `Mango Code Quality Auditor`
   - `Mango Architecture Auditor`
   - `Mango DX Auditor`
   - `Mango Adoption Market Auditor`

2. For each raw audit result, run the `audit-result-normalizer` skill.

3. Paste the normalized audit results into `Mango Go-Live Orchestrator`.

4. Use the orchestrator output to create the actual refinement roadmap.

## Usage Prompt for Auditor Agents

```markdown
Audit the current Mango.Specification repository from your assigned perspective.

Be evidence-first.
Inspect the actual codebase.
Do not modify files.
Do not create the final roadmap.
Return the required raw audit result format.
```

## Usage Prompt for the Normalizer Skill

```markdown
Use the audit-result-normalizer skill.

Normalize the following raw audit report into the strict orchestrator-ready format.

Hard rules:
- Do not invent findings.
- Do not create the final roadmap.
- Preserve negative conclusions.
- Separate evidence from interpretation.
- Mark unsupported claims as low-confidence.

Audit perspective:
[PASTE PERSPECTIVE]

Raw audit report:
[PASTE RAW AUDIT]
```

## Usage Prompt for the Orchestrator

```markdown
Synthesize the following normalized audit reports into a go-live readiness verdict and refinement roadmap.

Do not invent findings.
Deduplicate overlapping findings.
Prioritize correctness and API stability before DX polish.
Include a do-not-do list.

Normalized reports:
[PASTE NORMALIZED REPORTS]
```

## Important Notes

These agents are intentionally read-only. They use codebase/search-style tools and do not include edit tools.

If your Copilot environment uses different tool names, VS Code should ignore unavailable tools, but you may need to adjust the `tools` frontmatter manually.
