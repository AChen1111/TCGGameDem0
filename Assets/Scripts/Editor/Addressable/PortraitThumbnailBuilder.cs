using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>保留源图，只为列表生成 128 像素缩略图；由图集预构建流程调用。</summary>
public static class PortraitThumbnailBuilder
{
    public const string Path = UiAtlasPrebuild.Folder + "/PortraitThumbnailSource.png";
    public static Sprite[] Build()
    {
        var entries=AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>("Assets/AddressableCatalogs/SpriteCatalog.asset").Entries
            .Where(e=>e.assetName.StartsWith("a_",StringComparison.Ordinal)||e.assetName.StartsWith("af_",StringComparison.Ordinal))
            .OrderBy(e=>e.assetName,StringComparer.Ordinal).ToArray();
        var sprites=entries.Select(e=>AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(e.reference.AssetGUID)).OfType<Sprite>()
            .Single(s=>e.reference.SubObjectName.Length==0 || s.name==e.reference.SubObjectName)).ToArray();
        string signature=Hash128.Compute(string.Join("|",entries.Select((e,i)=>e.assetName+":"+AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(sprites[i]))))+"|128-v2").ToString();
        var old=(TextureImporter)AssetImporter.GetAtPath(Path);
        if(old!=null && old.userData==signature)return AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().ToArray();
        const int size=128, cell=136, columns=16;
        var sheet=new Texture2D(columns*cell,Mathf.CeilToInt(sprites.Length/(float)columns)*cell,TextureFormat.RGBA32,false);
        sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
        var regions=new SpriteRect[sprites.Length];
        var previous=RenderTexture.active;
        try
        {
            for(int i=0;i<sprites.Length;i++)
            {
                var s=sprites[i]; float factor=size/Mathf.Max(s.rect.width,s.rect.height);
                int w=Mathf.Max(1,Mathf.RoundToInt(s.rect.width*factor)),h=Mathf.Max(1,Mathf.RoundToInt(s.rect.height*factor));
                var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
                var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(s);
                UnityEngine.Graphics.Blit(s.texture,rt,new Vector2(uv.z-uv.x,uv.w-uv.y),new Vector2(uv.x,uv.y));
                RenderTexture.active=rt;
                var pixels=new Texture2D(w,h,TextureFormat.RGBA32,false);
                pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();
                int x=(i%columns)*cell+4,y=(i/columns)*cell+4;
                sheet.SetPixels32(x,y,w,h,pixels.GetPixels32());
                regions[i]=new SpriteRect {name="thumb_"+entries[i].assetName,spriteID=GUID.Generate(),rect=new Rect(x,y,w,h),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
                UnityEngine.Object.DestroyImmediate(pixels);RenderTexture.ReleaseTemporary(rt);
            }
            sheet.Apply();File.WriteAllBytes(Path,sheet.EncodeToPNG());
        }
        finally { RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(sheet); }
        AssetDatabase.ImportAsset(Path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(Path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.maxTextureSize=4096;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
        importer.userData=signature;importer.SaveAndReimport();
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var existing=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
        foreach(var region in regions)if(existing.TryGetValue(region.name,out var id))region.spriteID=id;
        provider.SetSpriteRects(regions);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(regions.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().ToArray();
    }
}
