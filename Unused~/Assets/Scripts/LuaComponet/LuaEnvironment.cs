using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XLua;
/// <summary>
/// 负责初始化Lua坏境,并将注册特定Loader到Lua环境中
/// Editor环境:以 Assets/Scripts/LuaRaw 为根目录直接加载 .lua 源码文件
/// 真机环境:从 Resources/LuaBundle.bytes 中加载 luac 编译后的字节码
/// 两种环境均按“文件名”索引,require时无需带文件夹路径,
/// 例如 Assets/Scripts/LuaRaw/UI/Screen/BaseScreen.lua 只需 require("BaseScreen")
/// </summary>
public class LuaEnvironment
{
    private LuaEnv m_luaEnv;
    public LuaEnv LuaEnv => m_luaEnv;

#if UNITY_EDITOR
    /// <summary>是否在Editor下自动连接EmmyLua调试器。IDE未F5监听时只会Warning,不影响运行。</summary>
    public static bool EnableEmmyLuaDebug = true;
    private const int EmmyLuaPort = 9966;
#endif

    public void Init() {
        m_luaEnv = new LuaEnv();
        m_luaEnv.AddLoader(CustomLoader);
#if UNITY_EDITOR
        if (EnableEmmyLuaDebug)
        {
            TryConnectEmmyLua();
        }
#endif
    }

#if UNITY_EDITOR
    //Editor环境:以LuaRaw为根目录,递归扫描所有.lua文件,按文件名建立索引
    private static readonly string LuaRawRoot = Application.dataPath + "/Scripts/LuaRaw/";

    //文件名(不含扩展名) -> 文件完整路径
    private Dictionary<string, string> m_fileIndex;

    private byte[] CustomLoader(ref string filepath) {
        if (m_fileIndex == null)
        {
            BuildFileIndex();
        }

        //require时只需要文件名,不需要文件夹路径,例如 require("BaseScreen")
        string fileName = filepath;
        int lastDot = fileName.LastIndexOf('.');
        if (lastDot >= 0)
        {
            fileName = fileName.Substring(lastDot + 1);
        }

        if (m_fileIndex.TryGetValue(fileName, out string fullPath))
        {
            //设置为真实路径,便于调试与报错定位
            filepath = fullPath;
            return ReadLuaBytes(fullPath);
        }
        // emmy_core 等原生库走 package.cpath,不算业务脚本缺失
        if (fileName != "emmy_core")
        {
            Debug.LogWarning($"[LuaEnvironment] Editor环境未找到Lua文件: {filepath}");
        }
        return null;
    }

    //递归扫描LuaRaw目录下所有.lua文件,按文件名(不含扩展名)建立索引
    //运行时热重载前需要重新调用,否则运行期间新增的.lua文件require不到
    public void BuildFileIndex() {
        m_fileIndex = new Dictionary<string, string>();
        if (!Directory.Exists(LuaRawRoot))
        {
            Debug.LogError($"[LuaEnvironment] 未找到Lua根目录: {LuaRawRoot}");
            return;
        }
        string[] files = Directory.GetFiles(LuaRawRoot, "*.lua", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (LuaPadWorkspace.SkipRuntimeScan(file))
            {
                continue;
            }
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (m_fileIndex.ContainsKey(fileName))
            {
                Debug.LogWarning($"[LuaEnvironment] 存在重名Lua文件: {fileName} , 已使用 {file} 覆盖之前的路径");
            }
            m_fileIndex[fileName] = file;
        }
    }

