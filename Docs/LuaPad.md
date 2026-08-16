# Lua Pad

独立 WebView2 窗口里的 Lua 草稿控制台。Play 后按 **F10** 打开；点网页顶栏 **运行** 把缓冲 `DoString` 进当前 XLua。不嵌在 Game 画面里。

相关文档：[Lua 系统](LuaSystem.md)

---

## 用法

1. Play 后按 **F10**（再按一次隐藏窗口）。第一次打开会下载 `emmylua_ls`、必要时 `dotnet publish` WebView 助手，可能要等几秒。
2. 在独立窗口里输入。`p` 出 `print()`，`f` 出 `function`；`Log.` / `BaseScreen:` 走 EmmyLua。
3. 点 **运行**。`print` / `Log.*` 出现在窗口底部。点 **关闭** 或窗口 X 只隐藏，不退出 Play。

补全读 `Assets/Scripts/LuaRaw`（含 `Include.lua` 的 `---@class` 桩）。`require` 按文件名，见 [LuaSystem.md](LuaSystem.md)。

---

## 实现过程

目标从一开始就是「运行时草稿 + 运行」，不要文件树、不要保存工程。编辑体验要对齐 VS Code / Monaco（Dark+、等宽、行号、高亮、关键字补全 + EmmyLua 成员补全）。

### 1. 先做 UITK 壳，再换编辑器

第一期在 Game 画面里用 UI Toolkit 做顶栏（运行 / 关闭）和 `TextField`。执行走 `LuaPadRunner.RunInGame` → `LuaManager.DoString`。

坑：直接 Play **GameScene** 时场景里没有 `LuaManager`；**SampleScene** 会 Init 后 `LoadScene("GameScene")` 并 `DontDestroyOnLoad`。`RunInGame` 必须先 `FindFirstObjectByType<LuaManager>()`，没有再 `Instance` + `BeginInit()`。已有但 `IsDone == false` 时返回「Lua 正在初始化」，避免和 `SingletonManager` 双 Init。不要用 `LuaManager.Instance` 在无场景单例时造空壳（`IsDone` 会永远 false → 「Lua 未就绪」）。

### 2. 用 Monaco + WebView2 盖住输入框（已废弃）

为了高亮和补全，在 UITK 的 `editor-host` 上叠一层无边框 WebView2，每帧 `SetWindowPos` 对齐 Game 视图。Monaco 静态页由 Vite 打进 `StreamingAssets/LuaPad/`。

试过并否决的做法：

| 做法 | 结果 |
|------|------|
| `SetParent` / `WS_CHILD` 嵌进 Unity HWND | Unity 交换链会盖住或失败，窗口漂在桌面左上角 8×8 |
| 无边框 overlay + 每帧 `SetWindowPos` | 键盘焦点和 Unity Game 抢；切出再切回窗口消失；`w<200 \|\| h<80` 时不放置，看起来像「没编辑器」 |
| `GWLP_HWNDPARENT` 挂 Unity 主窗 | 层级跟 Unity 走了，但输入仍被 Game 视图吃掉 |
| UITK `TextField` 当 Monaco 没对齐时的后备 | `cursorIndex` 在 `value-changed` 时常为 0，补全光标不可靠 |

### 3. 改成独立工具窗（当前）

不做 Unity 内部界面。`LuaPadHost` 只负责 F10 和拉起 `LuaPadSession`。`LuaPadBrowser.exe` 是普通可缩放 WinForms + WebView2：任务栏可见、可拖、可输入。运行 / 关闭 / 输出都在网页里。

不放到远程服务器：`DoString` 必须打进**当前 Unity 进程**的 XLua；LSP 也要读本机 `LuaRaw`。本机 HTTP + TCP 桥就够。

### 4. 补全分两路

- **关键字 / 内置**：Monaco 本地表（`print`→`print()`，`function`→`function`）。必须 `import` Monaco 的 `suggestController`，否则 `editor.api` 精简包不出 Suggest 弹层。
- **成员**：仅当光标前是 `.` / `:` 才问 `emmylua_ls`（`Log.`、`BaseScreen:`）。

`textDocument/completion` 超时（曾 15s）会打崩 WebView 读线程。现在 completion 进线程池、超时 2s 返回空数组、关键字路径不调 LSP。

---

## 架构

```
F10 → LuaPadHost
        │
        ├─ LuaPadSession.Start()
        │     ├─ emmylua_ls  (stdio JSON-RPC)
        │     ├─ LuaPadHttpServer  (127.0.0.1:随机端口, 提供 Monaco 静态页)
        │     └─ LuaPadBrowser.exe
        │           └─ WebView2 ──postMessage──► TCP 行协议 ──► Session
        │
        └─ 运行: LuaPadRunner.RunInGame → LuaManager.DoString
```

