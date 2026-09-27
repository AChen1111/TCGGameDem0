# 个人资料 UI 与头像素材维护

运行时代码和组件在 `Assets/Scripts` 的 HotUpdate 程序集中。这里的 C# 是 Unity Pipeline 在编辑器内临时编译的资源制作脚本，不进入主包。

## 资源

- 头像：230 张，ID 与 `ProfileIcon{ID}_L.png` 一致；地址 `a_{ID}`。
- 头像框：60 张，ID 与 `ProfileFrame{ID}_L.png` 一致；地址 `af_{ID}`。
- 每框独立遮罩：`af_{ID}_Mask`，与框图尺寸相同；内轮廓参数在 `ImportProfileAssets.Inside` 中。开放式框同样用独立完整轮廓，不能使用框图本身作为 Mask。
- CommonUI：经核对的 14 张切图，导入器保存九宫格边距。
- 默认赠送头像 1010001 与头像框 1030001；其余头像 200 金币、框 500 金币。修改价格使用 `TableData`，再通过 `Tools/AddToBytes` 生成。

资源导入（原始目录位于脚本的 `Source` 常量）：

```powershell
unity command run_script --file Tools/ProfileUI/ImportProfileAssets.cs --entry ImportProfileAssets.Main --timeout_ms 120000 --timeout 130
```

`BuildProfilePrefabs` 是本次改造的首次制作脚本，会替换大厅和社交窗口中的旧图像节点，并合并旧窗口注册。已经完成迁移的项目应直接在 Unity Prefab 编辑器维护资源，不重复执行首次制作步骤。`RetireLegacyProfile` 记录旧窗口和旧头像的清理步骤。

`FixLobbyPortrait` 记录大厅移除旧框缩放层的修复；`FixShopPortraitCards` 记录商城透明卡片和独立购买按钮的首次修复。商城购买按钮必须与会被隐藏的旧图片节点分开，拥有勾选也不能放在旧图片节点下。后续维护直接编辑已保存的 Prefab，不重复执行添加点击组件的首次修复脚本。

## 窗口行为

`ProfileEditWindowProperties(ProfileEditTab.Name/Avatar/AvatarFrame)` 决定打开页签。每页保留独立草稿；确认只提交本页；关闭丢弃未提交草稿。已装备绿色勾与待确认高亮分开。资料列表仅显示已拥有项，商城负责购买。

`AvatarPortraitView` 通过三个序列化 Image 和 AspectRatioFitter 显示头像、内轮廓 Mask、装饰框。所有消费者都使用该组件，业务代码不查找组件。

## 验证与截图

```powershell
unity command run_tests --mode editor --filter ProfileCustomizationTests --async_tests true
unity command run_script --file Tools/ProfileUI/PreviewProfile.cs --entry PreviewProfile.Main
dotnet test Backend/tests/AChen.Backend.Api.Tests --configuration Release --filter 'FullyQualifiedName~AvatarFrameTests|FullyQualifiedName~AvatarMigrationTests|FullyQualifiedName~PlayerEndpointsTests|FullyQualifiedName~SocialEndpointsTests'
```

预览脚本在独立 Preview Scene 中渲染，不切换用户场景；输出在 `Temp/ProfileWork/Screenshots`。预览网格用样本数据展示布局，实际拥有过滤、草稿切换和关闭重开由 `ProfileCustomizationTests` 验证。

后端迁移只清理旧头像拥有记录并初始化默认头像框，保留金币、昵称、壁纸和卡牌资产；由 EF 迁移历史保证只执行一次。新的客户端、后端和包含 `avatar-frames.bytes` 的配置应配套发布。本任务不自动部署远端服务或执行生产数据库迁移。
`RefineShopPortraitLayout` 清理透明商品卡的旧 TMP 内边距与行实例价格定位覆盖。`VerifyShopPurchase` 在已打开商城的本地 Play 样本会话中检查射线点击、确认文本和取消，并截图；它清空当前内存访问令牌以禁止真实交易，不写磁盘令牌，运行结束后停止 Play。
