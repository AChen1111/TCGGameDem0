---
name: project-coding-standards
description: Enforces this project's coding rules: new code is C# only, keep code minimal, skip optional edge-case guards, use installed agent-skills on demand, ask when unsure, and list invoked skills in every reply.
---

# Project Coding Standards

编码时必须遵守以下规则。

## a. 代码尽量精简

- Prefer the smallest correct change.
- Do not add unrelated comments, wrappers, or defensive noise.

## b. 边界条件

- 允许报错；不要为无关边界条件加报错处理。
- 可加可不加的边界条件，一律不加。

## c. 新代码一律 C#

不要新增 Lua 模块、界面或玩法。新逻辑写 C#，进 `HotUpdate` 程序集。XLua/Lua 已归档到 `Unused~/`，不要移回 `Assets/`。

## d. 编码时按需使用已安装 skills

写或改代码时，按当前步骤选用 `.cursor/skills/` 里已安装的 skill，不要只凭猜测开工。

本轮若使用 `/quit`，跳过项目级 skills，直接做。否则先读 `.cursor/skills/using-agent-skills/SKILL.md` 做路由，再打开对应 `SKILL.md` 并按流程执行。常见对应：

- 实现切片 → `incremental-implementation`
- 先写测试 → `test-driven-development`
- 查官方文档再写 → `source-driven-development`
- 调试 → `debugging-and-error-recovery`
- Unity 编辑器/场景/编译/测试 → `unity-pipeline`
- 提交/分支 → `git-workflow-and-versioning`

skill 里若链接 `reference.md` 或 `.cursor/references/`，一并打开。

## e. 不确定就问

有不确定的内容，请询问。不要猜着实现。

提问时用选项弹窗（`AskQuestion` 工具），不要在正文里用 `Q:` / 列表罗列选项。每个问题给出可选项，把推荐项放第一个并标注「(推荐)」。

## f. 每次输出列出本次 skills

在每次对用户的输出结果末尾，列出这次调用的所有 skills：

```
Skills: skill-a, skill-b, ...
```
