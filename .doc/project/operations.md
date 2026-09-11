# 日常操作

分层与职责见 [系统架构](architecture.md)。接口与错误码见 [后端 API](../backend/api/README.md)。

## 1. 常用入口

| 操作 | 方式 |
| --- | --- |
| 打开工程 | Unity `6000.5.2f1` |
| 启动后端 | `Tools/后端服务/启动`，或 `dotnet run --project Backend/src/AChen.Backend.Api` |
| 检查后端 | `http://127.0.0.1:5080/health`；就绪为 `/ready` |
| 运行游戏 | 唯一构建场景 `Assets/Scenes/PreInit.unity` 进 Play Mode |
| 发布热更 | 备好发布密钥后，`Tools/热更发布/Build And Publish Release` |
| 发布配置 | `Tools/配置表/发布窗口`，或浏览器 `/admin/game-config` |
| 加金币 | `Tools/运营工具`，或 `POST /api/accounts/admin/gold` |
| 管理台 | 浏览器 `http://127.0.0.1:5080/admin/content/login` |

后端启动时自动执行 EF Core 迁移。SQLite 默认 `Backend/src/AChen.Backend.Api/Data/achen.db`。内容文件默认 `Data/content`。

离线 Unity 文档：[Manual](../unity-official-6000.5.2f1/en/Manual/index.html) · [Script Reference](../unity-official-6000.5.2f1/en/ScriptReference/index.html)

## 2. 环境与配置

监听地址由 `launchSettings.json` 定为 `http://127.0.0.1:5080`。客户端 `BackendConfig` 与 `LoadDll.backendUrl` 默认同一地址，超时 10 秒。内容频道默认 `development`，必须出现在 `ContentDelivery:AllowedChannels` 里。

`appsettings.json` 里 `Auth:SigningKey` 和 `ContentDelivery:PublishKey` 默认为空，长度都必须 ≥ 32，否则进程起不来。

| 变量 / 配置键 | 用途 |
| --- | --- |
| `ACHEN_BACKEND_AUTH_SIGNING_KEY` | JWT 签名密钥；对应 `Auth:SigningKey` |
| `ACHEN_CONTENT_PUBLISH_KEY` | 内容与配置发布密钥；对应 `ContentDelivery:PublishKey` |
| `ACHEN_CONTENT_VERSION` | `Tools/热更发布/Build And Publish Release From Env` 使用的内容版本；缺省为 `0.2.0` |
| `Auth__SigningKey` / `ContentDelivery__PublishKey` | ASP.NET 配置覆盖（双下划线） |
| `ConnectionStrings:Default` | SQLite 路径，相对后端 ContentRoot |
| `ContentDelivery:StorageRoot` | 内容文件根目录 |
| `ContentDelivery:AllowedChannels` | 允许的频道，默认只有 `development` |
| `GameConfigGit:RepositoryRoot` | 配置草稿 Git 仓库 |
| `GameConfigGit:RemoteUrl` | 可选远端；空则只能本地快照 |

从 Unity `Tools/后端服务` 启动时：若环境变量未设，窗口为本次 Editor 会话生成 ≥ 32 字符密钥，写入进程环境 `Auth__SigningKey` 与 `ContentDelivery__PublishKey`。发布窗口按顺序取：环境变量 `ACHEN_CONTENT_PUBLISH_KEY` → 后端服务会话密钥 → 窗口内仅内存输入（不写 EditorPrefs）。

用 `dotnet run` 时必须自己提供这两把密钥，例如：

```powershell
$env:Auth__SigningKey = "<至少32字符>"
$env:ContentDelivery__PublishKey = "<至少32字符>"
dotnet run --project Backend/src/AChen.Backend.Api
```

命令行启动不会自动把密钥写给 Unity。要在 Editor 里发版或加金币，把同一把 `ACHEN_CONTENT_PUBLISH_KEY` 配进用户或进程环境。

## 3. 管理台

1. 后端已在 5080 监听。
2. 打开 `http://127.0.0.1:5080/admin/content/login`，填与后端一致的发布密钥。
3. 登录后进入内容列表；顶栏切到配置表或发布历史。
4. 配置表：改草稿或导入 CSV → 需要版本历史时用 Git 保存/推送 → 点发布。发布前客户端看不到草稿。
5. 账号列表在 `/admin/accounts`。当前这两页未挂发布密钥鉴权；加金币仍走带密钥的 JSON 接口或 Unity 运营工具。

## 4. 测试

| 范围 | 方式 |
| --- | --- |
| 后端 | `dotnet test Backend/AChen.Backend.sln` |
| Unity Editor | `Window > General > Test Runner`，跑 `Assets/Tests/Editor`（程序集 `HotUpdate.Editor.Tests`） |
| Unity 自动化 | [Unity Pipeline](../unity-pipeline/index.md) |

Editor 测试覆盖认证会话、配置缓存、UI 生成、Addressable Catalog、ALog、后端服务窗口等。不要用 Play Mode 冒充这批测试。
