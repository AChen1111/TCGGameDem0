---
description: Run a web performance audit via the web-performance-auditor persona
---

Read and follow `.cursor/rules/persona-web-performance-auditor.mdc` and `.cursor/skills/performance-optimization/SKILL.md`.

`/webperf` targets **web** applications. This repo is a Unity project — do not run a CWV audit unless the user is explicitly reviewing browser-facing UI. For gameplay/runtime perf, use `performance-optimization` in a Unity context instead.

## Mode

- **Deep** — Lighthouse/PSI/CrUX/DevTools artifacts or Chrome DevTools MCP available
- **Quick** — default; source scan only; every finding is `potential impact`

Never fabricate metrics. Return the auditor report as-is; no merge step.
