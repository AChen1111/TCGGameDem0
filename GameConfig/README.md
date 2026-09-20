# 游戏配置

日常只改 `TableData/` 下的 CSV，然后点 Unity 菜单 **`Tools/AddToBytes`**。不要手改 `Assets/GameConfiguration/` 里的生成文件，也不要再用旧的逐表导出菜单。

## 目录

| 位置 | 用途 |
| --- | --- |
| `TableData/*.csv` | 唯一源表，全部平铺，不要放进子目录 |
| `Assets/GameConfiguration/*.bytes` | 生成产物 |
| `Assets/GameConfiguration/LocalizationSettings.asset` | 字体映射，由按钮校验并归组；字体资源仍在原目录 |

CSV 第一行是字段名，第二行是字段类型，第三行起是数据。类型必须写明：`string`、`int`、`long`、`float`、`bool` 及其数组，可空标量加 `?`。数组单元格写 JSON 数组。卡牌 ID 用字符串，保留前导零。

## 产物

每张顶层 CSV 输出同名 `.bytes`（`BinaryTable`）。新加一张表后，再点一次按钮即可生成、归组并在登录前预加载；业务行为仍要另接代码。

## 按钮做什么

`Tools/AddToBytes` 只生成配置并同步 Addressables，不构建、不发布。它会：

1. 扫描 `TableData/`，子目录中的 CSV 会报路径并要求迁到顶层。
2. 校验文件名、表头、类型、数据和九个必需类别的跨表引用。
3. 先在临时目录生成整套产物，成功后再更新 `Assets/GameConfiguration/`；失败则保留上一套。
4. 地址写成 `GameConfig/<文件主名>`，统一标签 `GameConfig`。字体映射地址是 `GameConfig/LocalizationSettings`。
5. 清理没有源表的生成产物，保留字体资产和其他手管文件。
6. 相同内容不重写，保留资产 GUID。

发布窗口在构建 Addressables 前会调用同一套生成逻辑。内容包是协议版本 3，清单用 `configs[]` 登记类别、地址、格式、路径、大小和 SHA-256。

## 登录前加载

远程模式仍先把 Addressables 全量下到磁盘缓存，再按标签加载全部配置和字体映射。全部解析并校验通过后才进入初始化与登录。失败停在启动页，显示失败原因，可重试。
