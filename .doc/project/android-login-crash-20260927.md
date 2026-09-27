# Android 登录闪退修复报告（2026-09-27）

## 结论

登录响应中的数组进入 Newtonsoft.Json 默认合约初始化时，会扫描序列化回调方法；当前 Android IL2CPP / HybridCLR 组合在读取这些方法的参数元数据时发生原生非法内存访问，直接终止游戏。不是普通 C# 异常，外层 try/catch 无法恢复。

修复放在热更业务层的 `Assets/Scripts/Network/Http/BackendJson.cs`：CLR 数组直接建立 `JsonArrayContract`，跳过数组不需要的回调扫描；普通集合与业务 DTO 保持默认 camelCase 解析流程。无需更换已安装的 APK。

## 证据与定位

- 设备：MEIZU 21，Android 16，arm64；Unity 6000.5.2f1。
- 17:56:13、17:56:59 两次用户操作均触发 `SIGSEGV / SEGV_ACCERR`，Unity 主线程退出。
- `libil2cpp.so` Build ID 为 `3846de6d2e927ba4d125b51593bf681cc3e9839e`，与本地符号文件一致。
- 符号化调用链：`DefaultContractResolver.CreateArrayContract → InitializeContract → ResolveCallbackMethods → GetCallbackMethodsForType → RuntimeMethodInfo.GetParameters → Reflection.GetParamObjects → Method.GetParamName → GlobalMetadata.GetParameterInfo → GetIl2CppTypeFromIndex`。
- 崩溃前 12 个 AOT 补充元数据均返回 OK，业务配置已完成初始化。
- 已安装 APK 与本地最终 APK 的 SHA-256 一致：`c5cd793c7a2d430be3272a717930918c3dd8df7d17482ec1448595f1809e56b9`。没有依据把故障直接归结为安装包或 AOT 文件错配。
- 18:01:52 恢复原有 Android 热更内容后再次崩溃，说明问题仍存在于原发布版本。
- 诊断过程中，提前解析空数组或仅加入类型追踪的重编译 DLL 曾正常运行。因此不能把临时诊断版运行成功当成修复，也不能据此断言只有某一个数组字段有问题。正式修复明确移除了已确认的危险调用路径。
- 崩溃发生在登录响应反序列化，未进入抽卡或大图预览的传感器交互流程；现有证据不支持陀螺仪是本次原因。

底层参数元数据为何在该组合下失效仍需运行时级专项排查；本次交付的是业务层规避修复，并未修改或升级 HybridCLR / IL2CPP 原生运行时。官方通用排错资料未给出与本次堆栈完全对应的已确认结论：[HybridCLR 常见错误](https://www.hybridclr.cn/docs/help/commonerrors)。

## 修复与验证

- 仅改变后端 JSON 的数组合约创建方式，不更改登录协议、账号资料或客户端配置。
- 官方 HybridCLR Android Player 脚本编译成功。
- 相关 `CardDrawJsonTests`、`SocialJsonTests` 共 10 项通过，覆盖玩家卡牌、抽卡结果、卡池、好友和收件箱数组解析。
- 同一已安装 APK 加载修复 DLL 后，用户于 18:03:04 登录成功，18:03:09 大厅完成入场，进程 PID 7413 保持运行，未再次出现该崩溃。
- 没有以 Editor 测试代替原生崩溃验证；该故障的主要回归依据是原手机、原 APK 的登录前后对照。
- 已移除临时 `[DEBUG-login-crash]` 类型追踪和启动预热代码。

修复 DLL SHA-256：`84fa585cb056df805b98b704a6c4f905a9ed3a83b846040906357206174fb853`。

## 交付状态

Android 热更版本 `login-fix-20260927` 已发布到本地开发后端 `http://127.0.0.1:5080`，内容 ID：`68e12bc0-8521-42a3-aeda-a93a7d4e875c`。发布的 DLL 哈希与手机验证过的 DLL 一致。此次没有重建或安装 APK，也没有发布 Windows 内容。

已停止临时代理并恢复 `adb reverse tcp:5080 tcp:5080`。18:05:17 手机从正常后端加载上述正式内容，18:05:18 自动恢复玩家会话成功并进入 GameScene，随后大厅完成入场；PID 8145 保持运行。验证覆盖手动登录与重启后自动恢复两条原故障链路，没有追加无关全量验收。

调试中出现过一次 `INVALID_REFRESH_TOKEN / 401`：原故障在后端轮换刷新令牌后、客户端保存新令牌前崩溃，使旧令牌失效。用户重新登录后已恢复，无需清空数据或重新注册。日志中的 Play Asset Delivery 类缺失提示在成功与失败版本中均出现，本次成功登录和大厅运行不受其影响，未将其误判为闪退原因。

原始日志、符号化结果、编译产物与相关测试结果保存于 `TestBuild/LoginCrash-20260927/`。临时代理仅用于当前 USB 设备对照验证，不作为最终运行依赖。
