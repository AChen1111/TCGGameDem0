using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class AddPunctFallbackFont
{
    const string SourcePath = "Assets/UI/Fonts/FZYouHJW_512B.ttf";
    const string FallbackPath = "Assets/UI/Fonts/FZZYJW Punct SDF.asset";
    const string MainFontPath = "Assets/UI/Fonts/FZZYJW SDF.asset";
    const string Chars = "【】「」『』（）〔〕①②③④⑤⑥⑦⑧⑨⑩：；、。！？…—·☆★";

    public static string Run()
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
        if (source == null)
        {
            return "source-missing";
        }

        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackPath);
        if (fallback == null)
        {
            fallback = TMP_FontAsset.CreateFontAsset(
                source, 90, 9, GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic, true);
            AssetDatabase.CreateAsset(fallback, FallbackPath);
            if (fallback.material != null)
            {
                fallback.material.name = "FZZYJW Punct Atlas Material";
                AssetDatabase.AddObjectToAsset(fallback.material, fallback);
            }

            Texture2D[] atlases = fallback.atlasTextures;
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i] != null)
                {
                    atlases[i].name = "FZZYJW Punct Atlas";
                    AssetDatabase.AddObjectToAsset(atlases[i], fallback);
                }
            }
        }

        fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fallback.isMultiAtlasTexturesEnabled = true;
        bool added = fallback.TryAddCharacters(Chars, out string missing);
        TMP_FontAsset main = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MainFontPath);
        if (main != null)
        {
            if (main.fallbackFontAssetTable == null)
            {
                main.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            }

            if (!main.fallbackFontAssetTable.Contains(fallback))
            {
                main.fallbackFontAssetTable.Add(fallback);
            }

            EditorUtility.SetDirty(main);
        }

        EditorUtility.SetDirty(fallback);
        AssetDatabase.SaveAssets();
        bool hasBracket = fallback.HasCharacter('【', searchFallbacks: false, tryAddCharacter: false);
        bool hasCircle = fallback.HasCharacter('①', searchFallbacks: false, tryAddCharacter: false);
        return "added=" + added + ";missing=" + missing + ";has【=" + hasBracket + ";has①=" + hasCircle
            + ";file=" + File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, FallbackPath));
    }
}
