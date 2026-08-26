# Unity 客户端架构

| 层 | 位置 | 职责 |
| --- | --- | --- |
| AOT 启动层 | `Assets/AOT` | 启动、下载清单、加载元数据和热更新程序集 |
| 热更新业务层 | `Assets/Scripts`、`HotUpdate.asmdef` | 游戏流程、登录、配置和业务逻辑 |
| UI 层 | `Assets/Scripts/UI` | 界面、交互和状态展示 |
| 资源层 | `Assets/AddressableAssetsData`、`Assets/AddressableCatalogs` | Addressables 构建、目录和远程资源更新 |
| 自动化层 | `Packages/com.unity.pipeline` | 通过 Unity Editor 执行场景、对象、测试和截图操作 |

唯一构建入口为 `Assets/Scenes/PreInit.unity`。客户端通过 HTTP 与后端完成认证、玩家数据、游戏配置和内容更新。

