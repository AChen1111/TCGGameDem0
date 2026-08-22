---@diagnostic disable: undefined-global
---@class Log 写到 C# ALog，堆栈可跳转 Lua 源码
Log = {}

local m_strDefaultCategory = "Lua"
local m_nLevelLog = 0
local m_nLevelWarning = 1
local m_nLevelError = 2

---写入 ALog，堆栈从调用 Log.XXX 的那一行开始
---@param nLevel number
---@param strMessage string
---@param strCategory string|nil
local function _write(nLevel, strMessage, strCategory)
    if not ALog.Enabled then
        return
    end
    local strStack = debug.traceback("", 3)
    ALog.LuaWrite(nLevel, strCategory or m_strDefaultCategory, tostring(strMessage), strStack)
end

---信息日志
---@param strMessage string
---@param strCategory string|nil
function Log.Info(strMessage, strCategory)
    _write(m_nLevelLog, strMessage, strCategory)
end

---警告日志
---@param strMessage string
---@param strCategory string|nil
function Log.Warn(strMessage, strCategory)
    _write(m_nLevelWarning, strMessage, strCategory)
end

---错误日志
---@param strMessage string
---@param strCategory string|nil
function Log.Error(strMessage, strCategory)
    _write(m_nLevelError, strMessage, strCategory)
end

---记一条 Error 后 error() 中断当前执行
---@param strMessage string
---@param strCategory string|nil
function Log.Throw(strMessage, strCategory)
    _write(m_nLevelError, strMessage, strCategory)
    error(tostring(strMessage), 2)
end
