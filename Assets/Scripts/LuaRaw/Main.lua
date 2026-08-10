Main = {}
Main.__index = Main

--加载公共模块
require("Include")
require("module")
--模块列表
local m_module = moduleList

--根据名称初始化一个table并返回
function Main.Init(typeName,go)
    local module = m_module[typeName]
    if module == nil then
        print("module not found: " .. tostring(typeName))
        return nil
    end
    local table = {}
    setmetatable(table, module)
    table.gameObject = go
    return table
end

--根据名称调用func
function Main.CallFunction(table, functionName)
    local func = table[functionName]
    if func ~= nil then
        func(table)
    else
        print("function not found: " .. functionName)
    end
end

--生命周期转发:模块未定义对应函数时跳过
local function callLifecycle(table, name)
    local func = table[name]
    if func ~= nil then
        func(table)
    end
end

--运行时重新加载
function Main.runtimeReload(typeName)
    --从全局环境_G中获取旧表(moduleList等外部引用都指向这张表)
    local oldTable = _G[typeName]
    if oldTable == nil then
        print("runtimeReload: module not loaded: " .. tostring(typeName))
        return
    end

    --清掉缓存,强制require重新执行源文件
    package.loaded[typeName] = nil
    _G[typeName] = nil
    require(typeName)

    --把新表内容合并进旧表,保证所有已持有oldTable引用(包括各实例的元表)自动生效
    local newTable = _G[typeName]
    for k, v in pairs(newTable) do
        oldTable[k] = v
    end

    --把_G和package.loaded都恢复指向旧表,保证身份一致,便于下次reload
    _G[typeName] = oldTable
    package.loaded[typeName] = oldTable
end

