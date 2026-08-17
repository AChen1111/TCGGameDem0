# Lua Pad

系统浏览器里的 Lua 草稿控制台。Play 后按 **F10** 打开默认浏览器；点网页顶栏 **运行** 把当前游标范围内的行 `DoString` 进当前 XLua。不嵌在 Game 画面里。

相关文档：[Lua 系统](LuaSystem.md)

---

## 用法

1. Play 后按 **F10**（再按一次视为隐藏；浏览器标签关不掉，下次 F10 会重新打开 URL）。第一次打开会下载 `emmylua_ls`，可能要等几秒。
2. 在浏览器里输入。`p` 出 `print()`，`f` 出 `function` 块。`Log.` / `go.`（先 `---@type UnityEngine.GameObject`）走 EmmyLua。Unity API 桩由 **Tools → Lua → Generate EmmyLua API** 生成到 `LuaRaw/EmmyApi/`。
3. 最左侧 gutter 有两个游标：开始 **▼**、结束 **▲**（默认第 1 行到最后一行）。在 glyph / 行号栏按下并拖动最近的游标（或点中对应 glyph）；两行以上时开始与结束不重合。
4. 点 **运行**。只执行开始行到结束行（含两端，1-based，与 Monaco 一致）。`print` / `Log.*` 出现在窗口底部。点 **关闭** 会 `window.close()`（多数情况下标签仍在，不影响 Unity）。关掉浏览器标签不会把 Play 打崩。
5. 左侧草稿栏列出 `LuaRaw/LuaPadDrafts/` 里已有草稿，点一项加载进编辑器。顶栏填草稿名（仅 `[A-Za-z0-9_-]`，自动 `.lua`）后点 **存为草稿**。栏可收起。

补全读 `Assets/Scripts/LuaRaw`（含 `EmmyApi/` 反射生成的 Unity/C# 桩，以及 `Include.lua` 别名）。`require` 按文件名，见 [LuaSystem.md](LuaSystem.md)。草稿 `.lua` 不进 LuaBundle，Editor 运行时扫描也会跳过，避免被当成模块 `require`。

---

## 实现过程

目标从一开始就是「运行时草稿 + 运行」，不要文件树、不要保存工程。编辑体验要对齐 VS Code / Monaco（Dark+、等宽、行号、高亮、关键字 snippet + EmmyLua 成员补全）。

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

### 3. 独立 WinForms + WebView2 工具窗（已废弃）

曾用 `LuaPadBrowser.exe`（WinForms + WebView2）+ TCP JSON 行协议把页面 `eval` 回 Unity。系统 Chrome 标签用不了这条桥。助手源码仍在 `Tools/LuaPadBrowser/`，F10 不再启动它。

### 4. 系统浏览器 + 本机 HTTP（当前）

`Application.OpenURL` 打开 `http://127.0.0.1:<port>/`。页面用同源 `POST /rpc` 跟当前 Unity 进程说话：`changed` / `run` / `close` / `completion` / `signatureHelp` / `draftsList` / `draftSave` / `draftLoad`。执行仍是 `LuaPadRunner.RunInGame` → `LuaManager.DoString`，不把 DoString 放到远程服务器。静态页只服务 `StreamingAssets/LuaPad`；草稿读写走 `/rpc`，浏览器不直写工程。

Unity 的 `HttpListener` 在 netstandard 下没有可用的 WebSocket Accept，所以用请求-响应 HTTP 而不是 WS。补全 / 运行 / 诊断都随这次 POST 返回。

### 5. 补全分两路

- **关键字 / snippet**：Monaco 本地表。`if` → `if ${1:condition} then … end`，同样有 `for` / `function` / `while` / `repeat`。`print` 仍是 `print()`。必须 `import` Monaco 的 `suggestController`（以及 snippet / parameterHints），否则 `editor.api` 精简包不出 Suggest 弹层。
- **成员 / 全局名**：标识符前缀或 `.` / `:` 问 `emmylua_ls`（`L`→`Log`，`Log.`→`Error`）。C# 把 LSP 的 `label` / `detail` / `insertText` / `kind` / `documentation` 转成 Monaco 项，列表里能看到 `Error (strMessage, strCategory) -> nil`。输入 `(` 会再问 `textDocument/signatureHelp`。

`textDocument/completion` 超时 2s 返回空数组。标识符前缀也会问 LSP，好补出 `Log` 这类全局名。

---

## 架构

```
F10 → LuaPadHost
        │
        ├─ LuaPadSession.Start()
        │     ├─ emmylua_ls  (stdio JSON-RPC)
        │     ├─ LuaPadHttpServer  (127.0.0.1:随机端口, Monaco 静态页 + POST /rpc)
        │     └─ Application.OpenURL(origin)
        │
        └─ 运行: LuaPadRunner.RunInGame → LuaManager.DoString
```

