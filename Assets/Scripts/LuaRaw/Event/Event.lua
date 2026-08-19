---@class EventListener
---@field target table
---@field fn fun(target: table, ...: any)

---@class Event 全局事件中心
Event = {}

---@type table<string, EventListener[]>
local m_map = {}

---订阅。回调为 fn(target, ...)
---@param strId string
---@param target table
---@param fn fun(target: table, ...: any)
function Event.Add(strId, target, fn)
    local list = m_map[strId]
    if list == nil then
        list = {}
        m_map[strId] = list
    end
    list[#list + 1] = { target = target, fn = fn }
end

---取消同一 strId + target + fn
---@param strId string
---@param target table
---@param fn fun(target: table, ...: any)
function Event.Remove(strId, target, fn)
    local list = m_map[strId]
    for i = #list, 1, -1 do
        local item = list[i]
        if item.target == target and item.fn == fn then
            table.remove(list, i)
        end
    end
end

---取消该 target 的全部订阅
---@param target table
function Event.RemoveByTarget(target)
    for _, list in pairs(m_map) do
        for i = #list, 1, -1 do
            if list[i].target == target then
                table.remove(list, i)
            end
        end
    end
end

---派发。本次回调列表在派发前快照，期间增删不影响本轮
---@param strId string
function Event.Dispatch(strId, ...)
    local list = m_map[strId]
    if list == nil then
        return
    end
    local snapshot = {}
    for i = 1, #list do
        snapshot[i] = list[i]
    end
    for i = 1, #snapshot do
        local item = snapshot[i]
        item.fn(item.target, ...)
    end
end
