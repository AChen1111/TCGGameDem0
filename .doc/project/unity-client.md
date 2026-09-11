# Unity 客户端

客户端分 AOT 启动与热更新业务两层。业务全部编入 `HotUpdate.dll`。构建入口始终是 `Assets/Scenes/PreInit.unity`。系统分层与启动顺序见 [系统架构](architecture.md)。

## 1. 程序集

| 程序集 | 位置 | 运行时 |
| --- | --- | --- |
| AOT（默认程序集） | `Assets/AOT` | Player 启动时已在包内 |
| `HotUpdate` | `Assets/Scripts` | Editor 直接加载；Player 从内容服务下载 |
| `Assembly-CSharp-Editor` | `Assets/Editor` | 仅 Editor；不引用热更运行时 |
| `HotUpdate.Editor` | `Assets/Scripts/Editor` | 仅 Editor；引用 `HotUpdate` |

`HotUpdate` 引用 UniTask、LitMotion、Addressables、Input System、TextMeshPro、UGUI、SuperScrollView、Spine。不引用后端工程，也不引用第二个热更业务 DLL。

## 2. 热更层目录

| 目录 | 职责 |
| --- | --- |
| `Bootstrap/` | `SceneEntry` 按 Kind 打开登录窗或大厅面板；`GameFlow` 订阅登录 / 退出并决定下一场景 |
| `Player/` | `PlayerSession` 持有令牌与当前玩家；`AuthFlow` 做输入校验和错误提示；`PlayerChangePublisher` 比较前后资料，只派发变化字段 |
| `Network/Http/` | `BackendHttpClient` 传输；`BackendJson` 序列化；`BackendHttpError` 转问题详情；`BackendConfig` 读地址与超时 |
| `Network/Auth/` | `AuthApi` 端点与 DTO；`AuthSessionStore` 持久化 Refresh Token |
| `Network/GameConfig/` | 拉取、ETag 缓存、内存索引；失败时可用本地最后一份快照 |
| `UI/Core/` | `UIFrame`、Panel / Window 层、ListView、从预制体生成脚本的 Authoring |
| `UI/LogIn/`、`UI/PreGameUI/`、`UI/BaseUI/` | 登录、大厅 / 商城 / 头像 / 改名、通用提示与选择窗 |
| `UI/Widgets/` | 资料、金币、头像、布局小部件；再按 `Player/`、`Layout/` 分子目录 |
| `UI/Tween/`、`UI/Behaviours/` | 点击缩放等动效；加载文案、Spine 点击等行为 |
| `Addressable/` | `AddressableLoader`、各 Catalog、`AddressKeys`（由 Catalog 工具生成） |
| `Common/` | `EventCenter` / `GameEvent`、`SceneLoader`、`MonoSingleton` / `SingletonManager`、`StringValidator` |
| `LogSystem/Runtime/` | `ALog`、`ALogCategories` |

`Init` 场景里的 `SingletonManager` 按列表初始化常驻单例，再调用 `GameFlow.GetStartupSceneAsync`。会话恢复与全局订阅不放进 `LogIn` / `GameScene` 的 `SceneEntry`。

## 3. UI 怎么打开

每个业务场景带一份 `UISettings`。`SceneEntry` 加载后调用 `CreateUIInstance()` 得到 `UIFrame`：

- 登录：`OpenWindow(AddressKeys.Prefab.LogInWindow)`
- 大厅：`ShowPanel(AddressKeys.Prefab.PreGameUIPanel)`

Panel 常驻分层级显示；Window 进栈，带遮罩层。屏幕控制器从 `APanelController` / `AWindowController` 派生。预制体上的 `UiScreenGenerator` 按前缀收集引用并生成绑定代码，生成块夹在固定标签之间，不要手改标签内代码。

界面只展示 `PlayerSession` 与 `GameConfigStore` 已经算好的结果。购买、改名、换头像走会话方法；忙碌与失败通过 `GameEvent.LobbyEntering` / `LobbyEntryFailed` 等事件回传，不在窗口里复制一份流程状态机。大厅商城的卡包品类只展示配置，`TryGetPurchaseTarget` 为空，不能走购买接口。

新增一个界面：

1. 在 `Assets/UI/Prefab` 做预制体，按前缀命名子节点。
2. 挂 `UiScreenGenerator`，选 Panel 或 Window，填类名和脚本目录。
3. 点「收集UI引用」再「创建UI脚本」；生成块夹在固定标签之间，不要手改标签内代码。
4. 把预制体加进该场景的 `UISettings`，需要远程加载时编入 Addressables 并更新 Catalog / `AddressKeys`。
5. 业务里只调用 `UIFrame.ShowPanel` 或 `OpenWindow`。

