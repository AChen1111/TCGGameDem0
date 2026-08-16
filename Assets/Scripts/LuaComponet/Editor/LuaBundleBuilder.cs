using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XLua;

/// <summary>
/// 负责把 Assets/Scripts/LuaRaw 目录下的所有 .lua 文件
/// 用 luac 编译(string.dump)后打包成 Resources/LuaBundle.bytes
/// 格式: int32文件数量 + 循环{文件名(不含扩展名/路径), int32长度, luac字节码}
/// 与 LuaEnvironment 的 Loader 保持一致:按文件名索引,require时无需带文件夹路径
/// </summary>
public static class LuaBundleBuilder
{
    private const string LuaRawRoot = "Assets/Scripts/LuaRaw";
    private const string OutputDir = "Assets/Resources";
    private const string OutputPath = OutputDir + "/LuaBundle.bytes";

    [MenuItem("Tools/Lua/Build LuaBundle")]
    public static void Build() {
        string rawRootFull = Path.GetFullPath(LuaRawRoot);
        if (!Directory.Exists(rawRootFull))
        {
            Debug.LogError($"[LuaBundleBuilder] 未找到Lua根目录: {LuaRawRoot}");
            return;
        }

        string[] luaFiles = Directory.GetFiles(rawRootFull, "*.lua", SearchOption.AllDirectories);
        var bundled = new List<string>();
        foreach (string file in luaFiles)
        {
            if (file.Replace('\\', '/').Contains("/EmmyApi/"))
            {
                continue;
            }
            bundled.Add(file);
        }
        luaFiles = bundled.ToArray();
        if (luaFiles.Length == 0)
        {
            Debug.LogWarning($"[LuaBundleBuilder] {LuaRawRoot} 下没有任何.lua文件");
            return;
        }

        //临时创建LuaEnv,用 load + string.dump 编译出luac字节码(等效luac)
        LuaEnv luaEnv = new LuaEnv();
        try
        {
            var compile = luaEnv.DoString(@"
                return function(source, chunkname)
                    local f = assert(load(source, chunkname))
                    return string.dump(f, false)
                end")[0] as LuaFunction;

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            //按文件名(不含扩展名)去重,检测重名
            var seenNames = new HashSet<string>();
            foreach (string file in luaFiles)
            {
                string moduleName = Path.GetFileNameWithoutExtension(file);
                if (!seenNames.Add(moduleName))
                {
                    Debug.LogWarning($"[LuaBundleBuilder] 存在重名Lua文件: {moduleName} , 打包出的LuaBundle中该模块名将只保留最后一个文件的内容");
                }
            }

            using (var stream = new FileStream(OutputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(luaFiles.Length);
                foreach (string file in luaFiles)
                {
                    //模块名 = 文件名(不含扩展名),与LuaEnvironment的Loader按文件名索引保持一致
                    string relative = Path.GetRelativePath(rawRootFull, file).Replace('\\', '/');
                    string moduleName = Path.GetFileNameWithoutExtension(file);

                    byte[] source = File.ReadAllBytes(file);
                    byte[] bytecode = compile.Func<byte[], string, byte[]>(source, "@" + relative);
                    if (bytecode == null || bytecode.Length == 0)
                    {
                        throw new Exception($"编译失败: {relative}");
                    }

                    writer.Write(moduleName);
                    writer.Write(bytecode.Length);
                    writer.Write(bytecode);
                    Debug.Log($"[LuaBundleBuilder] 已打包 {moduleName} ({bytecode.Length} bytes)");
                }
            }

            compile.Dispose();
            AssetDatabase.Refresh();
            Debug.Log($"[LuaBundleBuilder] 打包完成: {OutputPath},共 {luaFiles.Length} 个模块");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaBundleBuilder] 打包失败: {e}");
        }
        finally
        {
            luaEnv.Dispose();
        }
    }
}
