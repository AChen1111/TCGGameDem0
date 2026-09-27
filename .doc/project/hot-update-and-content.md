# 热更新与内容分发

Unity 顶部 `mBulid` 菜单负责 Android、Windows x64 的热更内容构建和发布。旧开发工作台已删除；Editor Play Mode 使用本地程序集和工程资源。

## 1. 启动链

```text
PreInit → LoadDll
  Editor：使用已编译的 HotUpdate 程序集，绑定本地会话
  Player：读取包内 AOT 元数据，获取当前平台 Manifest，校验并准备内容文件
    → 加载 AOT 补充元数据 → Assembly.Load(HotUpdate.dll)
HotUpdateEntry.Boot
  Editor：本地 Addressables 与工程内 GameConfig
  Player：使用已缓存的 Addressables catalog 与资源
  → 加载 Init 场景 → 登录或大厅
```

Editor 不读取 `Library/Development/editor-session.json`，本地配置哈希由 `LocalGameConfiguration` 根据实际加载的配置字节计算。进入 Play Mode 不再自动启动后端、生成配置或上传编辑器配置；后端若要使用 Editor 配置，仍需通过保留的接口单独发布匹配配置。

## 2. 热更 DLL 来源

热更新程序集为 `Assets/Scripts/HotUpdate.asmdef`。`mBulid/构建 DLL` 使用 HybridCLR 官方 `CompileDllCommand.CompileDll(临时目录, target, developmentBuild)`，按当前平台及 Development 设置在新的 `Temp/mBulid/<任务>/Compile/` 编译 Player 脚本。确认 HotUpdate.dll 存在、非空且程序集名称正确后，复制到当前版本目录，从最终文件计算 SHA-256。不要发布 `Library/ScriptAssemblies` 中的 Editor DLL。

完整主包构建前使用 HybridCLR 官方 `Generate/All` 准备防裁剪配置、AOT 元数据与桥接代码，生成和打包必须保持相同平台及 Development 设置，再用 Unity 构建 Player。`PlayerAotMetadata` 在主包构建前，按 `LoadDll.AotDllNames` 将该平台 `HybridCLRData/AssembliesPostIl2CppStrip/<平台>/` 中的文件同步到 `StreamingAssets/HybridCLR/<程序集>.dll.bytes`；源文件缺失或程序集名称不匹配时停止构建。该同步先于 HybridCLR 清空裁剪输出，跳过官方生成元数据的临时 Player 构建；也可手动执行 `HybridCLR/Copy AOT Metadata to StreamingAssets`。同步仅复制已有生成产物，不代替 Generate/All；切换 Development、升级 AOT 依赖后必须重新生成。不要继续沿用之前手工复制的 `.bytes`，否则可能出现 UniTask 异步状态机 `.ctor` 的 MethodNotFind。mBulid 四项操作不构建主包，也不替换主包中的 AOT 元数据。协议从 4 升至 5，需要更新一次主包；删除了 `development-apk.txt` 的随机标识检查，AOT 与热更接口兼容性仍需 Player 实测。

### mBulid 操作顺序

完整 Player 主包必须包含 Addressables 初始化产物（Android APK 中的 `assets/aa/settings.json` 和本地启动 catalog）。工程设置 `BuildAddressablesWithPlayerBuild = BuildWithPlayer`，使用 Packed Mode 在构建主包时自动生成并加入这些文件；不依赖之前 mBulid 构建留下的 Library 缓存。若已安装主包缺少这些文件，需重新构建并安装主包。mBulid 发布目录中的远程 catalog/bundles 不替代主包的初始化文件，主包构建也不会自动发布后端内容。

Addressables 的 `Shared Bundle Settings` 使用 `CustomGroup → Remote_Shared`，使自动生成的 `monoscripts` 与 `unitybuiltinassets` 包随远程资源一起构建和发布。默认本地组即使为空也可能承载这些公共包，不能只检查业务组是否为 Remote。mBulid 构建前检查公共组的远程路径，构建后拒绝版本目录外的 Bundle，避免 catalog 引用 APK 中不存在的旧包名。`monoscripts` 是资源的脚本类型信息包，不是 `HotUpdate.dll` 或 AOT 补充元数据。

