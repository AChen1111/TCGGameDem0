# 英雄卡组：逐卡建模与效果响应实现手册

> 更新：2026-09-09。本文是可据以编写卡牌逻辑的设计规格，不是已实现代码。
>
> [决斗系统架构](duel-architecture.md)回答“规则与同步怎么分层”；本文回答“具体一张卡怎么定义、每个效果怎么发动和结算”。范围是英雄构筑：40 张主卡组、15 张额外，共 19 种卡。其他卡组沿用本文的拆卡方法，不复用英雄专属判断。现有登录与内容链见 [系统架构](architecture.md)。
>
> 规则依据以各条目所列 OCG 官方卡文和 FAQ 为主。工程字段、执行步骤是据此作出的设计，不是恢复出的 Master Duel 原始代码。MD 一致性仍须实机核对；少数尚缺直接裁定的分支在第 9 节单独列出，不能当作已验证结果。

联网采用 UDP 经典确定性帧同步：服务器和双方客户端执行同一套卡效。UI 按席位隐藏信息，但完整内核及帧记录可以读取或推导双方秘密。这是可信好友原型的边界。下文的“响应步骤”是规则时机，不是 UDP 包到达或网络逻辑帧边界。

## 1. 写一张卡时，实际要交付什么

不要从“这张卡继承哪个怪兽类”开始。每种卡交付以下四项：

1. 一条 `CardDefinition`：印刷属性、规则身份、所在卡组类别、能力列表、融合配方等。
2. 零个或多个 `AbilityDefinition`：分别描述发动效果、永续能力、召唤手续、召唤限制。
3. 所需的具名 C# 处理器或已有处理器的参数组合：负责条件判断和逐步处理，不负责 UI、联网和动画。
4. 一组局面案例：正常处理、连锁后条件改变、目标离场、处理部分成功，以及必要的时机例外。

这里的 YAML 和伪代码只是文档记法，不要求实现 YAML 解释器或新的规则 DSL。实际可以用不可变 C# 定义对象、普通 C# 条件方法和显式步骤处理器表达。

### 1.1 静态卡片数据

| 字段 | 应表达的内容 | 不要存成什么 |
| --- | --- | --- |
| `CardId` | 稳定卡片编号，本文用八位密码作为示例 | 本地化名称、Unity 实例 ID、网络连接 ID |
| `PrintedCharacteristics` | 印刷等级、属性、种族、攻防、怪兽 / 魔法 / 陷阱种类 | 当前受到效果影响后的攻防 |
| `RuleIdentity` | 卡名身份、系列信息，以及需要的同名处理 | `name.Contains("英雄")` |
| `DeckCategory` | 主卡组卡 / 额外卡组卡 | 当前所在区域 |
| `AbilityIds` | 本卡具有的多个独立能力 | 一段无法区分成本和处理的 `OnUse()` |
| `FusionRecipeId` | 素材约束的引用 | 在融合魔法脚本中按融合怪兽名字写 switch |
| `SummonConstraints` | 例如只能融合召唤 | 普通可被无效的永续效果 |

运行时的 `CardInstance` 另外保存拥有者、控制者、区域、表示形式、存在期、召唤记录和持续修正。同种卡的三张副本不能共用运行时次数和状态。

通常怪兽身份不能通过 `AbilityIds.Count == 0` 推导。效果被无效的效果怪兽仍不是因此就变成通常怪兽；某些二重怪兽在墓地视为通常怪兽，又需要区域内有效身份查询。

### 1.2 每个能力必须回答的字段

| 字段 | 要回答的问题 |
| --- | --- |
| `AbilityId` | 哪张卡的哪个独立能力？建议 `CardId.稳定后缀` |
| `Kind` | 是不入连锁手续、发动效果、永续能力，还是非效果规则条件？ |
| `EffectCategory` | 实际执行来源是怪兽、魔法还是陷阱效果？ |
| `ActiveLocation` | 从哪里发动 / 在哪里适用？是否必须表侧？ |
| `OfferWindow` | 自由行动、快速响应，还是指定诱发检查点？ |
| `TriggerPattern` | 如果依赖历史事件，具体是哪种事件、谁与谁的关系？ |
| `TimingEligibility` | 是否要求紧接该事件的时点？能否在伤害步骤发动？ |
| `Optionality` | 可以不发动，还是符合条件后必须进入强制处理流程？ |
| `ActivationCondition` | 发动时额外检查什么？ |
| `CostProcedure` | 发动手续里实际支付什么？没有就明确为 None |
| `ActivationTargetSpec` | 发动时是否取对象，取几个，按什么条件选？ |
| `ResolutionGuard` | 处理时卡文要求再次检查的条件，不能直接重跑全部发动条件 |
| `ResolutionHandler` | 处理步骤和中途选择是什么？失败后哪些步骤还继续？ |
| `UsageLimit` | 次数限制按卡名还是实体、在哪个节点消耗？没有不要擅自加一回合一次 |
| `ResetPolicy` | 效果结果什么时候失效？来源离场是否影响它？ |
| `Evidence` | 卡文 / FAQ、参考版本、推导与待核验项 |

处理对象不等于发动对象。`ActivationTargetSpec=None` 的效果，也可以在处理过程中让玩家选择卡。

### 1.3 发动时状态与处理时状态

`ChainLink` 固定本次发动玩家、源卡及存在期、效果 ID、模式、发动对象、成本结果和规则要求保存的信息。`ResolutionFrame` 另外保存步骤、正在等待的选择、实际操作结果和局部数值。

重要约束：

- 取对象条件与处理时对象有效性分开。不能把“发动时自己场上的怪兽”直接重新套用成“处理时必须仍由自己控制”的通用规则。
- 对象离场再返回是新的存在期，旧连锁不能重新绑定。
- 卡效已经入连锁后，来源离场不等于自动取消效果；但某些处理需要来源仍在，必须逐步骤表达。
- “可合法选择”不等于“一定受影响或一定被破坏”。抗性一般不应被错误写进所有选卡过滤器。
- 合法性查询不能提前抽卡、送素材、消耗次数，也不能预先暴露处理时才选择的信息。

## 2. 响应逻辑不是每张卡各写一套

单卡只声明自己的发动入口和例外；共同的交接、连锁与逆序结算由引擎执行。本文给每个能力标记以下模式。

| 模式 | 用于什么 | 是否建立连锁 | 对方在哪响应 |
| --- | --- | --- | --- |
| `N` | 标准通常召唤、盖放、战斗等基础动作 | 动作本身不入连锁 | 召唤等程序规定的窗口、成功后的响应 |
| `P` | 泡沫人自己的手牌特殊召唤手续 | 不入连锁 | 召唤无效的合法窗口及召唤成功后，不是连锁其“发动” |
| `S` | 本卡组的六张通常魔法 | 1 速卡的发动，通常从自己主要阶段自由状态发起 | 建立连锁项后，交接合法快速响应 |
| `T` | 通常陷阱发动 | 2 速；需要符合设置回合和当前时机规则 | 可以发起或加入合法连锁，按连锁规则交接 |
| `G` | 怪兽诱发效果 | 在指定诱发组织流程中建立连锁项 | 先按政策组织诱发，再开放合法快速响应 |
| `C` | 永续抗性、战斗保护 | 不发动、不入连锁 | 没有一个独立的“响应抗性”窗口 |
| `F` | 非效果的融合召唤条件 | 不发动、不入连锁 | 作为召唤合法性的一部分，不是可被无效的卡效 |