### 目录

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/LuaPad/LuaPadHost.cs` | Play 后 Boot；F10 开关；主线程 `Pump` |
| `Assets/Scripts/LuaPad/LuaPadSession.cs` | LSP + HTTP + Browser 生命周期；处理网页消息 |
| `Assets/Scripts/LuaPad/LuaPadBrowser.cs` | 启动助手进程；TCP 发 `visible` / `eval` / `quit` |
| `Assets/Scripts/LuaPad/LuaPadLspClient.cs` | `emmylua_ls` stdio 客户端 |
| `Assets/Scripts/LuaPad/LuaPadHttpServer.cs` | 只服务 `StreamingAssets/LuaPad` |
| `Assets/Scripts/LuaPad/LuaPadRunner.cs` | 截获 `print`/`Log`，`DoString` |
| `Assets/Scripts/LuaPad/LuaPadTextUtil.cs` | 前缀、关键字表、`NeedsLsp` |
| `Assets/Scripts/LuaPad/LuaPadWorkspace.cs` | Editor=`LuaRaw`；Player=`StreamingAssets/LuaWorkspace` |
| `Assets/Scripts/LuaPad/LuaPadMainThread.cs` | 后台线程投递到 `Update` |
| `Tools/LuaPad/` | Monaco 源（Vite） |
| `Tools/LuaPadBrowser/` | WebView2 助手源 |
| `Assets/StreamingAssets/LuaPad/` | `npm run build` 产物 |
| `Library/LuaPad/` | `emmylua_ls.exe`、`LuaPadBrowser.exe`（gitignore） |
| `Assets/Scripts/LuaRaw/.emmyrc.json` | LuaJIT；`requirePattern: ["?.lua","**/?.lua"]` |
| `Assets/Scripts/LuaRaw/LuaPadScratch.lua` | LSP 用的草稿缓冲（不要放进 `ignoreGlobs`） |

`LuaPadNative*` 是旧 overlay 坐标映射，当前独立窗口不再走它。

### 启动参数与进程桥

助手命令行：`--port <tcp> --url http://127.0.0.1:<http>/`。Unity 先 `TcpListener` 再 `Process.Start`，助手连回后双方用 UTF-8 JSON 一行一条。

Unity → 助手：

| `cmd` | 含义 |
|-------|------|
| `visible` | `value: true/false` 显示/隐藏窗体 |
| `eval` | 在页面执行 JS（读缓冲、推补全、推诊断/输出） |
| `quit` | 真正退出进程（Play 结束 `Dispose`） |

助手 / 页面 → Unity（`method`）：

| `method` | 线程 | 含义 |
|----------|------|------|
| `changed` | 主线程 | 同步草稿到 LSP |
| `run` | 主线程 | `DoString`，结果推 `output` |
| `close` | 主线程 | 隐藏窗口 |
| `completion` | 线程池 | 仅 `.` / `:` 时问 LSP |

窗口 X：`FormClosing` 取消关闭，改为 `Hide` 并发 `close`。下次 F10 只 `Show`，不重启进程。

### 补全

1. 输入字母：Monaco 本地 `luaSyntax` 过滤前缀；`print()` 插成 snippet `print($0)`。
2. 输入 `.` / `:`：`post({ method:"completion", id, text, line, character })`。
3. C#：`NeedsLsp` 为真才 `textDocument/completion`（2s，失败空列表）；否则只回关键字。
4. `luaPadOnHost({ id, items })` 解开 Monaco Promise。

### 运行

`LuaPadRunner.RunInGame` 订阅 `Application.logMessageReceived`，执行完取 `print`/`Debug.Log` 文本推回页面 `{ ok, output }`。

不要在 EditMode 里对 `LuaManager.BeginInit()` 做完整 `RunInGame` 初始化测试：`UIFrame.Init` 的 `DontDestroyOnLoad` 会失败。

---

## 依赖与构建

- **emmylua_ls 0.25.1**：首次打开从 GitHub Releases 下到 `Library/LuaPad/`
- **LuaPadBrowser**：首次打开 `dotnet publish` 到 `Library/LuaPad/`；`Tools/LuaPadBrowser/Program.cs` 比 exe 新会自动重编
- Monaco：改 `Tools/LuaPad/src` 后必须

```
npm --prefix Tools/LuaPad run build
```

本机 Clash 会打断 Pipeline localhost，Unity 命令必须加 `--proxy-disable`。改 C# 后：`set_autotick` → `recompile` → `run_tests --mode editor --filter LuaPad`。

真机：`IPreprocessBuild` 把 `LuaRaw` 拷到 `StreamingAssets/LuaWorkspace`（LSP 要源码；运行仍走 `LuaBundle.bytes`）。仅 Windows。

菜单：**Tools → Lua → Copy LuaWorkspace to StreamingAssets**、**Tools → Lua → Publish LuaPadBrowser**。
