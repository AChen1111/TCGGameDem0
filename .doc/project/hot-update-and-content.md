# 热更新与内容分发

热更新解决两件事：把业务程序集送到 Player，把 Addressables 资源送到 Player。两者打进同一次内容 Release，由 Manifest 描述。系统位置见 [系统架构](architecture.md)。

## 1. 启动链

```text
PreInit
  LoadDll
    取内容 Manifest
    加载 AOT 元数据
    加载 HotUpdate.dll
  HotUpdateEntry.Boot
    UpdateDetector 按 Addressables 基址更新资源
    加载 Init 场景
  SingletonManager → 登录或大厅
```

`CodeUpdate` 负责 Manifest 与 DLL 字节。`HotUpdateEntry` 只负责资源更新和进入 `Init`。业务登录不在 AOT 层发生。

## 2. 三种运行方式

| 方式 | 程序集从哪来 | Addressables 从哪来 |
| --- | --- | --- |
| Editor 默认 | 已编译的 `HotUpdate` 程序集 | 编辑器资源 |
| Player | `GET /api/content/manifests/latest` 后下载 `HotUpdate.dll.bytes` | Manifest 给出的 Addressables 基址 |
| Editor 勾选 `LoadDll.useRemoteContentInEditor` | 与 Player 相同 | 与 Player 相同 |

Player 还要先 `LoadImage` AOT 补充元数据（`AOTGenericReferences.PatchedAOTAssemblyList`），再 `Assembly.Load` 热更 DLL。缺 `HotUpdateEntry.Boot` 则启动失败，不进入 `Init`。

默认后端地址 `http://127.0.0.1:5080`，频道 `development`。改 `LoadDll` 上的 `backendUrl` / `channel` 以指向其他环境。Manifest 请求必须带查询参数 `channel`、`platform`、`appVersion`。

## 3. 发布链

```text
构建热更新 DLL
  → 构建 Addressables
  → 生成 Manifest（schema、releaseId、channel、platform、appVersion、contentVersion、产物哈希）
  → 上传后端 ContentDelivery
  → 切换活动 Release
  → 客户端按 Manifest 拉取
```

发布前准备 `ACHEN_CONTENT_PUBLISH_KEY`（与后端 `ContentDelivery:PublishKey` 一致）。无环境变量时，可先用 `Tools/后端服务` 启动，会话里会生成一把密钥。菜单：`Tools/热更发布/Build And Publish Release`。`Build And Publish Release From Env` 额外读取 `ACHEN_CONTENT_VERSION`。

环境变量与密钥来源见 [日常操作](operations.md)。

Manifest 里的热更产物带路径、大小、SHA-256。Addressables 产物带 catalog 与 catalog hash。客户端校验哈希后再加载，失败停在启动条并显示原因。

内容文件一经发布按 Release 路径不可变。回滚靠切换活动 Release，不改已上传文件。未使用的 Release 可由管理接口删除。

## 4. 和业务代码的边界

- AOT 只认识 `HotUpdateEntry.Boot(Action<float>, string addressablesBaseUrl, Action<string>)`。
- 热更层用 `AddressableLoader` 按 `AddressKeys` 取资源，不自己拼 CDN URL。
- 配置表走 `GameConfig` HTTP，不打进 Addressables 包冒充卡片定义热更。
- 决斗规则若共享给服务器，用链接的纯 C# 源码，不把整个 `HotUpdate.dll` 丢给 ASP.NET。
