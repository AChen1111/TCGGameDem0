# deVoid UI Framework

项目 UI 的现行方案。源码在 `Assets/Scripts/UI/Core/`，来自 [yankooliveira/uiframework](https://github.com/yankooliveira/uiframework) 的 **Core / Editor / Panel / Window**（未导入 examples；已去掉 Properties 与开合动画层）。

Lua UIFrame 已弃用，见 [UIFramework.md](UIFramework.md)。

---

## 最短路径

1. Project 窗口右键 `Create → deVoid UI → UI Frame Prefab`，拖进场景。
2. 屏幕脚本继承 `APanelController` / `AWindowController`。
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

类型在全局命名空间，生成的界面脚本不需要 `using deVoid.UIFramework`。

## 生命周期

业务覆写这些方法（原 `OnPropertiesSet` / `WhileHiding` 已改名）：

| 方法 | 何时调用 |
|------|----------|
| `OnOpen` | 首次 `Show` |
| `OnHide` | 暂时隐藏：`HidePanel`，或 Window 被新窗盖住 |
| `OnClose` | Window 出栈（`CloseWindow` / `UI_Close`） |
| `OnResume` | 曾经打开过、隐藏后再 `Show` |

`AddListeners` / `RemoveListeners` 仍在 `Awake` / `OnDestroy`。开关是立即 `SetActive`，动画写在界面自己身上。

界面 Prefab 上勾选 **关闭后销毁**：`CloseWindow` / 勾选后的 `HidePanel` 会立刻 `Destroy`。下次 `Open`/`Show` 会按 Id 从登记的 Prefab 再实例化。Window 被其它窗盖住时的 `Hide` **不会**销毁。

Window 的弹窗 / 盖住时隐藏 / 排队，以及 Panel 的 Priority，都是控制器上的序列化字段，不再走 Properties 泛型。

用 `UISettings` 时会自动 `RegisterScreenPrefab`。手动注册要同时调 `uiFrame.RegisterScreenPrefab(id, prefab)`。

## 新建一个 Panel / Window

在界面 Prefab 根节点挂 `UiScreenGenerator`（`Add Component → UI/UI Screen Generator`）：

1. **类别** `Panel` 或 `Window`
2. **名称** 类名，如 `PreGamePanel`
3. **路径** 脚本目录，如 `Assets/Scripts/UI`
4. 子物体命名 `前缀_名字`（如 `Btn_Play`）
5. 点 **收集UI引用**，再点 **创建UI脚本**（字段写在 `// --tag_start: 自动生成--` 与 `// --tag_end: 自动生成--` 之间）
6. 再点 **创建UI脚本** 只改 tag 内字段，tag 外手写逻辑保留
7. 等编译结束后，把生成的脚本挂到同一节点，再点一次 **收集UI引用** 把字段填上

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
