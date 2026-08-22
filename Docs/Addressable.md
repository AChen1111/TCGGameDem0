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

## 本地模拟热更

决策见 [ADR-005](decisions/ADR-005-local-cdn-update-detector.md)。CDN 是 `ServerData/` 上的 Python `http.server`。

```
Tools/HotUpdate/Build Addressables
Tools/HotUpdate/Publish Code To Local CDN
Tools/HotUpdate/Start Local CDN
```

或：`python tools/serve_cdn.py`

然后跑 `Bootstrap`。代码包走 `CodeUpdate`（AOT），AB 走 `UpdateDetector`（HotUpdate）。Play Mode 要选 **Use Existing Build** 才能真正下远程包。

