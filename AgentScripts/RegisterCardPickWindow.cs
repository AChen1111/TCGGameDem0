using UnityEditor;
using UnityEngine;

public static class RegisterCardPickWindow
{
    public static string Run()
    {
        const string prefabPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";
        AddressableCatalogMenu.AddPrefab(prefabPath);

        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (settings == null || prefab == null)
        {
            throw new System.Exception("找不到 UISetting 或 CardPickWindow 预制体");
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty prop = so.FindProperty("screensToRegister");
        for (int i = 0; i < prop.arraySize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
            {
                AssetDatabase.SaveAssets();
                return "catalog-ok;uisetting-exists";
            }
        }

        int index = prop.arraySize;
        prop.arraySize = index + 1;
        prop.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        return "catalog-ok;uisetting-added";
    }
}
