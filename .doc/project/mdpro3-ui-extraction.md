# MDPro3 UI 素材提取

2026-09-27，从 `D:/Game/MDPro3` 的 UI 打包资源导出到
`C:/Users/ldc20/OneDrive/Desktop/UI`。

## 输出

- 5,355 张独立 Sprite PNG。
- 3,910 张原始 Texture2D PNG，包含完整图集、菜单贴图、图标及图标资源包中的模型纹理。
- 16 个独立字体文件：12 个 TTF、4 个 OTF；原包中的 18 个字体对象有重复文件。
- 总计 9,281 个素材文件，823,847,121 字节。
- 18 个 `Font Texture` 对象宽高为 0，没有静态像素，标记为 `empty_runtime_texture`，不生成 PNG。

实际目录示例：

```text
UI/
  Sprites/
    CommonUI/GUI_CommonButtonBack.png
    Icon/Mate/1000001.png
    Menus/
    Builtin/
  Textures/
    CommonUI/
    Icon/
    FontAtlases/
  Fonts/
    FZZYJW.ttf
  ui_manifest.json
  validation_summary.json
```

`Sprites` 是按打包 Sprite 数据还原裁切、旋转及紧密网格后的独立图片。
`Textures` 保存完整贴图和图集。图标按源资源路径分类，包含头像、头像框、卡组盒、同伴、卡包、卡套和场地等。

`ui_manifest.json` 记录素材名称、源 Bundle、对象编号、文件路径、SHA-256，以及 Sprite 的原始 rect、pivot、border、pixels_per_unit。上述元数据是原始 Sprite 数据；没有创建 Unity `.meta`、Prefab 或自动绑定当前项目的 UI 引用。

## 提取命令

依赖 Python、UnityPy 1.25.3 和 Pillow。工具不进入 Unity 程序集。

```powershell
python -X utf8 .tools/extract_mdpro3_ui.py `
  'D:\Game\MDPro3' `
  'C:\Users\ldc20\OneDrive\Desktop\UI'
```

读取 `StandaloneWindows64/MDPro3` 下的 icon、prefab、texture、font 资源包以及 `MDPro3_Data/data.unity3d`，共 4,271 个 Bundle 加主程序数据包。合并加载以解析 Sprite 的跨包图集引用；使用有容量限制的纹理缓存，避免每个图标包重复保留整张解码图集。

原始包只读。输出同名文件内容不同时附加对象编号，仍冲突则拒绝覆盖。重复运行沿用清单中已完成的条目；不自动修复或替换用户修改过的已导出文件。

## 验收

- 所有 9,281 个素材文件的 SHA-256 均与导出清单一致。
- 9,265 张 PNG 的文件校验全部通过。
- 16 个 TTF/OTF 均能通过 Pillow FreeType 加载。
- 抽查返回按钮 Sprite，裁切与透明通道正常。
- 提取错误、文件验证错误均为 0。

本次为离线素材提取，文件验收已完成；没有在 Unity 中重新搭建或运行 UI 页面，也没有验证原始 UI Prefab 的行为。
