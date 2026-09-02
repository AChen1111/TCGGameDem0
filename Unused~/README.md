# Unused~ — 已停用的 XLua / Lua

Unity 不导入仓库根目录，也不导入以 `~` 结尾的文件夹。这里的文件不会编译、不会进包。

不要把这些目录移回 `Assets/`。游戏运行时一律 C#。归档背景见 [.doc/project/](../.doc/project/README.md)。

`Tools/` 下的 `node_modules`、`bin`、`obj` 不入库；需要时在本机自行还原。
