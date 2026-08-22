# 日志系统（LogSystem）使用说明

分类日志 + UI Toolkit 控制台。Lua 路径已随 XLua 归档（[ADR-007](decisions/ADR-007-archive-xlua.md)），现行只走 C# `ALog`。

菜单：**Window → AChen → 日志控制台**

相关文档：[Lua 系统](LuaSystem.md) · [Agent Skills 审计](AgentSkills.md)

---

## 1. 快速开始

### Lua

`Main.lua` 已 `require("Log")` / `require("LogCategories")`，业务里直接用：

```lua
Log.Info("普通日志")
Log.Warn("警告", "UI")                 -- 第二参数为分类，默认 "Lua"
Log.Error("错误", LogCategories.Net)   -- 推荐用配置生成的常量
Log.Throw("主动报错")                  -- 先记 Error 日志，再 error() 中断
```

### C#

```csharp
ALog.Log("普通日志");
ALog.LogWarning("警告", "UI");
ALog.LogError("错误", ALogCategories.Net);
```

分类字符串会出现在控制台左侧过滤列表；未配置过的分类，第一次打日志时也会自动出现。

---

## 2. 控制台功能

| 功能 | 说明 |
|------|------|
| Clear | 清空当前会话日志 |
| 配置分类 | 填写「显示名 + 英文变量名」，生成映射类 |
| 出包启用日志 | 关闭后正式包内不再打 ALog / Log 日志（Editor 仍启用） |
| 搜索框 | 按消息内容过滤 |
| Log / Warning / Error | 按等级过滤，并显示条数；Warning 黄字黄底，Error 红字红底 |
| 左侧分类勾选 | 按分类显示/隐藏 |
| 详情区堆栈行 | 点击可跳转到 C# / Lua 源码行 |
| 全选 / 取消全选 | 勾选或清空当前过滤列表里的条目，配合「复制」使用 |
| 调用堆栈图 | 打开竖向调用链；无堆栈时显示「无堆栈信息」 |

---

## 3. 配置报错分类

1. 打开日志控制台 → **配置分类**
2. 每行填写：
   - **显示名**：控制台里看到的分类（也是打日志时传入的字符串），如 `网络`
   - **英文变量名**：合法 C#/Lua 标识符，如 `Net`
3. 点 **生成映射类**

生成文件：

- `Assets/Scripts/LogSystem/Runtime/ALogCategories.cs`
- 配置落盘：`Assets/Scripts/LogSystem/Editor/ALogCategoryConfig.json`

示例生成结果：

```csharp
public static class ALogCategories
{
    public const string Net = "网络";
}
```

```lua
LogCategories = {
    Net = "网络",
}
```

用法：

```csharp
ALog.LogError("超时", ALogCategories.Net);
```

```lua
Log.Error("超时", LogCategories.Net)
```

> 这两个文件由工具生成，请勿手改；改分类请走控制台重新生成。

---

## 4. 出包关闭日志

控制台工具栏 **出包启用日志**：

- **勾选**（默认）：正式包正常写日志并转发 `Debug.Log`
- **取消勾选**：正式包内 `ALog.*` / `Log.*` 直接跳过；不挂接原生日志监听

设置资源：`Assets/Scripts/LogSystem/Resources/ALogSettings.asset`（会打进包体）。

说明：

- **Editor 始终启用**，方便调试
- `Log.Throw` 在关闭日志时仍会 `error()`，只是不记日志

---

## 5. 调用堆栈与跳转

- Lua 日志自动附带 `debug.traceback`
- C# 日志自动采集调用栈
- 详情列表点击可跳源码；「调用堆栈图」按栈帧画调用链，节点同样可点跳转
- 依赖 Loader 把 Lua chunkname 设为源文件路径（本项目 `LuaEnvironment` 已如此）

---

## 6. 目录结构

```
Assets/Scripts/LogSystem/
  Runtime/     ALog、解析器、ALogCategories、ALogSettings
  Resources/   ALogSettings.asset（出包开关）
  Editor/      控制台窗口、分类配置、堆栈图、跳转

Assets/Scripts/LuaRaw/
  Log.lua              Lua 日志 API
  LogCategories.lua    分类常量（生成）
```

---

## 7. 内置分类

| 分类 | 来源 |
|------|------|
| `Default` | C# `ALog` 未指定分类时 |
| `Lua` | Lua `Log.*` 未指定分类时 |
| `Unity_Native` | 原生 `Debug.Log` / 未处理异常（仅日志系统启用时捕获） |

自定义分类通过「配置分类」或打日志时传入字符串即可。
