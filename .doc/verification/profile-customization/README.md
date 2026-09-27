# 头像、头像框与个人资料窗口验收记录

日期：2026-09-27。Unity 6000.5.2f1，连接本机 Editor；后端使用临时 SQLite / TestServer。

## 已完成

- 导入 230 张头像、60 张框、60 张独立遮罩、14 张已确认 CommonUI 切图；默认头像 1010001、框 1030001 免费，其余分别 200 / 500 金币。配置通过现有生成器生成 `.bytes`。
- `ProfileEditWindow` 接入 UIFrame、UISettings 和 Addressables；两个大厅入口分别打开玩家名 / 头像页。三个页签独立草稿，已拥有过滤、确认不关闭、关闭重置、五格行复用。
- 大厅、商城、好友、申请列表绑定共享 `AvatarPortraitView`；头像按比例覆盖并裁剪，框装饰位于 Mask 外。
- 清理旧头像、旧选择 / 改名窗口、旧注册、旧 Catalog 项及无引用的头像特效材质和 Shader。
- 数据迁移新增头像框字段并一次性重置旧头像；金币、壁纸、卡牌等其他资产保留，后续迁移启动不会清空新购买记录。

## 验证结果

| 验证 | 结果 |
| --- | --- |
| 后端完整 `dotnet test`（Release） | 70 / 70 通过 |
| `ProfileCustomizationTests` | 2 / 2 通过 |
| `GameConfigTests` | 5 / 5 通过 |
| `GameConfigCsvEditorParserTests` | 8 / 8 通过 |
| `PlayerStateEventTests` | 5 / 5 通过 |
| `AuditProfile` | 8 个预制体、11 个头像组件、104 处引用完整，无 Missing Script |
| Unity 编译 | 完成，无编译错误 |
| 源码、CSV、文档差异检查 | 通过；Unity 自动序列化资源保留原生空字段格式 |

后端验证包含头像框正常购买、金币不足、重复购买、未拥有装备拒绝、装备后改名保留框、会话资料持久化、社交 DTO 同步，以及旧数据库升级仅执行一次。原有头像 / 壁纸购买及版本冲突测试一并通过。

窗口 EditMode 验证包含两个独立草稿、仅展示拥有项、另一页草稿不影响组合预览、成功的无变更确认后窗口仍打开、关闭重开，以及第 46 行与首行之间的复用。EditMode 滚动测试直接加载行 Prefab；网络写入与持久化由后端集成测试覆盖。

## 截图

截图使用真实保存的 uGUI Prefab，在独立 Preview Scene 渲染。网格填充样本数据便于检查布局，不代表测试账号拥有全部物品。

- [头像页 1706×960](profile-tab1-1706x960.png)
- [头像页 1920×1080](profile-tab1-1920x1080.png)
- [头像页 1280×720](profile-tab1-1280x720.png)
- [玩家名页](profile-tab0-1706x960.png)
- [头像框页](profile-tab2-1706x960.png)
- [全部 60 张框裁剪检查](all-60-frames.png)

逐项检查了 60 张框的头像边界和外部装饰，圆形、六边形、方形与开放式框均使用独立遮罩；三个参考尺寸未见按钮、文字或网格越界。

## 范围与复现

没有进入连接真实服务的完整大厅 Play Mode 流程，也没有进行远端部署、Addressables 上传或生产数据库迁移。当前本机场景仍为 `PreInit`，未修改场景、未进入 Play Mode。新客户端、后端与含 `avatar-frames` 的配置需要配套发布。

```powershell
dotnet test Backend/tests/AChen.Backend.Api.Tests --configuration Release
unity command run_tests --mode editor --filter ProfileCustomizationTests --async_tests true
unity command run_script --file Tools/ProfileUI/AuditProfile.cs --entry AuditProfile.Main
unity command run_script --file Tools/ProfileUI/PreviewProfile.cs --entry PreviewProfile.Main
```

其他 Unity 测试按表中类名分别传入 `--filter`。JSON 结果保存在本目录。素材制作与维护说明见 `Tools/ProfileUI/README.md`。
