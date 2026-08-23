# Addressable 分组

Group 已写在 `Assets/AddressableAssetsData`。决策见 [ADR-004](decisions/ADR-004-addressable-groups.md)。

目录即分包单位：文件夹进 Group 后，该目录下资源打进同一个 AB。

| Group | 路径 | 打包 | 文件夹 |
|---|---|---|---|
| Local_Boot | Local | PackTogether | Init |
| Remote_Catalog | Remote | PackTogether | `Assets/AddressableCatalogs` |
| Remote_Shared | Remote | PackTogether | `Assets/UI/Prefab/BaseUI`、`Assets/Learn/MasterDuel/Fonts` |
| Remote_UI_Hall | Remote | PackSeparately | `Assets/UI/Prefab/Hall/<模块>/` |
| Remote_UI_Event | Remote | PackSeparately | `Assets/UI/Prefab/Event/<活动>/` |
| Remote_Card | Remote | PackSeparately | 卡期文件夹 |
| Remote_Scene | Remote | PackSeparately | `GameScene` 等业务场景 |

Player 构建列表只留 `Bootstrap`。`Init` 由 `HotUpdateEntry` 按地址加载（此时还没有 `AddressableLoader`）。之后场景一律 `AddressableLoader.LoadScene(AddressKeys.Scene.*)`。右键 `AddToSceneSO` 写入 `SceneCatalog` 并放进 `Remote_Scene`。

加载：`AddressableLoader.LoadUISettings(AddressKeys.UISettings.PreGameSceneUI)`。右键 `AddToUISettingsSO` / `AddToPrefabSO` / `AddToSpriteSO` / `AddToSceneSO` 会把所在文件夹标进对应组，并写入 `AddressKeys`（HotUpdate）。

## 后端内容发布与热更

现行决策见 [ADR-008](decisions/ADR-008-backend-content-delivery.md)。`ServerData/` 只是 Unity Addressables 的构建输出，不再由 Python 直接提供服务。

```
Tools/HotUpdate/Build And Publish Release
```

窗口会构建 Addressables 和 HybridCLR DLL，生成 `release-manifest.json` 与 ZIP，上传到 ASP.NET Core 后端并切换 `development` 当前版本。发布密钥优先读取 `ACHEN_CONTENT_PUBLISH_KEY`，否则只保存在窗口内存。

Player 先由 AOT `CodeUpdate` 查询同一个 Release 的清单、校验并加载 DLL，再由 HotUpdate `UpdateDetector` 注入 `{AChen.ContentBaseUrl}` 并更新 Catalog/AB。Editor 默认直接使用本地 HotUpdate 程序集和本地 Addressables；需要验证远端链路时，在启动场景的 `LoadDll` 上启用 `useRemoteContentInEditor`。

