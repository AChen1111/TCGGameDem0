---@class UIWindowQueueItem 等待弹出的窗口
---@field strId string
---@field props table|nil

---@class WindowLayer 窗口层：历史栈、弹出队列、转场挡点击
---@field m_tfWindow UnityEngine.Transform
---@field m_tfPriority UnityEngine.Transform
---@field m_goDarken UnityEngine.GameObject
---@field m_goCanvas UnityEngine.GameObject
---@field m_map table<string, UILayerEntry>
---@field m_history string[]
---@field m_queue UIWindowQueueItem[]
---@field m_current BaseScreen|nil
---@field m_nTransition number
WindowLayer = {}
WindowLayer.__index = WindowLayer

---创建 Window 层
---@param tfWindow UnityEngine.Transform
---@param tfPriority UnityEngine.Transform
---@param goDarken UnityEngine.GameObject
---@param goCanvas UnityEngine.GameObject
---@return WindowLayer
function WindowLayer.New(tfWindow, tfPriority, goDarken, goCanvas)
    local self = setmetatable({}, WindowLayer)
    self.m_tfWindow = tfWindow
    self.m_tfPriority = tfPriority
    self.m_goDarken = goDarken
    self.m_goCanvas = goCanvas
    self.m_map = {}
    self.m_history = {}
    self.m_queue = {}
    self.m_current = nil
    self.m_nTransition = 0
    return self
end

---登记 Window，首次 Open 再实例化
---@param strId string
---@param cfg UIScreenConfig
function WindowLayer:Register(strId, cfg)
    self.m_map[strId] = { cfg = cfg, luaTable = nil }
end

---打开窗口；非 ForceForeground 且已有当前窗则入队
---@param strId string
---@param props table|nil
function WindowLayer:Open(strId, props)
    local screen = self:_ensure(strId)
    UIFrame._applyConfig(screen, self.m_map[strId].cfg)
    if self.m_current ~= nil and self.m_current.m_strScreenId == strId then
        screen.m_props = props
        screen:OnPropertiesSet()
        return
    end
    if self.m_current ~= nil and not screen.m_bForceForeground then
        self.m_queue[#self.m_queue + 1] = { strId = strId, props = props }
        return
    end
    self:_doOpen(strId, props)
end

---关闭当前窗口；非当前窗则警告。关完后出队或回退历史
---@param strId string
function WindowLayer:Close(strId)
    if self.m_current == nil or self.m_current.m_strScreenId ~= strId then
        Log.Warn("CloseWindow: " .. tostring(strId) .. " is not the current window")
        return
    end
    local closing = self.m_current
    self:_hide(closing, function()
        self.m_current = nil
        if #self.m_queue > 0 then
            local queued = table.remove(self.m_queue, 1)
            self:_doOpen(queued.strId, queued.props)
        elseif #self.m_history > 0 then
            local strPrevId = table.remove(self.m_history)
            self:_doOpen(strPrevId, nil)
        else
            self:_syncPopupMask()
        end
    end)
end

---真正打开：当前窗入历史，按 HideOnForegroundLost 决定是否藏起
---@param strId string
---@param props table|nil
function WindowLayer:_doOpen(strId, props)
    local nextScreen = self:_ensure(strId)
    if self.m_current ~= nil then
        local old = self.m_current
        self.m_history[#self.m_history + 1] = old.m_strScreenId
        if old.m_bHideOnForegroundLost then
            self:_hide(old, function() end)
        end
    end
    self.m_current = nextScreen
    self:_show(nextScreen, props)
    self:_syncPopupMask()
end

---写入属性并播打开动画
---@param screen BaseScreen
---@param props table|nil
function WindowLayer:_show(screen, props)
    screen.m_props = props
    screen:OnPropertiesSet()
    self:_beginTransition()
    screen:AnimIn(function()
        self:_endTransition()
    end)
end

---播关闭动画后隐藏，不 Destroy
---@param screen BaseScreen
---@param fnDone fun()
function WindowLayer:_hide(screen, fnDone)
    self:_beginTransition()
    screen:WhileHiding()
    screen:AnimOut(function()
        screen.gameObject:SetActive(false)
        self:_endTransition()
        fnDone()
    end)
end

---懒加载实例，Popup 挂到优先层
---@param strId string
---@return BaseScreen
function WindowLayer:_ensure(strId)
    local entry = self.m_map[strId]
    if entry.luaTable ~= nil then
        return entry.luaTable
    end
    local cfg = entry.cfg
    local tfParent = cfg.isPopup and self.m_tfPriority or self.m_tfWindow
    local screen = UIFrame._instantiate(strId, cfg, tfParent)
    entry.luaTable = screen
    return screen
end

---当前是 Popup 时显示遮罩并压到窗口下方
function WindowLayer:_syncPopupMask()
    local bPopup = self.m_current ~= nil and self.m_current.m_bIsPopup
    self.m_goDarken:SetActive(bPopup)
    if bPopup then
        self.m_goDarken.transform:SetAsFirstSibling()
        self.m_current.gameObject.transform:SetAsLastSibling()
    end
end

---转场开始，第一段时关掉射线避免连点
function WindowLayer:_beginTransition()
    self.m_nTransition = self.m_nTransition + 1
    if self.m_nTransition == 1 then
        LuaUiUtil.SetRaycasterEnabled(self.m_goCanvas, false)
    end
end

---转场结束，全部结束后重新打开射线
function WindowLayer:_endTransition()
    self.m_nTransition = self.m_nTransition - 1
    if self.m_nTransition == 0 then
        LuaUiUtil.SetRaycasterEnabled(self.m_goCanvas, true)
    end
end