魔法速度只是许可条件之一。1 速诱发效果可能依同时诱发规则排列为多个连锁项，不意味着它们能随时连锁普通发动；2 速也不意味着能在所有伤害步骤窗口发动。

### 2.1 模式 S / T 的公共流程

1. UI 请求发动能力，而不是直接执行效果。
2. 引擎校验时机、卡片位置、适用限制、成本与对象等。
3. 执行发动手续；若取对象，在这里选择并记录。进入提交后不能随意撤回已支付成本。
4. 建立 `ChainLink`；询问具有下一响应机会的玩家是否发动合法响应。
5. 新发动加入连锁；双方依当前窗口完成放弃后，开始逆序处理。
6. 轮到某项时，按其 `ResolutionHandler` 处理。需要选卡就挂起当前项，仅接受该选择。
7. 一项处理结束继续处理当前连锁，不能每完成一个操作就插入新连锁。
8. 整条连锁及必要清理结束后，再到规定检查点处理新诱发和响应。

本卡组六张通常魔法都没有卡文规定的额外发动成本；不要把融合素材、检索结果或复活对象当作发动成本。正常从手牌发动通常魔法，也不能用“先送墓作为成本”替代把卡发动到场上的手续。

### 2.2 模式 G 的公共流程

```text
事件发生
  → 记录触发候选，不执行效果
  → 到达允许的诱发检查点
  → 检查时点仍有效、源卡资格和本卡条件
  → 按强制 / 可选及双方顺序政策组织
  → 必要时询问是否发动、顺序和发动对象
  → 建立连锁项
  → 合法快速响应
  → 逆序处理
```

强制效果不能统一使用“能产生收益才发动”的过滤器。例如新星主宰的强制抽卡不能因为牌库没有卡就静默不发动。需要对象却没有对象的强制诱发，也应由明确的强制诱发政策处理；本文将盖亚对应分支列为核验项，不生成永远无法回答的选卡面板。

### 2.3 调度器与单卡处理器的最小契约

以下是接口语义示意，不是需要直接复制编译的实现：

```text
EffectHandler.CanOffer(context) -> 可否提供本次发动 / 手续
EffectHandler.BuildActivation(context) -> 发动手续步骤
EffectHandler.Resolve(frame, readOnlyQueries, operations)
    -> Continue | NeedDecision | Complete

TriggerMatcher.Match(eventRecord) -> 触发候选
TimingPolicy.Check(candidate, checkpoint) -> 允许 / 不允许及原因
ContinuousProvider.Query(queryContext) -> 一项规则贡献
```

只有执行器操作系统可写状态。处理器不能 `UI.ShowDialog()`，也不能在 `EventCenter.OnSummoned` 回调中直接 `Draw(2)`。挂起帧保存效果 ID 和步骤，恢复时按注册表找到处理器。三端按相同规则顺序生成相同选择 ID，选择答案进入服务器封帧，再由三端共同恢复；网络与回放共享这份有序输入，不各端重新询问历史选择。

`ProcessFrame` 是效果处理的执行现场；`FrameBundle` 是网络层一个逻辑帧的输入，两者不能混淆。等待选卡期间的 `NoInput` 不等于 `Pass`，不会推进到下一个响应窗口。随机数、候选顺序、实例 / 选择 ID 和数值舍入遵守主架构的确定性约束。

## 3. 构筑与静态定义清单

主卡组怪兽每种 3 张，共 18；六种魔法各 3，共 18；两种陷阱各 2，共 4。额外五种各 3，共 15。下列中文名称作为阅读简称，身份以卡片编号为准。

| 编号 | 卡名 | CardId | 类别 | 属性 / 等级 / ATK / DEF | 数量 |
| --- | --- | --- | --- | --- | --- |
| C01 | 羽毛人 | `21844576` | 通常怪兽 | 风 / 3 / 1000 / 1000 | 主 3 |
| C02 | 爆炎女郎 | `58932615` | 通常怪兽 | 炎 / 3 / 1200 / 800 | 主 3 |
| C03 | 闪光人 | `20721928` | 通常怪兽 | 光 / 4 / 1600 / 1400 | 主 3 |
| C04 | 黏土人 | `84327329` | 通常怪兽 | 地 / 4 / 800 / 2000 | 主 3 |
| C05 | 泡沫人 | `79979666` | 效果怪兽 | 水 / 4 / 800 / 1200 | 主 3 |
| C06 | 狂野人 | `86188410` | 效果怪兽 | 地 / 4 / 1500 / 1600 | 主 3 |
| C07 | 融合 | `24094653` | 通常魔法 | 不适用 | 主 3 |
| C08 | E－紧急呼救 | `00213326` | 通常魔法 | 不适用 | 主 3 |
| C09 | O－超越灵魂 | `63703130` | 通常魔法 | 不适用 | 主 3 |
| C10 | 奇迹结合（奇迹融合） | `45906428` | 通常魔法 | 不适用 | 主 3 |
| C11 | H－炽热之心 | `74825788` | 通常魔法 | 不适用 | 主 3 |
| C12 | R－公平正义 | `37318031` | 通常魔法 | 不适用 | 主 3 |
| C13 | 英雄信号 | `22020907` | 通常陷阱 | 不适用 | 主 2 |
| C14 | 英雄炸裂 | `37412656` | 通常陷阱 | 不适用 | 主 2 |
| C15 | 火焰翼人 | `35809262` | 融合效果怪兽 | 风 / 6 / 2100 / 1200 | 额外 3 |
| C16 | 不死鸟魔侠 | `41436536` | 融合效果怪兽 | 炎 / 6 / 2100 / 1200 | 额外 3 |
| C17 | 盖亚 | `16304628` | 融合效果怪兽 | 地 / 6 / 2200 / 2600 | 额外 3 |
| C18 | 新星主宰者 | `01945387` | 融合效果怪兽 | 炎 / 8 / 2600 / 2100 | 额外 3 |
| C19 | 大龙卷 | `03642509` | 融合效果怪兽 | 风 / 8 / 2800 / 2200 | 额外 3 |

这里全部怪兽均为战士族和元素英雄。卡片密码保留八位显示形式；不要因为 `00213326`、`01945387` 等前导零而产生不同的文本索引。

## 4. 19 种卡逐张建模

### C01. 羽毛人：只写数据，不写专属效果

