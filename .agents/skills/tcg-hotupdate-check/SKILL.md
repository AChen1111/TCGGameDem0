---
name: tcg-hotupdate-check
description: 检查或修复 TCGCardDem0 的 AOT 启动、HybridCLR 热更代码、Addressables 内容及主包兼容性。用于热更加载失败、代码与资源不匹配、打包前检查；仅检查不执行构建、上传或激活内容。
---

# TCG 热更检查

工程根目录是本技能目录的 `../../..`。读取相关工作区差异、[Unity 版本](../../../ProjectSettings/ProjectVersion.txt) 和 [包依赖](../../../Packages/manifest.json)，保留已有代码与生成产物的修改。

## 以当前链路为准

先定位问题发生在 Editor、本机 Player 还是远端内容加载。读 [热更与内容文档](../../../.doc/project/hot-update-and-content.md) 理解背景，再核对：

| 检查对象 | 当前源码入口 |
| --- | --- |
| Editor / Player 启动分支、AOT 加载、Boot 签名 | [LoadDll](../../../Assets/AOT/LoadDll.cs) |
| 内容请求、平台、哈希与会话 | [CodeUpdate](../../../Assets/AOT/CodeUpdate.cs) |
| Editor 本地配置加载与配置哈希 | [LocalGameConfiguration](../../../Assets/Scripts/Network/GameConfig/LocalGameConfiguration.cs) |
| 后端内容接收、存储与下载 | [LatestContentService](../../../Backend/src/AChen.Backend.Api/Features/ContentDelivery/LatestContentService.cs) |
| 客户端与服务端共享协议 | [DevelopmentContent](../../../Assets/Shared/Configuration/DevelopmentContent.cs) |
| mBulid 菜单、构建与发布 | [MBulidPipeline](../../../Assets/Editor/Release/MBulidPipeline.cs)、[MBulidPackage](../../../Assets/Editor/Release/MBulidPackage.cs) |

核对基线：`LoadDll` 使用编译条件区分 Editor 和 Player，没有 `useRemoteContentInEditor` 开关。开发工作台及 Play Mode 接管已删除；新 `mBulid` 顶部菜单独立构建/发布 Android 与 Windows x64 内容，不上传 Editor 配置，不构建主包。不要恢复旧发布器。

## 检查顺序

1. 获取触发步骤、首个有效错误、当前平台和本次内容标识；不要先删缓存或重建全部内容。
2. 沿启动链检查：入口场景 → AOT 元数据 → HotUpdate 程序集 → 精确 `HotUpdateEntry.Boot` 签名 → Addressables → 配置就绪。Editor 成功不能证明 Player 的 DLL、裁剪或 AOT 补充元数据有效。
3. 核对 Manifest 协议 5、平台、文件路径、大小与 SHA-256，以及 `contentVersion`、独立 GUID `contentId`、`configHash` 的生成与消费。旧 `apkCompatibility` / `development-apk.txt` 随机标识检查已移除，协议变更需要更新主包。
4. 检查当前平台 `Version/<版本>_安卓` 或 `<版本>_win` 下的 `build-info.json` 完成标记与产物哈希；DLL 来自任务独立临时目录的官方 Player 编译，发布路径为 `HybridCLR/HotUpdate.dll`。AOT 元数据仍来自主包。不要用旧 `Library/Development/` 产物或 Editor DLL 判断构建成功；Editor 配置哈希来自实际本地配置。
5. 检查 AOT / 热更程序集边界。AOT、桥接需求或兼容性变化可能需要完整 Player 包；不要将所有 C# 变化都判为仅替换 HotUpdate.dll 即可。

## 执行范围

- “检查、诊断、打包前检查”只读取证据；修复请求允许修改相关实现并验证，但不隐含发布。
- 用户要求本地打包时区分 Player、Code、Resources。mBulid 四项操作不运行 Generate/All 或构建主包；内容发布只上传已校验产物。远端上传仅在任务包含上传时执行，已有授权不反复确认。后端 Player 发布先删除目标平台旧目录/记录再接收新包，失败后无当前版本且不恢复旧版；其他平台与 Editor 配置保留。
- Editor Play Mode 不再由项目工作台自动准备后端或同步配置；检查其他运行入口与运营脚本时仍以实际代码为准。
- 使用 [Pipeline 构建命令](../../../.doc/unity-pipeline/commands/build-and-compilation.md) 前核对工程和实时参数。重载或超时后读取状态与日志，确认任务终态再决定下一步；不重复提交未知结果的构建或上传。

服务器状态不确定时先查询实际状态，保留任务标识与错误，不自动重发上传。
