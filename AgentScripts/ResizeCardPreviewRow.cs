using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ResizeCardPreviewRow
{
    const string CardInUiPath = "Assets/UI/Prefab/Hall/Shop/CardInUI.prefab";
    const string RowPath = "Assets/UI/Prefab/Hall/Shop/CardPreviewRowPrefab.prefab";
    const int CardsPerRow = 5;
    // Scr_Cards 视口 1321x925, 3 行铺满高度; 卡面 59:86.
    const float ViewportWidth = 1321f;
    const float CardWidth = 203f;
    const float CardHeight = 296f;
    const float Spacing = 16f;
    const int PadHorizontal = 121;
    const int PadVertical = 6;
    const float RowHeight = 308f;

    public static string Run()
    {
        ResizeCard();
        int count = ResizeRow();
        AssetDatabase.SaveAssets();
        return $"cards={count}; card={CardWidth}x{CardHeight}; row={ViewportWidth}x{RowHeight}; pad={PadHorizontal},{Spacing}";
    }

    static void ResizeCard()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(CardInUiPath);
        try
        {
            contents.GetComponent<RectTransform>().sizeDelta = new Vector2(CardWidth, CardHeight);
            PrefabUtility.SaveAsPrefabAsset(contents, CardInUiPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static int ResizeRow()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(RowPath);
        try
        {
            var rowRect = contents.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(ViewportWidth, RowHeight);

            var layout = contents.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(PadHorizontal, PadHorizontal, PadVertical, PadVertical);
            layout.spacing = Spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CardPreviewRowItem row = contents.GetComponent<CardPreviewRowItem>();
            CardInUI[] current = CollectCards(contents.transform);
            for (int i = current.Length - 1; i >= CardsPerRow; i--)
            {
                UnityEngine.Object.DestroyImmediate(current[i].gameObject);
            }

            current = CollectCards(contents.transform);
            if (current.Length != CardsPerRow)
            {
                throw new Exception("行内卡数=" + current.Length + ", 期望=" + CardsPerRow);
            }

            var cardSize = new Vector2(CardWidth, CardHeight);
            for (int i = 0; i < current.Length; i++)
            {
                current[i].GetComponent<RectTransform>().sizeDelta = cardSize;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rowRect);

            var so = new SerializedObject(row);
            SerializedProperty array = so.FindProperty("m_Cards");
            array.arraySize = current.Length;
            for (int i = 0; i < current.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = current[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, RowPath);
            return current.Length;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static CardInUI[] CollectCards(Transform root)
    {
        var list = new System.Collections.Generic.List<CardInUI>();
        for (int i = 0; i < root.childCount; i++)
        {
            CardInUI card = root.GetChild(i).GetComponent<CardInUI>();
            if (card != null)
            {
                list.Add(card);
            }
        }

        return list.ToArray();
    }
}
