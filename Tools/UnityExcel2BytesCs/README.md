# UnityExcel2BytesCs 项目适配版

来源: 用户提供的 `UnityExcel2BytesCs/Assets/Editor/Excel2CsBytesTool.cs` 和 `Assets/Scripts/BaseTable.cs`。原始源码保存在 `Original/`, 不进入 Unity 的 Assets 目录。

原工具的 XLSX 字段名、类型、说明三行结构, 现在由 `Localization.schema.json` 提供。保留按字段定义生成 C# 数据类与 bytes 的流程, 导出器移植为 PowerShell, 不依赖生成类先编译、不加载 Assembly-CSharp.dll。当前接入的是 CSV, 没有引入原版 Excel.dll、SharpZipLib、示例场景或缓存目录。

## 导出语言表

在项目根目录执行:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1
```

也可以在 Unity 中使用菜单 **SDGSupporter → Excel → Export Localization CSV**。菜单调用同一个 PowerShell 导出器, 通过 ALog 输出完成或失败信息。没有自动刷新、编译、构建或自动导表钩子。

输入和生成文件:

- CSV 主表: `TableData/Localization/Translations.csv`, 不随安装包发布。
- 字段映射: `Localization.schema.json`, 将 `key/zh-CN/en` 映射为 `Key/Chinese/English`。
- bytes: `TableData/Generated/Translations.bytes`。
- C# 数据类: `Assets/Shared/Localization/Generated/TranslationRow.cs`, 属于 AChen.Shared。

修改 CSV 后需要重新导出, 将源表和生成产物一并纳入版本控制。仅修改文案时, 生成 C# 内容不变, 工具不会重写它。不要手动修改生成的 C#。最初添加工具及数据类后, Unity 菜单需等编辑器正常编译脚本后才可用; 命令行导出不需要这一步。

## 数据约束与二进制格式

支持原工具的 `string/int/bool` 及这些类型的数组, 数组仍以 `#` 分隔。CSV 使用 UTF-8, 支持标准引号、逗号与多行字段。导出时处理 `<br>` 和行尾, 检查表头、重复/空 key、两种语言非空文案中的占位符。校验失败时保留之前的产物。

二进制采用显式字段读写, 不再使用 XML 中转、BinaryFormatter 或本机绝对路径。格式为 `ETB1` 魔数、32 字节 schema SHA-256、Int32 行数, 后接逐行字段。整数为 little-endian Int32, 布尔值为单字节, 字符串使用 .NET BinaryWriter 的 7-bit 字节长度 + UTF-8, 数组为 Int32 数量 + 元素。该格式与原版 BinaryFormatter bytes 不兼容。

生成的读取类核对格式和 schema, 发布器将 bytes 打入统一配置快照, `LocalizationService` 在 Addressables 配置初始化完成后安装语言字典。运行时不读取或解析 CSV。

`inspect_export.py` 可只读核对本次语言表的 bytes 与 CSV 是否逐字段一致。它不执行 Unity、C# 编译或游戏测试。

## 统一内容发布

现有内容发布窗口会在构建 Addressables 前自动导出卡牌和多语言表, 校验商品、卡池、资源引用和壁纸偏移, 生成 `Assets/GameConfiguration/config.json`。此文件是生成产物, 请修改 CSV 或 Inspector 源数据。
