# 卡组窗口交付与验收

2026-09-27，Unity 6000.5.2f1，PC 16:9，使用项目原有启动场景、真实登录会话及本地后端。

## 交付

- 大厅卡组按钮打开 `DeckListWindow`，新建时调用后端创建空卡组。
- 四个窗口沿用 `AWindowController`、`UiScreenGenerator`、`UISettings` 和 Addressable Catalog；业务位于 HotUpdate。
- 目录：`Assets/UI/Prefab/Hall/Deck/`，包含列表、编辑、命名、未保存窗口和四个条目 Prefab。
- 使用提供的金色卡盒 `DeckCase2004_L.png` 与 CommonUI 按钮、输入框素材；面板为银色角标、蓝色扫描线、紫色斜条背景。
- 右侧按卡牌及版本分组，显示卡名和拥有数；未拥有灰显。左侧详情与中间卡组不显示拥有数，中间每张卡独立一格。
- 卡名模糊搜索复用工坊提取的 `CardNameSearch.Matches`；保留工坊原来的 ID 搜索。
- 主卡组/额外卡组自动分类，使用已有数量、版本库存和跨版本同名限制规则；不足 40 张允许作为草稿保存。
- 保存调用 `PlayerSession.SaveDeckAsync`，成功后使用服务端结果建立草稿快照。失败保留草稿，修订冲突沿用确认窗口提示。
- 五种 UI 卡面材质用于列表、详情和拖动卡图，支持 uGUI 裁剪。

```csharp
var result = await PlayerSession.Instance.SaveDeckAsync(m_state.Draft, ct);
m_state.AcceptSaved(result);
```

## 验收结果

- 编辑器卡组相关测试：56/56 通过，包含新增 3 项草稿状态测试；见 `editor-tests.json`。
- 176 处序列化组件引用有效；两个 Shader 无编译错误；34 个卡组 UI 翻译键已生成二进制。
- 实际 UI 射线命中验证单击详情、右侧拖入两次、重复牌展开两格、左侧拖回右侧、详情 +1/-1。
- 搜索「青眼」得到 4 个版本格，无匹配显示空状态，清空恢复 124 个格。
- 验证额外怪兽自动入额外卡组、改名、继续编辑、放弃改动后重新读取、保存并返回。
- 后端重新读取完整 CardId/Rarity/Count 与名称、修订号；最终主卡组 2 张、额外 1 张、修订号 2。
- 删除取消保留卡组，确认删除成功；全部临时验收卡组已删除。
- 返回复测等待两个关闭动画结束；避免工具直接调用按钮绕过实际 UI 的过渡期输入屏蔽。

截图：`01-list.png`、`02-name.png`、`03-empty.png`、`04-edit.png`、`05-drag.png`、`06-unsaved.png`（1920×1080），`07-edit-720p.png`（1280×720）。已逐图检查面板、卡名数量、金色卡盒和缩略图间距；没有遮挡或卡图重叠。拖动截图显示实际卡图与主卡组接收高亮。

拖动旧截图 `05-drag-before-fix.png` 记录了选中刷新清空缩略图纹理导致白块的问题；修复为在 `Select()` 前捕获纹理。最终修复通过独立 HotUpdate 编译（0 错误，13 条原有警告）。用户处理 Unity 阻塞弹窗后，23:09 对最终代码再次完整执行交互验收，拖动纹理非空断言通过，修复后的 `05-drag.png` 已补拍并查看。验收脚本确认后端保存与重新打开、放弃修改、保存返回、删除取消和确认均通过，临时卡组已经删除；见 `acceptance.txt`。

```csharp
var texture = m_View.Texture;
Select();
m_data.Window.BeginCardDrag(m_data, texture, e.position);
```

## 重建与验证

停止 Play、等待编译完成后，用 Live Editor 执行：

```powershell
unity command run_script --file Tools/DeckUI/BuildDeckPrefabs.cs --entry BuildDeckPrefabs.Main
unity command run_script --file Tools/DeckUI/VerifyDeckUI.cs --entry VerifyDeckUI.Assets
```

`BuildDeckPrefabs` 从当前指定的桌面素材路径导入，已有 Prefab 可直接使用。`ScaffoldDeckWindows` 仅用于最初脚手架，已加保护，现有控制器不会被重写。

交互脚本 `VerifyDeckUI.Acceptance` 需已从卡组列表创建名为「金色卡盒 · 界面验收」的空卡组；此脚本会保存、改名并删除该测试卡组。截图保存在项目根目录 `.doc`，不进入运行时资源。
