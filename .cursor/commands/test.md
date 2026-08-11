---
description: Run TDD workflow — write failing tests, implement, verify. For bugs, use the Prove-It pattern.
---

Read and follow `.cursor/skills/test-driven-development/SKILL.md`.
See `.cursor/references/testing-patterns.md` for principles; this repo uses Unity Test Framework + Lua.

For new features:
1. Write tests that describe the expected behavior (they should FAIL)
2. Implement the code to make them pass
3. Refactor while keeping tests green

For bug fixes (Prove-It pattern):
1. Write a test that reproduces the bug (must FAIL)
2. Confirm the test fails
3. Implement the fix
4. Confirm the test passes
5. Run the relevant suite for regressions

Unity: drive tests via `unity-pipeline` (`run_tests`). Browser-only issues may also use `browser-testing-with-devtools`.
