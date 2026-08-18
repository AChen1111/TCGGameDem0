# 事件中心（Event）使用说明

Lua 全局发布/订阅。用 `target + fn` 订阅，界面销毁时按 target 一键解绑。

`Main.lua` 已 `require("Event")` / `require("EventIds")`。`BaseScreen.OnDestroy` 会自动 `Event.RemoveByTarget(self)`，业务界面不用手写解绑。

相关文档：[Lua 系统](LuaSystem.md) · [deVoid UI](DeVoidUI.md) · [Lua UIFrame（已弃用）](UIFramework.md)

---

## 1. 快速开始

在 `EventIds.lua` 登记事件名：

```lua
EventIds = {
    GoldChanged = "GoldChanged",
}
```

订阅 / 派发：

```lua
function HomeWindow:AddListeners()
    Event.Add(EventIds.GoldChanged, self, self.OnGoldChanged)
end

function HomeWindow:OnGoldChanged(nGold)
    -- self 已作为 target 传入
end

-- 任意模块
Event.Dispatch(EventIds.GoldChanged, 100)
```

非 UI 模块自己管生命周期：

```lua
Event.Remove(EventIds.GoldChanged, self, self.OnGoldChanged)
-- 或一次性清掉该对象全部订阅
Event.RemoveByTarget(self)
```

---

## 2. API

| API | 说明 |
|-----|------|
| `Event.Add(strId, target, fn)` | 订阅。回调为 `fn(target, ...)` |
| `Event.Remove(strId, target, fn)` | 取消同一 id + target + fn |
| `Event.RemoveByTarget(target)` | 取消该 target 的全部订阅 |
| `Event.Dispatch(strId, ...)` | 派发；无监听者则跳过 |

派发前会快照监听列表：本轮回调里 Add/Remove 不影响本轮已快照的调用。

---

## 3. 约定

- 事件名用 `EventIds` 常量，不要散落字符串。
- UI 在 `AddListeners` 里 `Event.Add`；销毁走 `BaseScreen.OnDestroy`，不必在 `RemoveListeners` 里再解一次。
- 热重载全部 Lua **不会**重跑 `Event.lua`，避免把已有订阅清掉。改 `EventIds` 会重载。
- 改 `Event.lua` 本身后需重进 Play，或接受订阅被清空。
