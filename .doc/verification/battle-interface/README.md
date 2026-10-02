# 战斗界面需求对照与交付记录

更新日期：2026-10-02。唯一规格为[战斗界面纠偏与完整交付计划](../../project/battle-interface-mdpro3-implementation.md)，旧计划仅保留[摘要](../../project/battle-interface-plan.md)。本记录区分已完成的代码、场景中的实际操作记录与仍需确认的显示或设备行为。

## 本次收尾状态

用户已要求停止验收，仅整理当前工作快照。本项实现尚未全部完成，本文保留未完成状态，不能视为完整交付确认。

- 战斗Canvas已改为Screen Space-Camera，专用UICamera、相机栈及菜单/列表坐标适配已保存。
- 按钮输入资源曾出现引用ActionId与持久输入资源不一致；生成器和场景引用已修正，Point/Click能解析。真实鼠标点击的完整闭环尚未最终确认，不能仅凭按钮回调记录认定所有按钮可用。
- 阶段/回合原演出、触摸多指、宽屏及PC/Mobile的剩余使用表现仍留待后续处理；此轮不继续核对。
- 本轮字体新增变化仅为字形与图集缓存，已恢复开工副本。原有字体、其他Prefab和Backend改动保留，不纳入本项提交。
- 已清除误放入Assets的临时截图，以及旧拖放箭头代码、材质和导入记录；原素材、研究日志和开工备份保留。
## 依据与已替换要求

- 原始需求：桌面《战斗界面.md》，提出独立战斗场景、Game/UI分层、区域浏览、详情大图、独立高亮、各类移区动画、调试栏及必要初始化后直达。
- 前计划：固定双方演示构筑、只读数据源与命令、区域占位、表示形式、计时、阶段及统一动画收尾。
- 验收报告：桌面《界面验收.md》，要求按原项目校准比例、修复黑区及盘位，改为点击动作和合法区域选择，完善额外、墓地、除外的表现接口。
- 后续确认：禁用生成美术；使用MDPro3原资源；主额外牌堆斜放；墓地除外不铺实体卡；手牌不挡下方卡区；战斗Canvas改为 **Screen Space-Camera**。
- 明确替换：拖牌、拖动箭头、松手选表示以及上下分割视口均退出执行规格。适合迁移的原模型、层级、动画与布局公式直接适配。
- 本期边界：只有表现及基础区域约束；不执行真实卡效、祭品、连锁、战斗裁判、联网或AI。同伴、Auto、动作记录、信息、设置和回放不纳入。

## 需求到实现的对照

