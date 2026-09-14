# 多语言使用说明

## 数据与字体

- `TableData/Localization/Translations.csv` 是后续维护的唯一语言主表, 列为 `key,zh-CN,en`。CSV 使用 UTF-8, 用支持标准 CSV 引号和多行单元格的编辑器维护。
- 初次导入完整 411 条, 另补充加载动画、启动错误及大厅遗漏文案。`card.*` 已收录, 暂不绑定卡牌 UI。
- `Assets/Resources/Localization/Settings.asset` 配置中英 TMP 字体。字体与导出的 `Translations.bytes` 内置安装包, CSV 不打包, 不通过远端内容更新。
- `映射表.md` 仅为初始参考。`import_mapping.py` 拒绝覆盖已存在的 CSV。

修改 CSV 后执行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1`, 或点击 Unity 菜单 **SDGSupporter → Excel → Export Localization CSV**。导出器同时生成 bytes 和 C# 数据类, 无需先编译生成的类。详情见 [导表工具说明](../UnityExcel2BytesCs/README.md)。运行时仅首次读取 bytes 建立字典。

## UI 组件

每个需本地化的 TMP UI 文本挂 `LocalizedText`, 在 Inspector 填写 key。静态文本不勾选 Dynamic Content; 动态文本勾选该项, 首次业务赋值前显示空白。昵称与输入框的实际输入文本不挂组件, Placeholder 需要挂。

```csharp
LocalizationService.SetLanguage(GameLanguage.English);
LocalizationService.SetLanguage(GameLanguage.SimplifiedChinese);

title.Localized().SetKey("shop.pack.01");
price.Localized().SetKey("ui.common.gold_amount",
    new Dictionary<string, object> { ["gold"] = priceGold.ToString("N0") });

message.Localized().SetMessage(new LocalizedMessage("ui.shop.confirm_buy",
    new Dictionary<string, object>
    {
        ["gold"] = priceGold,
        ["name"] = new LocalizedMessage("shop.avatar.00")
    }));
```

运行时切换自动更新启用的组件; 隐藏组件下次启用时刷新。选择保存在 `TCG.Localization.Language`, 缺省值为简体中文。本版没有新增切换入口。

动态内容通过 `SetKey` / `SetMessage` 更新, 无内容时调用 `Clear()`; 不再直接赋值 `TMP_Text.text`。组件不会翻译参数中的普通字符串。复用列表绑定新数据时须重新设置 key 和全部参数。

缺少 key、当前语言翻译或必要参数显示 `Null`, ALog 的 `Localization` 分类记录 key/语言/原因, 同类问题每次运行仅记录一次。导出时拒绝重复 CSV key; 运行时遇到损坏或 schema 不匹配的 bytes 会记录错误, 不回退读取 CSV。

## 启动与发布

`AChen.Shared` 包含多语言及 ALog 运行时。日志原目录通过 asmref 加入共享程序集, 保留原脚本 GUID。HotUpdate 和其编辑器程序集显式引用共享程序集。

`Assets/Shared/link.xml` 保留共享接口, 避免安装包裁剪掉仅由热更新代码调用的方法。

启动回调由 `Action<string>` 改为 `Action<LocalizedMessage>`, `LoadDll` 的反射签名同步更新。首次发布必须将新安装包和对应 HotUpdate DLL 配套发布; 旧 DLL 不支持新启动签名。不要仅发布 DLL 或语言 CSV。

## 本次静态绑定

`bindings.json` 记录绑定的组件和嵌套覆盖。Pipeline 的 attach_script 在未编译时无法解析新组件, 因本次禁止编译, 改用 `bind_assets.py` 写入序列化绑定。无需运行该脚本即可使用已提交的资源; 它不是运行时自动扫描或构建钩子。

静态检查工具 `inspect_bindings.py` 仅检查 CSV、GUID、组件挂载和 key 覆盖, 不启动 Unity、编译或运行游戏测试。

待用户要求后进行运行验收: 中文首次启动、英文切换与重启记忆、隐藏窗口与列表复用、嵌套商品名、启动错误、Null 去重日志、字体材质及英文排版。此次未执行这些运行检查。
