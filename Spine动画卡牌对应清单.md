# UI Spine 动画与本项目卡牌对应清单

## 范围与匹配规则

资源来源：`C:\Users\ldc20\OneDrive\Desktop\UI\Spine`。本清单只登记该目录中能按卡牌编号匹配到当前项目 `TableData/Cards.csv` 的 Spine 资源。

匹配方式是将 Spine 下的数字目录名左侧补零至 8 位，再与 `Cards.csv` 的 `CardId` 精确比较：

```text
示意代码：CardId = SpineFolderName.PadLeft(8, '0')
```

当前卡牌表有 91 个卡牌编号；Spine 资源目录有 535 个编号组，其中 20 个能匹配卡牌表。20 张卡牌对应 32 个卡牌 Spine 骨骼 JSON（包含 SD/HD 版本）。动画名取自各骨骼 JSON 的 `animations` 字段。

## 对应清单

表中路径相对于资源根目录 `UI/Spine`。同一卡牌列出多个路径时，它们是不同画质或不同资源版本。

| 卡牌 ID | 卡名 | Spine 骨骼 JSON（相对路径） | 动画名 |
| --- | --- | --- | --- |
| 01948619 | 特异英雄 神杖先驱 / Xtra HERO Wonder Driver | `1948619/MonsterCutin2/1948619.json` | `animation` |
| 19324993 | 特异英雄 地狱裂魔 / Xtra HERO Infernal Devicer | `19324993/MonsterCutin2/19324993.json` | `animation` |
| 22908820 | 元素英雄 日出侠 / Elemental HERO Sunrise | `22908820/MonsterCutin2/22908820.json` | `animation` |
| 23204029 | 对极英雄 混沌侠 / Contrast HERO Chaos | `23204029/MonsterCutin2/23204029.json` | `animation` |
| 32828466 | 唤醒你沉睡的元素英雄 / Wake Up Your Elemental HERO | `32828466/MonsterCutin2/32828466.json` | `animation` |
| 40044918 | 元素英雄 天空侠 / Elemental HERO Stratos | `40044918/MonsterCutin2/40044918.json` | `animation` |
| 50954680 | 水晶翼同调龙 / Crystal Wing Synchro Dragon | `50954680/MonsterCutin2/50954680.json` | `animation` |
| 55063751 | 海龟坏兽 加美西耶勒 / Gameciel, the Sea Turtle Kaiju | `55063751/MonsterCutin/highend_hd/1/P12106JS.json`<br>`55063751/MonsterCutin/sd/0.5/P12106JS.json` | 两个版本均为 `animation` |
| 55171412 | 元素英雄 水波新宇侠 / Elemental HERO Aqua Neos | `55171412/MonsterCutin2/55171412.json` | `animation` |
| 56733747 | 元素英雄 闪光新宇翼侠 / Elemental HERO Shining Neos Wingman | `56733747/MonsterCutin/highend_hd/0.88/P17443JS.json`<br>`56733747/MonsterCutin/sd/0.44/P17443JS.json` | 两个版本均为 `animation` |
| 58004362 | 特异英雄 十字人 / Xtra HERO Cross Crusader | `58004362/MonsterCutin2/58004362.json` | `animation` |
| 58481572 | 假面英雄 暗爪 / Masked HERO Dark Law | `58481572/MonsterCutin/highend_hd/0.611/P11313JS.json`<br>`58481572/MonsterCutin/sd/0.31/P11313JS.json` | 两个版本均为 `animation` |
| 60461804 | 命运英雄 毁灭凤凰人 / Destiny HERO - Destroyer Phoenix Enforcer | `60461804/MonsterCutin/highend_hd/0.91/P16524JS.json`<br>`60461804/MonsterCutin/sd/0.455/P16524JS.json` | 两个版本均为 `animation` |
| 63288574 | Sky Striker Ace - Kagari | `63288574/MonsterCutin/p14054/sd/0.445/P14054JS.json`<br>`63288574/MonsterCutin/p3899/highend_hd/0.87/P3899JS.json`<br>`63288574/MonsterCutin/p3899/sd/0.453/P3899JS.json` | `P14054JS`: `animation`；`P3899JS`: `animation`、`animation2` |
| 63767246 | No.38 希望魁龙 银河巨神 / Number 38: Hope Harbinger Dragon Titanic Galaxy | `63767246/MonsterCutin/highend_hd/0.88/P12260JS.json`<br>`63767246/MonsterCutin/sd/0.44/P12260JS.json` | 两个版本均为 `animation` |
| 75147529 | 闪刀姬-泽克 / Sky Striker Ace - Zeke | `75147529/MonsterCutin/highend_hd/1/P14947JS.json`<br>`75147529/MonsterCutin/sd/0.5/P14947JS.json` | 两个版本均为 `animation` |
| 89631139 | 青眼白龙 / Blue-Eyes White Dragon | `89631139/MonsterCutin/p13571/sd/0.35/P13571JS.json`<br>`89631139/MonsterCutin/p3801/highend_hd/0.87/P3801JS.json`<br>`89631139/MonsterCutin/p3801/sd/0.435/P3801JS.json` | `P13571JS`: `P13571`；`P3801JS`: `animation`、`animation_kde` |
| 89943723 | 元素英雄 新宇侠 / Elemental HERO Neos | `89943723/MonsterCutin2/89943723.json` | `animation` |
| 90673289 | Sky Striker Ace - Shizuku | `90673289/MonsterCutin/p13571/sd/0.35/P13571JS.json`<br>`90673289/MonsterCutin/p3433/highend_hd/0.545/P3433JS.json`<br>`90673289/MonsterCutin/p3433/sd/0.2725/P3433JS.json` | `P13571JS`: `P13571`；`P3433JS`: `animation`、`animation_kde` |
| 93347961 | 元素英雄 火焰翼侠-火焰一击 / Elemental HERO Flame Wingman - Infernal Rage | `93347961/MonsterCutin2/93347961.json` | `animation` |

## 备注

- `63288574` 目录还包含 `63288574/MasterDuel_Wallpaper/63288574.json`，内有 `blink`、`closeLeftA`–`closeLeftD`、`closeRightA`–`closeRightD`、`idle`、`interaction`、`interaction_noWing`、`recover` 等壁纸交互动画；它属于壁纸资源，不计入上面的卡牌登场 Spine 数量。
- `63288574` 和 `90673289` 在 `card-name-catalog.csv` 中没有名称记录，因此保留项目 `Translations.csv` 可读到的英文名称，不补写未确认的中文名。
- Spine 源资源仍位于桌面提取目录。该清单只说明编号与动画资源的对应关系，不代表资源已导入或绑定到卡牌界面。
