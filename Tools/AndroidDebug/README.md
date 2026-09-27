# 安卓调试终端

双击项目根目录的 **安卓调试.cmd**，在 CMD 窗口输入编号并回车。需要 Python 3.10+ 和 ADB，无第三方 Python 依赖。当前电脑可直接使用。

| 编号 | 功能 |
| --- | --- |
| 1 | 刷新、列出并切换设备，显示未授权与离线状态 |
| 2 | 实时 Unity 日志，以及 Java / 原生崩溃信息 |
| 3 | 实时错误和致命日志 |
| 4 | 实时全部日志（可能较多） |
| 5 | 查看并保存手机已有崩溃记录 |
| 6 | 一键配置手机 5080 → 电脑 5080 |
| 7 | 自定义端口，选择手机 → 电脑或电脑 → 手机 |
| 8 | 查看转发列表，按编号删除一条 |
| 0 | 退出工具，保留转发配置 |

实时查看时按 **Ctrl+C 返回菜单**。日志自动保存在 `Temp/AndroidDebug/logs/`，文件名包含日期、设备与模式。只停止本工具创建的 logcat 子进程，不停止 ADB 服务，不清空手机日志。日志不绑定游戏 PID，游戏闪退或重新启动后仍继续采集。USB 断开时等待同一台设备重新连接，重连时会再次读取最近 200 条，因此可能出现重复行。屏幕刷屏过快时会跳过部分显示，日志文件保留完整采集内容。

Unity 模式通过日志标签过滤，包含此手机上所有 Unity 应用与系统崩溃记录。需要更广的上下文时选择全部日志；导出的崩溃缓冲区可能包含历史记录和其他应用，分析时注意时间与包名。

## 端口方向

- **手机 → 电脑（reverse）**：手机访问 `127.0.0.1:5080`，转到电脑的 5080。本项目后端通常用这一项；电脑服务必须已监听对应端口。
- **电脑 → 手机（forward）**：电脑访问本地端口，转到手机上的服务端口。

转发针对所选设备，同方向同源端口的再次设置会覆盖原映射。移除操作只删除所选的一条，保留其他调试端口。USB 断开或设备/ADB 重启后转发可能失效，重新连接后查看列表并重新添加。

## 命令行

在项目根目录执行（全局 `--adb`、`--serial` 放在子命令之前）：

```bat
安卓调试.cmd devices
安卓调试.cmd --serial 461QNGDTD89ES reverse 5080
安卓调试.cmd reverse 8080 5080
安卓调试.cmd forward 9000 9001
安卓调试.cmd mappings
安卓调试.cmd remove reverse 5080
安卓调试.cmd logs --mode unity
安卓调试.cmd logs --mode all --history 500 --duration 30
安卓调试.cmd crash
```

也可直接运行 `python -X utf8 Tools/AndroidDebug/main.py`。多台手机连接时菜单需选择设备，命令行需指定 `--serial`，不会自动选错手机。

ADB 自动优先复用正在运行的 Unity / Android Studio ADB，其次检查 PATH、Android SDK 环境变量、用户 SDK 与 Unity 安装目录；避免为找设备而主动重启已有 ADB 服务。手动指定：

```bat
安卓调试.cmd --adb "D:\UnityEditor\6000.5.2f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
```

也支持环境变量 `ANDROID_ADB`（ADB 路径）和 `ACHEN_PYTHON`（Python 路径）。启动器在同一个 CMD 窗口运行，配置 UTF-8，支持目录包含空格和中文。

## 验证

```bat
python -m unittest discover -s Tools/AndroidDebug -p "test_*.py" -v
```

ADB 与日志参数依据 [ADB 官方命令说明](https://android.googlesource.com/platform/packages/modules/adb/+/HEAD/docs/user/adb.1.md) 和 [Logcat 官方说明](https://developer.android.com/tools/logcat)。
