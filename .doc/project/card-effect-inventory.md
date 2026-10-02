# 当前卡池效果盘点

> 盘点日期：2026-10-02。此文按当前项目配置源表整理，用于 gameplay 规则拆分；卡文仍以 `TableData/Translations.csv` 为准。

## 范围与现状

- 当前 `TableData/all-cards.csv` 与 `TableData/Cards.csv` 配置了 **91 张卡**：Card01 31 张、Card02 33 张、Card03 27 张。CSV 第二行是字段类型声明，不是卡牌。
- 卡名、类型和中英文卡文来自 `TableData/Translations.csv`。
- Card02 中的英雄构筑与 `.doc/project/hero-card-modeling.md` 的 19 种核心卡建模规格对应；其余卡池也纳入本次盘点。
- 当前 `Assets/Scripts/Duel` 下是决斗表现层与演示会话；`.doc/project/duel-architecture.md` 和英雄卡手册明确标注为设计规格。卡文数据已配置，不等于这些效果已有可执行规则实现。

## 统一口径：17 类可复用机制

统一为 **17 类**：前 3 类属于效果流程基础设施；后 14 类是会改变牌局状态或结果的操作族。下表“涉及卡数”按逐卡索引中的行为标签统计，同一张卡可同时计入多个类别，所以各行数字不能相加成卡牌总数。`CHECK` 是所有发动流程共用的规则能力，逐卡不重复标注。

| 编号 | 类别 | 涉及卡数 | gameplay 需要表达的内容 |
| --- | --- | --- |
| 01 | 条件、合法性与时机检查 `CHECK` | 公共 | 卡片位置、阶段、触发事件、对象候选、次数、区域限制、处理时条件。所有效果共用检查框架，逐卡配置条件。 |
| 02 | 选择、取对象与信息读取 `SELECT` | 55 | 发动时取对象、处理时选择、展示 / 确认卡片、查看对方手牌、随机选择、宣言卡名。对象引用要与处理时选择分开。 |
| 03 | 成本与发动手续 `COST` | 41 | 支付 LP、丢弃、作为成本解放 / 送卡 / 除外自身、取除超量素材等；成本与非连锁召唤手续分开记录，并保存成本发生时点。 |
| 04 | 抽卡 `DRAW` | 11 | 抽取数量、触发抽卡、抽卡后追加选择或丢弃。 |
| 05 | 检索与加入手牌 `SEARCH` | 24 | 从卡组 / 墓地 / 除外区找卡并加入手牌，包含公开结果与洗牌。 |
| 06 | 送去墓地 `GY` | 19 | 从卡组、手牌或场上送墓；区分成本与效果、素材处理、送墓替代及实际去向。 |
| 07 | 召唤与特殊召唤 `SUMMON` | 33 | 从手牌、卡组、墓地、除外区或额外卡组召唤 / 复活；融合、同调、超量、连接召唤及非连锁手续需要调用各自召唤规则。 |
| 08 | 返回、洗回与盖放 `RETURN` | 13 | 回手、回卡组 / 额外卡组、卡组顶 / 底、墓地盖放魔陷；需记录目标区域及离场后的新卡片状态。 |
| 09 | 破坏 `DESTROY` | 19 | 单卡、多卡、同时破坏、按条件破坏；与送墓和破坏替代分开记录。 |
| 10 | 除外 `BANISH` | 22 | 除外单卡 / 同名卡 / 卡组顶卡，区分表侧与里侧以及费用 / 效果。 |
| 11 | 无效与效果改写 `NEGATE` | 18 | 无效发动、无效效果、无效后破坏、把已发动效果改成另一效果；这些结果不能合并成单一“无效”布尔值。 |
| 12 | 保护、替代与限制 `RULE` | 21 | 战斗 / 效果破坏保护、效果抗性、不能成为对象、禁止连锁 / 发动 / 召唤，以及“改为除外”等替代规则。 |
| 13 | 控制权与攻击目标变化 `CONTROL` | 5 | 得到控制权、控制权归还、改变攻击对象或直接攻击许可。 |
| 14 | 数值与卡片身份修改 `MODIFY` | 18 | ATK / DEF 加减、减半 / 倍增 / 设定值，卡名 / 属性 / 规则身份变化。 |
| 15 | 战斗规则 `BATTLE` | 6 | 直接攻击、多次攻击、战斗保护、战斗后时点和战斗事件衍生处理。 |
| 16 | 效果伤害与 LP 恢复 `LP` | 6 | 效果伤害、回复 LP。支付 LP 归 `COST`，不在这里重复计算。 |
| 17 | 装备、素材与指示物 `ATTACH` | 7 | 装备卡、超量素材叠放 / 取除、指示物增减及其资源数量。 |

