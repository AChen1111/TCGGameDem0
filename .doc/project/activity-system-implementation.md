# 活动 CSV 配置与独立发布

活动继续使用大厅现有 UIFrame、ActivityWindow、ActivityPopupWindow、ActivityDetailView 和条目 Prefab。新增配置、协议和加载代码位于 HotUpdate。主包启动协议、游戏 DLL、普通配置哈希及 Addressables 版本不参与活动发布。

## 编辑与生成

唯一编辑来源为平铺的 `ActivityTableData/`：总表 `activities.csv` 每行一个活动，`DetailTable` 引用不带扩展名的独立子表。CSV 使用字段名行、类型行、数据行，数组使用 JSON 数组字符串并按 CSV 规则加引号。字符串活动 ID 区分大小写；表名不能仅大小写不同。

```csv
ActivityId,Type,DetailTable,IsEnabled,ScheduleMode,StartsAt,EndsAt
string,int,string,bool,int,string?,string?
daily_gold,1,daily_gold,True,0,,
```

以上仅为字段局部示意，实际总表还包含名称、说明、排序、显示方式、图片、锁定与完成展示、弹窗策略、四组开放条件数组。完整字段以源表第二行类型及 ActivityMasterRow 为准。

玩法类型：0 公告、1 礼包、2 签到、3 兑换、4 抽卡里程碑。排期：0 长期、1 限时；显示：0 页面、1 弹窗、2 两者。周期：0 全活动、1 北京时间自然日。限时起止时间必须含时区，结束时刻不包含在有效期内；长期活动起止时间留空。

- 礼包与兑换每档一行：EntryId、NameKey、Description、PeriodKind、LimitPerPeriod、TotalLimit（留空表示无总次数限制）、SortOrder，以及 RewardTypes/RewardIds/Amounts/CardVariants 四组等长奖励数组。兑换增加正数 CostGold。
- 签到增加 DayIndex 与 AllowCatchUpClaims。签到日从 1 连续递增，补领开关各行一致；所需天数等于行数。
- 里程碑增加正数且唯一的 Threshold 与 PackIds。参与卡包数组各行一致，仅累计成功抽卡次数。
- 公告只有一行：Content、ImageResourceKey、ActionKind（close/activity）、ActionTarget（目标活动 ID）。

奖励类型：1 金币（RewardId=gold、CardVariant=0），2 卡牌（已发布的卡图 ID、CardVariant=0..4）。开放条件使用 ConditionTypes/ConditionValues/ConditionActivityIds/ConditionTimes 四组等长数组；类型可为 ownedCardKindsAtLeast、activityCompleted、playerCreatedBefore、playerCreatedAfter。无关字段使用 0 或空字符串占位，全部条件须满足；前置活动不能形成循环且必须可永久完成。

```csv
RewardTypes,RewardIds,Amounts,CardVariants
int[],string[],long[],int[]
"[1,2]","[""gold"",""00213326""]","[200,1]","[0,0]"
```

执行 `Tools/AddToActBytes` 只生成活动配置到 `Assets/ActivityConfiguration/`，沿用 BinaryTable 编码，检查活动表结构、数组和活动之间的引用。不读取普通配置，不检查文案、图片、卡牌或卡包是否已发布。`Tools/AddToBytes` 继续只处理 `TableData/` 和 `Assets/GameConfiguration/`。活动标签为 ActivityConfig/ActivityConfig.Generated，独立分组不参与普通 Addressables 构建；generated.json 是生成记录，不是编辑来源。

总表历史行由人工保留。停止活动时设 IsEnabled=False 或调整 EndsAt。没有自动删行管理工具。新增图片、文案与卡牌仍由现有普通内容流程准备，活动生成、发布及加载均不检查普通资源引用，不要求重新发布所有平台。实际显示使用客户端现有资源，卡牌奖励沿用服务端现有卡牌结算数据。

## 发布

打开 `Tools/活动配置/发布活动`，使用现有内容发布密钥及后端地址，点击发布已生成活动包。工具读取最近生成清单，核对全部文件大小/SHA-256及源 CSV 哈希。CSV 有变化时必须先执行 AddToActBytes；发布按钮不会隐式生成。总表和全部引用子表一次上传，全平台共用同一发布版本。

发布包是平铺 ZIP：manifest.json、activities.bytes、全部子表.bytes。清单包含 SchemaVersion=2、ReleaseId（新的 UUID）、ExpectedRevision（当前活动发布修订）、SourceHash，以及 Files 中每个文件的 Table/Size/Sha256。工具只查询当前活动修订并上传活动包，不请求普通内容版本、平台或配置哈希。