| 来源 | 要求 | 实现说明 | 当前实际状态 |
| --- | --- | --- | --- |
| 原MD、素材确认、验收 | 单一完整森林遗迹与三维感 | 复用原`Mat_002`近远场、`Grave_002`、`AvatarStand_002`及其POS节点；来源见`Assets/Art/MDPro3Battle/source-manifest.json` | 已有完整场面截图；外侧地面和树林衔接可见 |
| 验收 | 消除黑区、异常蓝边及透明矩形边缘 | 恢复`ground_boundary`、`mos_boundary`、`leafShadow`与`leafShadow02`，按原Shader家族适配URP | 当前场面已恢复外侧地面；行动半场的蓝色`PlayableGuide`保留 |
| 原MD、验收 | 全屏透视、手牌比例及不遮我方魔陷区 | 主镜头原位置/角度、宽高比FOV公式；保留`CardPlane/Pivot/Offset/Turn/CardModel`，按原算法扇形和抬升 | 16:10场面、抬升及选位快照已有；较宽比例待最终记录 |
| 后续确认 | 主卡组与额外卡组斜放，有厚度、数量和空堆表现 | 原`DuelDeckAppearance`，原yaw角，`card_shuffle`厚度跟随数量；卡背使用持久材质绑定 | 当前截图四处牌堆均有卡背；空堆与数量变化待最终场景记录 |
| 原计划 | 远端手牌显示卡背，可检查双方区域 | `DemoVisibilityPolicy`将显示策略与`CardPosition`分离，区域检查仍允许双方 | 当前截图远端手牌显示卡背；双方列表待最终记录 |
| 原计划 | 双方完整卡位与共享额外区，灵摆使用两端 | 怪兽/魔陷各五、场地区及四种集合区，中央共享额外区；动作profiles提供目标，过滤占位 | 场面卡位完整；Link调试入共享额外区已有实际记录 |
| 原MD、前计划 | 同卡副本独立，占位和数量一致 | 每张卡独立`InstanceId`，只读`CardView`，确认命令才移区 | 点目标移区、抽牌数量、回区及重置已有记录；满场、空组、副本独立待最终记录 |
| 验收 | 点击卡后详情及上方动作；不弹放置窗口 | HUD复用`CardDetailView`独立侧栏；菜单完全读取`CardView.Actions` | 场景坐标选卡后菜单与左详情可见；普通动作直接进入选位 |
| 验收、规则约束 | 通常召唤表攻、设置里守、特召先表示、Link仅攻 | `DuelActionProfile`给出表示及目标，UI不推导召唤资格 | 通召选位、特召守备选择、Link仅攻击已有实际记录；设置待最终记录 |
| 验收 | 黄色合法空位、提示条、点击目标才移区 | `PendingAction`维护步骤，`ConfirmActionTarget`确认目标 | Screen Space-Camera下选位已记录，确认后落入所选怪兽区 |
| 前计划、验收 | 非法点击、取消和重复操作不改变状态 | 待操作/动画参与输入锁；取消清理指针、选择及高亮 | 非法魔陷目标、重复结束回合与取消已有记录，手牌与行动方保留 |
| 原MD | 明确发动表现，不用高亮当卡效结算 | 独立`Effect`变更，播放提示及卡牌反馈，结束恢复原高亮；不执行卡效 | 已实现接口及动画，待最终场景记录 |
| 原MD、后续确认 | 墓地/除外隐藏实体卡，点击看内容 | 进入动画结束后关闭实体与Collider；原区域Shader分别维护存在、hover、press和可行动参数 | 入墓、除外隐藏已记录；场景坐标点击墓地能打开右侧Panel |
| 原MD、验收 | 非模态右列表、卡文、关闭和大图 | `BattleZoneWindow`改为Panel，无全屏shade；复用列表、侧栏及现有`CardZoomWindow` | 列表行选卡、显示左详情、打开及关闭大图已有记录；关闭后区域Panel仍可见且windowBusy为false |
| 验收 | 左详情不透草地，名字/属性/卡文可读 | 独立副本加不透明背景，仅侧栏矩形阻挡；保留原内容绑定；副本节点隔离生成器前缀 | 不透明侧栏已有快照；原窗口和Prefab未修改 |
| 后续确认 | UI Canvas使用Screen Space-Camera | BattleUIFrame保留独立UICamera，URP Overlay加入主镜头栈；菜单和列表坐标转换使用该相机 | `06-camera-ui.json`记录模式、UICamera及相机栈；后续点击、选位、表示仍可操作 |
| 原MD、验收 | 左中计时盘、右中回合阶段盘 | 原Timer/Phase根保持原点，使用内部文字和碰撞；进度、颜色与行动方绑定视图 | 左右盘位置已对照；相机模式下实际修正文字高度与材质后，右盘TURN/Main1与左盘180均可读 |
| 原MD | 六阶段、结束回合、不可用项禁用 | 原DP/SP/MP1/BP/MP2/EP图、底板及cursor，按`AvailablePhases`绑定 | 已实现，待最终窗口与阶段推进记录 |
| 原MD、前计划 | 抽牌、入场、入墓、除外、回手、回主组、回额外及重排 | LitMotion在同一透视空间插值分层pose，终态统一同步 | 抽牌及各类移区、Link回额外、移区可见性已有记录 |
| 原MD、前计划 | 翻面与转向 | 表示窗口提交`ChangePosition`，`Turn`与朝向随pose动画改变；里侧隐藏统计 | 守备翻为里守并跳至终态已有记录，状态与视觉对应 |
| 原MD、前计划 | 阶段、回合原演出 | 原Prefab、AnimationClip与Timeline轨道适配，保留动画位置、偏移与文本曲线 | 已接入，待最终阶段、回合及跳过记录 |
| 原MD、前计划 | 卡牌、牌堆独立高亮接口及脉冲轮廓 | `SetEffectAvailable(bool)`，每对象MPB控制，轮廓层使用原卡网格 | 已实现；双方独立高亮待最终场景记录 |
| 前计划 | 动画跳过、途中重置、退出收尾 | 统一保存finish动作与generation；跳过到最终pose，重置停止motion/粒子/演出恢复快照 | 选位后落场跳过、重复移区锁、途中重置已有记录；退出待最终记录 |
| 前计划 | 固定40主/15额外/5手，混合卡池及项目构筑限制 | `DuelDemoPreset`读取现有卡池与分区/限量配置，使用本地fixture库存，含额外类型 | 场景初始手牌、额外及抽牌变化已有记录；构筑源表不依赖玩家账号 |
| 前计划 | LP8000、先手回合1 Main1、180秒，仅行动方计时 | 玩家视图提供名称/头像/LP；本地计时暂停、重置、归零只提示 | 初始快照、暂停及移区中继续控制已有记录；归零和行动方计时待最终记录 |
| 原MD、前计划 | 一排完整调试栏、手机横向滚动 | 双方/区域/卡选择、查看、抽牌、各移区、表示、高亮、阶段、回合、计时、跳过、重置 | 一排栏可见；抽牌、入墓、表示、暂停、跳过、重置按钮已有记录 |
| 原MD、前计划 | 鼠标及安卓横屏、UI遮挡、主指针 | Input System鼠标与`primaryTouch`捕获；GraphicRaycaster遮挡；移动超过阈值不提交点击，无拖牌入口 | 场景屏幕坐标点选已有记录；触摸设备注入、第二指与安全区待最终记录 |
| 原MD、前计划 | 正常初始化后直达BattleScene，独立UISetting及热更代码 | 保留PreInit/内容/配置/Init单例；Init目标为BattleScene，SceneEntry加载BattleUISetting；运行时在HotUpdate | 启动入口与引用已接入，最终启动及退出记录待更新 |
| 项目约束 | 保留原未提交字体、Prefab和Backend改动 | 任务前文件哈希/副本为基线；战斗UI和资源独立，提交范围只取本任务 | 本轮字形缓存已恢复到`Logs/BattleImplementationBaseline`副本；原字体、其他Prefab与Backend改动排除提交 |

