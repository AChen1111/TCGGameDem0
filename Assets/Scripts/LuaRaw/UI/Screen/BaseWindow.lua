---@class BaseWindow : BaseScreen 走历史栈的窗口基类
BaseWindow = {}
BaseWindow.__index = BaseWindow
setmetatable(BaseWindow, BaseScreen)
