using System.IO;
using UnityEngine;

/// <summary>Editor 工具共用的项目路径.</summary>
public static class EditorPaths
{
    public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    /// <summary>项目根目录下的相对路径拼接为绝对路径.</summary>
    public static string FromProjectRoot(params string[] parts)
    {
        return Path.GetFullPath(Path.Combine(ProjectRoot, Path.Combine(parts)));
    }

    /// <summary>Assets 相对路径(Assets/...)转为绝对路径.</summary>
    public static string AssetPathToAbsolute(string assetPath)
    {
        return Path.Combine(ProjectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
