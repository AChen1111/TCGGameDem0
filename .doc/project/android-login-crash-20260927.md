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

初轮修复时尚未查明底层参数元数据为何失效；后续离线取证已进一步定位到数组合成方法查错参数表的问题，详见下文。已交付的仍是业务层规避修复，并未修改或升级 HybridCLR / IL2CPP 原生运行时。官方通用排错资料未给出与本次堆栈完全对应的已确认结论：[HybridCLR 常见错误](https://www.hybridclr.cn/docs/help/commonerrors)。

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

## 后续根因定位：数组合成方法查错参数表

用户要求继续定位后，使用原始崩溃寄存器、与 Build ID 匹配的 libil2cpp 反汇编、原 APK 的 `global-metadata.dat`、原发布 HotUpdate.dll 和本地原生运行时源码做只读交叉分析。这一轮没有回退手机版本、重启游戏、修改原生代码或重新打包。

### 已定位的代码问题

以下路径均相对 `HybridCLRData/LocalIl2CppData-WindowsEditor/il2cpp/libil2cpp/`：

1. `metadata/ArrayMetadata.cpp:602`：数组类型的 `klass->image` 使用元素类型的 image。因此，热更 DTO 的数组属于 HotUpdate image。
2. 同文件 `ConstructGenericArrayMethod`（约 290–312 行）：非泛型的系统数组实现方法通过 `memcpy` 复制 `MethodInfo`，保留原始 `methodMetadataHandle`，但 `klass` 改成具体数组类型。
3. `vm/Method.cpp:117` 的 `GetParamName`：非泛型方法不会经过泛型定义还原，直接把上述 `klass` 和原始方法元数据句柄传入 `GetParameterInfo`。
4. `vm/GlobalMetadata.cpp:1520`：方法定义来自原始句柄，但查参数记录却调用 `GetParameterDefinitionFromIndex(klass->image, methodDefinition.parameterStart + paramIndex)`。**方法定义来自系统库，参数表却根据热更数组的 image 选择。**
5. `hybridclr/metadata/MetadataModule.h:212` 按该 image 访问对应的 `InterpreterImage::_params`。这条路径只有断言；本次 Release 二进制没有在访问前阻止越界。

上述表选择代码也存在于本地 `HybridCLRData/il2cpp_plus_repo` 的提交 `dbd9768e008b6c3b7804fc54954f172e20e8c4d8`，不是本次业务修复引入的逻辑。尚未据此判断其他版本是否也有问题。

### 与真实产物、寄存器对照

| 检查项 | 结果 |
| --- | --- |
| 原发布 HotUpdate.dll SHA-256 | `89c446185dc65eb07b0b55d6ec15aca25dab8e0bb099cbb5d3c3616e0adf5d9f`，与故障版本 Manifest 一致 |
| APK 元数据版本 | 107，按当前运行时的紧凑索引格式解码 |
| 非泛型、且有参数的 `InternalArray__*` 方法 | 唯一为 `System.Array.InternalArray__RemoveAt(int index)`，在数组上以 `IList<T>.RemoveAt` 实现方法出现 |
| 该方法在 AOT 参数表中的参数索引 | **6030**；参数名 `index`，正确类型索引为 33056 |
| 原 HotUpdate.dll 全部方法的参数总数 | **4658**，与 `InitMethodDefs` 逐个方法追加参数的方式对应 |
| 错表访问 | 在 HotUpdate 参数表访问索引 **6030**，超出有效范围 0–4657 |
| 崩溃时实际传入的类型索引 | `w0 = 0x0C44100E`，即 **205787150** |
| AOT 类型表实际大小 | **57174** 项 |
| 最终崩溃指令 | `0x31b9b40: ldr x0, [x8]`，`x8 = 0x6cef61bed0`，等于日志中的 fault address |

反汇编显示，这个错误类型索引被当作 AOT 类型索引，用 `索引 × 8` 计算指针表偏移，得到 **1646297200 字节**的偏移，随后读取非法地址。越界读取的内容依赖相邻内存，因此加入日志、预热或重编译改变内存布局后，临时“不崩”不能证明问题消失。

**代码级根因已经明确：数组合成方法的参数元数据按数组元素所属程序集查找，而非按原始方法定义查找，导致跨表索引越界。** `InternalArray__RemoveAt` 是原 APK 中满足这条非泛型、有参数路径的唯一候选。由于本轮没有重新挂原生断点，原现场 `MethodInfo*` 的具体数组类型名称仍是由调用链与产物推导，未作为独立现场读数记录；不得把 `OwnedCardDto[]` 的具体名字写成已被断点直接确认。

### 原生层修复方向与当前状态

本地代码已存在按方法定义选择参数表的重载。需要验证的原生修复候选是：

```diff
- GetParameterDefinitionFromIndex(klass->image, methodDefinition.parameterStart + paramIndex)
+ GetParameterDefinitionFromIndex(methodDefinition, methodDefinition.parameterStart + paramIndex)
```

该重载根据方法定义的来源区分 AOT 和热更元数据，避免被数组改写的 `klass->image` 误导。此处记录的是**待验证的原生修复候选**，本轮没有应用。若要采用，需要重新构建主包，并直接覆盖热更类型数组的 `RemoveAt` 参数反射、普通数组和泛型方法参数反射，再验证原登录链路。

当前手机继续使用已验证的业务层规避修复。此次仅补充取证报告，未新增验收测试。可复算的 APK 元数据解析脚本为 `TestBuild/LoginCrash-20260927/inspect_metadata.py`，结果为同目录 `metadata-forensics.json`；热更方法参数总数使用 Cecil 只读统计，没有执行旧热更代码。
