# Python 运营终端

独立管理后端服务、账号礼品和账号金币，不提供 Unity 内容构建或上传功能。

## 启动

需要 Python 3.10 或更新版本，仅使用标准库，无需 `pip install`。

- 双击项目根目录 `运营工具.cmd`。
- Unity 中点击 `Window > TCG > Python 终端`，或 `Tools > 运营工具 > Python 终端`，会弹出独立 PowerShell 窗口并自动进入菜单；退出菜单后窗口保留，可继续输入 PowerShell 命令。
- 在终端中运行 `python -X utf8 Tools/OperationsCli/main.py`。
- 指定地址：`python -X utf8 Tools/OperationsCli/main.py --backend http://127.0.0.1:5080`。

输入数字并回车选择菜单。`0` 返回或退出，`Ctrl+C` 取消当前输入或等待。退出时会停止由本终端启动的后端；Unity 或其他终端启动的服务不会被接管。

启动器通过系统绝对路径调用 PowerShell，由 `Start.ps1` 配置 UTF-8 并定位 Python。即使 Unity 继承的 PATH 不完整，也会查找系统 Python 启动器和当前用户的 Python 安装目录。`运营工具.cmd` 使用 ASCII 内容及 CRLF 换行，`Start.ps1` 使用带 BOM 的 UTF-8，兼容 Windows PowerShell 5.1。

## 功能

| 菜单 | 操作 |
| --- | --- |
| 后端服务 | 健康/就绪检查、构建并启动本地后端、停止本会话进程、查看日志、打开注册页 |
| 账号礼品 | 发金币和/或卡牌到指定账号收件箱，支持卡牌数量、稀有度、自动/预设/自定义标题 Key |
| 账号金币 | 查询余额，确认后向指定账号添加金币 |
| 连接设置 | 修改后端地址 |

### 本地后端

本地启动需要 .NET 8 SDK 和已准备好的 `Backend` 子模块。使用现有后端项目、Development 配置、数据库和内容存储目录，启动命令先 `dotnet build`，再运行产物 DLL。日志保存在 `Temp/OperationsCli/backend.log`。

终端对接当前 Development 后端。金币和礼品操作无需发布密钥；连接设置只填写后端地址。本地启动仍为后端内容分发接口准备发布密钥。

| 环境变量 | 用途 |
| --- | --- |
| `ACHEN_BACKEND_URL` | 默认后端地址，未设置时为 `http://127.0.0.1:5080` |
| `ACHEN_BACKEND_AUTH_SIGNING_KEY` | 本地后端身份签名密钥，未设置时本会话生成 |
| `ACHEN_PYTHON` | 可选，供 `.cmd` 启动器使用的 Python 可执行文件路径 |

身份签名密钥用于玩家登录 Token，由后端启动器在未配置时自动生成，无需在运营菜单输入；自定义该环境变量时须至少 32 个字符。

## 验证范围

经用户授权，已在真实交互终端验证正常 PATH 和缺失 PATH 下的启动、中文显示、菜单选择与返回、无效输入、礼品和金币操作取消、后端地址参数传递、显式 Python 路径和无效路径提示。使用与 Unity 入口相同的 PowerShell 编码命令启动链路，确认退出菜单后仍可执行 PowerShell 命令。

上述验证针对启动及菜单交互；未执行内容构建、后端启动或真实账号写操作，未自动点击 Unity 菜单。
