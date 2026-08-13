---
name: project-coding-standards
description: Enforces this project's coding rules: keep code minimal, skip optional edge-case guards, Lua Hungarian naming, EmmyLua annotations, CS aliases in Include.lua, use installed agent-skills on demand, ask when unsure, and list invoked skills in every reply. Use when writing or editing any code in this repo (Lua, C#, or otherwise).
---

# Project Coding Standards

编码时必须遵守以下规则。

## a. 代码尽量精简

- Prefer the smallest correct change.
- Do not add unrelated comments, wrappers, or defensive noise.

## b. 边界条件

- 允许报错；不要为无关边界条件加报错处理。
- 可加可不加的边界条件，一律不加。

## c. Lua 命名

编写 Lua 代码时使用如下命名法：

- 成员变量：`m_类型+变量名`（匈牙利简写）
- 私有函数：以 `_` 开头

示例：

| 种类 | 写法 |
|------|------|
| number | `m_nCount` |
| string | `m_strName` |
| bool | `m_bReady` |
| button | `m_btnConfirm` |
| 私有函数 | `_updateView` |

## d. Lua EmmyLua 注解

编写或修改 Lua 时必须加 EmmyLua 注解：

- 类型：`---@class`、`---@field`
- 函数：`---@param`、`---@return`
- 局部变量需要类型时：`---@type`

## e. Lua C# 别名

禁止在业务 Lua 里直接写 `CS.xxx`。

把 C# 类型/静态类的重命名写进 `Assets/Scripts/LuaRaw/Include.lua`，业务代码只用别名。

```lua
-- Include.lua
GameObject = CS.UnityEngine.GameObject
LuaUiUtil = CS.LuaUiUtil

-- 业务代码
local go = Object.Instantiate(prefab)
LuaUiUtil.SetRaycasterEnabled(go, false)
```

## f. 编码时按需使用已安装 skills

写或改代码时，按当前步骤选用 `.cursor/skills/` 里已安装的 skill，不要只凭猜测开工。

先读 `.cursor/skills/using-agent-skills/SKILL.md` 做路由，再打开对应 `SKILL.md` 并按流程执行。常见对应：

- 实现切片 → `incremental-implementation`
- 先写测试 → `test-driven-development`
- 查官方文档再写 → `source-driven-development`
- 调试 → `debugging-and-error-recovery`
- Unity 编辑器/场景/编译/测试 → `unity-pipeline`
- 提交/分支 → `git-workflow-and-versioning`

skill 里若链接 `reference.md` 或 `.cursor/references/`，一并打开。

## g. 不确定就问

有不确定的内容，请询问。不要猜着实现。

提问时用选项弹窗（`AskQuestion` 工具），不要在正文里用 `Q:` / 列表罗列选项。每个问题给出可选项，把推荐项放第一个并标注「(推荐)」。

## h. 每次输出列出本次 skills

在每次对用户的输出结果末尾，列出这次调用的所有 skills：

```
Skills: skill-a, skill-b, ...
```
