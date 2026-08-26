# 项目操作

| 操作 | 方式 |
| --- | --- |
| 打开项目 | Unity `6000.5.2f1` |
| 启动后端 | Unity 菜单 `Tools/AChen/启动后端服务`，或执行 `dotnet run --project Backend/src/AChen.Backend.Api` |
| 检查后端 | `http://127.0.0.1:5080/health`；就绪检查为 `/ready` |
| 运行游戏 | 从唯一构建场景 `Assets/Scenes/PreInit.unity` 进入 Play Mode |
| 发布内容 | 设置 `ACHEN_CONTENT_PUBLISH_KEY`，执行 `Tools/HotUpdate/Build And Publish Release` |
| 后端测试 | `dotnet test Backend/AChen.Backend.sln` |
| Unity 自动化 | 参考 [Unity Pipeline](../unity-pipeline/index.md) |

后端启动时自动执行 EF Core 迁移。默认 SQLite 数据文件位于后端运行目录。

离线 Unity 文档：[Manual](../unity-official-6000.5.2f1/en/Manual/index.html) · [Script Reference](../unity-official-6000.5.2f1/en/ScriptReference/index.html)

