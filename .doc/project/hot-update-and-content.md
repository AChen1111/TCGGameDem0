# 热更新与内容分发

## 启动链

`PreInit -> LoadDll -> 获取内容 Manifest -> 加载 AOT 元数据与 HotUpdate.dll -> HotUpdateEntry.Boot -> 更新 Addressables -> Init 场景 -> 登录、配置和游戏 UI`

## 运行模式

- Editor 默认直接使用已加载的热更新程序集，便于快速开发。
- Player 从内容服务获取 DLL 和 Addressables 内容。
- 开启 `LoadDll.useRemoteContentInEditor` 后，Editor 也走远程内容链路。

## 发布链

`构建热更新 DLL -> 构建 Addressables -> 生成 Manifest -> 上传后端内容服务 -> 客户端按版本更新`

发布前需配置环境变量 `ACHEN_CONTENT_PUBLISH_KEY`，然后执行 Unity 菜单 `Tools/HotUpdate/Build And Publish Release`。

