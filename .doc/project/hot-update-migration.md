# 最小启动层与业务热更迁移

## 程序集边界

- `TCG.Bootstrap`：`Assets/AOT` 的下载、文件校验、AOT 元数据、反射加载、进度和重试。仅下载界面的基础安全区适配留在这里。`Assets/HybridCLRGenerate` 通过 asmref 编入此程序集。
- `AChen.Shared`：无 UnityEngine 引用，只有 Manifest、文件/配置清单、`StartupContext` 和 `LocalizedMessage` 等启动接口。错误消息 `ToString()` 只输出 key 与参数。
- `HotUpdate`：卡牌、卡组、配置解析与校验、本地化、业务 UI、适配、日志和会话。Spine、SuperScrollView、UIAdapter 的运行时代码通过 asmref 编入同一个 DLL；第三方原源码与组件 GUID 保留。
- `HotUpdate.Editor`：通过 `Assets/Editor` 的 asmref 接收构建、配置生成、运营和资源工具。Editor 可以引用启动与热更程序集，Player 启动层不反向引用业务。
- `HotUpdate.Editor.Tests`：项目 Editor 测试统一显式引用上述程序集。

入口签名为 `HotUpdateEntry.Boot(Action<float>, StartupContext, Action<LocalizedMessage>)`。这是主包与热更的 ABI 变化，旧主包不能加载迁移后的热更 DLL，必须统一更换新主包。

## 配置与资源

下载层只校验内容标识、路径、大小、SHA-256 和文件完整性，不调用 `GameConfigTables` 或 `ConfigArtifacts`。
热更配置初始化先检查清单规则、整体 configHash 和各表字节，再组装业务配置并安装卡牌/翻译。失败不会把配置标记为就绪。
后端 csproj 链接热更目录下的同一份纯 C# 配置源码，不复制实现。

`CardRow.cs` 现在位于 `Assets/Scripts/Network/GameConfig/Generated`，`TranslationRow.cs` 位于 `Assets/Scripts/Localization/Generated`。当前配置流程通过 `PublishedConfigBuilder` 生成 bytes，不再使用旧 Excel/schema 生成器。

字体设置与日志设置属于远程配置资源。日志设置地址为 `GameConfig/ALogSettings`，配置初始化显式加载、安装并在重试时释放。
`LoadIN.prefab` 已从 Resources 移到 `Assets/UI/Prefab/Common`，保留 GUID，通过远程地址 `UI/LoadIN` 在热更配置初始化后预加载。挂热更脚本的资源必须从 AssetBundle 还原，不能因为实例化晚于 DLL 加载就继续留在主包 Resources；此次 Android 实测发现并修复了该问题。[HybridCLR 官方说明](https://www.hybridclr.cn/docs/basic/monobehaviour)
主包启动场景为 `PreInit`。`Init` 场景属于 `Remote_Scene`，不再被编辑器工具移回已删除的 `Local_Boot`。
热更入口显式清理配置/本地化/日志、事件中心、游戏流程、加载窗口与 Spine 图集缓存，不依赖动态 DLL 的 RuntimeInitializeOnLoadMethod 自动运行。

业务初始化失败后，重试会重新获取 Manifest 与文件；DLL 哈希不变时复用已加载程序集并重试业务初始化，DLL 改变时提示重新进入游戏，避免同进程重复加载新程序集。
主包构建前同步当前平台裁剪后的 AOT 元数据和补充元数据设置清单，并删除旧工具留下的 `StreamingAssets/HybridCLR/HotUpdate.dll.bytes`。

## 第三方审查

| 库 | 归属及依据 |
| --- | --- |
| Spine、SuperScrollView | 无 PreInit 引用；运行时代码并入 HotUpdate，Editor 程序集改为引用 HotUpdate |
| UIAdapter | 业务代码并入 HotUpdate；PreInit 的 SafeAreaAdapter 替换为独立 BootstrapSafeArea |
| UniTask | 暂保留 AOT 中间件，维持全局 PlayerLoop 初始化及其他包的程序集引用；所有项目 async 业务仍在 HotUpdate |
| LitMotion / Extensions | 暂保留 AOT 中间件，维持 Burst、Jobs 和 PlayerLoop；项目动画与 UI 行为仍在 HotUpdate |
| Unity 引擎/模块、HybridCLR、原生插件 | 保留 AOT/native |

这次仍只有一个业务热更 DLL，不引入多个 DLL 的清单或加载排序。[HybridCLR 官方 FAQ](https://www.hybridclr.cn/docs/help/faq) 说明热更部分的 Burst 代码会解释执行；因此没有为了迁移中间件而改变现有动画计算路径。

## 发布验收记录

- 代码迁移和启动程序集隔离已实现。
- 新增 StartupBoundaryTests 覆盖业务类型归属、主包无业务引用、无翻译配置时的错误文本、入口签名及 Resources 不引用热更组件。
- 后端相关测试 68 项通过。
- 配置生成通过；Editor 218 项完成检查（217 项通过，跨平台 AOT 清单同步后剩余 1 项复跑通过）。
- 最终 Android 主包构建成功（0 错误、8 警告）；Windows 主包构建成功（0 错误、10 警告）。
- Android 实机验证下载失败、配置错误、同进程重试恢复、首次与缓存启动；11 张配置表初始化成功，进入大厅，Spine、商城、设置及加载窗口正常，无 Missing Script。
- Windows 以 batchmode/nographics 验证下载失败、配置错误、首次与缓存启动、加载窗口组件及 11 个 AOT 元数据加载；失败恢复采用重新启动，未验证 Windows 重试按钮点击及实际画面。
- Android 保持主包不变，成功加载第二次 `migration-20260927-b` 热更：业务入口和新迁移的日志代码均显示 `migration-b`，再次进入大厅。构建 APK 与安装 APK 的前后 SHA-256 均为 `C5CD793C7A2D430BE3272A717930918C3DD8DF7D17482EC1448595F1809E56B9`。
- Windows 的第二次热更已构建并发布，主包 GameAssembly.dll 前后 SHA-256 均为 `5B04EA450613EDFD5EC05EC445C060B3DA2A78D07B4F2026D597AF2E5D5D693E`。按用户要求不再追加测试，未再次运行 Windows 第二次热更。

最终发布内容：Android `migration-20260927-b` / `fb4d8c99-3239-4bf0-9c3a-765e001e5088`；Windows `migration-20260927-b` / `3306de2c-c521-40bb-83d2-6f8b7ebf1b75`。

交付文件位于 `TestBuild/HotUpdateMigration-20260927`：Android/TCGGameDem0.apk、Windows/TCGGameDem0.exe（分发时保留整个 Windows 目录）。Evidence 目录保存构建结果、测试结果、Player 日志、截图、GUID 和主包哈希记录。临时故障代理已停止，ADB 5080 反向转发已恢复为直连本地服务。

发布前不得把已有 Version 目录的旧 DLL/bundles 当作此次迁移的产物；必须重新构建并核对最终哈希。
