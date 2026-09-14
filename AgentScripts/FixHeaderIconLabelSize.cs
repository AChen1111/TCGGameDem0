using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FixHeaderIconLabelSize
{
    const string PrefabPath = "Assets/UI/Prefab/BaseUI/BtnAndText.prefab";
    const float FontSize = 20f;
    const float ButtonWidth = 92f;

    public static string Run()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var layout = contents.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = contents.AddComponent<LayoutElement>();
            }

            layout.minWidth = ButtonWidth;
            layout.preferredWidth = ButtonWidth;
            layout.flexibleWidth = 0f;
            layout.layoutPriority = 2;
            EditorUtility.SetDirty(layout);

            Transform desc = contents.transform.Find("desc");
            if (desc == null)
            {
                return "missing-desc";
            }

            var text = desc.GetComponent<TextMeshProUGUI>();
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 36f);
            text.margin = Vector4.zero;
            text.enableAutoSizing = true;
            text.fontSize = FontSize;
            text.fontSizeMin = FontSize;
            text.fontSizeMax = FontSize;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.Top;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Top;
            EditorUtility.SetDirty(text);
            EditorUtility.SetDirty(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        return "locked=" + FontSize + ";width=" + ButtonWidth;
    }
}
