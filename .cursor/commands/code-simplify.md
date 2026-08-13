---
description: Simplify code for clarity and maintainability — reduce complexity without changing behavior
---

Read and follow `.cursor/skills/code-simplification/SKILL.md`.

Simplify recently changed code (or the specified scope) while preserving exact behavior:

1. Read `.cursor/rules/project-coding-standards.mdc` and `.cursor/rules/project-stack.mdc`
2. Identify the target code — recent changes unless a broader scope is specified
3. Understand purpose, callers, edge cases, and test coverage before touching it
4. Scan for simplification opportunities (nesting, long functions, duplication, dead code)
5. Apply each simplification incrementally — verify after each change
6. Confirm behavior is unchanged and the diff is scoped

If verification fails after a simplification, revert that change and reconsider.
