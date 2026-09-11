using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BindShopOwnedItemRefs
{
    public static string Run()
    {
        var report = new StringBuilder();
        report.AppendLine(BindOwnedItem(
            "Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab",
            itemObjectName: "AvatarShopItemPrefab",
            buttonObjectName: "Button",
            ownerObjectName: "Owner"));
        report.AppendLine(BindOwnedItem(
            "Assets/UI/Prefab/Hall/Shop/WallpaperShopItemRowPrefab.prefab",
            itemObjectName: "Icon",
            buttonObjectName: "Icon",
            ownerObjectName: "Owner"));
        report.AppendLine(BindRowItems("Assets/UI/Prefab/Hall/Shop/AvatarShopItemRowPrefab.prefab"));
        report.AppendLine(BindRowItems("Assets/UI/Prefab/Hall/Shop/WallpaperShopItemRowPrefab.prefab"));
        AssetDatabase.SaveAssets();
        return report.ToString().Trim();
    }

    static string BindOwnedItem(
        string prefabPath,
        string itemObjectName,
        string buttonObjectName,
        string ownerObjectName)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Transform itemTransform = FindChild(root.transform, itemObjectName);
            if (itemTransform == null)
            {
                return $"{prefabPath}: 找不到物体 {itemObjectName}";
            }

            var item = itemTransform.GetComponent<ShopOwnedItem>();
            if (item == null)
            {
                return $"{prefabPath}: 找不到 ShopOwnedItem";
            }

            Transform buttonTransform = FindChild(root.transform, buttonObjectName);
            Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
            Image main = button != null ? button.GetComponent<Image>() : null;
            Transform owner = FindChild(root.transform, ownerObjectName);
            TextMeshProUGUI title = FindTmp(root.transform, "Txt_Titile");
            TextMeshProUGUI remainTime = FindTmp(root.transform, "Txt_RemainTime");
            TextMeshProUGUI value = FindTmp(root.transform, "Txt_Value");

            var so = new SerializedObject(item);
            so.FindProperty("m_BtnAll").objectReferenceValue = button;
            so.FindProperty("m_ImgMain").objectReferenceValue = main;
            so.FindProperty("m_TxtTitle").objectReferenceValue = title;
            so.FindProperty("m_TxtRemainTime").objectReferenceValue = remainTime;
            so.FindProperty("m_TxtValue").objectReferenceValue = value;
            so.FindProperty("m_GoOwned").objectReferenceValue = owner != null ? owner.gameObject : null;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            return $"{prefabPath}: Btn={button != null}, Img={main != null}, Title={title != null}, Remain={remainTime != null}, Value={value != null}, Owned={owner != null}";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string BindRowItems(string prefabPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var row = root.GetComponent<ShopOwnedItemRow>();
            if (row == null)
            {
                return $"{prefabPath}: 找不到 ShopOwnedItemRow";
            }

            ShopOwnedItem[] items = root.GetComponentsInChildren<ShopOwnedItem>(true);
            var so = new SerializedObject(row);
            SerializedProperty array = so.FindProperty("m_Items");
            array.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            return $"{prefabPath}: m_Items={items.Length}";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChild(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    static TextMeshProUGUI FindTmp(Transform root, string name)
    {
        Transform child = FindChild(root, name);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }
}
