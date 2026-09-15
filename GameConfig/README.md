# 游戏配置编辑与发布

## 唯一编辑源

- `GameConfig/game-config.csv`: 头像、壁纸和卡包商品。
- `GameConfig/card-gacha.csv`: 分池卡牌权重和稀有度权重。
- `GameConfig/all-cards.csv`: CardAll 的卡牌与来源池。
- `TableData/Card/Cards.csv`: 卡牌属性。
- `TableData/Localization/Translations.csv`: 游戏多语言文本。
- `Assets/UI/Prefab/Hall/PreGameUI/WallpaperDisplayConfig.asset`: Inspector 编辑壁纸偏移。
- `Assets/GameConfiguration/LocalizationSettings.asset`: Inspector 编辑语言字体映射。

`Assets/GameConfiguration/config.json` 是生成产物，不要直接编辑。
卡牌与语言表先导出为 `TableData/Generated/*.bytes`，发布时嵌入配置 JSON；客户端不再从 Resources 加载这些表。

## 发布流程

继续使用 Unity 现有内容发布窗口。它会自动导表、校验引用和字段、生成配置、构建 Addressables、生成热更新 DLL，并上传完整内容包。

`Remote_GameConfig` 使用远端构建及加载路径，配置 JSON 与语言设置打在同一个 Bundle；卡图和字体实体资源沿用原资源组。壁纸源资产仅用于编辑，运行时读取 JSON 中已发布的偏移。

内容包清单升级为 schemaVersion 2，必须包含 `GameConfig/config.json`。该文件与 Bundle 中的配置 JSON 来自同一生成文件，清单携带文件大小和 SHA-256。后台校验成功后才允许激活。价格、卡池和资源随同一个 Release 切换和回滚。

## 客户端与交易

启动时下载当前 Release，配置全部加载完成后才进入登录流程。自动登录复用本次启动的版本检查；手动登录和注册进入大厅时再次检查。查询失败时暂停进入并允许重试。

本次运行始终固定同一 Release。登录或交易发现版本变化后，提示退出并重新启动。不会在运行中替换配置，也不会自动重试购买或抽卡。

购买、抽卡及配置相关请求携带：

- `X-Content-Release`: 当前 Release GUID。
- `X-Content-Channel`: 发布渠道。
- `X-Content-Platform`: 平台。
- `X-Content-App-Version`: 安装包版本。

后台按目标验证活动 Release，缺少版本或旧版本返回 `CONTENT_UPDATE_REQUIRED`；缺少有效发布配置返回 `CONTENT_NOT_READY`。版本验证发生在扣款和发卡之前。原有玩家 Revision 并发校验继续保留。

## 首次部署

1. 这是安装包协议升级：共享 AOT 配置类型、启动逻辑和表结构指纹均发生变化，需要发布新的客户端安装包，不能只替换旧安装包的热更新 DLL。
2. 部署新版后台。启动时执行 `20260915090000_PublishedConfiguration`，删除旧商品、卡池和配置历史表；账号、金币和背包表保留。该迁移不支持 Down 恢复已删除的数据。
3. 为新版 AppVersion 构建并发布完整内容包，激活该 Release，再开放新版客户端。
4. 新版包激活前，交易被阻止。旧内容包没有统一配置，不能在新版后台上重新激活；回滚请选择带 schemaVersion 2 配置的完整发布包。

原后台配置页面、CSV 导入 API 和独立配置发布窗口已移除。玩家管理和内容发布后台继续使用。旧 `Data/game-config-git` 目录不再被读取；历史部署残留目录可在下线旧版本后清理。

## 验收

首次下载、断网重试、无变化不重复下载、配置校验失败不激活、旧版本交易不扣款、不发卡、完整 Release 回滚、迁移保留账号资产。

本次修改未自动执行构建、编译、数据库迁移或测试；后续按明确的验收或发布指令执行。
