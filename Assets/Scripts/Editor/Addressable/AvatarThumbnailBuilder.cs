using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>由头像原图生成列表用缩略图，并以「原 key + _S」登记到 SpriteCatalog，打包进单页 AvatarThumbs 图集。</summary>
public static class AvatarThumbnailBuilder
{
    public const string SourceFolder = "Assets/UI/Sprite/ProfileCustomization/Avatars";
    public const string ThumbFolder = "Assets/UI/Sprite/ProfileCustomization/AvatarThumbs";
    public const string KeySuffix = "_S";
    // 240 + 4 像素 padding 时 4096 图集每行 16 张，单页容纳 256 张。
    const int ThumbSize = 240;

    [MenuItem("Tools/Atlas/生成头像缩略图并重建图集")]
    public static void BuildAndPrepareAtlases()
    {
        Build();
        UiAtlasPrebuild.Prepare();
    }

    public static void Build()
    {
        Directory.CreateDirectory(ThumbFolder);
        var sources = Directory.GetFiles(SourceFolder, "*.png").Select(p => p.Replace('\\', '/')).ToArray();
        var created = sources.Select(WriteThumbnail).ToArray();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in created) ConfigureImporter(path);
        Register(sources, created);
    }

    static string WriteThumbnail(string source)
    {
        string name = Path.GetFileNameWithoutExtension(source);
        if (name.EndsWith("_L", StringComparison.Ordinal)) name = name.Substring(0, name.Length - 2);
        string target = ThumbFolder + "/" + name + KeySuffix + ".png";
        if (File.Exists(target) && File.GetLastWriteTimeUtc(target) >= File.GetLastWriteTimeUtc(source)) return target;

        var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        src.LoadImage(File.ReadAllBytes(source));
        float scale = (float)ThumbSize / Mathf.Max(src.width, src.height);
        int dw = Mathf.Max(1, Mathf.RoundToInt(src.width * scale));
        int dh = Mathf.Max(1, Mathf.RoundToInt(src.height * scale));
        var dst = new Texture2D(dw, dh, TextureFormat.RGBA32, false);
        dst.SetPixels32(AreaDownsample(src.GetPixels32(), src.width, src.height, dw, dh));
        File.WriteAllBytes(target, dst.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(src);
        UnityEngine.Object.DestroyImmediate(dst);
        return target;
    }

    // 预乘 Alpha 的面积平均，避免透明边缘被黑色像素污染。
    static Color32[] AreaDownsample(Color32[] src, int sw, int sh, int dw, int dh)
    {
        var result = new Color32[dw * dh];
        float fx = (float)sw / dw, fy = (float)sh / dh;
        for (int y = 0; y < dh; y++)
        {
            float y0 = y * fy, y1 = y0 + fy;
            for (int x = 0; x < dw; x++)
            {
                float x0 = x * fx, x1 = x0 + fx;
                float r = 0, g = 0, b = 0, a = 0, area = 0;
                for (int sy = (int)y0; sy < Mathf.Min(sh, Mathf.CeilToInt(y1)); sy++)
                {
                    float wy = Mathf.Min(y1, sy + 1) - Mathf.Max(y0, sy);
                    for (int sx = (int)x0; sx < Mathf.Min(sw, Mathf.CeilToInt(x1)); sx++)
                    {
                        float w = wy * (Mathf.Min(x1, sx + 1) - Mathf.Max(x0, sx));
                        Color32 c = src[sy * sw + sx];
                        float pa = c.a * w;
                        r += c.r * pa; g += c.g * pa; b += c.b * pa; a += pa; area += w;
                    }
                }
                result[y * dw + x] = a <= 0 ? new Color32(0, 0, 0, 0) : new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(r / a), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(g / a), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(b / a), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(a / area), 0, 255));
            }
        }
        return result;
    }

    static void ConfigureImporter(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
            && importer.maxTextureSize == 256 && !importer.mipmapEnabled) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 256;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void Register(string[] sources, string[] thumbs)
    {
        var catalog = AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>(AddressableCatalogSetup.SpritePath);
        for (int i = 0; i < sources.Length; i++)
        {
            string sourceGuid = AssetDatabase.AssetPathToGUID(sources[i]);
            var entry = catalog.Entries.FirstOrDefault(e => e.reference != null && e.reference.AssetGUID == sourceGuid
                && !e.assetName.EndsWith(KeySuffix, StringComparison.Ordinal));
            if (entry == null)
            {
                Debug.LogWarning("头像原图未登记到 SpriteCatalog，跳过缩略图登记: " + sources[i]);
                continue;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(thumbs[i]);
            catalog.EditorAdd(entry.assetName + KeySuffix, new AssetReferenceSprite(AssetDatabase.AssetPathToGUID(thumbs[i]))
            {
                SubObjectName = sprite.name
            });
        }

        EditorUtility.SetDirty(catalog);
        AddressableCatalogSetup.SyncAddressKeys();
        AssetDatabase.SaveAssets();
    }
}
