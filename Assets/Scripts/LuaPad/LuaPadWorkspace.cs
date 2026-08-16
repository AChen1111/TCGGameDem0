using System.IO;
using UnityEngine;

public static class LuaPadWorkspace
{
    public const string ScratchFileName = "LuaPadScratch.lua";
    public const string StreamingFolder = "LuaWorkspace";

    public static string SourceRoot =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "Scripts", "LuaRaw"));

    public static string RuntimeRoot
    {
        get
        {
#if UNITY_EDITOR
            return SourceRoot;
#else
            return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, StreamingFolder));
#endif
        }
    }

    public static string ScratchPath => Path.Combine(RuntimeRoot, ScratchFileName);

    public static void CopySourceTo(string dest)
    {
        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, true);
        }
        CopyDir(SourceRoot, dest);
    }

    static void CopyDir(string src, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (string file in Directory.GetFiles(src))
        {
            if (file.EndsWith(".meta"))
            {
                continue;
            }
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
        }
        foreach (string dir in Directory.GetDirectories(src))
        {
            CopyDir(dir, Path.Combine(dest, Path.GetFileName(dir)));
        }
    }
}
