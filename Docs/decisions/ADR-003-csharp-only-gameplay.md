# ADR-003: 新代码一律 C#，不再新增 Lua

## Status
Accepted（存量已按 [ADR-007](ADR-007-archive-xlua.md) 归档到 `Unused~/`）

## Date
2026-08-20

## Context
项目原约定「玩法优先 Lua」，是 IL2CPP 不能热更 C# 时的分工。ADR-001 已把 UI 切到 C# + HybridCLR；ADR-002 业务进 `HotUpdate` DLL。Lua 热更这条主因已不成立，继续双栈会让决斗/玩法再写一套 Lua 规则机。

## Decision
- 新玩法、UI、决斗、活动一律 C#，进 `HotUpdate`，靠 HybridCLR 热更。
- 不要新增 Lua 模块、Lua 界面、Lua 玩法。
- XLua、`LuaRaw/`、LuaPad、Lua UIFrame 已归档到 `Unused~/`，不要移回 `Assets/`。

## Alternatives Considered

### 继续玩法 Lua + UI C#
- Pros: 少动现有 Lua 工具链
- Cons: 两套语言、两套热更、规则机容易分叉
- Rejected: 热更已由 HybridCLR 覆盖

### 立刻删除 XLua
- Rejected: 存量 Lua 仍在跑，先停新增再迁

## Consequences
- Agent 与文档不再把 Lua 当默认实现语言。
- 动存量 Lua 仍遵守匈牙利命名、EmmyLua、`Include.lua` 别名。
- 决斗等新系统不要进 `LuaRaw/`。