```yaml
CardId: "21844576"
Printed: { Kind: NormalMonster, Attribute: Wind, Race: Warrior, Level: 3, ATK: 1000, DEF: 1000 }
RuleIdentity: { Archetype: ElementalHERO }
AbilityIds: []
```

响应模式为公共 `N`。通常召唤成功会产生召唤事件，允许其他卡按规则响应；羽毛人自己没有召唤诱发，也没有攻击诱发。可被 E 检索、被 O 复活、被英雄炸裂回收；作为素材的合法性由具体配方判断。

验收：正常召唤后连锁中不凭空出现“羽毛人的效果”；两份副本可以成为两张独立实体。风属性不等于鸟兽族。[官方定义][Avian]

### C02. 爆炎女郎：另一条数据定义

与 C01 相同结构，替换为 `58932615 / 炎 / 战士 / 3 / 1200 / 800`，保留 `NormalMonster`、元素英雄身份和空能力列表。

响应仍为 `N`。卡片描述中的火焰不是效果伤害指令。它与羽毛人满足火焰翼人和不死鸟魔侠的具名配方，也可以填新星主宰者的炎属性素材角色。

验收：攻击造成的是公共战斗计算结果，不额外烧血；两张爆炎女郎不能冒充“羽毛人 + 爆炎女郎”。[官方定义][Burstinatrix]

### C03. 闪光人：通常身份必须显式保存

定义为 `20721928 / 光 / 战士 / 4 / 1600 / 1400`，`NormalMonster`，元素英雄，空能力列表；响应为 `N`。

验收：它能被“墓地元素英雄通常怪兽”查询选中；一个被无效、没有当前有效效果的泡沫人不能因此也被选中。当前 ATK 受强化时，不能改写印刷 ATK 1600。[官方定义][Sparkman]

### C04. 黏土人：守备能力由公共规则表达

定义为 `84327329 / 地 / 战士 / 4 / 800 / 2000`，`NormalMonster`，元素英雄，空能力列表；响应为 `N`。

验收：守备 2000 不是“不被战斗破坏”的能力；它可以填盖亚的地属性素材角色，但不能一个实体同时填盖亚的两个角色。[官方定义][Clayman]

### C05. 泡沫人：两个能力必须拆开

基础定义为 `79979666 / 水 / 战士 / 4 / 800 / 1200 / EffectMonster`。

#### C05-P1：手牌仅此卡时的特殊召唤手续

```yaml
AbilityId: "79979666.summon_only_hand"
Kind: NonChainSummonProcedure
CardTextCategory: UnclassifiedEffect
ActiveLocation: Hand
OfferWindow: OwnMainPhaseOpen
Condition: HandContainsOnlyThisInstance
Cost: None
Targets: None
ResponseMode: P
```

过程：检查手牌恰好只有本卡及公共召唤许可 → 选择合法场位和表示形式 → 进入特殊召唤尝试 → 处理允许的召唤无效窗口 → 成功则产生 `SpecialSummonSucceeded` → 进入成功后的诱发检查。

该手续不要求场上为空，不建立发动连锁，不把泡沫人从手牌送墓作为成本。手续是否可用与第二效果是否能抽卡分别判断。[官方补足][Bubble]

#### C05-E2：召唤成功时可选抽二

```yaml
AbilityId: "79979666.draw_two"
Kind: TriggerEffect
ActiveLocation: FaceUpMonsterZone
Trigger: SelfNormalOrFlipOrSpecialSummonSucceeded
TimingEligibility: OptionalWhenImmediate
Optionality: Optional
SpellSpeed: 1
DamageStep: AllowedAtApplicableSummonTriggerWindow
ActivationCondition: NoOtherCardsInOwnHandOrField
Cost: None
ActivationTargets: None
ResolutionGuard: NoOtherCardsInOwnHandOrField
Handler: DrawTwo
UsageLimit: None
ResponseMode: G
```

处理：在合法诱发检查点确认时点和资格 → 满足条件时询问是否发动 → 组织连锁并接受合法响应 → 处理时再次检查手牌 / 场上有无其他卡 → 满足才抽 2。

不要在事件刚发生时永久固定“场上卡数”：发动该特召效果的通常魔法可能随后清理离场。官方存在通过英雄到来完成特召后满足条件发动抽卡的例子。[条件与处理][Bubble]、[英雄到来案例][BubbleAlive]

验收：场上有盖卡时 P1 可以特召，但 E2 不因此可用；E2 发动后、处理前手牌增加其他卡则不抽；CL2 特召后又处理 CL1 的其他事项，不能简单将此可选“时”诱发无条件延迟到连锁后发动。第 7 节给出完整跨卡例子。

### C06. 狂野人：永续查询，不存在“发动抗性”

定义为 `86188410 / 地 / 战士 / 4 / 1500 / 1600 / EffectMonster`。

```yaml
AbilityId: "86188410.trap_immunity"
Kind: ContinuousEffect
ActiveLocation: FaceUpMonsterZone
Provider: AffectabilityQuery
ApplicationSubject: ThisMonster
RejectedEffectOrigin: TrapEffect
Activation: None
Targets: None
ResponseMode: C
```

每个规则操作尝试影响狂野人时，查询“这项处理是否作用于本怪兽、实际是否来自陷阱效果、抗性目前是否有效”，再决定是否适用。不要让其订阅 `TrapActivated` 后把整张陷阱的效果取消。[官方定义与补足][Wild]

直接相关的验收：英雄炸裂可以先完成回收，然后选攻击力符合条件的狂野人；抗性有效时，它不会被该陷阱破坏，但回收不撤销。

为后续复用必须保留三个区分：

- 陷阱可选择某怪兽，与处理能否影响它不同。[官方选择案例][WildSelect]
- 作用于玩家或其他怪兽的部分，不由狂野人的抗性整段挡掉；幻影雾剑的不同处理有不同适用结果。[逐处理作用对象][WildFog]
- 复制陷阱处理的怪兽效果，其实际来源仍可能是怪兽效果，不能按被复制模板的卡种判抗性。[复制案例][WildCopy]

无效与抗性的适用先后也必须保留：官方技能抽取案例的先后结果不同，不能写死“抗性永远优先”。[适用顺序案例][WildDrain]

### C07. 融合：一个参数化融合处理器

```yaml
CardId: "24094653"
AbilityId: "24094653.fuse"
Kind: NormalSpellActivation
Window: OwnMainPhaseOpen
SpellSpeed: 1
Cost: None
ActivationTargets: None
Handler: FusionByEffect
ProductFilter: FusionMonster
MaterialSources: [OwnHand, OwnMonsterZone]
MaterialDisposition: UseAsFusionMaterial
ResponseMode: S
```

发动前：存在当前合法融合方案即可，不需要把最终融合怪兽和素材作为公开对象锁定。合法方案包括素材组合、场位和召唤许可，不等于“预先处理素材”。

处理步骤：

