# deVoid UI Framework

项目 UI 的现行方案。源码在 `Assets/Plugins/deVoidUI/`，来自 [yankooliveira/uiframework](https://github.com/yankooliveira/uiframework) 的 **Core / Editor / Panel / Window / ScreenTransitions**（未导入 examples 仓库）。

完整手册：[MANUAL.md](../Assets/Plugins/deVoidUI/MANUAL.md)

Lua UIFrame 已弃用，见 [UIFramework.md](UIFramework.md)。

---

## 最短路径

1. Project 窗口右键 `Create → deVoid UI → UI Frame Prefab`，拖进场景。
2. 屏幕脚本继承 `APanelController` / `AWindowController`（可带 `TProps`）。
3. 注册并打开：

```csharp
uiFrame.RegisterScreen("HomeWindow", homeWindowPrefab);
uiFrame.OpenWindow("HomeWindow");
uiFrame.ShowPanel("HudPanel");
```

或用 `Create → deVoid UI → UI Settings`，把 Frame Prefab 和 Screens 配好后：

```csharp
uiFrame = uiSettings.CreateUIInstance();
```

命名空间：`deVoid.UIFramework`。

## 新建一个 Panel / Window

在界面 Prefab 根节点挂 `UiScreenGenerator`（`Add Component → UI/UI Screen Generator`）：

1. **类别** `Panel` 或 `Window`
2. **名称** 类名，如 `PreGamePanel`
3. **路径** 脚本目录，如 `Assets/Scripts/UI`
4. 子物体命名 `前缀_名字`（如 `Btn_Play`）
5. 点 **收集UI引用**，再点 **创建UI脚本**
6. 等编译结束后，把生成的脚本挂到同一节点，再点一次 **收集UI引用** 把字段填上

`Btn_Play` → 字段 `m_BtnPlay`。

| 前缀 | 组件 |
|------|------|
| Btn | Button |
| Img | Image |
| Txt | TextMeshProUGUI |
| Tog | Toggle |
| Sld | Slider |
| Inp | TMP_InputField |
| Scr | ScrollRect |
| Raw | RawImage |
| Drop | TMP_Dropdown |
| Go | GameObject |