示意处理链（伪代码，不是项目现有实现）：

```text
CHECK(当前是否允许发动)
  -> COST(支付并保存成本结果)
  -> SELECT(记录对象或处理时选择)
  -> 执行 DRAW / SUMMON / DESTROY 等规则操作
  -> 保存实际结果并生成后续触发事件
```

## 逐卡效果行为索引

下表列出卡文涉及的主要行为标签，不复述完整卡文。`—` 表示通常怪兽，没有卡牌专属效果操作；它仍参与公共召唤、战斗和区域规则。表中省略每个效果都要经过的通用时机 / 合法性检查。

标签：`SELECT` 选择或读取信息；`COST` 成本 / 发动手续；`DRAW` 抽卡；`SEARCH` 检索；`GY` 送墓；`SUMMON` 召唤；`RETURN` 返回 / 洗回 / 盖放；`DESTROY` 破坏；`BANISH` 除外；`NEGATE` 无效 / 改写；`RULE` 保护 / 限制 / 替代；`CONTROL` 控制权 / 攻击目标；`MODIFY` 数值 / 身份修改；`BATTLE` 战斗规则；`LP` 效果伤害 / 回复；`ATTACH` 装备 / 超量素材 / 指示物。

### Card01（31 张）

| CardId | 卡名 | 主要行为 |
| --- | --- | --- |
| 01639384 | 神龙骑士 闪耀 | `SELECT COST NEGATE RULE` |
| 02129638 | 青眼双爆裂龙 | `SUMMON BANISH RULE BATTLE` |
| 02530830 | 银河眼光波刃龙 | `SELECT COST SUMMON DESTROY BANISH` |
| 06853254 | 复活之福音 | `SELECT SUMMON BANISH RULE` |
| 08165596 | No.90 银河眼光子卿 | `SELECT COST SEARCH NEGATE DESTROY RULE ATTACH` |
| 08240199 | 青色眼睛的贤士 | `COST SEARCH GY SUMMON` |
| 14558127 | 灰流丽 | `COST NEGATE` |
| 18963306 | 银河眼光波龙 | `SELECT COST CONTROL NEGATE MODIFY RULE` |
| 23434538 | 增殖的G | `COST DRAW` |
| 33698022 | 月华龙 黑蔷薇 | `SELECT RETURN` |
| 35261759 | 强欲而贪欲之壶 | `COST DRAW BANISH` |
| 38120068 | 抵价购物 | `COST DRAW` |
| 38517737 | 青眼亚白龙 | `SELECT SUMMON DESTROY MODIFY` |
| 39030163 | 银河眼重铠光子龙 | `SELECT COST DESTROY ATTACH` |
| 39701395 | 调和的宝札 | `COST DRAW` |
| 40908371 | 苍眼银龙 | `SELECT SUMMON RULE` |
| 41620959 | 龙之灵庙 | `GY` |
| 45467446 | 白色灵龙 | `SELECT COST SUMMON BANISH MODIFY` |
| 48800175 | 龙觉醒旋律 | `COST SEARCH` |
| 50954680 | 水晶翼同调龙 | `SELECT NEGATE DESTROY MODIFY RULE` |
| 55063751 | 海龟坏兽 加美西耶勒 | `SELECT COST SUMMON BANISH NEGATE RULE ATTACH` |
| 59822133 | 青眼精灵龙 | `SELECT COST SUMMON NEGATE DESTROY RULE` |
| 63356631 | 凤翼的爆风 | `SELECT COST RETURN` |
| 63767246 | No.38 希望魁龙 银河巨神 | `SELECT COST NEGATE CONTROL MODIFY BATTLE ATTACH` |
| 64332231 | 圣刻神龙-九神龙 | `COST DESTROY ATTACH` |
| 71039903 | 太古的白石 | `SELECT COST SUMMON BANISH SEARCH` |
| 71587526 | 因果切断 | `SELECT COST BANISH` |
| 79814787 | 传说的白石 | `SEARCH GY` |
| 83994433 | 闪珖龙 星尘 | `SELECT RULE` |
| 89631139 | 青眼白龙 | `—` |
| 97268402 | 效果遮蒙者 | `SELECT COST GY NEGATE` |

