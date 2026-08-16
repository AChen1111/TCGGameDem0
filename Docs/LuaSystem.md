# Lua 系统使用说明

本项目用 **XLua** 做 C# ↔ Lua 桥接：玩法/UI 优先写在 Lua，C# 负责环境、生命周期转发与 Inspector 注入。

相关文档：[UI 框架](UIFramework.md) · [事件中心](Event.md) · [日志系统](LogSystem.md) · [Lua Pad](LuaPad.md) · [Agent Skills 审计](AgentSkills.md)

---

## 1. 目录与职责

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/LuaComponet/LuaEnvironment.cs` | 创建 `LuaEnv`、CustomLoader、EmmyLua 连接 |
| `Assets/Scripts/LuaComponet/LuaManager.cs` | 单例；`require 'Main'`；热重载 API |
| `Assets/Scripts/LuaComponet/LuaComponet.cs` | 挂场景物体；按类型名建 Lua 表；注入引用；转发生命周期 |
| `Assets/Scripts/LuaComponet/Editor/` | Bundle 打包、热重载窗口、Console 报错跳转 |
| `Assets/Scripts/LuaRaw/` | Lua 源码根目录 |
| `Assets/Scripts/LuaRaw/Main.lua` | 入口：Init / 生命周期转发 / 热重载 |
| `Assets/Scripts/LuaRaw/module.lua` | `moduleList` 注册表 |
| `Assets/Scripts/LuaRaw/Include.lua` | 全局别名（如 `GameObject`、`Color`） |
| `Assets/Scripts/LuaRaw/Event/` | Event、EventIds（用法见 [Event.md](Event.md)） |
| `Assets/Scripts/LuaRaw/UI/Core/` | UIFrame、Layer、UIConfig（用法见 [UIFramework.md](UIFramework.md)） |
| `Assets/Scripts/LuaRaw/UI/Screen/` | BaseScreen / BasePanel / BaseWindow |

---

## 2. 加载约定

### require 只按文件名

`Assets/Scripts/LuaRaw/UI/Screen/BaseScreen.lua` → `require("BaseScreen")`，**不带路径**。

文件名全局唯一；重名时后扫描到的路径会覆盖，并打 Warning。

### Editor vs 真机

| 环境 | 加载方式 |
|------|----------|
| Editor | 递归扫描 `LuaRaw/**/*.lua`，按文件名索引；chunkname = **绝对路径**（便于报错跳转） |
| 真机 | `Resources/LuaBundle.bytes` 中的 luac 字节码 |

打包菜单：**Tools → Lua → Build LuaBundle**

### 入口

`LuaManager.OnInit` → `LuaEnvironment.Init()` → `require 'Main'` → 绑定 `Main.Init` / `runtimeReload` 等。

---

## 3. 写一个 Lua 模块

1. 在 `LuaRaw/`（或子目录）新建 `Foo.lua`：

```lua
Foo = {}
Foo.__index = Foo

function Foo:Awake()
    -- self.m_btnXxx 来自 Inspector 注入
end

function Foo:OnClicked()
end
```

2. 在 `module.lua` 注册：

```lua
require("Foo")
moduleList["Foo"] = Foo
```

3. 场景物体挂 `LuaComponet`，`Type Name` 填 `Foo`。

4. Inspector 里配置：
   - **Object References**：`name` → Lua 字段名（如 `m_btnConfirm`），拖入组件。也可在 Hierarchy / Inspector 右键 **AddToLuaComponet**，把当前物体或组件写入目标 `LuaComponet`（同名自动加 `_1`）
   - **Data References**：int/float/string/bool 注入

### 生命周期

`LuaComponet` 会转发：`Awake` / `Start` / `OnEnable` / `OnDisable` / `OnDestroy`。  
模块未定义对应函数则跳过。也可用 `CallLuaFunction("自定义名")` 调其它函数。

### 命名规范

- 成员：`m_类型+变量名`（`m_nCount`、`m_strName`、`m_bReady`、`m_btnConfirm`）
- 私有函数：以 `_` 开头（`_updateView`）

---

## 4. 热重载（仅 Play）

| 入口 | 作用 |
|------|------|
| **Tools → Lua → Lua重载窗口** | 按模块单个重载 |
| **Tools → Lua → 重载全部Lua**（Ctrl+Shift+R） | 全部已注册模块 + 重跑 Include/Log/module |
| Inspector `LuaComponet`「重写读取脚本」 | 重载本模块并刷新本实例 |

行为要点：

- 重载前会 `BuildFileIndex()`，运行中新增的 `.lua` 也能被 require
- 模块表**原地更新**，已有实例元表仍指向旧表身份，代码生效、运行时状态保留
- 不自动重跑生命周期；只刷新函数缓存与 Inspector 注入

---

## 5. 报错跳转

**Tools → Lua → 启用Lua报错跳转**（默认开）。

XLua 的 `LuaException` 双击 Console 时，会解析日志里的 `绝对路径.lua:行号:`，用外部编辑器打开真正的 Lua 源码（而不是跳到 XLua C# 抛错点）。

前提：CustomLoader 把 chunkname 设为绝对路径（本项目已做）。

分类日志控制台也会解析堆栈并支持点击跳转，见 [LogSystem.md](LogSystem.md)。

---

## 6. EmmyLua 调试（可选）

`LuaEnvironment.EnableEmmyLuaDebug`（默认 true）：Editor 启动时尝试连接本机 EmmyLua（9966）。

流程：IDE 先 F5 监听 → 再进 Unity Play。找不到 `emmy_core.dll` 只会 Warning，不影响运行。

---

## 7. 调用链小结

```
场景物体 LuaComponet
    → LuaManager.GetLuaTable(typeName, go)
        → Main.Init → moduleList[typeName] 建表
    → 注入 Object/Data References
    → CallLuaFunction("Awake"/…)

热重载
    → LuaManager.RuntimeReload(typeName)
        → Main.runtimeReload → 合并进旧表
    → LuaComponet.RefreshInstance
```

---

## 8. Lua Pad

Play / Windows 真机按 **F10** 打开独立 Web 窗口，点 **运行** 把草稿 `DoString` 进当前 XLua。实现过程、架构与已知坑见 [LuaPad.md](LuaPad.md)。

---

## 9. 常见注意

- `LuaManager` 须先于 `LuaComponet` 初始化（Script Execution Order / 单例就绪）
- 真机务必先 Build LuaBundle，否则找不到模块
- 新增模块别忘了写进 `module.lua` 的 `moduleList`
- 日志请用 `Log.*` / `LogCategories.*`，不要只靠 `print`（见日志文档）
