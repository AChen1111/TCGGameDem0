# Unity 客户端架构

| 层 | 位置 | 职责 |
| --- | --- | --- |
| AOT 启动层 | `Assets/AOT` | 启动、下载清单、加载元数据和热更新程序集 |
| 热更新业务层 | `Assets/Scripts`、`HotUpdate.asmdef` | 游戏流程、登录、配置和业务逻辑 |
| UI 逻辑 | `Assets/Scripts/UI` | 界面、交互和状态展示 |
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