### Card02（33 张）

| CardId | 卡名 | 主要行为 |
| --- | --- | --- |
| 00213326 | E-紧急呼唤 | `SEARCH` |
| 01948619 | 特异英雄 神杖先驱 | `SELECT SEARCH SUMMON RETURN` |
| 08949584 | 英雄到来 | `COST SUMMON` |
| 09411399 | 命运英雄 魔性人 | `COST SUMMON BANISH` |
| 10186633 | EN切换 | `DRAW SUMMON RETURN BANISH` |
| 17955766 | 新空间侠·水波海豚 | `SELECT COST DESTROY LP` |
| 19324993 | 特异英雄 地狱裂魔 | `SELECT SEARCH` |
| 21143940 | 假面变化 | `SELECT GY SUMMON` |
| 22908820 | 元素英雄 日出侠 | `SELECT SEARCH DESTROY MODIFY` |
| 23204029 | 对极英雄 混沌侠 | `SELECT NEGATE MODIFY` |
| 24094653 | 融合 | `GY SUMMON` |
| 24224830 | 墓穴的指名者 | `SELECT BANISH NEGATE` |
| 24299458 | 禁忌的一滴 | `SELECT COST NEGATE MODIFY RULE` |
| 27780618 | 幻影英雄 仿生人 | `COST SEARCH GY BANISH` |
| 32828466 | 唤醒你沉睡的元素英雄 | `SUMMON DESTROY MODIFY BATTLE LP` |
| 40044918 | 元素英雄 天空侠 | `SELECT SEARCH DESTROY` |
| 42141493 | 欢聚友伴·茸茸长尾山雀 | `COST DRAW RETURN` |
| 45906428 | 奇迹融合 | `SUMMON BANISH` |
| 50720316 | 元素英雄 影雾女郎 | `SEARCH GY` |
| 55171412 | 元素英雄 水波新宇侠 | `COST RETURN DESTROY` |
| 56733747 | 元素英雄 闪光新宇翼侠 | `SELECT SUMMON DESTROY MODIFY RULE BATTLE LP` |
| 58004362 | 特异英雄 十字人 | `SELECT COST SEARCH SUMMON` |
| 58481572 | 假面英雄 暗爪 | `SELECT BANISH RULE` |
| 60461804 | 命运英雄 毁灭凤凰人 | `SELECT DESTROY SUMMON MODIFY` |
| 63060238 | 元素英雄 烈焰侠 | `SEARCH GY MODIFY RULE` |
| 65681983 | 抹杀之指名者 | `SELECT BANISH NEGATE` |
| 66011101 | No.60 刻不知之杜加雷斯 | `SELECT COST DRAW SUMMON MODIFY RULE ATTACH` |
| 73642296 | 屋敷童 | `COST NEGATE` |
| 75047173 | 至爱接触 | `SUMMON RETURN RULE` |
| 81439173 | 愚蠢的埋葬 | `GY` |
| 89943723 | 元素英雄 新宇侠 | `—` |
| 93347961 | 元素英雄 火焰翼侠-火焰一击 | `SELECT SEARCH SUMMON COST` |
| 94145021 | 小丑与锁鸟 | `COST GY RULE` |

