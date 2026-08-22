---@class UILayerEntry Layer 内已注册界面
---@field cfg UIScreenConfig
---@field luaTable BaseScreen|nil

---@class PanelLayer 可同时显示的面板层
---@field m_tfPanel UnityEngine.Transform
---@field m_tfPriority UnityEngine.Transform
---@field m_map table<string, UILayerEntry>
PanelLayer = {}
PanelLayer.__index = PanelLayer

---创建 Panel 层
---@param tfPanel UnityEngine.Transform
---@param tfPriority UnityEngine.Transform
---@return PanelLayer
function PanelLayer.New(tfPanel, tfPriority)
    local self = setmetatable({}, PanelLayer)
    self.m_tfPanel = tfPanel
    self.m_tfPriority = tfPriority
    self.m_map = {}
    return self
end

---登记 Panel，首次 Show 再实例化
---@param strId string
---@param cfg UIScreenConfig
function PanelLayer:Register(strId, cfg)
    self.m_map[strId] = { cfg = cfg, luaTable = nil }
end

---显示 Panel，按 Priority 挂到对应 Para-Layer
---@param strId string
---@param props table|nil
function PanelLayer:Show(strId, props)
    local screen = self:_ensure(strId)
    local nPri = screen.m_nPanelPriority or 0
    local tfParent = nPri > 0 and self.m_tfPriority or self.m_tfPanel
    screen.gameObject.transform:SetParent(tfParent, false)
    screen.m_props = props
    screen:OnPropertiesSet()
    screen:AnimIn(function() end)
end

---隐藏 Panel，不 Destroy
---@param strId string
function PanelLayer:Hide(strId)
    local entry = self.m_map[strId]
    local screen = entry.luaTable
    screen:WhileHiding()
    screen:AnimOut(function()
        screen.gameObject:SetActive(false)
    end)
end

---懒加载实例，已有则复用
---@param strId string
---@return BaseScreen
function PanelLayer:_ensure(strId)
    local entry = self.m_map[strId]
    if entry.luaTable ~= nil then
        return entry.luaTable
    end
    local cfg = entry.cfg
    local nPri = cfg.panelPriority or 0
    local tfParent = nPri > 0 and self.m_tfPriority or self.m_tfPanel
    local screen = UIFrame._instantiate(strId, cfg, tfParent)
    entry.luaTable = screen
    return screen
end