1. 根据结算时局面重新查询合法融合方案；没有则结束，不弹空选择框。
2. 玩家选择融合怪兽及完整素材组合；这两项选择都发生在处理内，不开放新连锁。
3. 验证组合，包括两个槽不能使用同一实体；对自己里侧素材的使用按融合专门政策判断，不沿用“只能查询公开怪兽”的普通过滤器。
4. 以融合素材身份处理选中的卡，通常送墓，记录实际去向。
5. 成功满足素材使用条件后进行融合召唤；素材处理与召唤不是同时事项。
6. 记录召唤成功，继续结束当前连锁；融合怪兽的诱发在规定检查点才组织。

验收：素材送墓被替代为除外，不能一律判融合失败；连锁适用融合禁止区域后，处理时不能融合，则不先送掉素材。[基础与素材去向][Poly]、[融合禁止区域][PolyBlocked]

### C08. E－紧急呼救：不取对象的处理时检索

```yaml
CardId: "00213326"
AbilityId: "00213326.search_ehero"
Kind: NormalSpellActivation
Cost: None
ActivationTargets: None
Preflight: HasLegalElementalHeroMonsterInOwnMainDeck
Handler: SearchDeckToHand
Selection: { At: Resolution, Count: 1, Filter: ElementalHeroMonster }
ResponseMode: S
```

发动时不宣布具体找哪一张，创建连锁后正常交接响应。处理时重新列出卡组候选 → 选择 1 张 → 按检索规则公开结果并加入手牌 → 进行相应洗牌手续 → 结束当前连锁项。公开结果不是公开牌库顺序；检索和洗牌属于公共操作协议。[卡文与不取对象][Emergency]

验收：处理前原本候选消失，应重新查询其他合法候选；已无合法候选则结束。重复收到同一选卡答案不能再次加入另一张卡。不会因为本卡名称含“英雄”就只能搜索 `CardDefinition.Name` 中带某个中文字符串的卡。

### C09. O－超越灵魂：发动时锁定墓地对象

```yaml
CardId: "63703130"
AbilityId: "63703130.revive_normal_ehero"
Kind: NormalSpellActivation
Cost: None
ActivationTargets: { Zone: OwnGraveyard, Filter: ElementalHeroAndNormalMonsterInThisZone, Count: 1 }
Handler: SpecialSummonOriginalTarget
ResponseMode: S
```

发动前要有合法对象与特召方案。发动时选择墓地对象并写入 `ChainLink.TargetRefs`；对方可据此响应。处理时只找这一对象的原存在期，确认所需条件和召唤许可，再选择当时合法场位 / 表示形式并特殊召唤。

对象离开墓地不能换一张；离开后又回来也不能重新绑定。不要把发动时选对象误写成“提前占用一个将来场位”。正常怪兽身份按墓地规则查询，官方允许墓地视为通常怪兽的二重英雄作为对象。[官方补足][Oversoul]

验收：本卡组的四张通常英雄可选，泡沫人与狂野人不可仅因效果被无效就可选。复活引起的新诱发不得插入本连锁项的中间。

### C10. 奇迹结合：换参数，不复制融合引擎

```yaml
CardId: "45906428"
AbilityId: "45906428.miracle_fuse"
Kind: NormalSpellActivation
Cost: None
ActivationTargets: None
Handler: FusionByEffect
ProductFilter: FusionMonsterAndElementalHero
MaterialSources: [OwnMonsterZone, OwnGraveyard]
MaterialDisposition: BanishAsFusionMaterial
ResponseMode: S
```

其响应与处理骨架复用 C07，但不能使用手牌素材，产物还必须符合元素英雄身份。除外素材在结算中完成，不是发动成本；除外处理与融合召唤非同时。[基础 FAQ][Miracle]

材料合法性逐张 / 逐方案查询。对方存在禁止墓地除外的海空时，全部素材均来自场上的方案仍可能合法，不能直接把整张卡的按钮禁用。[海空案例][MiracleKycoo]

官方允许使用衍生物作为素材。因此“素材使用成功”不能定义为“除外区真的多了一张卡”；衍生物合法离场消失也需要正确的操作结果。第一组英雄不产生衍生物，但接口不能把这一能力写死。[素材补足][Miracle]

验收：场上黏土人 + 墓地羽毛人可以按配方融合盖亚；手牌黏土人不能被这张卡选作素材。发动后某素材失去可用性，处理时重新选择合法方案。

### C11. H－炽热之心：发动入连锁，穿透本身不入连锁

```yaml
CardId: "74825788"
AbilityId: "74825788.heat"
Kind: NormalSpellActivation
Cost: None
ActivationTargets: { Filter: OwnFaceUpMonster, Count: 1 }
Handler: ApplyHeatModifiers
Results: [ATKPlus500, PiercingBattleDamage]
Lifetime: UntilCurrentTurnEnd
ResponseMode: S
```

对象不必是英雄。发动时取对象，接受响应；处理时确认原对象仍满足本次处理所需状态并受魔法效果影响，给它建立两项有来源、有期限的结果。

后续攻击守备怪兽时，战斗计算器读取穿透许可；符合条件则产生战斗伤害，不再建立“H 穿透效果”的连锁，也不是再次造成一份效果伤害。通常魔法正常离场不会撤销已经适用的强化。[官方 FAQ][Heat]

验收：闪光人由 1600 加至 2100，攻击 2000 守备的黏土人时，在无其他效果情况下造成 100 战斗伤害；两张 H 的加攻分别存在，但穿透不会让同一次战斗重复造成两份伤害。期限与盖放 / 离场重置通过修正系统处理，不靠动画倒计时。

目标控制权变化、特殊无效和不同属性设定效果的组合必须使用具体处理政策；不自动复用发动筛选条件。详见第 9 节核验项。

### C12. R－公平正义：处理时动态数量、不取对象

```yaml
CardId: "37318031"
AbilityId: "37318031.justice"
Kind: NormalSpellActivation
Cost: None
ActivationTargets: None
CountAtResolution: OwnFaceUpElementalHeroCardsOnField
SelectionAtResolution: SpellTrapCardsOnEitherFieldExactlyCountedNumber
Handler: DestroySelectedGroup
ResponseMode: S
```

自己没有表侧元素英雄卡，或场上除本卡外没有魔陷时不能发动。数量统计是表侧元素英雄“卡”，不能只遍历怪兽区；处理时重算，不能锁定发动时的数量。[官方补足][Justice]

处理：读取当前 N → 查询可选魔陷 → 选择恰好 N 张 → 以同一组进行破坏处理，分别保存每张实际结果。可以选双方魔陷；对某张的抗性 / 保护不会使其他已合法处理全部撤销。

本设计将当前处理的 R 自身排除，并采用“必须能选足 N，否则不擅自改成尽可能破坏”的策略。其中自身排除得到官方星光大道案例支持；数量不足的完整发动 / 处理分支仍属待核验政策，不声称本轮取得了逐项直接裁定。[响应案例与推导依据][JusticeRoad]

