using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class PortraitListUpgrade
{
    public const string MaterialPath="Assets/UI/Shader/AvatarComposite.mat";
    public static void Apply()
    {
        var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if(material==null)
        {material=new Material(Shader.Find("TCG/UI/AvatarComposite"));AssetDatabase.CreateAsset(material,MaterialPath);}
        foreach(string path in new[]{"Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab","Assets/UI/Prefab/Hall/Profile/ProfileCosmeticRow.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var portraits=EditorUtility.CollectDependencies(new Object[]{root}).OfType<AvatarPortraitView>().Where(p=>p.transform.IsChildOf(root.transform)).ToArray();
                foreach(var portrait in portraits)
                {
                    var so=new SerializedObject(portrait);
                    if(so.FindProperty("m_listThumbnail").boolValue)continue;
                    foreach(Transform child in portrait.transform)child.gameObject.SetActive(false);
                    var rect=(RectTransform)new GameObject("PortraitComposite",typeof(RectTransform)).transform;
                    rect.gameObject.layer=5;rect.SetParent(portrait.transform,false);
                    rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                    var graphic=rect.gameObject.AddComponent<AvatarCompositeGraphic>();graphic.material=material;graphic.raycastTarget=false;
                    so.FindProperty("m_listThumbnail").boolValue=true;so.FindProperty("m_composite").objectReferenceValue=graphic;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
        }
        foreach(string path in new[]{"Assets/UI/Prefab/Hall/Shop/ShopWindows.prefab","Assets/UI/Prefab/Hall/Profile/ProfileEditWindow.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var deps=EditorUtility.CollectDependencies(new Object[]{root});
                foreach(var scroll in deps.OfType<UnityEngine.UI.ScrollRect>().Where(s=>s.transform.IsChildOf(root.transform)))
                {
                    var masks=EditorUtility.CollectDependencies(new Object[]{scroll.viewport.gameObject}).OfType<UnityEngine.UI.Mask>().Where(m=>m.transform==scroll.viewport).ToArray();
                    bool hideGraphic=masks.Any(m=>!m.showMaskGraphic);
                    foreach(var mask in masks)Object.DestroyImmediate(mask);
                    var rectMask=deps.OfType<UnityEngine.UI.RectMask2D>().FirstOrDefault(m=>m.transform==scroll.viewport);
                    if(rectMask==null)scroll.viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                    if(hideGraphic)foreach(var image in deps.OfType<UnityEngine.UI.Image>().Where(i=>i.transform==scroll.viewport))image.color=Color.clear;
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
        }
        AssetDatabase.SaveAssets();
    }
}
