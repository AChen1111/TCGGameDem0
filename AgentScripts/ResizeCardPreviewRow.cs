using System;
using UnityEditor;
using UnityEngine;

public static class ResizeCardPreviewRow
{
    const string CardInUiPath = "Assets/UI/Prefab/Hall/Shop/CardInUI.prefab";
    const string RowPath = "Assets/UI/Prefab/Hall/Shop/CardPreviewRowPrefab.prefab";
    const int CardsPerRow = 5;
    const float CardWidth = 131f;
    const float CardHeight = 191f;
    const float RowHeight = 199f;

    public static string Run()
    {
        ResizeCard();
        int count = ResizeRow();
        AssetDatabase.SaveAssets();
        return "cardsPerRow=" + count;
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
            rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, RowHeight);

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

            for (int i = 0; i < current.Length; i++)
            {
                current[i].GetComponent<RectTransform>().sizeDelta = new Vector2(CardWidth, CardHeight);
            }

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
