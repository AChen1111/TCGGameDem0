Main = {}
Main.__index = Main

--加载公共模块
require("Include")
require("Log")
require("LogCategories")
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
    --旧表身份以moduleList为准;require失败时_G会被清掉,但m_module仍持有引用
    local oldTable = _G[typeName] or m_module[typeName]
    if oldTable == nil then
        print("runtimeReload: module not loaded: " .. tostring(typeName))
        return
    end

    --只清package.loaded以强制重执行;不要先清_G,否则require失败后模块就丢了
    package.loaded[typeName] = nil
    require(typeName)

    --先清空旧表,保证源文件里已删除的函数不残留
    local newTable = _G[typeName]
    for k in pairs(oldTable) do
        oldTable[k] = nil
    end
    --把新表内容合并进旧表,保证所有已持有oldTable引用(包括各实例的元表)自动生效
    for k, v in pairs(newTable) do
        oldTable[k] = v
    end
    --合并进来的__index指向newTable,不改回来的话下次reload改的是oldTable而实例查的是newTable
    oldTable.__index = oldTable

    --把_G和package.loaded都恢复指向旧表,保证身份一致,便于下次reload
    _G[typeName] = oldTable
    package.loaded[typeName] = oldTable
end

--运行时重新加载全部模块
function Main.runtimeReloadAll()
    --先逐个更新已有模块,单个失败不影响其余模块
    for typeName in pairs(m_module) do
        local ok, err = pcall(Main.runtimeReload, typeName)
        if not ok then
            print("runtimeReloadAll failed: " .. tostring(typeName) .. " , " .. tostring(err))
        end
    end

    --再重跑Include与module,让新增模块进入moduleList(已有模块身份不变)
    package.loaded["Include"] = nil
    package.loaded["Log"] = nil
    package.loaded["LogCategories"] = nil
    package.loaded["module"] = nil
    require("Include")
    require("Log")
    require("LogCategories")
    require("module")
    m_module = moduleList
end

--返回模块名数组,供Editor侧列出
function Main.getModuleNames()
    local names = {}
    for typeName in pairs(m_module) do
        names[#names + 1] = typeName
    end
    return names
end

