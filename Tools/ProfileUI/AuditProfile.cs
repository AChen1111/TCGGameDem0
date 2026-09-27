using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/// <summary>核对本次涉及的预制体脚本与 Inspector 绑定，不修改场景或资源。</summary>
public static class AuditProfile
{
    public static string Main()
    {
        string[] paths = {
            "Assets/UI/Prefab/Hall/Profile/AvatarPortrait.prefab",
            "Assets/UI/Prefab/Hall/Profile/ProfileCosmeticRow.prefab",
            "Assets/UI/Prefab/Hall/Profile/ProfileEditWindow.prefab",
            "Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab",
            "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendRowPrefab.prefab",
            "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/FriendApplyRowPrefab.prefab",
            "Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab",
            "Assets/UI/Prefab/Hall/Shop/ShopWindows.prefab"
        };
        Type[] checkedTypes = { typeof(ProfileEditWindow), typeof(ProfileCosmeticRow), typeof(ProfileCosmeticItem),
            typeof(AvatarPortraitView), typeof(PlayerProfileView), typeof(FriendRowItem), typeof(GiftRequestRowItem), typeof(ShopOwnedItem) };
        int bindings = 0, portraits = 0;
        foreach (string path in paths)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            CheckMissing(root.transform);
            var components = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform));
            foreach (var component in components.Where(x => checkedTypes.Contains(x.GetType())))
            {
                if (component is AvatarPortraitView) portraits++;
                foreach (var field in component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(x => x.IsDefined(typeof(SerializeField), false)))
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        if ((UnityEngine.Object)field.GetValue(component) == null)
                            throw new Exception(path + ": " + component.name + "." + field.Name + " 未绑定");
                        bindings++;
                    }
                    else if (field.FieldType.IsArray)
                    {
                        foreach (UnityEngine.Object value in (Array)field.GetValue(component))
                        {
                            if (value == null) throw new Exception(path + ": " + field.Name + " 包含空引用");
                            bindings++;
                        }
                    }
                }
            }
        }
        return $"PASS: {paths.Length} prefabs, {portraits} portrait views, {bindings} serialized references; no missing scripts.";
    }
    static void CheckMissing(Transform node)
    {
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) != 0)
            throw new Exception(node.name + " 包含 Missing Script");
        foreach (Transform child in node) CheckMissing(child);
    }
}
