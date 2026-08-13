# Implementation Plan: Lua 运行时重置(单个 / 全部)

## Overview

在 Play 模式下，不重启游戏就能重新加载 Lua 模块源码并让已存在的实例立即生效。范围包括：修复现有 `Main.runtimeReload` 的缺陷、新增 `Main.runtimeReloadAll`、让 `LuaComponet` 的按钮走统一入口、以及新增 Editor 菜单/快捷键触发全量重置。

## 现状分析

| 位置 | 现状 |
|------|------|
| `Main.lua: runtimeReload` | 清 `package.loaded` → `require` → 把新表合并进旧表 |
| `LuaManager` | 缓存 `Main.Init` / `Main.runtimeReload` 两个委托 |
| `LuaComponet.RuntimeReload` | Odin `[Button]`，重载后清空委托缓存并重新注入引用 |
| `LuaEnvironment` | Editor 下 `m_fileIndex` 只在首次 loader 调用时构建，之后永不刷新 |

`_G` 方案本身成立：`Main.Init` 用 `setmetatable(instance, module)`，实例持有的是模块表的**引用**，只要保持模块表身份不变（原地改内容），所有实例自动指向新代码。

### 已发现的 3 个缺陷

1. **`__index` 指向被污染（会让第二次 reload 失效）**
   合并循环 `for k, v in pairs(newTable) do oldTable[k] = v end` 会把 `newTable.__index = newTable` 一起拷进来，导致 `oldTable.__index` 指向新表。下一次 reload 时改的是 `oldTable`，而实例查找走的是上一轮的 `newTable`，改动不生效。合并后必须把 `oldTable.__index` 重新指回 `oldTable`。

2. **不清理已删除的 key**
   只做覆盖式合并，源文件里被删掉的函数会永远残留在旧表上。

3. **Editor 文件索引不刷新**
   `LuaEnvironment.m_fileIndex` 构建一次后缓存，运行期间新增的 `.lua` 文件 require 不到。

## Architecture Decisions

- **重置粒度由 Lua 层负责，C# 只做转发和实例刷新。** `Main` 提供 `runtimeReload(typeName)` 与 `runtimeReloadAll()` 两个入口，C# 侧不感知 `_G` 细节。
- **保持模块表身份不变（原地更新）。** 不重建模块表，避免遍历所有实例改 metatable。
- **全量重置的顺序：** 先逐个重载 `moduleList` 中已有模块 → 再重新执行 `Include` 与 `module`，让新增模块能进入 `moduleList`，且已有模块在 `moduleList` 中的身份不变。
- **实例刷新统一走 `LuaComponet`。** 代码重载后由组件负责重建委托缓存、重新注入 Inspector 引用。
- **重置语义 = 只热更代码。** 保留实例上已有的运行时状态，不重跑 `Awake` / `Start`，不重建实例表。
- **Editor 入口 = EditorWindow。** 窗口列出所有模块，支持勾选单个重载与一键全部重载。

## Task List

### Phase 1: Lua 层重载正确性

#### Task 1: 修复 `Main.runtimeReload`

**Description:** 修正合并逻辑，保证多次重载都生效且不残留旧 key。

**Acceptance criteria:**
- [ ] 合并前清空 `oldTable` 的所有 key
- [ ] 合并后 `oldTable.__index == oldTable`
- [ ] `_G[typeName]` 与 `package.loaded[typeName]` 都恢复指向 `oldTable`

**Verification:**
- [ ] 手动：Play 中连续两次修改 `BaseUI:OnClicked` 的打印内容并 reload，两次都生效
- [ ] 手动：删掉 `BaseUI` 的一个函数后 reload，调用该函数报 not found

**Dependencies:** None
**Files:** `Assets/Scripts/LuaRaw/Main.lua`
**Scope:** XS

#### Task 2: 新增 `Main.runtimeReloadAll`

**Description:** 遍历 `moduleList` 逐个复用 `runtimeReload`，随后重新执行 `Include` 与 `module` 以纳入新增模块。

**Acceptance criteria:**
- [ ] 遍历 `moduleList` 的 key 逐个调用 `runtimeReload`
- [ ] 之后清掉 `Include` / `module` 的 `package.loaded` 并重新 require
- [ ] 重建后 `moduleList[name]` 仍是原模块表（身份未变）
- [ ] 另提供 `Main.getModuleNames()` 返回模块名数组，供 EditorWindow 列表使用

**Verification:**
- [ ] 手动：同时改两个模块，一次 reloadAll 后都生效
- [ ] 手动：新增一个 `.lua` 并注册进 `module.lua`，reloadAll 后 `moduleList` 中出现该模块

**Dependencies:** Task 1
**Files:** `Assets/Scripts/LuaRaw/Main.lua`
**Scope:** XS

### Checkpoint: Lua 层
- [ ] 在 Console 用 `LuaManager` 手动触发两个入口均无报错
- [ ] 编辑器不报 Lua 语法错

