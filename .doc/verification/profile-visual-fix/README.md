# 资料与商城头像显示修复验证

## 修复范围

- 资料窗口当前页签不再被 Button 的悬停/焦点 SpriteSwap 覆盖，保持绿色底和黑色文字。
- 大厅头像移出原有 0.45 倍缩放层，使用 96×96 组合头像，移除两层旧边框。
- 商城头像与头像框共用透明商品卡，移除灰白底板，重新排列名称、头像、价格和拥有标记；清理旧 TMP 边距和行实例价格定位覆盖。
- 购买按钮绑定到根节点透明 Image，避免旧图片节点隐藏时同步禁用按钮。
- 头像异步加载捕获销毁取消令牌，销毁后不再回写 Image。

## 验证结果

- Inspector 引用审计：8 个预制体、11 个头像组件、104 个引用通过，无 Missing Script。
- Unity EditMode：`ProfileCustomizationTests` 共 4 项全部通过，见 `editmode-tests.json`。覆盖商品目录、透明点击层、大厅头像尺寸、文字布局、行实例价格定位、页签焦点/悬停、草稿与滚动复用。
- 并行 UR 功能合并后全套 EditMode：239/239 通过，见 `combined-tests.json`；这轮结果包含测试关闭清理修复，早于最后的头像销毁取消补丁。后者已重新编译成功并用于最终 Play 截图。
- 本地 Play UI 样本会话：实际 EventSystem 射线命中商品根节点，头像 1010002 显示 200 金币确认；头像框 1030002 显示 500 金币确认；均点击取消并回到商城。见 `purchase-verification.txt`。
- 运行截图均为用户现有 Game View 的 2160×1080 实际渲染，未缩放布局模拟其他分辨率。

| 截图 | 内容 |
|---|---|
| lobby.png | 大厅头像只有当前装备框，头像尺寸正常 |
| avatar-tab-hover.png | 头像页获得焦点和悬停后保持绿色选中 |
| frame-tab-hover.png | 头像框页获得焦点和悬停后保持绿色选中 |
| shop-avatars.png | 透明头像商品卡、名称与价格 |
| shop-frames.png | 透明头像框商品卡、装饰完整保留 |
| shop-avatars-purchase.png | 头像购买确认弹窗 |
| shop-frames-purchase.png | 头像框购买确认弹窗 |

## 边界与复现

运行期间并行任务新增 UR 配置，运行服尚未部署相同 ConfigHash；真实交易可能触发 CONTENT_CHANGED。最终弹窗验证使用内存样本并清空当前内存访问令牌，未提交购买、未更改磁盘登录令牌、金币或服务器拥有记录。本次未部署运行服，不声称真实交易联调通过。

```powershell
unity command --proxy-disable --project-path D:/GameWorkplace/Doing/TCGCardDem0 run_tests --mode editor --filter ProfileCustomizationTests --async_tests true
# 已打开商城的本地 Play 样本会话：只验证点击、截图和取消，结束后停止 Play。
unity command --proxy-disable --project-path D:/GameWorkplace/Doing/TCGCardDem0 run_script --file Tools/ProfileUI/VerifyShopPurchase.cs --entry VerifyShopPurchase.Main
```

大厅截图包含另一任务新增的 UR 显示，商城包含卡牌工坊页签；这两项属于并行任务，本次提交只包含头像相关增量。保留原有字体、Addressables 和后端工作区改动。
