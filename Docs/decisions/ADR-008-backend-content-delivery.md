# ADR-008: 后端内容交付与不可变 Release

## Status

Accepted

## Date

2026-08-23

## Context

旧方案用 Python `http.server` 分别提供 Addressables 和 HybridCLR DLL，没有统一版本身份、服务端哈希校验、发布历史、并发切换或安全回滚。DLL、Catalog 与 AB 可能来自不同一次构建。

## Decision

- Unity Editor 继续构建 Addressables 与 HybridCLR；ASP.NET Core `ContentDelivery` feature 接管上传、校验、存储、下载、发布和回滚。
- DLL、Catalog、Catalog Hash 与全部 Bundle 组成不可变 Release，以 `platform + appVersion + contentVersion` 唯一。
- SQLite 保存 Release、文件、当前指针与发布历史；文件通过 `IContentStorage` 抽象，首版落本机磁盘。
- 管理 API 使用独立 `X-Content-Publish-Key`；玩家只访问最新版清单和不可变文件。
- Player 先取得最新版清单并验证 DLL SHA-256，再在 Addressables 初始化前注入 Release 专属根地址，保证代码与资源同版。
- 第一版只开放 `development`，支持 `StandaloneWindows64`、`Android`、`iOS`。Ready Release 永久保留。

## Consequences

- 发布入口统一为 `Tools/HotUpdate/Build And Publish Release`，旧 Python CDN 与两个本地 CDN 菜单删除。
- 回滚只切换数据库指针，不复制或覆盖文件；客户端缓存按 Release ID 隔离。
- 本地运行后端需要分别配置 JWT 签名密钥和内容发布密钥。
- 未来替换 S3/OSS 时实现新的 `IContentStorage`，无需改变发布状态机和 API。
