# 系统架构

当前可运行部分覆盖启动、登录、大厅、玩家资料、商城与内容热更。决斗规则与实时对战是独立设计，代码尚未落地，见 [决斗系统架构](duel-architecture.md)。

## 1. 四层各管什么

```text
┌─────────────────────────────────────────────┐
│  Unity Player                               │
│  ┌─────────────┐  加载 DLL / 元数据          │
│  │ AOT 启动层  │─────────────────┐          │
│  └─────────────┘                 ▼          │
│  ┌─────────────────────────────────────┐    │
│  │ 热更新业务层 HotUpdate.dll           │    │
│  │ 流程 · 会话 · HTTP · UI · 资源加载   │    │
│  └─────────────────────────────────────┘    │
│           ▲ Addressables 远程内容            │
└───────────┼─────────────────────────────────┘
            │ HTTP（认证、玩家、配置、Manifest）
            ▼
┌─────────────────────────────────────────────┐
│  AChen.Backend.Api  (.NET 8 / SQLite)       │
│  Auth · Players · GameConfig · Content      │
└─────────────────────────────────────────────┘
```

| 层 | 位置 | 职责 | 不负责 |
| --- | --- | --- | --- |
| AOT 启动 | `Assets/AOT` | 拉 Manifest、加载 AOT 元数据与 `HotUpdate.dll`、转交 `HotUpdateEntry.Boot` | 登录、UI、业务状态 |
| 热更新业务 | `Assets/Scripts`（`HotUpdate.asmdef`） | 场景流程、玩家会话、HTTP API、界面、Addressables 加载 | 原生 UDP 对战、账号存储 |
| Addressables 内容 | `Assets/AddressableAssetsData`、`Assets/UI` | 远程预制体、切图、字体、卡图、场景 | 规则裁定 |
| HTTP 后端 | `Backend/src/AChen.Backend.Api` | 认证、玩家资料、商城结算、配置下发、内容发布 | 实时对局、帧同步 |

客户端与后端只通过 HTTP 交换 JSON。实时对战不走这套 API，也不复用后端的 JWT / SQLite 启动链。

## 2. 进程从哪里跑到大厅

唯一构建入口是 `Assets/Scenes/PreInit.unity`。

```text
PreInit
  LoadDll
    Editor 默认：直接取已编译的 HotUpdate 程序集
    Player / 远程 Editor：拉 Manifest → 下载 DLL → 补 AOT 元数据
  HotUpdateEntry.Boot
    UpdateDetector 更新 Addressables
    加载 Init 场景
Init
  SingletonManager 按列表初始化常驻单例
  GameFlow.Initialize 订阅登录与退出
  有可用 Refresh Token → 恢复会话 → GameScene
  否则 → LogIn
LogIn / GameScene
  SceneEntry 按 Kind 加载 UISettings，创建 UIFrame
  登录成功后 GameFlow 再进 GameScene
```

| 场景 | 作用 |
| --- | --- |
| `PreInit` | AOT 启动与热更加载 |
| `Init` | 注册常驻单例、恢复会话、选择下一场景 |
| `LogIn` | 打开登录窗 |
| `GameScene` | 打开大厅面板 |
| `SceneUIRef` | UI 制作参考，不参与启动链 |

Editor 默认不走远程 DLL，便于改完即玩。勾选 `LoadDll.useRemoteContentInEditor` 后，Editor 与 Player 走同一条内容链。

## 3. 客户端怎么拆

业务代码全部编进一个 `HotUpdate.dll`。目录按职责切开，不按“再做一个热更 DLL”切开。