验收：发动时有 3 张英雄，处理时只剩 1 张，应按 1 张选择，不仍破坏 3 张；不让玩家选“最多 1 张”后以 0 张取消必做处理。响应条件还可能需要推导“是否确定会破坏己方至少两张”，不能只检查 `ContainsDestroyTag`。[JusticeRoad]

### C13. 英雄信号：伤害步骤结束时可发动的通常陷阱

```yaml
CardId: "22020907"
AbilityId: "22020907.signal"
Kind: NormalTrapActivation
SpellSpeed: 2
Window: ApplicableBattleDestructionResponseAtDamageStepEnd
EventCondition: OwnControlledMonsterWasBattleDestroyedAndSentToGraveyard
Cost: None
ActivationTargets: None
SelectionAtResolution: OwnHandOrMainDeckElementalHeroMonsterLevelAtMost4
Handler: SpecialSummonSelectedMonster
ResponseMode: T
```

本卡是具有事件条件的陷阱发动，不是怪兽诱发效果。它在适用伤害步骤结束窗口加入响应，不塞进“双方强制 / 可选怪兽诱发”的组织列表。[官方 FAQ][Signal]

确认事件发生、设置回合等通常陷阱规则满足，且存在合法特召方案后，玩家可以发动。处理时重新选择手牌或卡组中的候选，再由共同召唤程序完成特召；卡组选择涉及的公开与洗牌由公共操作处理。

验收：必须战斗破坏并送墓，不是刚判定战破，也不是被效果炸掉；自己控制的对方所有怪兽战破送入对方墓地也可满足事件关系；衍生物消失或送墓改除外不满足该送墓条件。新召唤怪兽的诱发只记录候选，不能立刻插入当前连锁。

### C14. 英雄炸裂：两个选择时点、一个连锁项

```yaml
CardId: "37412656"
AbilityId: "37412656.blast"
Kind: NormalTrapActivation
SpellSpeed: 2
Window: LegalFastWindowExceptDamageStep
Cost: None
ActivationTargets: { Zone: OwnGraveyard, Filter: ElementalHeroAndNormalMonsterInThisZone, Count: 1 }
Handler: RecoverThenDestroy
ResponseMode: T
```

发动时只取墓地怪兽为对象，不提前选择对方要被破坏的怪兽。即使对方没有表侧怪兽也可以发动；伤害步骤不能发动。[官方补足][Blast]

处理步骤：

```text
step 0: 找到原墓地对象；对象失效则 Complete
step 1: ReturnToHand；保存实际移动结果
step 2: 若没有成功加入手牌则 Complete
step 3: 从返回结果所指的卡读取规则要求的手牌攻击力
step 4: 查询对手当前表侧怪兽，当前 ATK ≤ 上一步阈值
step 5: 没候选则 Complete；有候选则 NeedDecision(必须选 1 张)
step 6: 对选中卡执行陷阱来源的破坏操作；Complete
```

步骤 1 与 6 不是同时处理，但步骤 5 的询问不会重新开放连锁。回收结果保留，后半段失败不回滚。通过返回结果取得新的区域引用，不能继续把移动前的墓地 `CardRef` 当作手牌引用。[Blast]

验收：回收闪光人后阈值按手牌数值读取，不拿它过去受强化的场上攻击力；对手没有符合者只回收；选中有效抗性的狂野人时可以不破坏成功，但不能退回闪光人。

### C15. 火焰翼人：战斗事件之后的强制效果伤害

静态定义：`35809262 / 风 / 战士 / 6 / 2100 / 1200 / FusionEffectMonster`。配方为两张不同实体，分别满足羽毛人与爆炎女郎的具名素材要求。

能力一：`SummonConstraints=[FusionOnly]`，模式 `F`。这是非效果的召唤条件，不是“只要曾经融合过就能随便复活”。

能力二：

```yaml
AbilityId: "35809262.battle_burn"
Kind: TriggerEffect
ActiveLocation: FaceUpMonsterZone
Trigger: ThisMonsterBattleDestroyedMonsterAndSentItToGraveyard
Window: DamageStepEndTriggerCheckpoint
Optionality: Mandatory
SpellSpeed: 1
Cost: None
ActivationTargets: None
Handler: DamageByDestroyedMonstersOriginalATK
ResponseMode: G
```

伤害计算先产生战斗伤害和战破判定；被破坏怪兽在规定时点实际送墓后，到伤害步骤结束才组织本效果。满足资格则必须发动，随后允许该窗口中的合法响应，最后在本连锁项处理时给予效果伤害。[官方卡文与补足][Flame]

`BattleRecord` 保存破坏者、被破坏者、实际去向和原本数值所需信息。使用的是被破坏怪兽的原本 ATK，不是交战时受强化或半减后的当前 ATK。具体读取时点和后续跨区域参照政策不能仅靠一个通用“历史攻击力”字段猜测；扩展分支见第 9 节。

验收：同归于尽不发动；未送墓不发动；伤害计算时不提前烧血；战斗伤害与这一效果伤害是不同原因的事件。来源已经合法发动后是否还能处理，与“尚未发动时来源已离场”必须分别判断。

### C16. 不死鸟魔侠：战斗破坏保护，没有效果响应流程

静态定义：`41436536 / 炎 / 战士 / 6 / 2100 / 1200 / FusionEffectMonster`。具名配方与火焰翼人相同，另有 `FusionOnly` 非效果限制。

```yaml
AbilityId: "41436536.battle_protection"
Kind: ContinuousEffect
ActiveLocation: FaceUpMonsterZone
Provider: BattleDestructionPermission
Subject: Self
Result: PreventBattleDestructionWhenThisEffectApplies
ResponseMode: C
```

战斗引擎在给它确定战斗破坏结果时查询此能力；若保护适用，不标记将被战斗破坏。没有“确认发动”、没有连锁，也不是送墓之后再复活。[官方补足][Phoenix]

验收：它仍可能受到战斗伤害；不防效果破坏；效果无效时不提供保护。不要用 `Immortal=true` 或 `AllDamage=0` 表达。

### C17. 盖亚：强制取对象，减攻成功后才增攻

静态定义：`16304628 / 地 / 战士 / 6 / 2200 / 2600 / FusionEffectMonster`。配方是“元素英雄怪兽 + 地属性怪兽”的两张不同实体，另有 `FusionOnly`。

```yaml
AbilityId: "16304628.halve_and_gain"
Kind: TriggerEffect
ActiveLocation: FaceUpMonsterZone
Trigger: SelfFusionSummonSucceeded
Optionality: Mandatory
SpellSpeed: 1
Cost: None
ActivationTargets: { Filter: OpponentFaceUpMonster, Count: 1 }
Handler: HalveTargetThenGain
ResultLifetime: UntilCurrentTurnEnd
ResponseMode: G
```

融合成功后在规定检查点组织强制诱发；选择一个发动对象，加入连锁后正常响应。处理时取目标当时的当前 ATK，执行半值设定；仅此处理成功，才按此次半值结果对盖亚执行加攻。两项修正分别绑定受影响实体和当前回合结束期限。[官方依赖说明][Gaia]

