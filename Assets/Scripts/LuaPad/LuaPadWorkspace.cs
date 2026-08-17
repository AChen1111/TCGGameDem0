using System;
using System.IO;
using UnityEngine;

public static class LuaPadWorkspace
{
    public const string ScratchFileName = "LuaPadScratch.lua";
    public const string StreamingFolder = "LuaWorkspace";
    public const string DraftsFolder = "LuaPadDrafts";

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

    public static string DraftsRoot => Path.Combine(SourceRoot, DraftsFolder);

    public static bool SkipRuntimeScan(string file)
    {
        return file.Replace('\\', '/').Contains("/" + DraftsFolder + "/");
    }

    public static string SanitizeDraftName(string name)
    {
        if (name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(0, name.Length - 4);
        }
        if (name.Length == 0)
        {
            throw new ArgumentException(name);
        }
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '_' && c != '-')
            {
                throw new ArgumentException(name);
            }
        }
        return name + ".lua";
    }

    public static string ResolveDraftPath(string name)
    {
        string file = SanitizeDraftName(name);
        string dir = Path.GetFullPath(DraftsRoot);
        string path = Path.GetFullPath(Path.Combine(dir, file));
        string prefix = dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(path);
        }
        return path;
    }

    public static string[] ListDrafts()
    {
        if (!Directory.Exists(DraftsRoot))
        {
            return Array.Empty<string>();
        }
        string[] files = Directory.GetFiles(DraftsRoot, "*.lua");
        var names = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            names[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        Array.Sort(names);
        return names;
    }

    public static string SaveDraft(string name, string text)
    {
        string file = SanitizeDraftName(name);
        Directory.CreateDirectory(DraftsRoot);
        File.WriteAllText(ResolveDraftPath(name), text ?? string.Empty);
        return Path.GetFileNameWithoutExtension(file);
    }

    public static string LoadDraft(string name)
    {
        return File.ReadAllText(ResolveDraftPath(name));
    }

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
            if (Path.GetFileName(dir) == DraftsFolder)
            {
                continue;
            }
            CopyDir(dir, Path.Combine(dest, Path.GetFileName(dir)));
        }
    }
}
