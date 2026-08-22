require("BaseScreen")
require("BasePanel")
require("BaseWindow")
require("UIConfig")
require("PanelLayer")
require("WindowLayer")
require("UIFrame")
require("PreGamePanel")

---@type table<string, table>
moduleList = {}
---@diagnostic disable-next-line: undefined-global
moduleList["PreGamePanel"] = PreGamePanel
