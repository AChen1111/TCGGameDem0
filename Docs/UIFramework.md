# UI 框架（Lua UIFrame）— 已弃用

> **已归档（[ADR-007](decisions/ADR-007-archive-xlua.md)）**  
> Lua UIFrame 源码在 `Unused~/`。新界面用 C# deVoid UI，见 [DeVoidUI.md](DeVoidUI.md)。

---

Lua 侧管界面生命周期、分层、窗口栈与弹出队列；C# 只做 Prefab 实例化、字段绑定与点击转发。仿 [uiframework](https://github.com/yankooliveira/uiframework)。

启动时**不再**调用 `UIFrame.Init()`。存量界面若仍要用 Lua UIFrame，需自行调用；新界面走 [DeVoidUI.md](DeVoidUI.md)。

相关文档：[Lua 系统](LuaSystem.md) · [日志系统](LogSystem.md)

---

## 1. 概念

| 类型 | 用途 | 打开 / 关闭 |
|------|------|-------------|
| **Panel** | 可同时显示（HUD、血条） | `UIFrame.ShowPanel` / `HidePanel` |
| **Window** | 同一时间一个当前窗，走历史栈 | `UIFrame.OpenWindow` / `CloseWindow` |

`UIFrame.Close(id)` 按注册的 `kind` 自动选 Hide 或 Close。界面内部用 `self:Close()`。

关闭只 `SetActive(false)`，不 Destroy；下次打开复用同一实例。

---

## 2. 加一个界面（最短路径）

以窗口 `HomeWindow` 为例。

### 2.1 Prefab

1. 在 `Assets/Resources/UI/` 做 Prefab（路径对应 `Resources.Load`，如 `UI/HomeWindow`）。
2. 根节点挂 **`LuaUiComponent`**（不是 `LuaComponet`）。
3. **Type Name** 填 Lua 类型名：`HomeWindow`。
4. **Ui Binds**：`luaName` 用 `m_类型+名`（如 `m_btnClose`），拖 Button 到 **Component**。
5. 设 **Screen Kind** = Window；需要弹窗再勾 **Is Popup**。

### 2.2 Lua 模块

`Assets/Scripts/LuaRaw/UI/Screen/HomeWindow.lua`（`require` 只认文件名，目录随意，文件名全局唯一）：

```lua
---@class HomeWindow : BaseWindow
HomeWindow = {}
HomeWindow.__index = HomeWindow
setmetatable(HomeWindow, BaseWindow)

---绑定按钮等监听，实例化后由框架调用一次
function HomeWindow:AddListeners()
    self.m_uiComp:AddClick(self.m_btnClose, "OnCloseClicked")
end

---打开时属性已写入 m_props，在此刷新界面
---@param self HomeWindow
function HomeWindow:OnPropertiesSet()
    -- 读 self.m_props
end

function HomeWindow:OnCloseClicked()
    self:Close()
end
```

### 2.3 注册

`module.lua`：

```lua
require("HomeWindow")
moduleList["HomeWindow"] = HomeWindow
```

`UIConfig.lua`：

```lua
UIConfig = {
    framePrefab = "UI/UIFrame",
    screens = {
        HomeWindow = {
            prefab = "UI/HomeWindow",
            kind = "window",
            -- isPopup = true,
            -- hideOnForegroundLost = true,
            -- forceForeground = true,
        },
    }
}
```

打开：

```lua
UIFrame.OpenWindow("HomeWindow", { title = "首页" })
```

---

## 3. 对外 API

| API | 说明 |
|-----|------|
| `UIFrame.Init()` | 实例化 `UI/UIFrame`、DontDestroyOnLoad、按 `UIConfig` 注册。已 Init 则跳过。 |
| `UIFrame.RegisterScreen(id, cfg)` | 运行时补登记，不预实例化。 |
| `UIFrame.ShowPanel(id, props)` | 显示 Panel；`props` 写入 `m_props` 后调 `OnPropertiesSet`。 |
| `UIFrame.HidePanel(id)` | 隐藏 Panel。 |
| `UIFrame.OpenWindow(id, props)` | 打开 Window（见第 5 节栈/队列）。 |
| `UIFrame.CloseWindow(id)` | 只关**当前**窗；不是当前窗会 `Log.Warn` 并忽略。 |
| `UIFrame.Close(id)` | 按 `kind` 转发 Hide / Close。 |

---

## 4. 配置项（`UIScreenConfig`）

