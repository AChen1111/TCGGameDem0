using UnityEditor;
using UnityEngine;

public static class FixFriendGiftLayout
{
    const string FriendWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendWindow.prefab";
    const string GiftWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftWindow.prefab";
    const string FriendRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendRowPrefab.prefab";
    const string RequestRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/FriendApplyRowPrefab.prefab";
    const string GiftRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftApplyRowPrefab.prefab";

    public static string Run()
    {
        FixRow(FriendRowPath);
        FixRow(RequestRowPath);
        FixRow(GiftRowPath);
        string friend = FixFriendWindow();
        string gift = FixGiftWindow();
        AssetDatabase.SaveAssets();
        return friend + ";" + gift + ";rows-ok";
    }

    static void FixRow(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1600f, 240f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string FixFriendWindow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FriendWindowPath);
        try
        {
            PlaceClose(root.transform.Find("Btn_Close"));
            PlaceSearch(root.transform.Find("Inp_Name"), new Vector2(-40f, 80f), new Vector2(400f, 100f));
            PlaceSearch(root.transform.Find("Btn_Find"), new Vector2(250f, 80f), new Vector2(76f, 76f));
            InsetPanel(root.transform.Find("Scr_List") as RectTransform, 24f, 160f, 24f, 90f);
            InsetPanel(root.transform.Find("Txt_Empty") as RectTransform, 24f, 160f, 24f, 90f);
            PrefabUtility.SaveAsPrefabAsset(root, FriendWindowPath);
            return "friend-layout-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string FixGiftWindow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GiftWindowPath);
        try
        {
            PlaceClose(root.transform.Find("Btn_Close"));
            InsetPanel(root.transform.Find("Scr_List") as RectTransform, 24f, 24f, 24f, 90f);
            InsetPanel(root.transform.Find("Txt_Empty") as RectTransform, 24f, 24f, 24f, 90f);
            PrefabUtility.SaveAsPrefabAsset(root, GiftWindowPath);
            return "gift-layout-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void PlaceClose(Transform close)
    {
        if (close == null) return;
        var rt = close.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(793f, -61f);
        rt.sizeDelta = new Vector2(76f, 76f);
        rt.localScale = Vector3.one;
    }

    static void PlaceSearch(Transform target, Vector2 pos, Vector2 size)
    {
        if (target == null) return;
        var rt = target.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
        rt.localScale = Vector3.one;
    }

    static void InsetPanel(RectTransform rt, float left, float bottom, float right, float top)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
        rt.localScale = Vector3.one;
    }
}
