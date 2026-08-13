---
description: Pre-launch checklist via parallel personas, then a go/no-go decision
---

Read and follow `.cursor/skills/shipping-and-launch/SKILL.md`.
Also use `.cursor/references/definition-of-done.md`.

`/ship` is a fan-out orchestrator. Run three personas in parallel, then merge into one go/no-go with a rollback plan.

## Phase A — Parallel fan-out

Launch three subagents in **one** turn (sequential calls defeat the purpose):

1. **code-reviewer** — follow `.cursor/rules/persona-code-reviewer.mdc`
2. **security-auditor** — follow `.cursor/rules/persona-security-auditor.mdc`
3. **test-engineer** — follow `.cursor/rules/persona-test-engineer.mdc`

Personas do not invoke other personas. Each returns only its report.

**Skip fan-out** only if the change touches ≤2 files, the diff is under 50 lines, **and** it does not touch auth, payments, data access, or config/env.

## Phase B — Merge

Synthesize in the main session:

1. Code quality (Critical/Important + build/test failures)
2. Security (Critical/High become launch blockers)
3. Performance (from reviewer axis; this is a Unity project, not CWV)
4. Docs / rollback / monitoring gaps

## Phase C — Decision

```markdown
## Ship Decision: GO | NO-GO

### Blockers
### Recommended fixes
### Acknowledged risks
### Rollback plan
### Specialist reports
```

Critical finding → default **NO-GO** unless the user explicitly accepts the risk. Rollback plan is mandatory before GO.
