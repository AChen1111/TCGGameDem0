---@class BaseScreen 屏幕基类，Panel/Window 的公共生命周期
---@field gameObject UnityEngine.GameObject
---@field m_strScreenId string
---@field m_props table|nil
---@field m_uiComp LuaUiComponent
---@field m_nScreenKind number
---@field m_bIsPopup boolean
---@field m_bHideOnForegroundLost boolean
---@field m_bForceForeground boolean
---@field m_nPanelPriority number
BaseScreen = {}
BaseScreen.__index = BaseScreen

---绑定按钮等监听，实例化后由框架调用一次
function BaseScreen:AddListeners()
end

---移除监听，销毁时由 OnDestroy 调用
function BaseScreen:RemoveListeners()
end

---打开时属性已写入 m_props，在此刷新界面
function BaseScreen:OnPropertiesSet()
end

---开始关闭动画前的清理
function BaseScreen:WhileHiding()
end

---打开动画，完成后必须调用 fnDone
---@param fnDone fun()
function BaseScreen:AnimIn(fnDone)
    self.gameObject:SetActive(true)
    fnDone()
end

---关闭动画，完成后必须调用 fnDone
---@param fnDone fun()
function BaseScreen:AnimOut(fnDone)
    fnDone()
end

---从界面内部关闭自己
function BaseScreen:Close()
    UIFrame.Close(self.m_strScreenId)
end

---GameObject 销毁时解绑监听
function BaseScreen:OnDestroy()
    self:RemoveListeners()
end
