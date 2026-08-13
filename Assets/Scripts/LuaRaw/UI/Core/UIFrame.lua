---@class UIFrame 对外唯一入口，路由到 PanelLayer / WindowLayer
UIFrame = {}

---@type boolean
local m_bInited = false
---@type UnityEngine.GameObject
local m_goCanvas
---@type PanelLayer
local m_panelLayer
---@type WindowLayer
local m_windowLayer

---用配置覆盖屏幕上的默认属性
---@param screen BaseScreen
---@param cfg UIScreenConfig
function UIFrame._applyConfig(screen, cfg)
    if cfg.isPopup ~= nil then
        screen.m_bIsPopup = cfg.isPopup
    end
    if cfg.hideOnForegroundLost ~= nil then
        screen.m_bHideOnForegroundLost = cfg.hideOnForegroundLost
    end
    if cfg.forceForeground ~= nil then
        screen.m_bForceForeground = cfg.forceForeground
    end
    if cfg.panelPriority ~= nil then
        screen.m_nPanelPriority = cfg.panelPriority
    end
end

---实例化 Prefab、注入绑定、调用 AddListeners，默认隐藏
---@param strId string
---@param cfg UIScreenConfig
---@param tfParent UnityEngine.Transform
---@return BaseScreen
function UIFrame._instantiate(strId, cfg, tfParent)
    local comp = LuaUiUtil.InstantiateUnder(cfg.prefab, tfParent)
    local go = comp.gameObject
    if not go.activeSelf then
        go:SetActive(true)
    end
    local screen = comp.LuaTable
    screen.m_strScreenId = strId
    UIFrame._applyConfig(screen, cfg)
    screen:AddListeners()
    go:SetActive(false)
    return screen
end

---创建 UIFrame 根节点并按 UIConfig 注册界面
function UIFrame.Init()
    if m_bInited then
        return
    end
    local prefab = Resources.Load(UIConfig.framePrefab)
    local go = Object.Instantiate(prefab)
    go.name = "UIFrame"
    Object.DontDestroyOnLoad(go)
    m_goCanvas = go
    local tf = go.transform
    m_panelLayer = PanelLayer.New(tf:Find("PanelLayer"), tf:Find("PriorityPanelLayer"))
    m_windowLayer = WindowLayer.New(
        tf:Find("WindowLayer"),
        tf:Find("PriorityWindowLayer"),
        tf:Find("PriorityWindowLayer/PopupDarken").gameObject,
        go
    )
    for strId, cfg in pairs(UIConfig.screens) do
        UIFrame.RegisterScreen(strId, cfg)
    end
    m_bInited = true
end

---按 kind 注册到对应 Layer，不预实例化
---@param strId string
---@param cfg UIScreenConfig
function UIFrame.RegisterScreen(strId, cfg)
    if cfg.kind == "panel" then
        m_panelLayer:Register(strId, cfg)
    else
        m_windowLayer:Register(strId, cfg)
    end
end

---显示 Panel
---@param strId string
---@param props table|nil
function UIFrame.ShowPanel(strId, props)
    m_panelLayer:Show(strId, props)
end

---隐藏 Panel
---@param strId string
function UIFrame.HidePanel(strId)
    m_panelLayer:Hide(strId)
end

---打开 Window
---@param strId string
---@param props table|nil
function UIFrame.OpenWindow(strId, props)
    m_windowLayer:Open(strId, props)
end

---关闭 Window
---@param strId string
function UIFrame.CloseWindow(strId)
    m_windowLayer:Close(strId)
end

---按注册 kind 关闭 Panel 或 Window
---@param strId string
function UIFrame.Close(strId)
    local cfg = UIConfig.screens[strId]
    if cfg.kind == "panel" then
        UIFrame.HidePanel(strId)
    else
        UIFrame.CloseWindow(strId)
    end
end
