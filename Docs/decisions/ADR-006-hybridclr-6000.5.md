# ADR-006: HybridCLR v8.14.1（Unity 6000.5）

## Status
Accepted

## Date
2026-08-20

## Context
Unity 是 6000.5.2f1。HybridCLR v8.13.0 的 Installer 在 `UNITY_6000_3_OR_NEWER` 下装 `6000.3.x` 的 libil2cpp。Player 构建时 Unity 生成的 C++ 调用 `Il2CppSharedGenericObject`、`il2cpp_codegen_ldind` 等 6000.5 API，于是 IL2CPP 编译失败。

官方 [v8.14.0](https://github.com/focus-creative-games/hybridclr_unity/blob/v8.14.1/RELEASELOG.md) 增加 6000.5.x；[v8.14.1](https://github.com/focus-creative-games/hybridclr_unity/blob/v8.14.1/RELEASELOG.md) 修了「在 6000.5 上误装 6000.3.x」。

## Decision
- `com.code-philosophy.hybridclr` 钉到 git tag `v8.14.1`。
- 升包后必须重跑 `HybridCLR/Installer`（版本号对不上就重装），再 `HybridCLR/Generate/All`。
- 不把 Unity 降到 6000.3。

## Alternatives Considered

### 继续用 v8.13.0
- Rejected: 6000.5 Player 编不过

### 降 Unity 到 6000.3 LTS
- Rejected: 当前工程已在 6000.5.2f1

## Consequences
- 升级后 `HybridCLRData/LocalIl2CppData-*` 会重建；旧 Bee/Il2cpp 缓存要清。
- ADR-001 的 v8.13.0 被本条覆盖。