局部变量应保存 `halfValue` 与操作是否适用，不能让盖亚攻击力通过实时引用目标而无限重算。例：目标当前 ATK 2000，则设为 1000，盖亚增加 1000，成为 3200；不是永久使它们相互依赖。

验收：目标在连锁中增攻，半值按处理时计算；目标已无法适用半攻，不给盖亚凭空加攻；半值出现小数按统一规则舍入，不用语言默认的银行家舍入。[数值舍入][Rounding]

无对象的强制诱发、源卡在处理前离场等分支在第 9 节明确列出，不把“强制”写成忽略对象和状态的任意操作。

### C18. 新星主宰者：战破后的强制抽卡，不要求送墓

静态定义：`01945387 / 炎 / 战士 / 8 / 2600 / 2100 / FusionEffectMonster`。配方“元素英雄怪兽 + 炎属性怪兽”，另有 `FusionOnly`。

```yaml
AbilityId: "01945387.draw_on_battle_destroy"
Kind: TriggerEffect
ActiveLocation: FaceUpMonsterZone
Trigger: ThisMonsterBattleDestroyedOpponentsMonster
Window: DamageStepEndTriggerCheckpoint
Optionality: Mandatory
SpellSpeed: 1
Cost: None
ActivationTargets: None
Handler: DrawOneForEffectActivator
ResponseMode: G
```

它与火焰翼人不能共用一个“战破并送墓”过滤器：新星主宰者只要求相关战破事实，战破衍生物或实际去向被替代为除外也会发动。到发动时源卡仍需具有该发动资格；提前盖放等会影响它。[基础 FAQ][Nova]、[盖放案例][NovaSet]

历史事件记录“谁战破了什么”，效果发动玩家在实际发动时确定。官方控制权案例中，伤害计算后被转移控制权的新星主宰者，到伤害步骤结束由新控制者发动并抽牌，不能把玩家锁定成攻击宣言时控制者。[控制权案例][NovaControl]

验收：只抽 1 张、无是否发动的可选询问；强制抽卡遇到牌库不足时由实际抽卡操作和胜负规则处理，不能通过隐藏触发来避免失败。

### C19. 大龙卷：一次性半值设定，不是持续光环

静态定义：`03642509 / 风 / 战士 / 8 / 2800 / 2200 / FusionEffectMonster`。配方“元素英雄怪兽 + 风属性怪兽”，另有 `FusionOnly`。

```yaml
AbilityId: "03642509.halve_opponent_board"
Kind: TriggerEffect
ActiveLocation: FaceUpMonsterZone
Trigger: SelfFusionSummonSucceeded
Optionality: Mandatory
SpellSpeed: 1
Cost: None
ActivationTargets: None
Handler: SnapshotAndHalveOpponentFaceUpMonsters
ResultLifetime: WhileAffectedMonsterRemainsFaceUpInMonsterZone
ResponseMode: G
```

融合成功后强制组织诱发；不在发动时取对象。处理时取得对手当前全部表侧怪兽，分别读取当时 ATK / DEF，建立半值结果。不是双方全场，不影响后来召唤的怪兽，也不会因来源大龙卷离场就统一恢复。[官方补足][Tornado]

没有“回合结束复原”。受影响怪兽保持相应表侧存在期间保留结果，离场 / 盖放等按该修正重置政策处理。不要实现为每次查询 `CurrentATK * 0.5`，否则会反复减半或随旧强化的失效错误变化。

官方例子：1000 攻的怪兽先被装备加至 1500，大龙卷使其成为 750；之后旧装备离场仍是 750。属性系统必须支持以适用时当前值计算并设定的新值，不是简单维护全部加法 Buff 后统一乘半。[数值案例][TornadoStat]

验收：第一次适用 1500 → 750，重复查询仍是 750；另一张大龙卷随后适用才产生新的一次半减。无怪兽可处理时，不生成一个必须从空列表选择的请求。

## 5. 哪些代码共享，哪些只属于一张卡

| 共享处理器 / 查询 | 被哪些卡使用 | 参数或局部差异 |
| --- | --- | --- |
| 普通怪兽动作 | C01–C06 | 卡片属性、次数、当前规则许可 |
| `FusionByEffect` | 融合、奇迹结合 | 素材来源、处置政策、产物过滤 |
| `SpecialSummon` | O、英雄信号、融合处理、泡沫人手续 | 来自效果还是手续、对象与选择来源、召唤种类 |
| `Draw` | 泡沫人、新星主宰者 | 数量、可选 / 强制、发动与处理条件 |
| `ReturnToHand` | 英雄炸裂、未来回收卡 | 对象、原因、实际结果 |
| `Destroy` / `DestroyGroup` | 英雄炸裂、R | 一张 / 同时一组、来源效果种类、实际受影响性 |
| `StatModifier` | H、盖亚、大龙卷 | 加值 / 当前值设定、计算时点、期限与重置 |
| `BattleRecord` | 火焰翼人、新星主宰者、英雄信号 | 是否要求送墓、事件角色、发动方、读取数值 |
| `AffectabilityQuery` | 狂野人及所有作用于怪兽的操作 | 逐处理作用主体和实际效果来源 |

专属部分应足够薄。例如英雄炸裂只编排“回收成功后按阈值选择破坏”；它不重新实现连锁优先权、普通陷阱设置规则、伤害步骤判断或卡牌动画。

### 5.1 融合配方的具体写法

```text
FlameWingmanRecipe / PhoenixEnforcerRecipe:
  role A = TreatedAsName(Avian)
  role B = TreatedAsName(Burstinatrix)
  constraint = DifferentPhysicalInstances

GaiaRecipe:
  role A = IsElementalHeroMonster
  role B = HasAttribute(Earth)
  constraint = DifferentPhysicalInstances

NovaMasterRecipe: GaiaRecipe 的 role B 改为 Fire
GreatTornadoRecipe: GaiaRecipe 的 role B 改为 Wind
```

角色可以重叠候选，但最终指派不能重复实体。两张地属性元素英雄可以各占一个角色；一张地属性元素英雄不能自己完成两人份素材。

融合替代素材、类型 / 名称变化、里侧素材可用性应进入 `MaterialPolicy`。不能要求所有未来素材都通过印刷密码直接相等，也不能因为配方简单就忽略自己的里侧怪兽。

### 5.2 数值结果必须保存什么

`StatModifierInstance` 保存受影响卡存在期、来源效果、本次计算数值、应用顺序、修正类别和重置条件。

- H：新增一份 +500 攻击力贡献，以及穿透许可，各自带期限。
- 盖亚：目标半值设定结果和自身增攻结果是两个记录，后者以本次操作成功为前提。
- 大龙卷：每个受影响怪兽一份 ATK / DEF 当前值设定结果，不持续查询对手全场。
- 非整数数值由公共 `StatMath` 处理；OCG 官方例子 725 的一半为 362.5，取 363。不要用会把 `.5` 默认舍入至偶数的计算方式。[Rounding]

