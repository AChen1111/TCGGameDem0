---
name: tcg-hotupdate-check
description: 检查或修复 TCGCardDem0 的 AOT 启动、HybridCLR 热更代码、Addressables 内容及开发工作台打包兼容性。用于热更加载失败、代码与资源不匹配、打包前检查；仅检查不执行构建、上传或激活内容。
---

# TCG 热更检查

工程根目录是本技能目录的 `../../..`。读取相关工作区差异、[Unity 版本](../../../ProjectSettings/ProjectVersion.txt) 和 [包依赖](../../../Packages/manifest.json)，保留已有代码与生成产物的修改。

## 以当前链路为准

先定位问题发生在 Editor、本机 Player 还是远端内容加载。读 [热更与内容文档](../../../.doc/project/hot-update-and-content.md) 理解背景，再核对：

| 检查对象 | 当前源码入口 |
| --- | --- |
| Editor / Player 启动分支、AOT 加载、Boot 签名 | [LoadDll](../../../Assets/AOT/LoadDll.cs) |
| 内容请求、平台、哈希与会话 | [CodeUpdate](../../../Assets/AOT/CodeUpdate.cs) |
| 打包、上传、编辑器运行及自动恢复 | [DevelopmentWorkbench](../../../Assets/Editor/Release/DevelopmentWorkbench.cs) |
| 构建输入、缓存与兼容性判定 | [DevelopmentInputs](../../../Assets/Editor/Release/DevelopmentInputs.cs) |
| 本地资源快照及内容包 | [DevelopmentPackage](../../../Assets/Editor/Release/DevelopmentPackage.cs) |
| 客户端与服务端共享协议 | [DevelopmentContent](../../../Assets/Shared/Configuration/DevelopmentContent.cs) |

核对基线：当前 `LoadDll` 使用编译条件区分 Editor 和 Player，没有旧文档的 `useRemoteContentInEditor` 开关；工作台入口为 `Tools/开发工作台`。旧 `Build And Publish Release` 文档和运营终端说明可能落后于实现。执行前重新核对这些入口是否仍存在，不恢复已删除的发布器来迁就文档。

## 检查顺序

1. 获取触发步骤、首个有效错误、当前平台和本次内容标识；不要先删缓存或重建全部内容。
2. 沿启动链检查：入口场景 → AOT 元数据 → HotUpdate 程序集 → 精确 `HotUpdateEntry.Boot` 签名 → Addressables → 配置就绪。Editor 成功不能证明 Player 的 DLL、裁剪或 AOT 补充元数据有效。
3. 核对 Manifest 协议、平台、文件路径、大小与 SHA-256，以及 `contentId`、`configHash`、`apkCompatibility` 的生成和消费逻辑。远端接口、鉴权要求取自当前代码，不照搬历史 Release 协议。
4. 检查 `Library/Development/<平台>/` 的代码与资源快照、构建输入指纹和实际文件。输出存在不等于与当前输入一致；资源快照应来自同次构建的文件列表，不混入旧 catalog。
5. 检查 AOT / 热更程序集边界。AOT、桥接需求或兼容性变化可能需要完整 Player 包；不要将所有 C# 变化都判为仅替换 HotUpdate.dll 即可。

## 执行范围

- “检查、诊断、打包前检查”只读取证据；修复请求允许修改相关实现并验证，但不隐含发布。
- 用户要求本地打包时使用当前 `DevelopmentWorkflow` 对应任务，区分 Player、Code、Resources；Upload 是独立远端写操作，只有任务包含上传时才执行。已有明确授权不反复确认。
- `RunEditor`、Play Mode 钩子或运营脚本可能包含后端准备与内容同步；调用前读实际方法，不能把名称中的“运行”当成无副作用检查。
- 使用 [Pipeline 构建与测试命令](../../../.doc/unity-pipeline/commands/build-and-compilation.md) 前核对工程和实时参数。重载或超时后读取状态与日志，确认任务终态再决定下一步；不重复提交未知结果的构建或上传。

## 验收

按修改范围运行 [CodeUpdateTests](../../../Assets/Tests/Editor/AOT/CodeUpdateTests.cs)、[HybridCLRSetupTests](../../../Assets/Tests/Editor/AOT/HybridCLRSetupTests.cs) 或相关测试。需要 Player 验证时记录实际平台和内容标识，检查错误提示及重试行为。

报告要分清：静态检查通过、编译通过、测试通过、本地包生成、Player 实测、服务器内容更新。未执行的层次不能用前一层结果代替。服务器状态不确定时先查询实际状态，保留任务标识与错误，不自动重发上传。
