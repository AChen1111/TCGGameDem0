# 后端

后端位于 `Backend/src/AChen.Backend.Api`，.NET 8、ASP.NET Core Minimal API、EF Core、SQLite。与客户端的职责划分见 [系统架构](architecture.md)。路径与请求体见 [后端 API](../backend/api/README.md)。

## 1. 模块

按功能目录组织，每个功能自带 Endpoints、Service、Contracts。

| 模块 | 代码位置 | 对外能力 |
| --- | --- | --- |
| Auth | `Features/Auth` | 注册、登录、刷新并轮换 Token、登出、查询当前用户 |
| Players | `Features/Players` | 引导数据、改昵称 / 当前头像 / 壁纸、购买外观 |
| AccountManagement | `Features/AccountManagement` | 发布密钥查询或增加指定账号金币 |
| GameConfig | `Features/GameConfig` | 已发布引导、草稿替换/发布、CSV、Git 快照 |
| ContentDelivery | `Features/ContentDelivery` | Release 生命周期、活动版本、Manifest、不可变文件 |
| Data | `Data/` | `AppDbContext`、迁移、SQLite 连接 |
| Infrastructure | `Infrastructure/` | 统一异常与 Problem Details |
| Admin | `Pages/Admin` | 内容、账号、配置的浏览器后台 |

`Program.cs` 只做宿主装配：配置校验、鉴权方案、限流、迁移、健康检查和 Endpoint 映射。业务规则不写在这里。

## 2. 启动与存储

启动时校验：

- `Auth`：Issuer、Audience、签名密钥长度、Access / Refresh 有效期
- `ContentDelivery`：存储根、发布密钥、包体与文件数上限、允许的频道
- `GameConfigGit`：仓库根、分支、远端、历史条数

通过后执行 EF Core 迁移。默认 SQLite 文件在后端运行目录。内容文件落在 `ContentDelivery:StorageRoot`，不进数据库 blob。

`GET /health` 只表示进程活着。`GET /ready` 还要能连上数据库且内容存储可写。

## 3. 鉴权与限流

| 方案 | 凭证 | 用途 |
| --- | --- | --- |
| JWT Bearer | `Authorization: Bearer` | 玩家接口 |
| Publish Key | `X-Content-Publish-Key` | 发布、运营金币、配置管理 |
| Content Admin Cookie | `AChen.ContentAdmin` | `/admin` 页面，8 小时，密钥指纹变化即失效 |

玩家改资料和购买必须带 `expectedRevision`，防止用过期引导数据覆盖。金币与拥有列表不能由 `PATCH /api/player/profile` 改写。购买价格取已发布配置的 `priceGold`，请求体不能指定金额。

限流按策略名挂到路由：

| 策略 | 窗口 | 用途 |
| --- | --- | --- |
| `auth` | 每 IP 每分钟 20 | 注册登录刷新 |
| `admin-login` | 每 IP 每分钟 5 | 管理台登录 |
| `content-upload` | 每 IP 并发 1 | 上传内容包 |
| `content-management` | 每 IP 每分钟 60 | Release 管理 |
| `player` / `game-config` / `content-manifest` | 每 IP 每分钟 120 | 常规读取与玩家操作 |

超限返回 `429`、`RATE_LIMITED`，`Retry-After: 60`。

## 4. 和客户端、决斗服务的关系

客户端用 `BackendHttpClient` 访问本 API，不直连数据库，不信任客户端上报的金币或价格。

决斗服务若落地，使用独立入口 `AChen.Duel.Server`：只提供进程生命周期和 UDP 后台服务，不引用本 API，不跑 JWT、SQLite 或发布密钥校验。本 API 继续承担账号、商城与内容，不删、不迁去对战进程。

购买接口只结算 `avatar` 和 `wallpaper`。卡包会出现在已发布配置和大厅列表，没有对应的购买 API。

## 5. 管理台页面

浏览器打开 `http://127.0.0.1:5080`。内容与配置页用发布密钥登录，换 Cookie `AChen.ContentAdmin`，8 小时，密钥指纹变化即失效。

| 路径 | 鉴权 | 做什么 |
| --- | --- | --- |
| `/admin/content/login` | 匿名；每 IP 每分钟 5 次 | 输入发布密钥 |
| `/admin/content` | Cookie | Release 列表、筛选 |
| `/admin/content/releases/new` | Cookie | 创建 Release |
| `/admin/content/releases/{id}` | Cookie | 详情、上传产物、设为活动版本 |
| `/admin/content/publications` | Cookie | 发布历史 |
| `/admin/game-config` | Cookie | 草稿编辑、CSV、Git、发布 |
| `/admin/accounts`、`/admin/accounts/{id}/edit` | 当前未挂鉴权 | 账号列表、改资料、删号 |
| `/register` | 匿名；走 `auth` 限流 | 浏览器注册页，与 JSON 注册并行 |

加金币的权威接口仍是 `POST /api/accounts/admin/gold`（Publish Key）。Unity 菜单 `Tools/运营工具` 调的是这个接口，不是账号管理页。

## 6. 配置表怎么发布

配置有两份修订：`editRevision` 保护草稿并发；`publishedRevision` 是客户端 `bootstrap` 看到的版本。

```text
改草稿（管理台 / CSV / PUT /api/game-config/admin/draft）
  → 可选：Git 保存快照、推远端、从历史载入
  → POST /api/game-config/admin/publish
  → 客户端 GameConfigManager 按 ETag 拉新 bootstrap
```

Unity：`Tools/配置表/发布窗口`，默认 CSV 为工程根下 `GameConfig/game-config.csv`。流程是解析预览 → 上传草稿 → 发布当前草稿。

浏览器：`/admin/game-config` 可逐条增改头像、壁纸、卡包，或导入不超过 5 MiB 的 CSV。Git 按钮：

| 操作 | 结果 |
| --- | --- |
| 保存快照 | 把当前草稿提交到 `GameConfigGit:RepositoryRoot` 本地库 |
| 推送 | 提交并 push 到 `RemoteName`（需配置 `RemoteUrl`） |
| 拉取 | 取远端历史，不自动覆盖草稿 |
| 载入版本 | 把某次提交写回草稿，再检查后发布 |

未发布过配置时，客户端 `bootstrap` 得到 `GAME_CONFIG_NOT_PUBLISHED`，进不了大厅。草稿与已发布配置冲突时返回 `GAME_CONFIG_CHANGED`，刷新后再提交。
