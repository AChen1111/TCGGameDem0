using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class ProbeTmpGlyphs
{
    const string FontPath = "Assets/UI/Fonts/FZZYJW SDF.asset";
    const string Sample = "【】「」『』（）①②③：、。太古的白石龙族调整效果召唤除外墓地";

    public static string Run()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            return "missing-font";
        }

        var missingNow = new StringBuilder();
        foreach (char c in Sample)
        {
            if (!font.HasCharacter(c, searchFallbacks: false, tryAddCharacter: false))
            {
                missingNow.Append(c);
            }
        }

        string target = "【】「」①②③";
        bool added = font.TryAddCharacters(target, out string missing);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();

        var still = new StringBuilder();
        foreach (char c in Sample)
        {
            if (!font.HasCharacter(c, searchFallbacks: false, tryAddCharacter: false))
            {
                still.Append(c);
            }
        }

        return "before=" + missingNow + ";tryAdd=" + added + ";missing=" + missing + ";after=" + still
            + ";atlasCount=" + font.atlasTextureCount + ";multi=" + font.isMultiAtlasTexturesEnabled;
    }
}