## 已确认的场景操作记录

记录位置：`Logs/BattleAcceptance`。记录主要通过场景对象、场地InputPointer屏幕坐标命中和按钮回调检查状态及视觉；按钮回调绕过物理输入模块，不能替代完整鼠标或安卓真机输入记录。

| 记录 | 操作与确认内容 |
| --- | --- |
| `06-camera-ui.json` | Canvas为ScreenSpaceCamera，UICamera进入主镜头栈，并保留原二维演出相机 |
| `07-camera-click-cancel.json` | 点手牌打开动作，选通召后点非法魔陷区域并重复结束回合；取消后仍在手牌并恢复输入 |
| `08-camera-special-choice.json` | 点选第二张手牌，进入特召表示窗口，选择表侧守备后显示合法目标 |
| `09-camera-confirm-skip.json` | 点击怪兽区确认，动画期间另一移区被拒绝；暂停计时、跳过后到达最终位置并恢复Collider |
| `10-flip.json` | 表示按钮切为里侧守备，跳过后显示卡背并隐藏统计 |
| `11-transfers.json` | 入墓、除外、回手、回主组、Link入共享额外及回额外；隐藏集合区实体；抽牌数量同步；动画中整局重置恢复初始快照 |
| `12-region-open.json` | 点击原墓地区域打开右列表，数量随区域内容更新 |
| `13-region-detail.json` | 点击列表行同步选中与左详情，卡图按钮打开现有大图窗口，Canvas仍为ScreenSpaceCamera |
| `14-region-stable.json` | 大图关闭后windowBusy为false；右墓地列表与左详情仍保留，恢复非模态浏览 |

旧`05-confirm-skip.json`属于切换相机前的中间记录；当前同一确认与跳过流程以`09-camera-confirm-skip.json`为准。用户已停止验收，后续记录本轮不再追加，不以代码存在或回调记录代替完整场景交付。

## 关键接口与最终适配

实际数据入口仍是`IDuelPresentationSource.Current / Changed / Submit`。界面从只读动作获取表示和目标，确认前不移区：

```csharp
source.Submit(new BeginCardAction(instanceId, actionId));
source.Submit(new ChooseActionPosition(position));
source.Submit(new ConfirmActionTarget(zone));
source.Submit(new CancelCardAction());
```

Screen Space-Camera实际适配包括序列化主镜头URP数据、独立UI相机及对应屏幕坐标换算；不修改大厅UIFrame：

```csharp
canvas.renderMode = RenderMode.ScreenSpaceCamera;
canvas.worldCamera = camera;
Root<UniversalAdditionalCameraData>(camera.gameObject).renderType = CameraRenderType.Overlay;
// 初始化时把本战斗frame的UI相机加入主镜头栈。
m_mainCameraData.cameraStack.Add(frame.UICamera);
```

## 画面记录与待完成对照

以下为直接复制的实际截图，均为1600×1000（16:10），未生成或加工图片。战斗Canvas使用Screen Space-Camera；截图显示原场地、四处卡背牌堆、远端卡背、底部手牌、LP栏及调试栏。

![全景与计时阶段盘](images/scene-camera-16x10.png)

来源：`Logs/BattlePreview/acceptance-camera-readable-phase-16x10.png`。重置后的实际场面，左盘180、右盘TURN 1/Main1可读；只有行动半场保留蓝色提示。

![墓地列表与卡牌详情](images/region-detail-camera-16x10.png)

来源：`Logs/BattlePreview/acceptance-region-detail-stable-camera-16x10.png`。我方墓地存在一张卡，实体不覆盖区域模型；点列表行显示左侧详情和可用动作，大图已关闭，列表为非模态Panel。原卡文、属性及统计复用现有详情视图。

当前仍需落实的交付状态：阶段和回合演出；双方区域列表；独立高亮；计时归零；占位边界；触摸主指针、UI遮挡及横屏设备表现。这些事项保留未完成，待用户以后决定是否继续。