| 目录 | 关键类型 | 职责 |
| --- | --- | --- |
| `Bootstrap/` | `GameFlow`、`SceneEntry` | 跨场景流程；场景只负责打开自己的首个界面 |
| `Player/` | `PlayerSession`、`AuthFlow`、`PlayerChangePublisher` | 令牌与玩家资料的唯一真相源；登录校验；字段级变更通知 |
| `Network/Http/` | `BackendHttpClient`、`BackendConfig`、`BackendJson` | 无状态传输：拼 URL、JSON、Bearer、超时、错误转换 |
| `Network/Auth/` | `AuthApi`、`AuthSessionStore` | 认证与玩家接口；令牌落盘 |
| `Network/GameConfig/` | `GameConfigManager`、`GameConfigClient`、`GameConfigStore` | 配置拉取、本地缓存、内存索引 |
| `UI/Core/` | `UIFrame`、`APanelController`、`AWindowController` | Panel / Window 两层框架 |
| `UI/LogIn`、`UI/PreGameUI`、`UI/BaseUI` | 登录窗、大厅、商城、通用弹窗 | 展示与交互，不持有会话真相 |
| `UI/Widgets`、`UI/Tween`、`UI/Behaviours` | 头像/金币/资料控件、动效、通用行为 | 可复用，不写业务流程 |
| `Addressable/` | `AddressableLoader`、`AddressKeys`、各 Catalog | 按地址加载 Sprite / Prefab / Scene / UISettings |
| `Common/` | `EventCenter`、`GameEvent`、`SceneLoader`、`MonoSingleton` | 应用通知、切场景、单例底座。`EventCenter` 不是卡牌时机调度器 |
| `LogSystem/` | `ALog`、`ALogCategories` | 分类日志；关键操作记名称、对象、结果，不进 `Update` |

依赖方向固定：

```text
UI  →  AuthFlow / GameFlow / GameConfigStore
        ↓
   PlayerSession（会话真相）
        ↓
   AuthApi / GameConfigClient
        ↓
   BackendHttpClient
```

UI 可以读 `PlayerSession` 和配置 Store，不能绕过会话直接改金币、拥有列表或令牌。商城购买走 `PlayerSession`，价格由服务端按已发布配置计算。`catalogType` 只有 `avatar` 和 `wallpaper`；卡包能展示，不能买。

## 4. 后端怎么拆

后端是单个 ASP.NET Core 进程，Minimal API + EF Core + SQLite。启动时自动迁移，并校验认证与内容发布配置。

| 模块 | 路径前缀 | 职责 |
| --- | --- | --- |
| Auth | `/api/auth` | 注册、登录、刷新、登出、当前用户 |
| Players | `/api/player` | 引导数据、改资料、购买 |
| AccountManagement | `/api/accounts/admin` | 按发布密钥给账号加金币 |
| GameConfig | `/api/game-config` | 已发布引导；`/admin/draft` 改草稿并发布 |
| ContentDelivery | `/api/content`、`/content` | Release、Manifest、不可变文件下载 |
| Admin Pages | `/admin` | 内容与配置的浏览器后台 |

三种鉴权互不混用：

| 方式 | 用在哪 |
| --- | --- |
| Bearer Access Token | 玩家资料与购买 |
| `X-Content-Publish-Key` | 内容发布、账号金币、配置管理 |
| 内容后台 Cookie | Razor Pages 管理台 |

公开接口只有健康检查、游戏配置、Manifest 和 Release 文件。限流按路由组分开：认证更严，配置与 Manifest 更宽，内容上传同一 IP 同时只允许一个。

## 5. 一次典型请求怎么走

登录：

```text
LogInWindow
  → AuthFlow.Validate / AuthenticateAsync
  → PlayerSession.LoginAsync
  → AuthApi POST /api/auth/login
  → 写入 Access / Refresh Token
  → 派发 PlayerLoggedIn
  → GameFlow 拉配置、切 GameScene
```

改昵称或装备已拥有外观：

```text
界面收集输入
  → PlayerSession 持锁调用 PATCH /api/player/profile
  → 服务端校验 expectedRevision、已拥有、已发布
  → PlayerChangePublisher 只派发变化字段
  → PlayerProfileView / PlayerGoldView 刷新
```

买头像或壁纸：

```text
ShopWindow → ShopCategory
  → PlayerSession.Purchase
  → POST /api/player/purchase { catalogType, itemId, expectedRevision }
  → 服务端按已发布 priceGold 扣款
  → 只加入拥有列表，不自动装备
```

内容更新：

```text
构建 HotUpdate.dll + Addressables
  → 生成 Manifest
  → 发布密钥上传 Release
  → 客户端 GET /api/content/manifests/latest
  → 下载 DLL 与 Addressables
```