`Tools/UI/创建 UI Frame Prefab` 只生成 Panel / Window / 遮罩骨架，不代替上面的屏幕脚本。

## 3.1 日志

`ALog.Log` / `LogWarning` / `LogError` 必须带 `ALogCategories`：`Default`、`Network`、`Event`、`UI`。新增分类只在 `ALogCategories` 加常量，控制台下拉会反射到。Editor 始终写入；Player 看 `ALogSettings.EnableInPlayer`。浏览、过滤、跳转走 Console 工具栏的 ALog，不要 `Debug.Log` 打业务事件。

## 4. UI 资源

`Assets/UI` 分开逻辑预制体和远程美术。Sprite 子目录地址使用 `Sprite/模块名`，避免与 Prefab 文件夹地址冲突。

| 路径 | 内容 | Addressables 组 |
| --- | --- | --- |
| `Prefab/` | 逻辑预制体（BaseUI、Hall、LogIn） | `Remote_Shared` / `Remote_UI_Hall` |
| `Fonts/` | 字体与 TMP SDF | `Remote_Shared` |
| `Sprite/` | UI 切图，按模块分子目录 | `Remote_UI_Hall` / `Remote_UI_Event` / `Remote_Card` |
| `Card/` | 卡包图与壁纸 | `Remote_Card` |
| `Shader/` | UI Shader | `Remote_Shared` |

地址常量写在 `AddressKeys`。头像与壁纸按编号拼地址：`a_{id:D2}`、`w_{id:D2}_Sprite`、`w_{id:D2}_Down`。新增可寻址资源后，用 Addressable 目录工具重新生成 `AddressKeys`，不要手写散落字符串。

## 5. 网络与会话边界

`BackendHttpClient` 无状态，可被多个 API 共用。`AuthApi` 不保存令牌；调用方传入 Access Token。`PlayerSession` 是唯一写入令牌和 `CurrentPlayer` 的地方，资料修改加锁，并用 `SessionVersion` 丢弃跨会话的过期异步结果。

`GameConfigManager` 每 5 分钟检查一次配置。本地缓存命中可先用，再按 ETag 刷新。后端不可达且本地没有快照时，进入大厅失败。

现有 `Network/` 只有 REST。对战 UDP 若落地，放在独立的 `Network/Duel`，不把 socket 类型带进卡效或 `AuthApi`。

## 6. Editor 扩展

| 位置 | 程序集 | 内容 |
| --- | --- | --- |
| `Assets/Editor/Backend` | Assembly-CSharp-Editor | 后端启停 |
| `Assets/Editor/Release` | 同上 | HybridCLR 工程检查、热更发布 |
| `Assets/Editor/GameConfig` | 同上 | 配置表编辑与发布 |
| `Assets/Editor/Ops` | 同上 | 运营加金币 |
| `Assets/Editor/Art` | 同上 | Spine 导入 |
| `Assets/Editor/Common` | 同上 | 菜单常量、Editor HTTP、发布密钥、路径、进程 |
| `Assets/Scripts/Editor/Addressable` | HotUpdate.Editor | Catalog 与 `AddressKeys` |
| `Assets/Scripts/Editor/LogSystem` | 同上 | ALog 控制台、堆栈图、跳转 |
| `Assets/Scripts/Editor/UI` | 同上 | UI 框架工具、切图导入 |

菜单按功能挂在 `Tools/后端服务`、`Tools/热更发布`、`Tools/配置表`、`Tools/运营工具`、`Tools/美术资源`、`Tools/UI`。窗口入口同时在 `Window/TCG/`。资产创建菜单用 `TCG/UI`、`TCG/Addressable`。

`HotUpdate.Editor` 不能引用 `Assets/Editor` 里的类型，菜单字符串与 `EditorMenus` 保持同一分组即可。

`Assets/AOT/Editor/` 仍有搬迁前的窗口和 `Tools/AChen`、`Tools/HotUpdate` 菜单。不要在那里改功能；以 `Assets/Editor` 与 `Assets/Scripts/Editor` 为准。

## 7. 第三方代码放哪

| 位置 | 内容 |
| --- | --- |
| `Assets/Plugins` | Asset Store 插件：Sirenix、SuperScrollView、vHierarchy、VoxelLabs |
| `Packages/` | UPM 包：Addressables、HybridCLR、UniTask、LitMotion、Pipeline 等 |
| `Assets/TextMesh Pro` | TMP 资源，仍在 `Assets/` 根下 |
| `Assets/Spine` | Spine 运行时 |

插件说明见 [外部插件](../plugins/README.md)。
