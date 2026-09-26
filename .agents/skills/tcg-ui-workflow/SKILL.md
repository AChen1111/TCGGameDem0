---
name: tcg-ui-workflow
description: 为 TCGCardDem0 新增或修改 uGUI 窗口、面板、Prefab 引用及界面交互，沿用 UIFrame、UiScreenGenerator 和 Addressables。用于制作界面、调整布局、绑定按钮或修复界面生命周期；纯 Unity UI 概念问答不需要本技能。
---

# TCG 界面工作流

本技能随项目保存。工程根目录是本文件所在目录的 `../../..`；下文命令和代码路径以工程根目录为基准。先检查相关文件的工作区差异，保留已有改动。文档与代码不一致时以当前实现为准，并指出差异。

## 定位现有界面

- 先读 [Unity 客户端的 UI 章节](../../../.doc/project/unity-client.md)，再找同类已实现窗口，核对控制器、Prefab、所属场景的 UISettings 和打开入口。
- 新屏幕使用 [UIFrame](../../../Assets/Scripts/UI/Core/UIFrame.cs)、`APanelController` / `AWindowController`。普通业务 UI 沿用框架；只有任务确实涉及框架行为时才修改 Core，并检查调用方。
- 绑定前读 [UiScreenGenerator](../../../Assets/Scripts/UI/Core/Authoring/UiScreenGenerator.cs) 与 [UiPrefixCollector](../../../Assets/Scripts/UI/Core/Authoring/UiPrefixCollector.cs)。节点前缀及组件类型以该映射为准，例如 `Btn_Confirm`、`Txt_Title`，不要凭名称猜序列化字段。

## 修改与接入

1. 明确本次目标：视觉调整、现有窗口交互，还是新增屏幕。仅视觉调整不重新生成整个控制器。
2. 编辑场景或 Prefab 前，按 [Pipeline 连接说明](../../../.doc/unity-pipeline/connectivity.md) 核对当前工程、活动场景、Play Mode、编译状态和未保存内容。命令及参数从当前可用工具或 `/api/commands` 获取；[Prefab 文档](../../../.doc/unity-pipeline/commands/prefabs.md) 用于查询具体操作。
3. 优先通过 Editor 修改序列化对象并保存对应资源，保持 `.meta`、GUID 和已有引用。桥接不可用时可继续代码与文件分析；不要用猜测的 YAML 修改冒充完成的 Prefab 绑定。
4. 新增或改变自动绑定字段时，通过生成器收集、生成或重建引用，等编译完成后核对控制器挂载及字段绑定。`GeneratedTagStart` / `GeneratedTagEnd` 之间由生成器维护，业务代码放在生成区外。
5. 按 [UISettings](../../../Assets/Scripts/UI/Core/UISettings.cs) 的现有注册方式接入，用 `UIFrame.ShowPanel` / `OpenWindow` 打开。需要地址时使用 [Catalog 工具](../../../Assets/Scripts/Editor/Addressable/AddressableCatalogMenu.cs) 更新生成常量，不另造地址字符串。
6. 会话写操作走当前 `PlayerSession` 方法；异步结果、取消、事件解绑和窗口关闭行为参考相邻实现。不要在 UI 复制金币、令牌或交易真相，也不要顺手增加未请求的购买能力。

## 验收与失败处理

- 等待编译结束，区分新增报错与修改前已有报错；检查 Missing Script、空引用、UISettings 注册和地址解析。
- 对视觉改动，在实际界面检查目标分辨率下的布局、文字、遮罩及点击区域，并保留截图。对交互改动检查打开、关闭、再次打开，以及相关失败路径。
- 改生成器或生命周期时，选择 [UiScreenGeneratorTests](../../../Assets/Tests/Editor/HotUpdate/UI/UiScreenGeneratorTests.cs)、[UiDestroyOnCloseTests](../../../Assets/Tests/Editor/HotUpdate/UI/UiDestroyOnCloseTests.cs) 等相关 EditMode 测试；单纯位置或颜色调整不强制全量测试。
- 进入 Play Mode 前检查当前开发工作台钩子的副作用；用户只要求检查时不触发运行、构建或上传。编辑器不可用时明确列出未完成的视觉、绑定或运行验证。
- 交付说明改了哪个界面、如何打开、做过哪些验证以及剩余问题。工具请求失败后先读取对象现状，避免重复添加组件、注册项或按钮监听。
