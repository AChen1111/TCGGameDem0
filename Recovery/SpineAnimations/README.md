# Spine 动画恢复结果

本目录从本机 `Yu-Gi-Oh! Master Duel` 的 UnityFS 缓存中恢复了 10 套完整 Spine 资源。

## 可直接使用的文件

每套资源位于同名子目录，包含：

- `*JS.json`：Spine 骨骼与动画数据。
- `*.atlas`：Spine 图集描述。
- `*.png`：图集纹理。

`manifest.json` 记录了每套资源的 Spine 导出版本、动画名称、源资源包和依赖包。

## Unity 导入

1. 使用 Spine 官方 `spine-unity 4.2` 运行时。
2. 将同一套的 JSON、atlas 和 PNG 一起复制到 Unity 的同一资源目录。
3. 等待 Spine 导入器生成 AtlasAsset 和 SkeletonDataAsset。
4. 在 SkeletonDataAsset 的预览或 SkeletonAnimation 组件中选择动画。

这些文件的 JSON 版本介于 Spine `4.2.20` 至 `4.2.43`。若运行时对补丁版本敏感，优先使用与 JSON 中 `skeleton.spine` 字段一致或更新的 Spine 4.2 运行时。

## 完整性

已检查：

- 10 份 JSON 均可解析，并至少包含一个动画。
- 每个 atlas 声明的纹理页都存在。
- 10 张 PNG 均可完整解码为 RGBA 图像。

本地恢复内容请仅在你拥有相应使用权的范围内使用或分发。
