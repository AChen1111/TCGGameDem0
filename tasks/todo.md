# TODO: Lua 运行时重置

## Phase 1: Lua 层重载正确性
- [x] Task 1: 修复 `Main.runtimeReload`（清旧 key + 修 `__index` 回指）— `Main.lua`
- [x] Task 2: 新增 `Main.runtimeReloadAll` 与 `Main.getModuleNames` — `Main.lua`

### Checkpoint
- [x] 连续两次 reload 均生效（Play 中实测 v1 → v2 → v3）
- [x] 删除的函数不残留

## Phase 2: C# 转发与实例刷新
- [x] Task 3: `LuaEnvironment.BuildFileIndex()` 改为 public（Editor 下刷新文件索引）
- [x] Task 4: `LuaManager.RuntimeReloadAll()` / `GetModuleNames()`
- [x] Task 5: `LuaComponet` 拆分 `RuntimeReload()` / `RefreshInstance()` + 暴露 `TypeName`

### Checkpoint
- [x] 编译无错
- [x] 运行期间新增的 `.lua` 注册进 module.lua 后能被 reloadAll 纳入

## Phase 3: Editor 入口
- [x] Task 6: `Tools/Lua/Lua重载窗口`（模块列表 + 单个重载 + 全部重载 + Ctrl+Shift+R）

### Checkpoint
- [ ] 待人工确认：在 Editor 里打开窗口点一次，确认 UI 与未激活对象的刷新数量

## 已决策
- 重置语义：只热更代码，保留实例运行时状态，不重跑 Awake/Start
- Editor 入口：EditorWindow，列出模块支持单个重载
