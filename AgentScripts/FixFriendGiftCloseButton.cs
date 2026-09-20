using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FixFriendGiftCloseButton
{
    const string FriendWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendWindow.prefab";
    const string GiftWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftWindow.prefab";

    public static string Run()
    {
        return Fix(FriendWindowPath) + ";" + Fix(GiftWindowPath);
    }

    static string Fix(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Transform close = root.transform.Find("Btn_Close");
            Transform empty = root.transform.Find("Txt_Empty");
            if (close == null || empty == null)
            {
                throw new System.Exception("缺少 Btn_Close 或 Txt_Empty: " + path);
            }

            TMP_Text tmp = empty.GetComponent<TMP_Text>();
            tmp.raycastTarget = false;
            empty.gameObject.SetActive(false);
            empty.SetSiblingIndex(root.transform.Find("Scr_List").GetSiblingIndex() + 1);
            close.SetAsLastSibling();
            close.gameObject.layer = root.layer;

            var button = close.GetComponent<Button>();
            var image = close.GetComponent<Image>();
            if (button != null && image != null)
            {
                button.targetGraphic = image;
                button.interactable = true;
                image.raycastTarget = true;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            return System.IO.Path.GetFileNameWithoutExtension(path) + "-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
