# ADR-005: 本地 CDN 与更新检测

## Status
Superseded by [ADR-008](ADR-008-backend-content-delivery.md)

## Date
2026-08-20

## Context
要打包模拟热更：远程 AB 和 HybridCLR 代码包都要从 HTTP 拉。仓库没有现成 CDN。Addressables Hosting Services 绑在 Editor 上，Player 用不了。

## Decision
- 本地 CDN：`python tools/serve_cdn.py`，根目录 `ServerData/`，`127.0.0.1:8000`。
- `Remote.LoadPath` = `http://127.0.0.1:8000/[BuildTarget]`。
- 代码包：`ServerData/HybridCLR/HotUpdate.dll.bytes` + `.hash`。AOT `CodeUpdate` 在 `Assembly.Load` 前下载到 `persistentDataPath`。
- 资源包：HotUpdate 里 `UpdateDetector` 走官方 `CheckForCatalogUpdates` → `UpdateCatalogs` → `GetDownloadSizeAsync` / `DownloadDependenciesAsync`。
- 关闭 Addressables 启动时自动更新 Catalog，改由检测器执行。

## Alternatives Considered

### Addressables Hosting Services
- Rejected: 只服务 Editor，Player 模拟不了

### 代码包也打进 Addressable
- Rejected: 必须先 `Assembly.Load` 才能进热更程序集

## Consequences
- 模拟：Build Addressables → Publish Code → Start Local CDN → 跑 Bootstrap。
- 本机 Clash 会劫持 127.0.0.1 HTTP，Player/Editor 拉包失败时先关代理。