属性计算需要区别当前值设定与普通增减的适用关系，不能宣称“把所有修正按一个全局整数 priority 排序”即可覆盖全部游戏王。第一版至少以 H、盖亚、大龙卷及旧加成消失的场景验证这一区别。

## 6. 英雄炸裂：从卡片定义到 UI 选择的完整例子

初始：A 的墓地有闪光人；B 场上有 1500 攻狂野人和 1800 攻的其他怪兽。A 的英雄炸裂已经满足设置回合要求，当前是允许通常陷阱发动、但不在伤害步骤的窗口。

| 步骤 | 引擎保存 / 处理什么 | UI 展示什么 | 是否可以发动新响应 |
| --- | --- | --- | --- |
| 1 | 查询 `37412656.blast` 发动资格 | 显示可发动 | 尚未发动 |
| 2 | 产生发动对象请求，A 选墓地闪光人 | 选择并公开墓地对象 | 发动手续中不开放普通响应 |
| 3 | 创建 CL1，保存闪光人旧墓地存在期引用 | 显示英雄炸裂 CL1 和对象 | 可以按连锁规则响应 |
| 4 | 双方放弃，固定连锁开始处理 | 连锁结算演出 | 不可再插入新响应 |
| 5 | 闪光人成功回手，保存返回结果与 1600 阈值 | 回手动画 | 不可 |
| 6 | 1800 攻怪兽不满足阈值；生成处理时选择，A 选狂野人 | 选择将破坏的怪兽，不标为新的发动对象 | 不可，这只是处理内选择 |
| 7 | 陷阱破坏操作查询狂野人受影响性；抗性有效则未破坏 | 表现未破坏结果 | 不可 |
| 8 | 当前连锁与清理完成，进入后续时机 | 按本地执行已封帧得到的结果更新局面 | 到新窗口才可响应 |

最终：闪光人在 A 手牌，狂野人仍在场。步骤 7 没有破坏成功，不会回滚步骤 5。此流程同时要求 `TargetRef`、`ResolutionSelection`、`OperationResult` 和 `EffectOrigin` 四种信息；一个 `OnActivate -> Destroy(target)` 无法表达。

依据：英雄炸裂的分段处理及狂野人的陷阱抗性；组合局面为纸面推演，不是 MD 实测。[Blast][Wild]

## 7. 火焰翼人 + 英雄信号 + 泡沫人：完整响应时序

初始：A 的火焰翼人战斗破坏 B 的怪兽并将其送墓；火焰翼人仍表侧留场。B 有之前盖放的英雄信号、手牌为空、卡组有泡沫人，其他召唤条件满足；双方 LP 足以使下述连锁正常处理完毕。

| 序号 | 规则位置 | 处理内容 | 不应该发生的事 |
| --- | --- | --- | --- |
| 1 | 伤害计算 | 比较数值，处理战斗伤害，记录将被战破 | 不立即抽卡或触发信号 |
| 2 | 伤害步骤结束的规定处理 | 被战破怪兽实际送墓，记录战破且送墓事实 | 不把送墓与战破判定混成同一时点 |
| 3 | 诱发组织 | 火焰翼人强制效果进入 CL1 | 不询问 A 是否放弃这个强制效果 |
| 4 | 合法连锁响应 | B 发动英雄信号作为 CL2 | 不把通常陷阱误塞入怪兽诱发分组 |
| 5 | 双方放弃后的逆序处理 | CL2 选择并特殊召唤泡沫人 | 不发动一个“泡沫人从手牌特召手续” |
| 6 | 仍在连锁处理 | 记录泡沫人的召唤事实，继续 CL1 | 不立刻询问抽二，更不立即抽二 |
| 7 | CL1 处理 | 火焰翼人造成效果伤害 | 不在 CL1 开始前开放新的快速响应 |
| 8 | 连锁后检查 | 检查泡沫人的可选“时”资格 | 召唤之后已有其他处理，不能简单因曾召唤成功就允许抽二 |

核心区别：`发生事件`、`记录候选`、`到合法发动时点`、`建立连锁`是四个步骤。候选队列不是“所有发生过的效果迟早都要执行”的任务队列。[Flame][Signal][Bubble]

反向比较：如果英雄信号是本连锁的最后处理，特召后没有使该时点失效的其他处理，并在正式检查时满足泡沫人空手空场等条件，才进入泡沫人可选抽二的正常资格检查。不能在召唤瞬间看到英雄信号尚未清理，就把条件永久判死。

本表是按官方卡文与补足建立的组合推演。实际 MD 中的提示行为还受客户端响应设置影响，录像验证必须同时记录设置。

## 8. 每张卡最少应有的验证案例

以下为应实现的验收规格，不是本次运行过的测试。

| 卡 | 正常案例 | 必须防住的错误 |
| --- | --- | --- |
| C01 羽毛人 | 普通召唤 / 作为具名素材 | 没有效果却建立卡效连锁 |
| C02 爆炎女郎 | 与羽毛人合法融合 | 将描述文本解释成额外烧血 |
| C03 闪光人 | 被 O 选中、被英雄炸裂回收 | 被无效的效果怪兽误当通常怪兽 |
| C04 黏土人 | 守备战斗、作为地素材 | 一张卡占两个素材槽 |
| C05 泡沫人 | 空手单卡手续成功，再检查抽二 | 把手续入连锁；漏做处理时条件检查；错过时点仍抽 |
| C06 狂野人 | 陷阱处理不能影响其本体 | 不能取对象、整张陷阱全部无效或永久无条件抗性 |
| C07 融合 | 处理时选素材后融合 | 提前把素材当成本；无融合许可仍先送素材 |
| C08 E | 处理时检索一张合法英雄 | 发动时公开锁定检索结果；使用过期候选 |
| C09 O | 对原墓地对象特召 | 目标离场后换目标；旧对象回来重新绑定 |
| C10 奇迹结合 | 场上 / 墓地素材除外后融合 | 偷用手牌；禁止墓地除外就拒绝全部场上方案 |
| C11 H | 加 500 并进行一次穿透战斗伤害 | 穿透再开连锁 / 当效果伤害 / 重复造成伤害 |
| C12 R | 结算时重算英雄卡数量并选等量 | 只数怪兽区；当取对象；当最多 N 张 |
| C13 英雄信号 | 战破送墓后的合法窗口拉怪 | 刚判战破就发；效果破坏 / 除外 / 衍生物消失误满足 |
| C14 英雄炸裂 | 回手后按处理时局面选破坏 | 伤害步骤可发；后半失败回滚；选卡时开新连锁 |
| C15 火焰翼人 | 战破送墓后强制烧原本 ATK | 未送墓仍烧；同归于尽仍发；使用战斗时强化值 |
| C16 不死鸟魔侠 | 战斗不破坏但正常处理伤害 | 万能无敌；送墓后用复活模拟保护 |
| C17 盖亚 | 2000 → 1000，自身 2200 → 3200 | 半攻失败仍加攻；只存一个相互引用的实时公式 |
| C18 新星主宰者 | 战破后强制抽一 | 额外要求送墓；可选发动；把发动方锁定在宣言时 |
| C19 大龙卷 | 结算时对手表侧怪兽各半值 | 回合末恢复；影响后来上场者；每查询一次再乘半 |