### Phase 2: C# 转发与实例刷新

#### Task 3: `LuaEnvironment` 支持刷新文件索引

**Description:** 暴露 Editor 下重建 `m_fileIndex` 的方法，供全量重置前调用，让运行期间新增的 `.lua` 能被 require 到。

**Acceptance criteria:**
- [ ] `RebuildFileIndex()` 为 public，非 Editor 平台为空实现或不参与编译
- [ ] 调用后新增文件可被 loader 找到

**Verification:**
- [ ] 构建通过（Editor 与 Player 两种编译分支）
- [ ] 手动：Play 中新建 `.lua`，reloadAll 后可 require

**Dependencies:** None
**Files:** `Assets/Scripts/LuaComponet/LuaEnvironment.cs`
**Scope:** XS

#### Task 4: `LuaManager` 增加全量重置转发

**Description:** 缓存 `Main.runtimeReloadAll` / `Main.getModuleNames` 委托并暴露 `RuntimeReloadAll()` 与 `GetModuleNames()`，重载前先调 `LuaEnvironment.RebuildFileIndex()`。

**Acceptance criteria:**
- [ ] `RuntimeReloadAll()` 与 `GetModuleNames()` 可用
- [ ] 调用顺序为「刷新文件索引 → Lua 重载」（单个重载同样先刷新索引）

**Verification:**
- [ ] 构建通过
- [ ] 手动：从 Editor 菜单触发无异常

**Dependencies:** Task 2, Task 3
**Files:** `Assets/Scripts/LuaComponet/LuaManager.cs`
**Scope:** XS

#### Task 5: 整理 `LuaComponet` 的重置入口

**Description:** 把现有 `RuntimeReload` 拆成「重载代码」与「刷新本实例」两步，新增只刷新实例、不重复重载代码的方法，供全量重置批量调用（避免 N 个组件把同一模块重载 N 次）。

**Acceptance criteria:**
- [ ] `RuntimeReload()`：重载本 `m_typeName` 的代码 + 刷新本实例
- [ ] `RefreshInstance()`：只清委托缓存 + 重新注入 Inspector 引用，不重跑生命周期、不重建实例表
- [ ] 暴露 `TypeName` 供 Editor 侧按模块名筛选实例
- [ ] 非 Play 模式下点击按钮不产生空引用

**Verification:**
- [ ] 构建通过
- [ ] 手动：Inspector 按钮改脚本后点击立即生效
- [ ] 手动：ObjectReference / DataReference 在重置后仍正确注入

**Dependencies:** Task 4
**Files:** `Assets/Scripts/LuaComponet/LuaComponet.cs`
**Scope:** S

### Checkpoint: C# 层
- [ ] 编译无错误无警告
- [ ] 单个重置端到端可用

### Phase 3: Editor 入口

#### Task 6: Lua 重载 EditorWindow

**Description:** 新增 `Tools/Lua/Lua Reload Window`，列出 `GetModuleNames()` 返回的所有模块，每行一个「重载」按钮，顶部一个「全部重载」按钮。重载后对受影响的 `LuaComponet`（含未激活对象）调 `RefreshInstance()`。

**Acceptance criteria:**
- [ ] 非 Play 模式下窗口显示提示、按钮禁用
- [ ] 单个模块重载只刷新 `TypeName` 匹配的实例
- [ ] 用 `Resources.FindObjectsOfTypeAll<LuaComponet>()` 覆盖未激活对象，用 `gameObject.scene.IsValid()` 过滤 Prefab 资产
- [ ] 完成后窗口内或 Console 显示刷新的实例数量
- [ ] 「全部重载」绑定快捷键

**Verification:**
- [ ] 手动：改两个模块 + 一个未激活对象上的组件，点「全部重载」后都生效
- [ ] 手动：只点某个模块的「重载」，其他模块的实例不受影响
- [ ] 手动：非 Play 模式下按钮为灰

**Dependencies:** Task 5
**Files:** `Assets/Scripts/LuaComponet/Editor/LuaReloadWindow.cs`
**Scope:** S

### Checkpoint: Complete
- [ ] 单个重置与全部重置均可用
- [ ] 连续多次重置稳定生效
- [ ] 不影响真机分支编译

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| 实例状态（self 上的字段）在重置后保留，可能与新代码不一致 | Med | 已定为「只热更代码」语义；需要清状态时手动重进 Play |
| 模块表被闭包 upvalue 捕获导致旧代码残留 | Med | 现有代码只通过 `_G`/`moduleList` 访问模块表，暂不处理；若后续出现再引入模块注册表 |
| `Resources.FindObjectsOfTypeAll` 捞到 Prefab 资产上的组件 | Med | 用 `gameObject.scene.IsValid()` 过滤 |
| 全量重置时某个模块 require 报错中断整体流程 | Med | Lua 侧用 `pcall` 包裹单个模块的重载，失败只打印并继续 |

## Open Questions

无（重置语义与 Editor 入口形态已确认）。
