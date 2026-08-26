# 后端架构

后端位于 `Backend/src/AChen.Backend.Api`，采用 .NET 8、ASP.NET Core Minimal API、EF Core 和 SQLite。

| 模块 | 职责 |
| --- | --- |
| Auth | 注册、登录和令牌认证 |
| Players | 玩家资料读取和更新 |
| GameConfig | 游戏配置下发 |
| ContentDelivery | Manifest、文件下载和内容发布 |
| Data | EF Core 数据访问、迁移和 SQLite 存储 |

服务启动时自动执行数据库迁移，并提供健康检查、鉴权和限流能力。接口路径与请求结构见 [后端 API](../backend/api/README.md)。

