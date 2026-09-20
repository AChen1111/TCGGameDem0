using System.Text;
using UnityEditor;
using UnityEngine;

public static class AddWindowFadeCanvasGroups
{
    static readonly string[] Paths =
    {
        "Assets/UI/Prefab/Hall/PreGameUI/Meau/SettingWindow.prefab",
        "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendWindow.prefab",
        "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftWindow.prefab",
    };

    public static string Run()
    {
        var log = new StringBuilder();
        for (int i = 0; i < Paths.Length; i++)
        {
            string path = Paths[i];
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<CanvasGroup>() == null)
                {
                    root.AddComponent<CanvasGroup>();
                }

                bool saved;
                PrefabUtility.SaveAsPrefabAsset(root, path, out saved);
                log.Append(root.name).Append(saved ? "-ok;" : "-save-fail;");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        return log.ToString();
    }
}
