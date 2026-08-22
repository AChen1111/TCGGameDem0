# ADR-001: C# deVoid UI + HybridCLR / Hot Reload

## Status
Accepted

## Date
2026-08-18

## Context
项目原先用 XLua UIFrame（仿 deVoid UI）写界面。同时需要 IL2CPP 热更新与 Editor 改代码免等编译。

## Decision
- UI 改为导入 [yankooliveira/uiframework](https://github.com/yankooliveira/uiframework) 核心（不含 examples）。
- 热更新装 HybridCLR `com.code-philosophy.hybridclr`（版本见 [ADR-006](ADR-006-hybridclr-6000.5.md)）。
- Editor 热重装装 Asset Store「Hot Reload」1.13.22。
- Lua UIFrame 标记为弃用，存量不删，新界面走 C#。

## Alternatives Considered

### 继续 Lua UI + 只加 HybridCLR
- Pros: 少改现有界面
- Cons: 与「C# 热更 / 免编译」方向冲突，两套 UI 会长期并存
- Rejected: 本次明确切到 deVoid UI

### 自研 C# UI
- Rejected: deVoid UI 已是现有 Lua 方案的原型，导入成本更低

## Consequences
- 新 UI 写在 C#，可走 HybridCLR 热更与 Hot Reload。
- Lua UI 已随 XLua 归档到 `Unused~/`（[ADR-007](ADR-007-archive-xlua.md)）。
