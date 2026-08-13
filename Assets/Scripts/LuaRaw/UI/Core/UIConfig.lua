---@class UIScreenConfig 界面注册配置
---@field prefab string Resources 路径
---@field kind '"panel"'|'"window"'
---@field isPopup boolean|nil
---@field hideOnForegroundLost boolean|nil
---@field forceForeground boolean|nil
---@field panelPriority number|nil

---@class UIConfig UI 注册表，替代原 UISettings
---@field framePrefab string
---@field screens table<string, UIScreenConfig>
UIConfig = {
    framePrefab = "UI/UIFrame",
    screens = {}
}