### Card03（27 张）

| CardId | 卡名 | 主要行为 |
| --- | --- | --- |
| 08491308 | 闪刀姬-飒天 | `GY BATTLE` |
| 09726840 | 闪刀起动-连刀 | `SELECT SUMMON MODIFY RULE GY` |
| 12421694 | 闪刀姬-魁奈 | `SELECT MODIFY BATTLE LP` |
| 17217034 | 合体术式-交闪零式 | `SELECT NEGATE DESTROY` |
| 20357457 | 未来之柱-奇亚诺丝 | `COST SUMMON SEARCH BANISH RETURN` |
| 24010609 | 闪刀机关-多任务战刀机 | `SELECT GY RETURN RULE` |
| 25072579 | 试号闪刀姬-天津 | `SELECT NEGATE DESTROY RULE` |
| 25311006 | 三战之才 | `SELECT DRAW CONTROL RETURN` |
| 26077389 | 闪刀姬-零衣 | `COST SUMMON` |
| 32807848 | 增援 | `SEARCH` |
| 34433770 | 闪刀亚式-双纽闪门 | `SELECT COST RETURN SUMMON BANISH DESTROY` |
| 35269904 | 三战之号 | `SELECT SEARCH RETURN` |
| 37351133 | 闪刀姬-露世 | `SELECT SUMMON NEGATE` |
| 49299410 | 嗤笑的黑山羊 | `SELECT COST BANISH RULE` |
| 50005218 | 闪刀空域-零区 | `SELECT SEARCH GY RETURN SUMMON` |
| 56741506 | 闪刀姬-阿泽莉娅·节制 | `SELECT COST BANISH SUMMON ATTACH` |
| 63013339 | 闪刀姬-卡米丽娅 | `SELECT GY SUMMON CONTROL` |
| 63166096 | 闪刀起动-交闪 | `SEARCH DRAW` |
| 63288574 | 闪刀姬-燎里 | `SELECT SEARCH MODIFY` |
| 70368879 | 成金哥布林 | `DRAW LP` |
| 73628505 | 星球改造 | `SEARCH` |
| 75147529 | 闪刀姬-泽克 | `SELECT BANISH GY MODIFY` |
| 76072561 | 闪刀姬=零露 | `SELECT COST SEARCH SUMMON DESTROY BANISH` |
| 83838727 | 抽卡面包 | `SELECT COST DRAW GY LP` |
| 90673289 | 闪刀姬-雫空 | `SELECT SEARCH MODIFY` |
| 98338152 | 闪刀机-黑寡妇抓锚 | `SELECT NEGATE CONTROL` |
| 98462037 | 闪刀姬-阿泽莉娅 | `SELECT COST DESTROY BANISH GY` |

## 对 gameplay 拆分的直接结论

1. 首版规则层先实现公共操作与结果记录，再让单卡编排它们；抽卡、检索、送墓、召唤、返回、破坏、除外、无效和数值修正是本卡池复用度最高的一组核心操作。
2. 不能把“条件检查 / 取对象 / 成本 / 处理”并成一个函数。例：融合素材是在处理时选择和处理；《E-紧急呼唤》的检索结果也是处理时选择；《因果切断》的对象则在发动时固定。
3. 效果描述不足以直接写成统一事件回调。诱发时点、对象锁定、成本结果、处理时重新选择、实际操作是否成功、保护 / 抗性和触发后续都要有独立记录。
4. 这份清单是按当前配置卡池盘点，不代表当前已有 91 张卡效，也不承诺所有卡文裁定细节已经核验。英雄 19 卡手册中的待核验分支仍保留为待核验。