    //连接EmmyLua调试器:先在IDE按F5监听,再进Unity Play
    // Windows上Emmy常只绑IPv6(::1),故127.0.0.1与::1都尝试
    private void TryConnectEmmyLua() {
        string dllPath = FindEmmyCoreDll();
        if (string.IsNullOrEmpty(dllPath))
        {
            Debug.LogWarning("[LuaEnvironment] 未找到 emmy_core.dll。请安装 VSCode 的 EmmyLua(tangzx) 插件。Cursor 的 theo.emmylua 通常不含原生调试库。");
            return;
        }

        // package.cpath 需要 ?.dll 模板,目录下实际文件名为 emmy_core.dll
        string cpathDir = Path.GetDirectoryName(dllPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(cpathDir))
        {
            return;
        }

        Debug.Log($"[LuaEnvironment] EmmyLua dll: {dllPath}");

        string lua = $@"
package.cpath = package.cpath .. ';{cpathDir}/?.dll'
local dbg = require('emmy_core')
local hosts = {{ '127.0.0.1', '::1', 'localhost' }}
local lastErr
for _, host in ipairs(hosts) do
    local ok, err = pcall(function()
        dbg.tcpConnect(host, {EmmyLuaPort})
    end)
    if ok then
        print('[EmmyLua] 已连接 ' .. host .. ':{EmmyLuaPort}')
        return
    end
    lastErr = err
end
print('[EmmyLua] 连接失败: ' .. tostring(lastErr))
print('[EmmyLua] 请先取消再F5启动 EmmyLua New Debug,确认出现 Wait for connection 后再点 Unity Play')
";
        try
        {
            m_luaEnv.DoString(lua);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LuaEnvironment] EmmyLua连接异常: {e.Message}");
        }
    }

    //在 VSCode / Cursor / Insiders 扩展目录中查找最新的 emmy_core.dll(优先 x64)
    private static string FindEmmyCoreDll() {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] roots =
        {
            Path.Combine(home, ".vscode", "extensions"),
            Path.Combine(home, ".cursor", "extensions"),
            Path.Combine(home, ".vscode-insiders", "extensions"),
        };

        var candidates = new List<string>();
        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }
            // 匹配 tangzx.emmylua-* / theo.emmylua-* 等
            foreach (string extDir in Directory.GetDirectories(root, "*emmylua*"))
            {
                string x64 = Path.Combine(extDir, "debugger", "emmy", "windows", "x64", "emmy_core.dll");
                string x86 = Path.Combine(extDir, "debugger", "emmy", "windows", "x86", "emmy_core.dll");
                if (File.Exists(x64))
                {
                    candidates.Add(x64);
                }
                else if (File.Exists(x86))
                {
                    candidates.Add(x86);
                }
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        // 取最近写入的一份(通常是最新安装的插件)
        return candidates.OrderByDescending(File.GetLastWriteTimeUtc).First();
    }

    static byte[] ReadLuaBytes(string fullPath)
    {
        byte[] bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            var sliced = new byte[bytes.Length - 3];
            Buffer.BlockCopy(bytes, 3, sliced, 0, sliced.Length);
            return sliced;
        }
        return bytes;
    }
#else
    //真机环境:从打包的LuaBundle.bytes中加载luac字节码,按文件名索引
    private const string LuaBundleName = "LuaBundle";
    private Dictionary<string, byte[]> m_luaBundle;

    //真机没有源文件索引,留空实现让调用方无需区分平台
    public void BuildFileIndex() {
    }

    private byte[] CustomLoader(ref string filepath) {
        if (m_luaBundle == null)
        {
            LoadLuaBundle();
        }

        //require时只需要文件名,不需要文件夹路径,例如 require("BaseScreen")
        string fileName = filepath;
        int lastDot = fileName.LastIndexOf('.');
        if (lastDot >= 0)
        {
            fileName = fileName.Substring(lastDot + 1);
        }

        if (m_luaBundle.TryGetValue(fileName, out byte[] bytes))
        {
            return bytes;
        }
        Debug.LogWarning($"[LuaEnvironment] LuaBundle中未找到模块: {fileName}");
        return null;
    }

    //解析LuaBundle.bytes: int32文件数量 + 循环{文件名, int32长度, 字节码}
    private void LoadLuaBundle() {
        m_luaBundle = new Dictionary<string, byte[]>();
        TextAsset asset = Resources.Load<TextAsset>(LuaBundleName);
        if (asset == null)
        {
            Debug.LogError($"[LuaEnvironment] 未找到 Resources/{LuaBundleName}.bytes,请先执行 Tools/Lua/Build LuaBundle 打包");
            return;
        }
        using (var stream = new MemoryStream(asset.bytes))
        using (var reader = new BinaryReader(stream))
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string moduleName = reader.ReadString();
                int length = reader.ReadInt32();
                byte[] data = reader.ReadBytes(length);
                m_luaBundle[moduleName] = data;
            }
        }
        Debug.Log($"[LuaEnvironment] LuaBundle加载完成,共 {m_luaBundle.Count} 个模块");
    }
#endif
}
