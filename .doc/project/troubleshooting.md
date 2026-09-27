# 故障分析

记录已定位的疑难故障：现象、排查证据链、根因和修复。新故障按同样结构追加。

## Android SDK Platform Tools 被识别成 0.0

日期：2026-09-27　环境：Unity `6000.5.2f1`、Unity Version Control `2.12.4`、Windows

- 现象：已安装 Platform Tools `37.0.1`，仍提示 `0.0 < 36.0.0`；点击更新还可能报找不到 `powershell`。重启 Editor 后复发。
- 根因：`com.unity.collab-proxy` 的 `Editor/Tool/FindTool.cs` 使用 `string.Concat(plasticInstallDir, Path.PathSeparator, processPaths)` 拼接路径列表，把 Unity 进程的 PATH 覆盖成 `C:\Program Files\PlasticSCM5\client;System.Collections.Generic.List` 类型名。Windows 系统目录丢失，`sdkmanager.bat` 无法调用 `findstr`，版本查询没有正常输出。
- 证据：Unity Hub 和系统 PATH 正常，只有 Editor PATH 损坏；同一 SDK 查询在损坏 PATH 下报找不到 `findstr`，恢复 PATH 后正确列出 `37.0.1`。官方对应问题为 [UUM-147589](https://issuetracker.unity.com/issues/23557/unity-process-path-environment-variable-is-corrupted-when-using-unity-version-control-with-plasticscm-desktop-app-installed)。
- 修复：本工程使用 Git、未配置 Plastic 工作区，通过 Package Manager API 移除 `com.unity.collab-proxy`；恢复当前 Editor 进程的 PATH，并清除 Android SDK 检测缓存。系统和用户 PATH 无需修改，也无需重装 SDK。
- 确认结果：包移除请求成功；通过 Unity 自身 `SDKManager.HighestVersionInstalled(PlatformTools)` 重新查询得到 `37.0.1`，Editor PATH 已不含损坏的列表类型名。本次没有运行完整 Player 构建。
- 若今后重新启用 Unity Version Control，先确认所用版本已修复该问题，避免再次引入。

## 编辑器导入 Spine 资源后持续卡顿、点击无响应

日期：2026-09-05　环境：Unity `6000.5.2f1`　Windows 11

### 现象

- 通过 `Tools/AChen/导入 Spine 恢复资源` 导入后，编辑器极卡，点击基本无响应。
- 资源导入结果正确，预制体、图集、SkeletonData 都生成成功。
- 只有重启 Unity 才能恢复，关闭导入窗口无效。

### 结论

根因与 Spine 导入脚本无关，在 `BackendServiceController` 的后端端口探测上。两者唯一的联系是共用 `Tools/AChen` 菜单。

### 排查过程

| 步骤 | 手段 | 结果与推论 |
| --- | --- | --- |
| 1 | 读 `Logs/Editor.log` | 导入成功，卡顿期间无循环报错。发现导入期间约 10 轮 Asset Pipeline Refresh，单轮 `PostProcessAllAssets` 达 940ms，同一图集被 Spine 后处理重复摄取 4 次以上 |
| 2 | `unity command editor_status` | 返回 `ready`，非 `blocked_by_dialog`。排除模态框或进度条卡死 |
| 3 | `unity command get_selection` | 选中为空。排除 Inspector 里活的 Spine 预览 |
| 4 | `run_script` 反射统计 | `SkeletonDataAssetInspector` 存活 0，`EditorApplication.update` 订阅者中没有 Spine 预览。排除缩略图预览泄漏 |
| 5 | 采样进程 CPU 时间 | 空闲状态持续占用 85% 的单个核心，工作集 4.1GB |
| 6 | 注入 `EditorApplication.update` tick 探针 | 每秒仅 0.3 次 tick，最大帧间隔 2045ms。判定为主线程被同步调用阻塞，而非重绘风暴 |
| 7 | `ProfilerDriver` 抓编辑器帧 | 连续 6 帧均约 2040ms，其中 **2027ms 在 `EditorApplication.update: BackendServiceController.Update`** |
| 8 | 读代码定位 | `Update()` 每 0.5 秒调用 `IsPortOpen()`，后者是无超时的同步 `TcpClient.Connect("127.0.0.1", 5080)` |
| 9 | PowerShell 计时回环连接 | 连接 5080 需 **2071ms** 才返回 `ConnectionRefused`（正常为亚毫秒）。系统中另有连接卡在 `SynSent`，本机存在拦截回环 SYN 的过滤驱动 |

### 根因链

1. `IsPortOpen()` 用无超时的同步 `TcpClient.Connect` 探测 5080 端口。
2. 本机有过滤驱动拦截回环 SYN，一次「连接被拒绝」被拖到约 2 秒。
3. `BackendServiceController.Update()` 每 0.5 秒探测一次，主线程几乎 100% 被阻塞，编辑器 tick 降到 0.3 次/秒，表现为点击无响应。
4. `BackendServiceController` 没有 `[InitializeOnLoad]`，注册 `EditorApplication.update += Update` 的静态构造函数只在该类**首次被访问**时执行。新开的编辑器不探测，所以不卡。
5. `Tools/AChen` 下只有三个菜单项，其中后端的两项带 validate 函数（`MenuItem` 第二参数为 `true`），分别读取 `BackendServiceController.CanStart` 和 `.CanStop`。Unity 必须在把菜单画出来之前调用 validate 函数决定灰显状态，因此**展开 `Tools/AChen` 菜单**就构成对该类的首次访问，轮询被点火并持续整个会话。
6. Spine 导入工具的入口在同一菜单下，操作上必然先展开菜单再点击，所以故障稳定表现为「导入完就卡」。重启恢复也符合「静态构造函数未被触发」。

### 验证方法

| 操作 | 预期 |
| --- | --- |
| 重启 Unity，不碰 `Tools/AChen`，做其他事 | 不卡 |
| 仅展开 `Tools/AChen` 后按 ESC 关闭，什么都不点 | 开始卡 |
| 后端服务已真正启动（5080 有监听） | 连接瞬时完成，不卡 |

### 修复

两处独立修复。

| 文件 | 修改 |
| --- | --- |
| `Assets/AOT/Editor/BackendServiceWindow.cs` | 新增 `s_OpenWindowCount`（在 `BackendServiceWindow` 的 `OnEnable` / `OnDisable` 中增减）和 `ShouldProbe` 门控；只有服务窗口打开，或状态为 `Building` / `Starting` / `Running` 时才走周期探测，其余情况 `Update()` 在排空日志与主线程队列后直接返回 |
| `Assets/AOT/Editor/SpineRecoveryImporter.cs` | 导入移出 `OnGUI`，改由 `EditorApplication.delayCall` 执行；删除多余的 `AssetUtility.ImportSpineContent(reimport: true)`，改为先落盘贴图并调好导入设置、再落盘 json 与 atlas，让 Spine 后处理只摄取一次（刷新轮数从约 10 降到 3）；进度条改为无条件 `finally` 清理；临时骨骼对象改用 `useObjectFactory: false` 并在 `finally` 中销毁；结束时释放解码图集占用的托管内存；`TextureMaxSize` 改为只读 PNG 的 IHDR 头取宽高 |

Spine 侧的问题只影响导入过程本身的耗时和整洁度，不会在导入结束后持续占用主线程。

### 遗留风险

- 单次约 2 秒的阻塞仍存在于两个一次性时机：`BackendServiceController` 首次被访问时 `RecoverState()` 的那一次，以及显式点击「启动后端服务」时。`RecoverState()` 中的探测被有意保留，它负责识别「5080 已被外部服务占用」，去掉会导致重复启动一个抢不到端口的后端。
- 彻底消除需把端口探测改为线程池异步执行加短连接超时，并通过已有的 `s_MainThreadActions` 队列回主线程消费结果。
- 机器层面：回环连接被拦截导致拒绝延迟 2 秒属于环境异常，值得单独排查安全软件或网络过滤驱动。

### 可复用的排查手法

| 手法 | 用途 |
| --- | --- |
| `unity command editor_status` | 区分「模态框阻塞」与「主线程繁忙」 |
| 注入 `EditorApplication.update` tick 探针，统计 tick 频率与最大帧间隔 | 区分「重绘风暴」（tick 频率极高）与「同步阻塞」（tick 频率极低、帧间隔大） |
| `ProfilerDriver.profileEditor = true` 抓帧后用 `RawFrameDataView` 聚合 total / leaf 耗时 | 直接点名吃掉主线程的 `EditorApplication.update` 订阅者 |
| `run_script` 执行一次性诊断代码 | 反射读 `EditorApplication.update` 订阅列表、统计 `Editor` 与组件存活实例，无需改动工程代码 |
| 采样进程 `TotalProcessorTime` | 确认编辑器空闲时是否真在烧 CPU |

编辑器 Profiler 抓帧要拆成两次 `run_script` 调用（开启、转储），中间等待若干秒。主线程被严重阻塞时，单次调用会超过 Pipeline 的 30 秒超时。
