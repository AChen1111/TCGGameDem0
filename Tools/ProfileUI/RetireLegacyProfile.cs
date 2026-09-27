using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RetireLegacyProfile
{
    public static string Main()
    {
        const string itemPath="Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab";
        var root=PrefabUtility.LoadPrefabContents(itemPath);
        var item=Resources.FindObjectsOfTypeAll<ShopOwnedItem>().Single(x=>x.transform.IsChildOf(root.transform)||x.gameObject==root);
        var old=(Image)new SerializedObject(item).FindProperty("m_ImgMain").objectReferenceValue;
        old.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprite/ProfileCustomization/Avatars/ProfileIcon1010001_L.png");old.gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root,itemPath);PrefabUtility.UnloadPrefabContents(root);
        var sprites=AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>("Assets/AddressableCatalogs/SpriteCatalog.asset");
        sprites.Entries.RemoveAll(x=>System.Text.RegularExpressions.Regex.IsMatch(x.assetName,@"^a_\d{2}$"));EditorUtility.SetDirty(sprites);
        var prefabs=AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>("Assets/AddressableCatalogs/PrefabCatalog.asset");
        prefabs.Entries.RemoveAll(x=>x.assetName=="SelfChooseWindow"||x.assetName=="ChangeNameWindow"||x.assetName=="AvatarItemPrefab");EditorUtility.SetDirty(prefabs);
        foreach(var path in new[]{"Assets/UI/Avatar","Assets/UI/Prefab/Hall/SelfWindow","Assets/UI/Prefab/Hall/PreGameUI/ChangeNameWindow.prefab",
            "Assets/Scripts/UI/PreGameUI/Avatar/AvatarSelectWindow.cs","Assets/Scripts/UI/PreGameUI/Avatar/AvatarItem.cs","Assets/Scripts/UI/PreGameUI/ChangeNameWindow.cs","Assets/Scripts/UI/PreGameUI/Meau/SocialPortrait.cs",
            "Assets/UI/Shader/AvatarRingFlow.mat","Assets/UI/Shader/AvatarRingFlow.shader","Assets/UI/Shader/AvatarStateFog.mat","Assets/UI/Shader/AvatarStateFog.shader"})
            AssetDatabase.DeleteAsset(path);
        var settings=UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        foreach(var group in settings.groups)foreach(var entry in group.entries.ToArray())
            if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(entry.guid)))settings.RemoveAssetEntry(entry.guid);
        AssetDatabase.SaveAssets();AddressableCatalogSetup.SyncAddressKeys();return "Legacy windows, avatars and catalog entries removed";
    }
}