### 目录

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/LuaPad/LuaPadHost.cs` | Play 后 Boot；F10 开关；主线程 `Pump` |
| `Assets/Scripts/LuaPad/LuaPadSession.cs` | LSP + HTTP 生命周期；处理 `/rpc` |
| `Assets/Scripts/LuaPad/LuaPadHttpServer.cs` | 静态页 + `POST /rpc` |
| `Assets/Scripts/LuaPad/LuaPadCompletion.cs` | LSP / 关键字 → Monaco 补全项 |
| `Assets/Scripts/LuaPad/LuaPadLspClient.cs` | `emmylua_ls` stdio 客户端 |
| `Assets/Scripts/LuaPad/LuaPadRunner.cs` | 截获 `print`/`Log`，`DoString` |
| `Assets/Scripts/LuaPad/LuaPadTextUtil.cs` | 前缀、关键字/snippet 表、`NeedsLsp`、`SliceLines` |
| `Assets/Scripts/LuaPad/LuaPadWorkspace.cs` | Editor=`LuaRaw`；Player=`StreamingAssets/LuaWorkspace`；草稿路径与文件名校验 |
| `Assets/Scripts/LuaPad/LuaPadMainThread.cs` | 后台线程投递到 `Update` |
| `Tools/LuaPad/` | Monaco 源（Vite） |
| `Assets/StreamingAssets/LuaPad/` | `npm run build` 产物 |
| `Library/LuaPad/` | `emmylua_ls.exe`（gitignore） |
| `Assets/Scripts/LuaRaw/.emmyrc.json` | LuaJIT；`requirePattern: ["?.lua","**/?.lua"]` |
| `Assets/Scripts/LuaRaw/LuaPadScratch.lua` | LSP 用的草稿缓冲（不要放进 `ignoreGlobs`） |
| `Assets/Scripts/LuaRaw/LuaPadDrafts/` | 命名草稿；gitignore `*.lua` / `*.lua.meta`，保留 `.gitkeep` |

`LuaPadNative*` 是旧 overlay 坐标映射，当前不再走它。`LuaPadBrowser*` 是旧 WebView 助手，F10 路径不再启动。

### HTTP 桥

页面 `fetch("/rpc")`，UTF-8 JSON。Clash 会打断 localhost，Unity CLI 必须 `--proxy-disable`；浏览器打开本页不受影响。

| `method` | 线程 | 返回 |
|----------|------|------|
| `changed` | 主线程 | `{ diagnostics }` |
| `run` | 主线程 | `{ ok, output }`；请求带 `text`、`startLine`、`endLine`（1-based 含两端） |
| `close` | 主线程 | `{}`（只翻 Unity 侧可见标记） |
| `completion` | HTTP 线程 | `{ items }`，标识符前缀或 `.` / `:` 问 LSP |
| `signatureHelp` | HTTP 线程 | LSP `SignatureHelp` JSON |
| `draftsList` | HTTP 线程 | `{ names }` |
| `draftSave` | HTTP 线程 | `{ ok, name }`；`name` 仅 `[A-Za-z0-9_-]`，自动 `.lua` |
| `draftLoad` | HTTP 线程 | `{ text }` |

关浏览器标签只会拆掉那次 HTTP；`OutputStream.Write` 仍吞 `IOException`，Unity 不崩。Play 结束 `Dispose` HTTP + LSP。

### 补全

1. 输入字母：`POST /rpc { method:"completion" }`；关键字 snippet 与 EmmyLua 全局名（`L`→`Log`）合并返回。
2. 输入 `.` / `:`：同一接口问成员（`Log.` → `Error` / `Info` …）。
3. C#：`NeedsLsp` 在标识符或 `.` / `:` 后为真才 `textDocument/completion`（2s，失败空列表）；`LuaPadCompletion.FromLsp` 转发参数 hint。
4. 输入 `(` / `,`：`signatureHelp`。

### 运行

`run` 用 `LuaPadTextUtil.SliceLines` 切出 `startLine`–`endLine`（1-based inclusive，以 `\n` 拼回），再 `LuaPadRunner.RunInGame`。订阅 `Application.logMessageReceived`，执行完取 `print`/`Debug.Log` 文本随 `/rpc` 返回 `{ ok, output }`。

不要在 EditMode 里对 `LuaManager.BeginInit()` 做完整 `RunInGame` 初始化测试：`UIFrame.Init` 的 `DontDestroyOnLoad` 会失败。

### 草稿

路径集中在 `LuaPadWorkspace`（`DraftsRoot` = `LuaRaw/LuaPadDrafts`）。`SanitizeDraftName` 后 `Path.Combine`，结果必须仍在草稿目录内。`LuaBundleBuilder` 与 `LuaEnvironment.BuildFileIndex` 跳过该目录；拷到 `StreamingAssets/LuaWorkspace` 时也不带草稿。

---

## 依赖与构建

- **emmylua_ls 0.25.1**：首次打开从 GitHub Releases 下到 `Library/LuaPad/`
- Monaco：改 `Tools/LuaPad/src` 后必须

```
npm --prefix Tools/LuaPad run build
```

本机 Clash 会打断 Pipeline localhost，Unity 命令必须加 `--proxy-disable`。改 C# 后：`set_autotick` → `recompile` → `run_tests --mode editor --filter LuaPad`。

真机：`IPreprocessBuild` 把 `LuaRaw` 拷到 `StreamingAssets/LuaWorkspace`（LSP 要源码；运行仍走 `LuaBundle.bytes`）。仅 Windows。F10 打开系统浏览器，Player 不需要 `LuaPadBrowser.exe`。

菜单：**Tools → Lua → Copy LuaWorkspace to StreamingAssets**、**Tools → Lua → Generate EmmyLua API**。