```csharp
// 当前项目实现：发布前先检查生成后是否改过 CSV
if (SourceHash() != manifest.SourceHash)
    throw new InvalidOperationException("活动 CSV 自生成后已变化，请先执行 Tools/AddToActBytes");
```

后端完整解析、校验引用与固定规则，先保存不可变版本文件，再在事务中更新当前版本指针。失败时保留当前发布版本。数据库保存总表、每活动定义版本、文件签名、发布历史和玩家记录；子表正文只保存在内容存储根目录的 activities/版本ID/表名.bytes 中。玩家领取的奖励快照及幂等响应仍正常保留。

活动列表、签到访问、弹窗回执和纯金币领取不读取普通配置。只有实际发放卡牌时，结算器读取请求对应的卡牌数据和溢出 UR 配置；活动发布不会提前遍历或校验任何平台的普通版本。

同 ID 首次发布后固定玩法、EntryId 集合、签到日/阈值、参与卡包、补领开关、周期及次数上限。奖励、兑换成本、文案、图片、展示与排期可更新。总表或子表变化时只递增该活动的 DefinitionVersion，未变化活动不递增；全球发布 Revision 每次成功发布递增。进度与次数按活动 ID 保留。同 RequestId 的原结果在时间、定义版本和玩家 Revision 检查之前识别，旧超时请求重试不会重复发奖。兑换确认期间版本变化要求重新确认成本。

后台活动页面只读，显示当前总表、全平台版本与发布历史；旧活动和礼包编辑接口已移除。不提供旧 JSON 活动配置转换。ActivityCsvRelease 迁移删除旧草稿/完整定义正文列，保留玩家记录及其约束。旧库若存在旧 JSON 活动定义，应在升级前按新 CSV 流程另行整理，迁移不会替它生成子表。

## 加载与界面

手工登录与账号恢复均在进入 GameScene 前等待当前开启活动的子表和玩家状态。当前开启只看启用与有效期，长期活动有效；个人条件不影响下载。禁用、未来和结束活动保留在总表中，不下载子表，也不提前创建详情。零个开启活动正常进入大厅。

```csharp
await GameConfigManager.Instance.InitializeAsync();
await ActivityConfiguration.WaitUntilReadyAsync();
await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
```

会话内每 60 秒同步，并在开始/结束和北京时间午夜边界重新计算。发现新发布版本或边界变化时显示现有 LoadIN，遮挡全部 UI，清除旧键盘焦点；全部当前开启子表与状态准备完毕后统一替换。加载失败显示具体原因和重试按钮，保持遮挡；重试获取最新总表，不使用旧配置继续交互。子表缓存按后端地址和文件哈希隔离，每次先由服务端总表确认哈希再复用缓存。切换账号/登出取消旧会话加载及定时同步。

本地 Development 后端可通过未纳入 Git 的 `Backend/src/AChen.Backend.Api/Data/activity-clock.json` 设置活动测试日期，例如 `{"Activities":{"DevelopmentTime":"2026-10-01T12:00:00+08:00"}}`。重启后端后活动时间从该时刻继续走动；删除文件并重启恢复真实时间。客户端仍使用后端返回的 ServerTime，登录令牌和账号创建时间使用真实时间。当前活动时间可查询后台总表接口的 serverTime。

`Assets/UI/Prefab/Common/LoadIN.prefab` 已增加 ActivityStatus、ActivityProgress、ActivityRetry；SceneTransitionOverlayView 的 Canvas、CanvasGroup、背景、状态、进度、按钮及标签均通过序列化字段绑定。重建这些引用可通过 unity-cli 执行 `Tools/Activities/ConfigureLoadingPrefab.cs` 的 Main。

签到仍仅在实际大厅入场后访问 visit 接口，弹窗在入场动画结束后按优先级调度；每次登录最多三次真实展示，回执按策略版本与周期保留。原金币、卡牌、异画及溢出 UR 结算和玩家 Revision 规则继续复用。

## 示例与工具

仓库包含六个独立子表：国庆礼包、每日金币、三日签到、每日卡牌兑换、抽卡十次里程碑和跳转公告。限时示例为 2026-10-01 至 2026-10-08（北京时间），每日福利与兑换长期有效。`python Tools/Activities/create_examples.py` 重建源示例 CSV（会覆盖这七张示例表，勿用它覆盖运营修改）；不会上传。`Tools/Activities/publish_generated.ps1` 可在 Unity 已生成产物后发布，同样检查源哈希与文件哈希。活动窗口截图仍为 Prefab 演示。

接口详见 Backend/docs/api/activities.md。活动代码交付不会自动发布示例到真实后端。