接口字段与错误码见 [后端 API](../backend/api/README.md)。环境变量、管理台登录和测试入口见 [日常操作](operations.md)。

## 6. Editor 工具放哪里

不依赖热更运行时的工具放 `Assets/Editor`，编入 `Assembly-CSharp-Editor`。依赖 `HotUpdate` 的扩展放 `Assets/Scripts/Editor`，编入 `HotUpdate.Editor`。

| 菜单 | 目录 | 做什么 |
| --- | --- | --- |
| `Tools/后端服务` | `Editor/Backend` | 启动、停止、探活本地 API |
| `Tools/热更发布` | `Editor/Release` | 构建 DLL / Addressables 并发布 |
| `Tools/配置表` | `Editor/GameConfig` | 编辑并发布游戏配置 |
| `Tools/运营工具` | `Editor/Ops` | 给账号加金币 |
| `Tools/美术资源` | `Editor/Art` | Spine 等资源处理 |
| `Tools/UI`、`Window/TCG` | `Scripts/Editor/UI`、`LogSystem`、`Addressable` | UI 生成、日志、Addressable 目录 |

公共能力集中在 `Editor/Common`：菜单根路径、HTTP、发布密钥、路径与进程。窗口类工具同时挂在 `Window/TCG/`。环境变量 `ACHEN_*` 属于后端契约，菜单名不再使用 `AChen` 前缀。

`Assets/AOT/Editor/` 仍留着搬迁前的副本，菜单还挂在 `Tools/AChen`、`Window/AChen`、`Tools/HotUpdate`。日常用 `Assets/Editor` 与 `Assets/Scripts/Editor` 里按功能分组的那套，不要在旧目录加新工具。

## 7. 扩展时往哪加

| 要做的事 | 加在哪 | 不要做的事 |
| --- | --- | --- |
| 新界面 | 预制体加 `UiScreenGenerator`，收集引用并生成脚本；挂进该场景的 `UISettings`；用 `UIFrame.ShowPanel` / `OpenWindow` | 在窗口里直接 `UnityWebRequest` 或改 Token |
| 新玩家字段 | 后端 `Players` + 客户端 `PlayerData` / `PlayerChangePublisher` | UI 自己缓存一份可写玩家对象 |
| 新 HTTP 接口 | `Network/` 下按领域建 API 类，复用 `BackendHttpClient` | 再写一套 JSON / 错误转换 |
| 新配置表 | 后端 `GameConfig` 草稿并发布，客户端只读 `GameConfigStore` | 把价格或库存写进客户端 |
| 新 Editor 功能 | 按上表归入 Backend / Release / Config / Ops / Art；菜单走 `EditorMenus` | 在 `Assets/AOT/Editor` 或 `AChen/` 菜单根加新入口 |
| 新日志 | `ALog` + `ALogCategories`（`Default` / `Network` / `Event` / `UI`），写在状态变更或失败点 | 在 `Update` 或属性 getter 里打日志 |
| 决斗规则 / UDP | 按 [决斗系统架构](duel-architecture.md) 新增独立服务与共享规则代码 | 把帧同步塞进现有 Auth API 或 `EventCenter` |

普通新卡只加定义与效果组合；新机制只扩展对应规则模块。那是决斗设计的复用标准，登录与内容链不为此改入口。

## 8. 继续往下读

| 文档 | 内容 |
| --- | --- |
| [Unity 客户端](unity-client.md) | 目录、UI 资源分组、程序集边界 |
| [热更新与内容分发](hot-update-and-content.md) | 启动链、发布链、Editor / Player 差异 |
| [后端](backend.md) | 模块、存储、鉴权与管理台 |
| [后端 API](../backend/api/README.md) | 路由、请求体、错误码 |
| [日常操作](operations.md) | 环境变量、管理台、启后端、发版、测试 |
| [决斗系统架构](duel-architecture.md) | UDP 确定性帧同步、三端同核、回放 |
| [英雄卡组手册](hero-card-modeling.md) | 19 种卡的字段、发动步骤与案例 |
| [故障分析](troubleshooting.md) | 已定位故障的证据与结论 |
