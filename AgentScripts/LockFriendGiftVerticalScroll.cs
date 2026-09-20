using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LockFriendGiftVerticalScroll
{
    const string FriendWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendWindow.prefab";
    const string GiftWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftWindow.prefab";

    public static string Run()
    {
        string friend = Lock(FriendWindowPath);
        string gift = Lock(GiftWindowPath);
        AssetDatabase.SaveAssets();
        return friend + ";" + gift;
    }

    static string Lock(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Transform scroll = root.transform.Find("Scr_List");
            var scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.horizontalScrollbar = null;
            Transform horizontal = scroll.Find("Scrollbar Horizontal");
            if (horizontal != null)
            {
                horizontal.gameObject.SetActive(false);
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            return root.name + "-vertical-only";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
