using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class LuaPadBuildCopy : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        CopyNow();
    }

    [MenuItem("Tools/Lua/Copy LuaWorkspace to StreamingAssets")]
    public static void CopyNow()
    {
        string dest = Path.Combine(Application.dataPath, "StreamingAssets", LuaPadWorkspace.StreamingFolder);
        LuaPadWorkspace.CopySourceTo(dest);
        AssetDatabase.Refresh();
        Debug.Log($"[LuaPad] 已拷贝 LuaRaw -> {dest}");
    }
}