通用校验再覆盖：同一效果选择重复发送只入帧一次；缺帧先等待补齐；动画中断不改变规则；从帧日志重演不重复询问或重复处理素材；三端相同输入得到相同规则摘要。正常 UI 不显示对手未公开候选和牌库顺序，但不承诺客户端内存或完整帧记录保密。

## 9. 已确认内容与必须标出的核验边界

基础卡种、取对象与否、主要发动条件、伤害步骤特许及本文列出的直接 FAQ 结论已按官方来源核对。三个研究分工分别覆盖主卡怪兽、魔法、陷阱与额外；本文没有将其当作 MD 实测或实现测试。

下列分支有具体建模位置，不能在实现时散落成任意判断：

| 核验项 | 本文采用 / 提出的政策 | 尚缺什么 |
| --- | --- | --- |
| R 数量不足、当前 R 自身候选 | 自身排除；按精确数量选择，不能满足则不改成尽可能破坏；发动可行性与处理分别判断 | 部分为卡文与官方响应例子的推导，需专项 MD / 直接裁定核对 |
| 盖亚无合法发动对象 | 强制触发仍交给明确政策处理，不强造对象、不挂死选择 | 本轮未取得专门 FAQ；需确定是否建立无对象连锁项等精确行为 |
| 盖亚处理前自己离场 | 依“先减攻、成功后增攻”的单向依赖保留可完成部分，不整段回滚 | 来源离场分支为规则推导，需专项确认 |
| 火焰翼人被破坏卡后续再移区 | 保留原本 ATK 的参照来源和事件信息，不能用交战时当前 ATK 替代 | 精确跨区域参照政策未在本轮证明 |
| H 对象控制权变化 / 特殊无效 / 盖放交互 | 发动过滤、处理有效性、修正重置分开，不自动重跑全部原筛选条件 | 组合裁定和 MD 版本核对 |
| 复杂属性覆盖叠加 | 区分加值和适用时定值；先覆盖已列 H / 盖亚 / 大龙卷案例 | 不声称已覆盖所有游戏王数值裁定 |

对于尚未核验的分支，在开发案例中使用明确的 `PendingRulingVerification` 标记。不能把“待核验”转换为已通过，也不能在正式对局中默默把不支持的情况按有利于一方的方式处理。第一组卡中的核验项应在宣称“19 种卡完整支持”前关闭。

写下一张卡时，先照第 1.2 节填全字段，再定位第 2 节的响应模式，最后编排第 5 节的共用操作；出现无法表达的规则，再扩展对应模块。不要先为每张卡生成一个巨大 `OnEvent`，之后再补联网、回放和时点判断。

## 10. 文档依据与官方来源

项目文档依据：[决斗系统架构](duel-architecture.md)，复用其卡片存在期、显式流程帧、时机检查点、UDP 输入帧、确定性重演与本地玩家视图边界。本文提供逐卡字段、处理步骤和响应案例，不新增可运行卡效实现。

主要官方卡片与 FAQ：

- [羽毛人][Avian]、[爆炎女郎][Burstinatrix]、[闪光人][Sparkman]、[黏土人][Clayman]。
- [泡沫人][Bubble]、[英雄到来与泡沫人][BubbleAlive]、[狂野人][Wild]。
- [融合][Poly]、[融合禁止区域][PolyBlocked]、[E－紧急呼救][Emergency]、[O－超越灵魂][Oversoul]。
- [奇迹结合][Miracle]、[奇迹结合与海空][MiracleKycoo]、[H－炽热之心][Heat]、[R－公平正义][Justice]、[R 与星光大道][JusticeRoad]。
- [英雄信号][Signal]、[英雄炸裂][Blast]、[火焰翼人][Flame]、[不死鸟魔侠][Phoenix]。
- [盖亚][Gaia]、[新星主宰者][Nova]、[新星主宰者盖放][NovaSet]、[新星主宰者控制权][NovaControl]、[大龙卷][Tornado]、[大龙卷数值][TornadoStat]。
- [数值舍入][Rounding]、[狂野人的选择与受影响][WildSelect]、[幻影雾剑的作用对象][WildFog]、[复制效果的来源][WildCopy]、[抗性与无效适用顺序][WildDrain]。

[Avian]: https://www.db.yugioh-card.com/yugiohdb/card_search.action?cid=6310&ope=2&request_locale=ja
[Burstinatrix]: https://www.db.yugioh-card.com/yugiohdb/card_search.action?cid=6311&ope=2&request_locale=ja
[Sparkman]: https://www.db.yugioh-card.com/yugiohdb/card_search.action?cid=6313&ope=2&request_locale=ja
[Clayman]: https://www.db.yugioh-card.com/yugiohdb/card_search.action?cid=6312&ope=2&request_locale=ja
[Bubble]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6393&ope=4&request_locale=ja
[BubbleAlive]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=12446&keyword=&ope=5&request_locale=ja&tag=-1
[Wild]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6478&ope=4&request_locale=ja
[WildSelect]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=49&keyword=&ope=5&request_locale=ja&tag=-1
[WildFog]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=17676&keyword=&ope=5&request_locale=ja&tag=-1
[WildCopy]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=17591&keyword=&ope=5&request_locale=ja&tag=-1
[WildDrain]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=23897&ope=5&request_locale=ja
[Poly]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=4837&ope=4&request_locale=ja
[PolyBlocked]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=13198&ope=5&request_locale=ja
[Emergency]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6672&ope=4&request_locale=ja
[Oversoul]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6674&ope=4&request_locale=ja
[Miracle]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6432&ope=4&request_locale=ja
[MiracleKycoo]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=13407&ope=5&request_locale=ja
[Heat]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6671&ope=4&request_locale=ja
[Justice]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6673&ope=4&request_locale=ja
[JusticeRoad]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=14450&ope=5&request_locale=ja
[Signal]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6356&ope=4&request_locale=ja
[Blast]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=7637&ope=4&request_locale=ja
[Flame]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6344&ope=4&request_locale=ja
[Phoenix]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=6655&ope=4&request_locale=ja
[Gaia]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=8122&ope=4&request_locale=ja
[Nova]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=9439&ope=4&request_locale=ja
[NovaSet]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=14080&ope=5&request_locale=ja
[NovaControl]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?ope=5&fid=14081&keyword=&tag=-1&request_locale=ja
[Tornado]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=8592&ope=4&request_locale=ja
[TornadoStat]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=9592&keyword=&ope=5&request_locale=ja&tag=-1
[Rounding]: https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=11530&keyword=&ope=5&request_locale=ja&tag=-1
