# ADR-007: 归档 XLua / Lua，不再进 Unity 导入

## Status
Accepted

## Date
2026-08-20

## Context
ADR-003 已规定新代码一律 C#。XLua 仍留在 `Assets/`，热更 DLL 继续引用 `XLua.LuaTable`。Player 从本地 CDN 拉到旧 DLL 时，`Assembly.Load` 因程序集名对不上而失败，Bootstrap 进度条停在 50%。项目不再使用 Lua。

## Decision
把 XLua、Lua 桥、Lua 源码、LuaPad、原生 `xlua` 插件移到仓库根目录 `Unused~/`（保留相对路径，方便以后查阅）。不删除。

`ImageAspectLayoutElement` 是 C# UGUI 布局组件，留在 `Assets/Scripts/UI/`。

## Alternatives Considered

### 直接删除
- Pros: 仓库更干净
- Cons: 历史脚本与插件不好对照
- Rejected: 先归档，确认无运行时依赖后再考虑删除

### 留在 `Assets/` 但去掉 asmdef 引用
- Rejected: 仍会编译、打进 Player，继续污染 HybridCLR AOT 补丁列表

## Consequences
- `HotUpdate` 不再引用 XLua；Editor 与 Player 都不再加载 `xlua` 原生库。
- `Resources/UI/PreGameUIPanel.prefab` 上的 `LuaUiComponent` 会显示 Missing Script，改用 C# `PreGameUIPanel`。
- 恢复方法：把 `Unused~/Assets/...` 移回 `Assets/`，并重新加上 asmdef 引用（不推荐）。
