using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;

/// <summary>通过 Unity Pipeline run_script 调用 Main；保留素材导入和遮罩的可复现来源。</summary>
public static class ImportProfileAssets
{
    const string Source = "C:/Users/ldc20/OneDrive/Desktop/UI/Sprites/";
    const string Target = "Assets/UI/Sprite/ProfileCustomization";
    static readonly string[] Common = {
        "GUI_CommonWindowS", "GUI_CommonButtonSetting", "GUI_CommonButtonSetting_On", "GUI_CommonButtonSetting_Over",
        "GUI_CommonItemFrameS_Base", "GUI_CommonItemFrameS_Frame", "GUI_CommonItemFrameS_Over", "GUI_CommonButtonPlof_Check_On",
        "GUI_CommonInputField_Base", "GUI_CommonInputField_Edit_Over", "GUI_CommonButtonM", "GUI_CommonButtonM_Over",
        "GUI_ButtonClose", "GUI_CommonScrollBar"
    };
    public static string Main()
    {
        Directory.CreateDirectory(Target + "/Avatars");
        Directory.CreateDirectory(Target + "/Frames");
        Directory.CreateDirectory(Target + "/Masks");
        Directory.CreateDirectory(Target + "/Common");
        var catalog = AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>("Assets/AddressableCatalogs/SpriteCatalog.asset");
        foreach (string file in Directory.GetFiles(Source + "Icon/ProfileIcon/HD", "*.png"))
        {
            int id = int.Parse(Path.GetFileName(file).Substring(11, 7));
            Import(file, Target + "/Avatars/" + Path.GetFileName(file), "a_" + id, Vector4.zero, catalog);
        }
        foreach (string file in Directory.GetFiles(Source + "Icon/ProfileFrame/HD", "*.png"))
        {
            int id = int.Parse(Path.GetFileName(file).Substring(12, 7));
            string path = Target + "/Frames/" + Path.GetFileName(file);
            Import(file, path, "af_" + id, Vector4.zero, catalog);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(file));
            int width = texture.width, height = texture.height;
            var mask = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                // 4x supersampling keeps the stencil boundary smooth at small portrait sizes.
                int inside = 0;
                for (int sy = 0; sy < 4; sy++) for (int sx = 0; sx < 4; sx++)
                    if (Inside(id, (x + (sx + .5f) / 4) / width, (y + (sy + .5f) / 4) / height)) inside++;
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)(inside * 255 / 16));
            }
            mask.SetPixels32(pixels); mask.Apply();
            string maskPath = Target + "/Masks/af_" + id + "_Mask.png";
            File.WriteAllBytes(maskPath, mask.EncodeToPNG());
            Import(maskPath, maskPath, "af_" + id + "_Mask", Vector4.zero, catalog);
            UnityEngine.Object.DestroyImmediate(mask); UnityEngine.Object.DestroyImmediate(texture);
        }
        foreach (string name in Common)
        {
            Vector4 border = name switch {
                "GUI_CommonWindowS" => new Vector4(16,16,16,16),
                "GUI_CommonButtonSetting" => new Vector4(5,5,5,5),
                "GUI_CommonButtonSetting_On" or "GUI_CommonButtonSetting_Over" => new Vector4(14,14,14,14),
                "GUI_CommonItemFrameS_Base" or "GUI_CommonItemFrameS_Frame" => new Vector4(14,14,14,14),
                "GUI_CommonItemFrameS_Over" => new Vector4(22,22,22,22),
                "GUI_CommonInputField_Base" => new Vector4(6,14,6,14),
                "GUI_CommonInputField_Edit_Over" => new Vector4(6,6,6,6),
                "GUI_CommonButtonM" => new Vector4(22,22,22,22),
                "GUI_CommonButtonM_Over" => new Vector4(30,30,30,30),
                "GUI_CommonScrollBar" => new Vector4(6,7,6,7),
                _ => Vector4.zero
            };
            Import(Source + "CommonUI/" + name + ".png", Target + "/Common/" + name + ".png", "Profile_" + name, border, catalog);
        }
        foreach (string dir in new[]{"Avatars", "Frames", "Masks", "Common"})
            AddressableCatalogSetup.MarkFolderInGroup(AddressableCatalogSetup.UiGroupForPath(Target), Target + "/" + dir, "Sprite/ProfileCustomization/" + dir);
        EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); AddressableCatalogSetup.SyncAddressKeys();
        return "Imported 230 avatars, 60 frames, 60 masks, 14 UI sprites";
    }
    static void Import(string source, string target, string key, Vector4 border, SpriteAddressableCatalog catalog)
    {
        if (source != target) File.Copy(source, target, true);
        AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(target);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(target);
        catalog.EditorAdd(key, new AssetReferenceSprite(AssetDatabase.AssetPathToGUID(target)) { SubObjectName = sprite.name });
    }
    // 内轮廓在每张原图坐标中配置；开放式装饰同样使用完整的内轮廓，不依赖框的透明连通区域。
    static bool Inside(int id, float u, float v)
    {
        float x = (u - .5f) * 2, y = (v - .5f) * 2;
        bool hex = (id >= 1030001 && id <= 1030025) || id is 1030027 or 1030028 or 1030029
            or 1030032 or 1030033 or 1030034 or 1031001 or 1031002 or 1033001 or 1033003;
        if (hex)
        {
            float radius = id switch { 1030001 => .92f, 1030027 => .79f, 1031001 => .84f, 1031002 => .82f, 1033003 => .96f, _ => .89f };
            return Mathf.Abs(y) <= radius && Mathf.Abs(x) <= radius - Mathf.Abs(y) * .5f;
        }
        if (id == 1032009) // 竖向六边形冰晶框。
            return Mathf.Abs(x) <= .66f && Mathf.Abs(y) <= .85f - Mathf.Abs(x) * .5f;
        if (id is 1032002 or 1032003 or 1032004 or 1032008 or 1031009)
        {
            float rx = id == 1032003 ? .73f : .68f, ry = id == 1032003 ? .80f : .72f;
            float dx = Mathf.Max(0, Mathf.Abs(x) - rx + .16f), dy = Mathf.Max(0, Mathf.Abs(y) - ry + .16f);
            return dx * dx + dy * dy <= .16f * .16f;
        }
        float r = id switch {
            1030026 => .72f, 1030030 => .63f, 1030031 => .68f,
            1031003 => .70f, 1031004 => .72f, 1031005 => .79f, 1031006 => .80f,
            1031007 => .67f, 1031008 => .63f, 1031010 => .64f, 1031011 => .63f,
            1031012 => .61f, 1031013 => .95f, 1031014 => .80f, 1032001 => .73f,
            1032005 => .65f, 1032006 => .74f, 1032007 => .71f, 1033002 => .81f,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "缺少逐框裁剪轮廓")
        };
        return x*x + y*y <= r*r;
    }
}
