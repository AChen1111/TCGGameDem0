---
description: Implement tasks incrementally — build, test, verify, commit. Add "auto" to run the whole plan in one approved pass.
---

Read and follow `.cursor/skills/incremental-implementation/SKILL.md` and `.cursor/skills/test-driven-development/SKILL.md`. Also apply `project-coding-standards`.

## Modes

- **`/build`** — implement the *next* pending task, then stop.
- **`/build auto`** — generate the plan if needed, get a single approval, then implement *every* task without stopping between them.

`$ARGUMENTS` selects the mode. Treat `auto` (canonical) or `all` as autonomous mode; anything else (or empty) is the default single-task mode.

## Default: one task

Pick the next pending task from the plan. Then:

1. Read the task's acceptance criteria
2. Load relevant context (existing code, patterns, types)
3. Write a failing test for the expected behavior (RED)
4. Implement the minimum code to pass the test (GREEN)
5. Run the relevant tests (Unity: `unity-pipeline` `run_tests`) and check for regressions
6. Verify compilation (`recompile` / console errors)
7. Commit with a descriptive message **only if the user asked to commit**
8. Mark the task complete and stop

## Autonomous: the whole plan (`/build auto`)

1. **Require a spec** at `SPEC.md`, `docs/SPEC.md`, or `spec/`. A README does not count. If none exists, stop and tell the user to run `/spec` first.
2. **Clean baseline.** If `git status --porcelain` shows uncommitted changes outside `SPEC.md`, `docs/SPEC.md`, `spec/*`, `tasks/plan.md`, `tasks/todo.md`, stop and ask.
3. **Plan if needed.** If there is no `tasks/plan.md`, follow `planning-and-task-breakdown`.
4. **Single checkpoint.** Present the full plan and wait for an unambiguous affirmative. Hedged responses are not approval.
5. **Execute every task** in dependency order with the default loop above. One commit per task **only if the user already approved committing**.
6. **Stop and ask** on test/build failure, spec ambiguity, or irreversible/high-risk steps.
7. **Summarize** completed tasks, tests, commits, and anything skipped.

If any step fails, follow `.cursor/skills/debugging-and-error-recovery/SKILL.md`.
