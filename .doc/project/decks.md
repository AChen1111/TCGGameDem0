# 卡组逻辑接入

卡组逻辑位于 `AChen.Decks`，无 UI 依赖。联网操作统一走 `PlayerSession`：自动带登录凭证、过期令牌刷新一次、串行修改，并拒绝跨账号会话的异步结果。`DeckApi` 仅负责存储协议，不执行组卡校验。

## 创建、编辑和删除

```csharp
var session = PlayerSession.Instance;
var saved = await session.CreateDeckAsync("青眼", cancellationToken);
var draft = new DeckDraft(saved);
draft.Name = "青眼卡组";
draft.SetCardCount("89631139", 0, 3, LocalGameConfiguration.DeckRules);
draft.SetCardCount("01639384", 1, 1, LocalGameConfiguration.DeckRules);
saved = await session.SaveDeckAsync(draft, cancellationToken);
draft = new DeckDraft(saved); // 后续修改使用保存返回的新 Revision

var decks = await session.GetDecksAsync(cancellationToken);
saved = await session.GetDeckAsync(saved.Id, cancellationToken);
await session.DeleteDeckAsync(saved.Id, saved.Revision, cancellationToken);
```

例中卡牌必须实际持有相应稀有度及数量。`SetCardCount` 设置该 CardId、稀有度的总数量，自动选择主卡或额外；0 表示删除，允许移除已经不在配置中的卡。`ToData()` 返回独立只读快照。编辑可以暂时不合法，保存时统一校验。失败不会修改草稿、已保存版本或收藏，成功后用返回值创建下一份草稿。

## 规则与错误

- 主卡 40–60、额外 0–15。`Draft` 仅放宽主卡最少 40 张，`Playable` 执行完整规则。
- 同 ID 跨分区、跨稀有度、跨重复条目合计最多 3 张；禁限配置可进一步降低到 0、1、2 张。
- 持有量按 `(CardId, Rarity)` 检查。多套卡组可复用同一收藏，不扣减数量。
- 卡组名称去除首尾空白后 1–64 字符，不能含控制字符，允许重名。
- `session.ValidateDeck(snapshot)` 默认执行 `Playable` 校验；`DeckValidator.Validate` 可传入配置、收藏和模式独立使用。返回 `DeckValidationResult`，包含 `IsValid` 和只读 `Issues`。
- `DeckValidationIssue` 提供 `Code`、`CardId`、`Rarity`、`Section`、`Actual`、`Allowed`。保存校验失败抛 `DeckValidationException`，完整结果在 `Result`。
- `CopyLimitExceeded` 表示超过配置上限（`Allowed=0` 即禁卡），`NotOwned` 表示具体稀有度持有不足。另有 `MainTooSmall`、`MainTooLarge`、`ExtraTooLarge`、`WrongSection`、`UnknownCard`、`InvalidEntry`、`InvalidName`、`ConfigNotReady`、`InventoryNotReady`。
- 网络失败抛现有 `BackendApiException`。`DECK_DATA_CHANGED` 表示版本冲突，应重新读取并由调用方决定如何合并草稿；不会自动重放覆盖请求。`DECK_NOT_FOUND` 不区分不存在和其他账号的卡组。

## 配置表和入包

`TableData/card-deck-sections.csv`：`CardId,Section` / `string,string`，每张卡恰好一行，`Section` 为 `Main` 或 `Extra`。当前 94 张卡，57 主卡、37 额外。

`TableData/card-banlist.csv`：`CardId,MaxCopies` / `string,int`。0 禁止、1 限制、2 准限制、3 最多三张；未列出的已知卡默认 3。当前为项目示例配置：增殖的 G（23434538）禁止、灰流丽（14558127）限制、假面变化（21143940）准限制，不代表官方赛制的禁限表。此表允许空数据，不允许文件缺失；重复 ID、未知卡、非法数量均失败。

卡组编辑器的右侧列表和中间主卡组、额外卡组缩略图在左上角显示禁限角标；0、1、2 分别使用 `GUI_T_Icon1_Limit00/01/02`，上限 3 不显示角标。`DeckCardCell` 的图片和 Sprite 数组通过 Prefab 序列化绑定，显示与添加校验共用 `DeckRules.GetMaxCopies`。`Tools/DeckUI/AuthorDeckLimits.cs` 负责导入图片、原位绑定 Prefab 并调用配置生成器。

通过 Unity `Tools/AddToBytes` / `PublishedConfigBuilder.Prepare()` 生成，地址为 `GameConfig/card-deck-sections` 和 `GameConfig/card-banlist`，分组 `Remote_GameConfig`，标签 `GameConfig`、`GameConfigGenerated`。不要手写 `.bytes`。

`LocalGameConfiguration.InitializeAsync` 同时验证两张表，成功后公布只读 `DeckRules`，配置缺失时不能创建或保存卡组。查询、删除已有卡组不依赖分类配置。禁限数据随配置内容包更新，重新加载后使用当前限制；不会自动删改已保存卡组。

新增表未加入服务端 `GameConfigTables.Required`，旧服务端配置包继续可读。新的客户端需要带两张新表的内容包。此版本不包含界面、Side Deck、同名 ID 合并、多赛制或定时禁限表。

## 服务端与验证

后端独立 `PlayerDecks` 表保存卡组，各卡组具有自己的 Revision。服务端只检查鉴权、归属、传输结构和并发，不检查卡牌目录、禁限、分区、张数或持有量，也不改玩家资产版本。

迁移随现有启动流程应用；开发期间只在临时测试数据库验证，不直接升级正在运行的真实数据库。接口细节见 `Backend/docs/api/README.md`。

Unity 测试按名称 `Deck` 过滤，后端测试为 `DeckEndpointsTests`、`DeckMigrationTests`；另外回归游戏配置和玩家接口。实现过程中按数量规则、禁限、草稿编辑、HTTP 协议等行为记录失败后实现的 TDD 循环。

### 本次验收（2026-09-27）

| 测试范围 | 通过 | 失败 | 跳过 |
| --- | ---: | ---: | ---: |
| Unity EditMode：`Deck` | 52 | 0 | 0 |
| Unity EditMode：`GameConfig` | 14 | 0 | 0 |
| Unity EditMode：`Auth` | 66 | 0 | 4 |
| Unity EditMode：`PlayerStateEvent` | 4 | 0 | 0 |
| Unity EditMode：`CardDrawJson` | 3 | 0 | 0 |
| 后端全套集成测试（Release） | 68 | 0 | 0 |

`Auth` 为名称包含匹配，也运行了 Unity Pipeline 的鉴权及 authoring 测试；4 项跳过均来自该包已有的版本兼容或未实现标记。卡组新增测试没有跳过。

生成器 `Tools/AddToBytes` 已产出两张配置的实际 `.bytes`。测试确认 CSV 与 bytes 一致、94 张分类覆盖、前导零保留、空禁限表可解码，以及两张表的 Addressables 分组、地址与标签。额外卡分类为融合 10、同调 5、超量 8、连接 14。旧内容读取和数据库升级保留账号、金币、资产版本与持有卡也已通过。

EditMode 中额外尝试的 Addressables 异步加载检查发生超时，已取消；最终保留实际 bytes 解码和 Addressables 注册校验，不将此次超时记为运行时加载通过。本次没有构建或发布完整客户端、远端资源包，也没有升级正在运行的服务数据库。

本地原始报告在 `Logs/DeckImplementation/`（Git 忽略）：`unity-*.json` 与 `backend.trx`。
