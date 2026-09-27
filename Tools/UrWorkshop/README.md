# UR 与卡牌工坊维护

运行时功能全部位于 `Assets/Scripts`（HotUpdate）。`Tools/UrWorkshop` 中的脚本只在 Unity Editor 里执行，不进入 AOT 或游戏运行时代码。

## 配置与资产

- `TableData/card-crafting.csv`：`Rarity,CostUr`，类型 `int,long`。当前只允许普通版 `0,30`。
- `TableData/card-recycling.csv`：`Rarity,DismantleUr,OverflowUr`，类型 `int,long,long`。必须完整覆盖 0–4；两个收益独立配置，均为正整数。
- 使用现有 `Tools/AddToBytes` 菜单编译。输出 `Assets/GameConfiguration/*.bytes`，地址 `GameConfig/card-crafting` / `GameConfig/card-recycling`，属于 `Remote_GameConfig` 组，沿用生成配置标签。
- 表缺失、重复版本、错误类型、非正价格和版本覆盖错误均拒绝加载。旧内容仍可由服务端读取；需要 UR 的交易必须具备有效经济配置，不能使用零价格默认值。
- 卡组禁限表不影响收藏上限或合成资格。合成目标以 `all-cards` 为准；当前 94 张全部开放。普通版持有量为零才可合成，其他版本独立。

新增副本结算保留历史超额：

```csharp
int kept = Math.Min(granted, Math.Max(0, 3 - owned));
int overflow = granted - kept;
// 已有 5 张再获得 2 张，只把新增的 2 张兑换。
```

## Prefab

独立资源位于 `Assets/UI/Prefab/Hall/Workshop`：

| Prefab | 职责 |
| --- | --- |
| UrBalance | 图标与余额；大厅和工坊共用，监听玩家 UR 变化 |
| CardWorkshopPanel | 模式、搜索、筛选、列表和交易确认 |
| CardWorkshopRow | 四列可复用列表行 |
| CardWorkshopItem | 静态卡图、名称、数量、拥有灰层和选择状态 |
| CardWorkshopDetail | 静态大图、版本、数量、报价及操作按钮 |
| UrNoticeWindow | 单个确定按钮的收益／拒绝／错误弹窗 |
| CardOverflowBadge | 抽卡结束后的灰层、UR 图标和逐张兑换量 |

原图导入为透明 Sprite：`Assets/UI/Sprite/CardWorkshop/Icon_Rarity_UR.png`。全部组件引用由序列化字段绑定；工坊大图与列表不使用实时闪卡。

首次构建脚本（会重建上述 Prefab，后续日常布局维护优先用 Unity Prefab 编辑器）：

```powershell
unity command run_script --file Tools/UrWorkshop/BuildUrPrefabs.cs --entry BuildUrPrefabs.Main
unity command menu --path Tools/AddToBytes
```

执行前停止 Play，协调其他编辑器任务。脚本使用 `PrefabUtility` 保存真实 Prefab，并登记 Prefab/Sprite Catalog、AddressKeys 和大厅 UISetting。大厅与商城现有内容仅添加必要绑定。

## 交易

工坊先显示本次收支及余额变化，确认后由 PlayerSession 串行调用后端。分解先查询每套已保存卡组的分区合计需求，受影响时直接提示卡组名；服务端在事务中再次检查。价格冲突响应携带最新报价，界面更新价格，要求重新确认。失败不会提前修改客户端资产。

抽卡的 `isOverflow/urGained` 来自服务器；只在全部卡最终展开为 `CanInspect` 状态后显示标记，标记没有 RaycastTarget，不拦截详情操作。礼品响应的 `urGained>0` 才显示单按钮收益弹窗。

验收结果见 `.doc/verification/ur-workshop/README.md`。后端接口见 `Backend/docs/api/README.md`。
