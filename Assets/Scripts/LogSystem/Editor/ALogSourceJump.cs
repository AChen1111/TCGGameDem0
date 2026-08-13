using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>把栈帧定位到源码:C#走Unity的OpenAsset,Lua不是Unity资源,直接调外部编辑器打开</summary>
public static class ALogSourceJump
{
    public static void Open(ALogFrame frame) {
        if (frame == null || !frame.CanJump)
        {
            return;
        }

        string path = frame.FilePath.Replace('\\', '/');
        if (!frame.IsLua && path.StartsWith("Assets/"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, frame.Line);
                return;
            }
        }

        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path).Replace('\\', '/');
        }
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ALog] 文件不存在: {path}");
            return;
        }
        if (!InternalEditorUtility.OpenFileAtLineExternal(path, frame.Line))
        {
            Debug.LogWarning($"[ALog] 打开失败,请检查 Preferences > External Tools 的编辑器配置: {path}:{frame.Line}");
        }
    }
}
