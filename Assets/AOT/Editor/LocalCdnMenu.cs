using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public static class HotUpdateBuildMenu
{
    [MenuItem("Tools/HotUpdate/Build Addressables")]
    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }
}
