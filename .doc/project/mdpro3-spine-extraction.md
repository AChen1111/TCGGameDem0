# MDPro3 Spine 运行资源提取

2026-09-27，从 `D:/Game/MDPro3` 的打包文件提取资源，输出到
`C:/Users/ldc20/OneDrive/Desktop/Spine`。

## 结果

- 扫描 `MDPro3_Data/StandaloneWindows64` 中 18,069 个文件，Spine 资源位于 MonsterCutin、MonsterCutin2 和 MasterDuel；解析错误为 0。
- 同时检查其他目录的 Unity Bundle 文件；仅找到 `MDPro3_Data/data.unity3d`，其中没有 Spine 骨骼或图集 TextAsset。
- 共 535 个编号，541 个源资源组，1,020 套骨骼/图集组合，1,956 张 PNG，资源大小 4,063,610,766 字节。
- 保留骨骼 JSON 和图集的原始字节，保留 PNG 的打包尺寸及像素；未修改源文件、图集坐标、PMA 数据或当前 Unity 资产。
- 按编号组织；同编号的 MonsterCutin、MonsterCutin2、MasterDuel 特效/壁纸及 HD/SD 版本分开保存。骨骼 JSON 复制到对应图集目录，方便整套使用。

实际输出示例：

```text
Spine/10000000/MonsterCutin/highend_hd/1/
  P4998JS.json
  P4998.atlas
  P4998.png
```

## 重复执行

依赖 Python、UnityPy 1.25.3、Pillow。本脚本是离线资源工具，不进入 Unity 的 AOT 或热更新程序集。

```powershell
python -X utf8 .tools/extract_mdpro3_spine.py `
  'D:\Game\MDPro3\MDPro3_Data\StandaloneWindows64' `
  'C:\Users\ldc20\OneDrive\Desktop\Spine\inventory.json'

python -X utf8 .tools/extract_mdpro3_spine.py `
  'D:\Game\MDPro3\MDPro3_Data\StandaloneWindows64' `
  'C:\Users\ldc20\OneDrive\Desktop\Spine\inventory.json' `
  --export 'C:\Users\ldc20\OneDrive\Desktop\Spine'
```

工具按源目录结构区分卡片编号和纯数字 Bundle 哈希。图集及纹理按原始容器路径配对；没有 TextAsset 容器路径的合并包，按同包及纹理名配对。与已有文件内容不同时拒绝覆盖，导出失败记录在清单中。已有完整清单的资源组在重复执行时跳过。

## 验证与局限

- 1,020 套骨骼 JSON 均可解析，均有骨骼和动画。
- 所有导出 JSON、atlas 的 SHA-256 与源 TextAsset 清单一致；所有 PNG 校验通过，并与导出清单中的 SHA-256 一致。
- 每张图集页的文件引用均已配齐，提取错误、验证错误以及清单外残留资源文件均为 0。
- 59 页的声明尺寸与实际 PNG 尺寸不同，保留原样。仅整体缩放不要求修改图集坐标。
- 按声明页尺寸归一化 UV 后，38 套资源出现低 alpha 区域疑点，24 套出现边界疑点。这是静态检查结果，不能直接认定动画缺件，也没有据此自动修复。
- 骨骼版本均为 Spine 4.2.x；没有在 Spine 编辑器或 Unity 中逐套播放验收。本任务恢复的是运行资源，没有恢复原始 `.spine` 编辑工程或打包前的高分辨率像素。

输出目录中的 `inventory.json` 记录源文件与对象哈希，`extraction_manifest.json` 记录各编号、目录、动画名、页尺寸与区域疑点，`validation_summary.json` 汇总验证，`atlas_review.json` 单独列出需要进一步查看的图集。

参考：[UnityPy 官方文档](https://github.com/K0lb3/UnityPy)、[项目图集检查说明](spine-atlas-repair.md)。
