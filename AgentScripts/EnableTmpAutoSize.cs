using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class EnableTmpAutoSize
{
    const string PrefabRoot = "Assets/UI/Prefab";
    const string EnglishFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    const string ChineseFontPath = "Assets/UI/Fonts/FZZYJW SDF.asset";
    const string PunctFontPath = "Assets/UI/Fonts/FZZYJW Punct SDF.asset";
    const string BtnAndTextPath = "Assets/UI/Prefab/BaseUI/BtnAndText.prefab";
    const string TextInBtnPath = "Assets/UI/Prefab/BaseUI/TextInBtn.prefab";

    public static string Run()
    {
        string fallback = AttachEnglishFallbacks();
        int prefabs = 0;
        int texts = 0;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int changed = ApplyToPrefab(contents, path);
                if (changed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    prefabs++;
                    texts += changed;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        AssetDatabase.SaveAssets();
        return "fallback=" + fallback + ";prefabs=" + prefabs + ";texts=" + texts;
    }

    static string AttachEnglishFallbacks()
    {
        TMP_FontAsset english = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EnglishFontPath);
        TMP_FontAsset chinese = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ChineseFontPath);
        TMP_FontAsset punct = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PunctFontPath);
        if (english == null)
        {
            return "english-missing";
        }

        if (english.fallbackFontAssetTable == null)
        {
            english.fallbackFontAssetTable = new List<TMP_FontAsset>();
        }

        int added = 0;
        added += AddFallback(english, punct);
        added += AddFallback(english, chinese);
        EditorUtility.SetDirty(english);
        return "added=" + added + ";count=" + english.fallbackFontAssetTable.Count;
    }

    static int AddFallback(TMP_FontAsset font, TMP_FontAsset fallback)
    {
        if (fallback == null || font.fallbackFontAssetTable.Contains(fallback))
        {
            return 0;
        }

        font.fallbackFontAssetTable.Add(fallback);
        return 1;
    }

    static int ApplyToPrefab(GameObject root, string path)
    {
        int changed = 0;
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (PrefabUtility.IsPartOfPrefabInstance(text.gameObject))
            {
                continue;
            }

            EnableAutoSize(text);
            if (path == BtnAndTextPath && text.gameObject.name == "desc")
            {
                FitIconLabel(text);
            }
            else if (path == TextInBtnPath)
            {
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.overflowMode = TextOverflowModes.Overflow;
            }

            EditorUtility.SetDirty(text);
            changed++;
        }

        return changed;
    }

    static void EnableAutoSize(TMP_Text text)
    {
        float max = text.enableAutoSizing && text.fontSizeMax > 1f ? text.fontSizeMax : text.fontSize;
        if (max < 1f)
        {
            max = 36f;
        }

        text.fontSizeMax = max;
        text.fontSizeMin = Mathf.Min(8f, max);
        text.enableAutoSizing = true;
    }

    static void FitIconLabel(TMP_Text text)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 36f);
        text.margin = Vector4.zero;
        text.fontSize = 20f;
        text.fontSizeMin = 20f;
        text.fontSizeMax = 20f;
        text.enableAutoSizing = true;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.Top;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Top;
    }
}
