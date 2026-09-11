# Unity 客户端架构

| 层 | 位置 | 职责 |
| --- | --- | --- |
| AOT 启动层 | `Assets/AOT` | 启动、下载清单、加载元数据和热更新程序集 |
| 热更新业务层 | `Assets/Scripts`、`HotUpdate.asmdef` | 游戏流程、登录、配置和业务逻辑 |
| 场景入口与流程 | `Assets/Scripts/Bootstrap` | 场景入口 `SceneEntry`（按 Kind 打开登录窗或大厅面板）与跨场景流程 `GameFlow` |
| 玩家会话 | `Assets/Scripts/Player` | `PlayerSession`（令牌、当前玩家与资料操作的唯一真相源）、`PlayerChangePublisher`（字段级变更事件）、`AuthFlow`（登录/注册校验与错误提示） |
| 网络协议 | `Assets/Scripts/Network` | `Http/`（`BackendHttpClient` 传输、JSON 与错误转换）、`Auth/`（`AuthApi` 端点与 DTO、令牌存储）、`GameConfig/`（配置拉取、缓存与内存索引），不含业务流程 |
| UI 逻辑 | `Assets/Scripts/UI` | 界面、交互和状态展示；`UI/Core` 为框架与生成工具，`UI/Widgets` 为可复用小部件，`UI/Tween` 为动效，`UI/Behaviours` 为通用行为组件 |
| Editor 工具 | `Assets/Editor`（Assembly-CSharp-Editor）、`Assets/Scripts/Editor`（`HotUpdate.Editor`） | 前者为后端服务、热更发布、配置表、运营与美术工具，菜单按功能挂在 `Tools/` 下并在 `Window/TCG/` 提供窗口入口；后者为 Addressable 目录、ALog 与 UI 框架的编辑器扩展 |
| UI 资源 | `Assets/UI` | 预制体与远程美术，见下表 |
| 资源层 | `Assets/AddressableAssetsData`、`Assets/AddressableCatalogs` | Addressables 构建、目录和远程资源更新 |
| 第三方插件 | `Assets/Plugins`、`Packages/` | Asset Store 插件与 UPM 包 |
| 自动化层 | `Packages/com.unity.pipeline` | 通过 Unity Editor 执行场景、对象、测试和截图操作 |

`Assets/UI` 约定：

| 路径 | 内容 | Addressables |
| --- | --- | --- |
| `Prefab/` | 逻辑预制体（BaseUI、Hall、LogIn） | `Remote_Shared` / `Remote_UI_Hall` |
| `Fonts/` | 字体与 TMP SDF | `Remote_Shared` |
| `Sprite/` | UI 切图（按模块分子目录） | `Remote_UI_Hall` / `Remote_UI_Event` / `Remote_Card` |
| `Card/` | 卡包图与壁纸 | `Remote_Card` |
| `Shader/` | UI 相关 Shader | `Remote_Shared` |

`Prefab` 是逻辑预制体；`Fonts` / `Sprite` / `Card` 是远程美术。Sprite 子目录地址使用 `Sprite/模块名`，避免与 Prefab 文件夹地址冲突。

`Assets/Plugins` 放置 Asset Store 插件（Sirenix、SuperScrollView、vHierarchy、VoxelLabs）。`TextMesh Pro` 仍在 `Assets/` 根下。

唯一构建入口为 `Assets/Scenes/PreInit.unity`。客户端通过 HTTP 与后端完成认证、玩家数据、游戏配置和内容更新。

