# 活动系统实现

活动入口已接入大厅现有 UIFrame。礼物按钮打开 `ActivityWindow`，邮件按钮打开原 `GiftWindow`。新增功能与协议都位于 `Assets/Scripts` 的 HotUpdate 程序集；后端直接编译同一份纯 C# 协议，未增加 AOT 功能代码。

## 界面与素材

实际 Prefab 位于 `Assets/UI/Prefab/Hall/Activities/`：

- `ActivityWindow.prefab`：普通活动窗口，左侧列表、右侧详情。
- `ActivityPopupWindow.prefab`：自动弹窗，复用同一详情。
- `ActivityDetailView.prefab`：时间、条件、进度、公告和奖励区域。
- `ActivityListItem.prefab`、`ActivityRewardItem.prefab`：活动条目与领取卡片。

两个窗口均继承 AWindowController、使用 Enqueue；自动窗口 IsPopup 为 true。窗口已登记到大厅 UISetting、PrefabCatalog 和 Addressables，按钮引用由 UiScreenGenerator 生成并绑定。其他组件均通过序列化字段引用。

```csharp
[SerializeField] ActivityDetailView m_Detail;
[SerializeField] ActivityRewardItem m_RewardPrefab;
```

从桌面 UI 包导入的 16 张图片位于 `Assets/UI/Sprite/Activities`。`import-metadata.json` 保留原文件路径、SHA256、pivot、border 和 pixelsPerUnit；导入器按这些信息设置 Sprite。重建入口为 Unity 菜单 `Tools/Activities/Build Prefabs`，按钮重新绑定入口为 `Tools/Activities/Rebind Generated References`。

界面演示截图：[活动窗口](../../Tools/Activities/preview-window.png)、[活动弹窗](../../Tools/Activities/preview-popup.png)。截图使用演示快照渲染实际 Prefab，不代表数据库已发布的活动。

## 服务端状态与发布

活动定义存储为完整草稿和不可变发布版本。`ActivityDefinitionRecord.ActiveVersion` 指向 `ActivityPublishedVersion`，各玩法的领取项、排期、条件和奖励快照在同一版本中保存。玩家进度、访问日、周期计数、领取记录、弹窗回执和幂等操作分别存储为带唯一约束的数据库记录。

新增迁移 `AddActivitySystem` 由现有应用启动迁移流程应用。后台可保存草稿、发布、下架、复制活动、查看历史和维护礼包。已发布礼包不可原地修改；已有玩家进度或领奖记录后，活动的奖励、成本、周期、开始时间及档位身份冻结。下架和恢复保留原进度。

领取与兑换复用现有 CardInventorySettlement，金币、卡牌、异画、溢出 UR 和玩家 Revision 同时结算。事务内同时写入领取次数及幂等结果：

```csharp
await using var transaction = await db.Database.BeginTransactionAsync(ct);
var result = await action();
await db.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);
```

同一 RequestId 的已完成结果在活动时间、定义版本与计数检查之前识别。新 RequestId 仍受领取项业务上限限制。抽卡里程碑在原抽卡库存提交时更新进度，失败的抽卡不产生活动进度。

## 客户端同步

ActivityManager 安装完整快照，拒绝旧响应覆盖新响应；使用服务端时间与 Stopwatch 计算倒计时。大厅每 60 秒同步，并在活动开始、结束和北京时间午夜重新同步。刷新同时读取玩家资料，恢复网络超时后已经提交的库存变化。

累计登录通过大厅 visit 接口按不同北京时间自然日计数。每日奖励使用日期 PeriodKey，整个活动使用 `all`。领取失败后保留旧快照、停止新领取并提供刷新入口；网络结果未知时保留原请求，按钮提供手动重试上一笔操作。

自动弹窗只在大厅入场完成后启动，优先级从高到低、排序号从小到大、ID 升序选择候选；最多三次真实展示/登录会话。显示完成后计数并提交回执，未提交的回执在后续同步重试。调度器等待现有窗口层释放当前窗口后才提交下一项，关闭后的公告跳转也走现有 ActivityWindow。

## 配置与使用

静态名称来自 `TableData/Translations.csv`，图片白名单来自 `TableData/activity-resources.csv`。两者均由现有 `Tools/AddToBytes` 生成二进制并注册到配置目录。活动操作继续使用 X-Content-Target/X-Config-Hash，并声明 X-Activity-Schema/X-Activity-Types；内容不兼容时按现有 CONTENT_CHANGED 流程处理。

启动本次后端与 AdminWeb，完成目标内容发布后，进入管理页面的「活动」页签，使用现有内容发布密钥载入文案目录。活动、礼包与兑换项选择 NameKey，页面展示已发布中文和英文；卡牌从已发布卡牌目录选择。

管理页的「创建计划书示例草稿」会创建六个示例和五份礼包，已有 ID 保留。也可运行相同的本地导入脚本：

```powershell
# ACHEN_CONTENT_PUBLISH_KEY 使用现有内容发布密钥
./Tools/Activities/seed_drafts.ps1 -PublishKey $env:ACHEN_CONTENT_PUBLISH_KEY
```

示例时间为计划书中的 2026 年 10 月 1 日至 8 日零点，金额为计划书演示值。每日福利与兑换长期开放。先发布国庆见面礼，再发布引用它的公告；其余活动可分别发布。首次导入只保存草稿，活动在管理员发布后才发送给玩家。

活动 API：`GET /api/activities`、`GET /api/activities/{id}`、`POST /api/activities/visit`、`POST /api/activities/{id}/claim`、`POST /api/activities/{id}/exchange`、`POST /api/activities/{id}/popup-shown`。管理 API 位于 `/api/admin/activities`，沿用现有发布密钥认证与版本并发检查。