Prefab 上的 `LuaUiComponent` 会注入默认值；`UIConfig` 里写了的字段会覆盖。

| 字段 | 默认（组件） | 说明 |
|------|-------------|------|
| `prefab` | 必填 | `Resources` 路径，不含扩展名 |
| `kind` | 必填 | `"panel"` / `"window"` |
| `isPopup` | false | Window：挂到 PriorityWindowLayer，并显示 `PopupDarken` |
| `hideOnForegroundLost` | true | Window：被后来的窗盖住时是否先 AnimOut 藏起 |
| `forceForeground` | true | Window：已有当前窗时，true 立刻抢前台；false 进队列 |
| `panelPriority` | 0 | Panel：`> 0` 挂 PriorityPanelLayer，否则 PanelLayer |

---

## 5. Window：栈、队列、转场

- **当前窗再 Open 自己**：只更新 `m_props` 并 `OnPropertiesSet`，不重新入栈。
- **`forceForeground = false`** 且已有当前窗：入队，等当前窗 Close 后再开。
- **Close 当前窗**：先出队；队列空则弹出历史栈上一窗；都空则收遮罩。
- **Close 非当前窗**：警告，不关。
- **转场中**关掉 Canvas `GraphicRaycaster`，`AnimIn`/`AnimOut` 都结束后再打开，避免连点。

默认 `AnimIn` / `AnimOut` 是立刻显隐。自定义动画时**必须**在结束时调用 `fnDone`，否则射线会一直关着：

```lua
function HomeWindow:AnimIn(fnDone)
    self.gameObject:SetActive(true)
    -- 播完动画后再：
    fnDone()
end
```

---

## 6. 生命周期（Lua）

| 函数 | 何时 |
|------|------|
| `AddListeners` | 首次实例化一次（在隐藏前） |
| `OnPropertiesSet` | 每次 Show / Open，此时 `m_props` 已写入 |
| `WhileHiding` | 开始关动画前 |
| `AnimIn(fnDone)` / `AnimOut(fnDone)` | 开/关动画；结束后必须 `fnDone()` |
| `Close` | 界面内关闭自己 |
| `OnDestroy` | 物体销毁时调 `RemoveListeners` |

`LuaUiComponent` 仍转发 Unity 生命周期：`Awake` / `Start` / `OnEnable` / `OnDisable` / `OnDestroy`。

点击绑定推荐：

```lua
self.m_uiComp:AddClick(self.m_btnClose, "OnCloseClicked")
Event.Add(EventIds.GoldChanged, self, self.OnGoldChanged)
```

全局事件见 [Event.md](Event.md)。`OnDestroy` 会 `Event.RemoveByTarget(self)`。

不要在业务 Lua 写 `CS.xxx`。C# 类型别名放 `Include.lua`（已有 `GameObject`、`Object`、`Resources`、`LuaUiUtil`、`ALog`）。

---

## 7. 根节点 `UI/UIFrame`

`Assets/Resources/UI/UIFrame.prefab`，Canvas 下四层：

```
UIFrame
  PanelLayer
  PriorityPanelLayer
  WindowLayer
  PriorityWindowLayer
    PopupDarken
```

不要手改场景里再挂一套；Init 会 Instantiate 并 `DontDestroyOnLoad`。

---

## 8. C# 侧（一般不用改）

| 类型 | 职责 |
|------|------|
| `LuaUiComponent` | 继承 `LuaComponet`；UiBind、屏幕元数据、公开 `LuaTable`、`AddClick` |
| `LuaUiUtil.InstantiateUnder` | `Resources.Load` + Instantiate 到父节点 |
| `LuaUiUtil.SetRaycasterEnabled` | 转场开关 GraphicRaycaster |

UiBind：填了 **Component** 则注入该组件，否则注入 **target** GameObject。Editor 下改 Component 会自动同步 target。

---

## 9. 常见注意

- Prefab 必须挂 `LuaUiComponent`，`Type Name` 与 `moduleList` 键一致。
- 新界面三处都要写：Lua 文件、`module.lua`、`UIConfig.screens`。
- `require` 只按文件名；真机先 **Tools → Lua → Build LuaBundle**。
- `LuaManager` 须先于 UI 初始化（场景里 `SingletonManager` 挂 `LuaManager`，`m_sceneName` 可空表示不切场景）。
- 日志用 `Log.*`，见 [LogSystem.md](LogSystem.md)。
