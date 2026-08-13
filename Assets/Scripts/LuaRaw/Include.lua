---@diagnostic disable: undefined-global
---@class UnityEngine.GameObject
---@field name string
---@field transform UnityEngine.Transform
---@field activeSelf boolean
---@field SetActive fun(self: UnityEngine.GameObject, value: boolean)

---@class UnityEngine.Transform
---@field gameObject UnityEngine.GameObject
---@field Find fun(self: UnityEngine.Transform, name: string): UnityEngine.Transform
---@field SetParent fun(self: UnityEngine.Transform, parent: UnityEngine.Transform, worldPositionStays: boolean)
---@field SetAsFirstSibling fun(self: UnityEngine.Transform)
---@field SetAsLastSibling fun(self: UnityEngine.Transform)

---@class UnityEngine.Object
---@field Instantiate fun(original: UnityEngine.Object): UnityEngine.GameObject
---@field DontDestroyOnLoad fun(target: UnityEngine.Object)

---@class UnityEngine.Resources
---@field Load fun(path: string): UnityEngine.Object

---@class LuaUiComponent
---@field gameObject UnityEngine.GameObject
---@field LuaTable BaseScreen
---@field TypeName string
---@field AddClick fun(self: LuaUiComponent, button: any, methodName: string)

---@class LuaUiUtil
---@field InstantiateUnder fun(prefabPath: string, parent: UnityEngine.Transform): LuaUiComponent
---@field SetRaycasterEnabled fun(canvasGo: UnityEngine.GameObject, enabled: boolean)

---@class ALog
---@field Enabled boolean
---@field LuaWrite fun(nLevel: number, strCategory: string, strMessage: string, strStack: string)

GameObject = CS.UnityEngine.GameObject
Color = CS.UnityEngine.Color
Object = CS.UnityEngine.Object
Resources = CS.UnityEngine.Resources
LuaUiUtil = CS.LuaUiUtil
ALog = CS.ALog
