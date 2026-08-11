---@diagnostic disable: undefined-global
--Lua日志:写入C#的ALog日志系统,自动附带debug.traceback,
--在 Window/AChen/日志控制台 里可点击堆栈跳转到Lua源码
Log = {}

local m_strDefaultCategory = "Lua"
local m_nLevelLog = 0
local m_nLevelWarning = 1
local m_nLevelError = 2

--level 3 表示traceback从调用Log.XXX的那一行开始
local function _write(nLevel, strMessage, strCategory)
    local strStack = debug.traceback("", 3)
    CS.ALog.LuaWrite(nLevel, strCategory or m_strDefaultCategory, tostring(strMessage), strStack)
end

function Log.Info(strMessage, strCategory)
    _write(m_nLevelLog, strMessage, strCategory)
end

function Log.Warn(strMessage, strCategory)
    _write(m_nLevelWarning, strMessage, strCategory)
end

function Log.Error(strMessage, strCategory)
    _write(m_nLevelError, strMessage, strCategory)
end

--主动报错:先记一条Error日志(带完整堆栈),再中断当前执行
function Log.Throw(strMessage, strCategory)
    _write(m_nLevelError, strMessage, strCategory)
    error(tostring(strMessage), 2)
end
