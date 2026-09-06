using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class BindShopItemTexts
{
    public static string Run()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine(BindItem(
            "Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab",
            "AvatarShopItem",
            "AvatarShopItemPrefab"));
        report.AppendLine(BindItem(
            "Assets/UI/Prefab/Hall/Shop/WallpaperShopItemRowPrefab.prefab",
            "WallpaperShopItem",
            "Icon"));
        return report.ToString().Trim();
    }

    static string BindItem(string prefabPath, string componentType, string itemObjectName)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Transform itemTransform = FindChild(root.transform, itemObjectName);
            if (itemTransform == null)
                throw new System.Exception($"{prefabPath} 找不到物体 {itemObjectName}");

            Component item = itemTransform.GetComponent(componentType);
            if (item == null)
                throw new System.Exception($"{prefabPath} 找不到组件 {componentType}");

            TextMeshProUGUI title = FindTmp(root.transform, "Txt_Titile");
            TextMeshProUGUI remainTime = FindTmp(root.transform, "Txt_RemainTime");
            TextMeshProUGUI value = FindTmp(root.transform, "Txt_Value");
            if (title == null || remainTime == null || value == null)
            {
                throw new System.Exception(
                    $"{prefabPath} 文本缺失: Title={title != null}, RemainTime={remainTime != null}, Value={value != null}");
            }

            SerializedObject so = new SerializedObject(item);
            so.FindProperty("m_TxtTitle").objectReferenceValue = title;
            so.FindProperty("m_TxtRemainTime").objectReferenceValue = remainTime;
            so.FindProperty("m_TxtValue").objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            return $"{prefabPath}: Title={title.name}, RemainTime={remainTime.name}, Value={value.name}";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChild(root.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }

    static TextMeshProUGUI FindTmp(Transform root, string name)
    {
        Transform child = FindChild(root, name);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }
}