1. **设置版本号**：保存当前平台内容版本到 `UserSettings/mBulid.json`，创建 `Version/<版本>_安卓` 或 `Version/<版本>_win`。同名目录删除重建，其他版本保留。内容版本不得包含路径或非法字符，不修改 `PlayerSettings.bundleVersion`。
2. **构建 DLL**：输出 `HybridCLR/HotUpdate.dll`、`HybridCLR/HotUpdate.dll.sha256`，更新 `build-info.json` 的代码完成标记。失败后不能复用旧 DLL。
3. **构建 Addressables**：先生成工程配置，再完整构建当前平台资源。临时将远程输出设为版本目录下的 `Addressables/`，catalog 用内容版本命名。依据本次 FileRegistry 记录 catalog、hash、bundles，并导出 `GameConfig/*.bytes`；完成后恢复设置。重建清空该版本资源和完成标记，保留 DLL。
4. **发布版本**：核对两个完成标记、配置和全部文件大小/SHA-256，生成 `manifest.json` 和 `Temp/mBulid/<任务>/Content.zip`。仅上传已构建版本，不隐式构建。本地 Development 后端允许无发布密钥请求，mBulid 和账号金币/礼品窗口不再提前拦截；其他环境仍由后端鉴权，配置过的 `ACHEN_CONTENT_PUBLISH_KEY` / 后端服务窗口本地密钥会随请求发送。地址采用 `ACHEN_BACKEND_URL` 或 `http://127.0.0.1:5080`。发布状态记录在 `Temp/mBulid/status.json`。

后端服务窗口入口为 `Tools/后端服务/打开窗口` 或 `Window/TCG/后端服务`，同一 Tools 子菜单提供启动与停止操作。

`Version/`、`Temp/`、`UserSettings/` 不纳入 Git。编译、播放、主包构建或已有 mBulid 任务进行时禁用操作。未选择当前平台版本时不能构建或发布。

## 3. 保留的分发协议

| 接口 | 用途 |
| --- | --- |
| `PUT /api/dev/content/<平台>` | 接收当前平台内容 ZIP，并替换该平台当前内容 |
| `PUT /api/dev/editor-config` | 接收 Editor 配置 ZIP |
| `GET /api/content/latest/<平台>` | 获取当前平台最新 Manifest |
| `GET /content/current/<平台>/<contentId>/<文件路径>` | 下载当前内容清单中的文件 |

上传接口要求内容发布鉴权；ZIP 上传携带 `X-Artifact-Sha256`。Manifest 使用 `DevelopmentProtocol` 版本 5，包含 `schemaVersion`、`platform`、`contentVersion`、`contentId`、`configHash`、`configs` 和 `files`。Player 包还包含 `hotUpdatePath`、`catalogPath` 和 `catalogHashPath`。DLL 包内路径为 `HybridCLR/HotUpdate.dll`。每次成功发布生成独立 GUID contentId，同一 contentVersion 重复发布也会更换 GUID。

存储根由 `ContentDelivery:StorageRoot` 指定，默认是后端 ContentRoot 下的 `Data/content`。Player 文件保存在 `current/<平台>/<版本_平台后缀>/`；数据库沿用 `CurrentContents` 的 Manifest JSON。下载 URL 继续使用 contentId 隔离缓存，服务端将其映射到版本目录。读取与替换支持原有 `current/<GUID>/` 存储目录。

Player 发布在鉴权和目标/归档哈希格式检查通过后取得发布锁，先删除目标平台旧目录（包括旧 GUID 目录）及数据库记录，再接收、解压和校验新包。成功后只保留该平台一个最新目录；中断或校验失败清理新包，该平台接口返回“尚未发布”，不恢复旧版。删除失败时停止接收上传。另一平台和 Editor 配置不会被清理。Editor 配置继续通过独立接口保存于 `current/<GUID>/`，不参与 mBulid 发布。

上传超时或连接断开时，工具不会自动重发；应查询 `/api/content/latest/<平台>` 确认后端状态。后端当前版本改变时，旧 contentId 下载请求返回 409，客户端重新获取最新 Manifest。

## 4. 客户端检查依据

Player 每次启动请求当前平台最新 Manifest，校验协议、平台、内容版本和 GUID，再按文件大小及 SHA-256 校验缓存。有效文件可复用，不匹配时下载并再次校验。Manifest 查询当前只按平台筛选，不使用 `channel` 或 `appVersion`。Editor 不查询最新 Manifest、不比较远端内容版本，后端内容接口不可用不影响本地程序集与资源初始化。

文件哈希用于检查产物完整性，不证明 AOT 与热更接口兼容。Editor 本地运行成功不能替代 Player 验证。

## 5. 业务边界

- AOT 通过 `HotUpdateEntry.Boot(Action<float>, StartupContext, Action<LocalizedMessage>)` 进入热更层。
- 热更层通过 Addressables 加载资源，配置在登录前按 `GameConfig` 标签加载。
- 共享配置和决斗规则仍使用纯 C# 共享代码，后端不会执行客户端 HotUpdate.dll。
- 客户端组卡配置解析器 `DeckRulesConfiguration` 与 `DeckSection` 位于 `Assets/Scripts/Network/GameConfig/`，编译进 `HotUpdate.dll`，可随热更更新。命名空间仍为 `AChen.Configuration`；不要将其移回主包 `AChen.Shared`。通用二进制表及客户端/后端共用协议仍位于 `Assets/Shared`。

主包/热更边界与第三方审查见 [最小启动层迁移](hot-update-migration.md)。业务配置、本地化、日志和卡牌代码已迁入 HotUpdate，后端链接相同源码；AChen.Shared 只保留启动接口。
