# ADR-002: HybridCLR 业务代码进 HotUpdate 程序集

## Status
Accepted

## Date
2026-08-19

## Context
项目已按 ADR-001 安装 HybridCLR v8.13.0，但未跑 Installer，业务脚本全在默认 `Assembly-CSharp`。需要把业务 C# 放进可热更 DLL，同时满足 HybridCLR 约束：AOT 不能引用热更程序集；挂了热更脚本的场景必须走 AssetBundle/Addressables，且先 `Assembly.Load` 再加载。

XLua 没有 asmdef 时，热更程序集无法引用它（asmdef 不能引用 `Assembly-CSharp`）。

## Decision
- `Assets/Scripts` 运行时业务编进 `HotUpdate` 程序集，关闭 `autoReferenced`。
- XLua 单独 AOT 程序集；`LoadDll` 留在 `Assembly-CSharp`。
- 首场景 `Bootstrap` 只挂 `LoadDll`；`Init` 标为 Addressable，加载热更 DLL 后再进。
- 首包 DLL 放 `StreamingAssets/HybridCLR/`（官方快速上手）；游戏资源仍走 Addressables。

## Alternatives Considered

### 整个 Assembly-CSharp 当热更 DLL
- Pros: 几乎不用拆程序集
- Cons: XLua 原生互操和 `RuntimeInitializeOnLoadMethod` 在真机上更容易失效
- Rejected: 插件应留 AOT

### 只配 HybridCLRSettings、不拆程序集
- Rejected: 真机无法热更业务代码

## Consequences
- 新业务脚本放 `Assets/Scripts`（非 Editor 目录）即进热更 DLL。
- AOT 只能通过反射调用 `HotUpdateEntry.Boot`。
- 打包前需 `HybridCLR/Installer`、`HybridCLR/Generate/All`，再 `Tools/HybridCLR/Copy Dlls To StreamingAssets`。
- Editor 脚本合进单一 `HotUpdate.Editor`（各 `Editor` 目录用 asmref），不随包、不热更。
