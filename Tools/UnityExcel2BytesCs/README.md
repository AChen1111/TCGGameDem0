# 旧导表工具

卡牌和多语言表已并入统一配置流程。日常只编辑 `TableData/` 下的 CSV，然后使用 Unity 菜单 **`Tools/AddToBytes`**。

不要再运行已删除的 `Excel2CsBytesTool.ps1`，也不要使用 `SDGSupporter/Excel` 菜单。独立 schema 已取消，字段类型写在 CSV 第二行。`Cards` 与 `Translations` 仍输出 bytes，读取接口不变。
