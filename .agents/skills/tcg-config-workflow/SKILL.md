---
name: tcg-config-workflow
description: 修改 TCGCardDem0 的 TableData 配置源表、二进制配置生成、共享数据结构及客户端加载校验。用于加配置字段或表、调整商品和本地化数据、排查生成或读取失败；不代替账号金币礼品等运营操作。
---

# TCG 配置工作流

工程根目录是本技能目录的 `../../..`。先检查涉及文件和 Backend 子模块的工作区状态；只处理本任务的配置链路。

## 找到真实数据链

按改动需要读取以下入口，不一次加载所有表：

- [PublishedConfigBuilder](../../../Assets/Editor/GameConfig/PublishedConfigBuilder.cs)：`TableData` 源目录、`Assets/GameConfiguration` 产物目录、`Prepare()` 与 `CompileDirectory()`。
- [BinaryTableCsv](../../../Assets/Editor/GameConfig/BinaryTableCsv.cs)、[BinaryTable](../../../Assets/Shared/Configuration/BinaryTable.cs)：CSV 字段、类型和二进制格式。
- [GameConfigTables](../../../Assets/Shared/Configuration/GameConfigTables.cs)、[PublishedGameConfig](../../../Assets/Shared/Configuration/PublishedGameConfig.cs)：表命名、地址、关联校验及共享数据结构。
- [LocalGameConfiguration](../../../Assets/Scripts/Network/GameConfig/LocalGameConfiguration.cs)、[GameConfigStore](../../../Assets/Scripts/Network/GameConfig/GameConfigStore.cs)：加载、就绪状态与业务读取。
- 涉及服务端消费时，查找 `Backend/src` 对应调用方并读 [后端 API 入口](../../../Backend/docs/api/README.md)。Backend 是独立子模块，变更与验证分别说明。

[项目文档](../../../.doc/project/README.md) 用于导航；历史缓存、ETag、草稿发布描述需要与当前实现核对，不能当成当前配置管线的保证。

## 修改步骤

1. 找到现有同类表与读取位置，区分“修改值”和“改变结构”。普通数值调整无需重构共享模型或发布协议。
2. 修改源 CSV 或实际数据源。当前 `CompileDirectory` 要求 CSV 平铺在 `TableData`，并拒绝无效表名及大小写冲突；新表不要藏在子目录。字段格式、引号、空值、数字范围以解析器为准。
3. 结构变化同步修改编码、解码、共享类型和实际消费者。检查主键、跨表引用、本地化 Key、资源地址、重复项与缺省语义；不要仅补一端字段。
4. 生成前检查受影响产物已有改动。通过 Unity 菜单 `Tools/AddToBytes` 或 `PublishedConfigBuilder.Prepare()` 生成，保留生成器的校验与回滚流程。不要手工修改 `.bytes` 作为最终修复。
5. 检查生成结果及差异：`Remote_GameConfig` 分组、`GameConfig` 标签、地址、LocalizationSettings 字体映射、SpriteCatalog 引用以及过期产物清理是否符合本次输入。
6. 跟踪客户端加载到组装和安装成功后的就绪状态。配置未就绪不能用空数据伪装加载成功；失败路径应保留可定位的表名或资源信息。

## 验证与交付

- 修改解析或格式时运行 [GameConfigCsvEditorParserTests](../../../Assets/Tests/Editor/Tools/GameConfigCsvEditorParserTests.cs) 及对应覆盖；共享组装和读取变化检查 [GameConfigTests](../../../Assets/Tests/Editor/HotUpdate/Network/GameConfigTests.cs)。新增语义需有正常和无效输入案例，普通调值不机械增加测试。
- 触及服务端时核对 Backend 中实际测试项目，运行相关范围；不要自动更新子模块远端版本。
- 核对生成前后数量、字段和相关游戏显示或业务读取。Editor 不可用时可以完成源表与代码检查，但必须明确“尚未生成/导入/运行验证”，不要伪造 `.bytes` 产物。
- 配置生成与远端上传分开。用户只要求改表或本地验证时不上传、不激活内容、不修改真实账号；需要发布时按当前开发工作台实现和既有授权执行。
- 交付列明源表、生成产物、共享或服务端变更、验证结果，以及是否存在尚未执行的生成或发布步骤。
