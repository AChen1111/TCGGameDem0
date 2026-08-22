# ADR-004: Addressable Group 与远程分包

## Status
Accepted

## Date
2026-08-20

## Context
大厅、活动、卡图要按更新粒度拆 AB。全部打成远端会让 Bootstrap/Init 无法启动；一份 `UISettings` 登记全部界面会让「按界面拆包」失效。Catalog SO 与资源名常量也要能随热更走。

## Decision
- 本地：`Local_Boot`（Init），`PackTogether`。HybridCLR DLL 仍走 StreamingAssets。
- 远端：`Remote_Catalog`（`PackTogether`，收 `Assets/AddressableCatalogs`）；`Remote_Shared`（`PackTogether`，收 `BaseUI` 与字体）；`Remote_UI_Hall` / `Remote_UI_Event` / `Remote_Card`（`PackSeparately`，每个模块文件夹一个 AB）；`Remote_Scene`（`PackSeparately`，业务场景，经 `SceneCatalog` + `AddressableLoader.LoadScene`）。
- 资源名写在 HotUpdate 的 `AddressKeys` 里。`AddTo*SO` 往对应 Catalog SO 登记条目并重写该文件。
- UI Frame、公共控件放 `Assets/UI/Prefab/BaseUI`；大厅界面放 `Assets/UI/Prefab/Hall/<模块>`；活动放 `Assets/UI/Prefab/Event/<活动>`。
- 开启 Remote Catalog。Group 与 Catalog SO 已检入，不再用菜单重建。

## Alternatives Considered

### Catalog SO 放本地
- Rejected: Catalog 本身也要热更；Init 上的 AssetReference 会按 GUID 拉 `Remote_Catalog`

### 每个 Panel 单独 Addressable 条目
- Rejected: 文件夹条目 + PackSeparately 即可按模块拆包

### 一个巨型 UISettings
- Rejected: `CreateUIInstance` 会实例化列表里全部 Screen，拆 AB 无效

## Consequences
- 业务侧用 `AddressKeys.*` 加载，不要手写字符串。
- 卡数据若更新远勤于卡图，再加 `Remote_CardData`。
