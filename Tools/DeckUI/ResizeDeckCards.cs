using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Author existing prefabs in place; keep GUIDs, local IDs and controller bindings.
public static class ResizeDeckCards
{
    const string Folder = "Assets/UI/Prefab/Hall/Deck/";
    const int Columns = 8;
    static readonly Vector2 PoolArt = new Vector2(56, 82);
    static readonly Vector2 DeckArt = new Vector2(76, 111);
    static T[] All<T>(GameObject root) where T:Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    static T Ref<T>(UnityEngine.Object host, string field) where T:UnityEngine.Object =>
        (T)new SerializedObject(host).FindProperty(field).objectReferenceValue;
    static void Rect(Transform root, string name, float x, float y, float w, float h)
    {
        var r = (RectTransform)root.Find(name);
        r.anchoredPosition = new Vector2(x, -y);
        r.sizeDelta = new Vector2(w, h);
    }
    static void Save(GameObject root, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(root, Folder + name + ".prefab");
        PrefabUtility.UnloadPrefabContents(root);
    }
    static void Cell(string name, Vector2 art, bool placed)
    {
        var root = PrefabUtility.LoadPrefabContents(Folder + name + ".prefab");
        var size = art + new Vector2(4, 4);
        ((RectTransform)root.transform).sizeDelta = size;
        Rect(root.transform, "Art", 2, 2, art.x, art.y);
        Rect(root.transform, "Unowned", 2, 2, art.x, art.y);
        Rect(root.transform, "Selected", 0, 0, size.x, size.y);
        var cell = All<DeckCardCell>(root).Single();
        Ref<TMP_Text>(cell, "m_Rarity").gameObject.SetActive(false);
        var count = Ref<TMP_Text>(cell, "m_Quantity");
        count.gameObject.SetActive(!placed);
        if (!placed)
        {
            count.transform.SetParent(root.transform, false);
            foreach (Transform old in root.transform.Cast<Transform>().Where(x => x.name == "QuantityBadge").ToArray())
                UnityEngine.Object.DestroyImmediate(old.gameObject);
            // The badge remains above the dimming and selected overlays.
            var badge = new GameObject("QuantityBadge", typeof(RectTransform), typeof(CanvasRenderer));
            var rect = (RectTransform)badge.transform;
            rect.SetParent(root.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-2, 2);
            rect.sizeDelta = new Vector2(20, 20);
            var background = badge.AddComponent<Image>();
            background.color = Color.black; background.raycastTarget = false;
            count.transform.SetParent(rect, false);
            count.rectTransform.anchorMin = Vector2.zero;
            count.rectTransform.anchorMax = Vector2.one;
            count.rectTransform.offsetMin = count.rectTransform.offsetMax = Vector2.zero;
            count.fontSize = 18; count.color = Color.white;
            count.alignment = TextAlignmentOptions.Center;
            count.margin = Vector4.zero; count.raycastTarget = false;
            count.text = "3";
        }
        Save(root, name);
    }
    public static string Main()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before authoring.");
        Cell("DeckCardCell", PoolArt, false);
        Cell("DeckPlacedCardCell", DeckArt, true);
        var row = PrefabUtility.LoadPrefabContents(Folder + "DeckCardRow.prefab");
        foreach (Transform child in row.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        ((RectTransform)row.transform).sizeDelta = new Vector2(520, 94);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "DeckCardCell.prefab");
        var so = new SerializedObject(All<DeckCardRow>(row).Single());
        var array = so.FindProperty("m_Items"); array.arraySize = Columns;
        // 8 * 60 + 7 * 4 + 2 * 6 = 520.
        for (int i = 0; i < Columns; i++)
        {
            var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab, row.transform);
            var rect = (RectTransform)child.transform;
            rect.anchoredPosition = new Vector2(6 + i * 64, -4);
            rect.sizeDelta = PoolArt + new Vector2(4, 4);
            array.GetArrayElementAtIndex(i).objectReferenceValue = All<DeckCardCell>(child).Single();
        }
        so.ApplyModifiedPropertiesWithoutUndo(); Save(row, "DeckCardRow");
        var root = PrefabUtility.LoadPrefabContents(Folder + "DeckEditWindow.prefab");
        var w = All<DeckEditWindow>(root).Single();
        foreach (var scroll in new[] { Ref<ScrollRect>(w, "m_MainScroll"), Ref<ScrollRect>(w, "m_ExtraScroll") })
        {
            var grid = All<GridLayoutGroup>(root).Single(x => x.transform == scroll.content);
            grid.cellSize = DeckArt + new Vector2(4, 4);
            // 8 * 80 + 7 * 6 + 2 * 10 = 702.
            grid.constraintCount = Columns; grid.spacing = new Vector2(6, 8);
            grid.padding = new RectOffset(10, 10, 8, 8);
        }
        Ref<RectTransform>(w, "m_DragRoot").sizeDelta = DeckArt;
        var drag = Ref<DeckCardView>(w, "m_DragCard");
        ((RectTransform)drag.transform).sizeDelta = DeckArt;
        Save(root, "DeckEditWindow");
        return "Authored eight columns in deck and pool; hidden thumbnail names; bottom-right black quantity badges.";
    }
}
