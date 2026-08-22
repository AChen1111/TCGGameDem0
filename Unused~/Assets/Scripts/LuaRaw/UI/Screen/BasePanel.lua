---@class BasePanel : BaseScreen 可同时显示的面板基类
BasePanel = {}
BasePanel.__index = BasePanel
setmetatable(BasePanel, BaseScreen)
