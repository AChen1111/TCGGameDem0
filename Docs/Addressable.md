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

加载：`AddressableLoader.LoadUISettings(AddressKeys.UISettings.PreGameSceneUI)`。右键 `AddToUISettingsSO` / `AddToPrefabSO` / `AddToSpriteSO` / `AddToSceneSO` 会把所在文件夹标进对应组，并写入 `AddressKeys`（HotUpdate）。
