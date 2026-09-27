using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Edit existing prefabs in place so their GUIDs, local IDs and bindings remain stable.
public static class ResizeDeckCards
{
    const string Folder = "Assets/UI/Prefab/Hall/Deck/";
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
        float width = art.x + 4;
        ((RectTransform)root.transform).sizeDelta = new Vector2(width, art.y + (placed ? 26 : 42));
        Rect(root.transform, "Art", 2, 2, art.x, art.y);
        Rect(root.transform, "Unowned", 2, 2, art.x, art.y);
        Rect(root.transform, "Selected", 0, 0, width, art.y + 4);
        Rect(root.transform, "Name", 0, art.y + 4, width, 22);
        Rect(root.transform, "Quantity", 0, art.y + 26, width, 16);
        var cell = All<DeckCardCell>(root).Single();
        Ref<TMP_Text>(cell, "m_Rarity").fontSize = 18;
        Ref<TMP_Text>(cell, "m_Quantity").fontSize = 18;
        Ref<TMP_Text>(cell, "m_Quantity").gameObject.SetActive(!placed);
        Save(root, name);
    }
    public static string Main()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before authoring.");
        var preview = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Shop/CardInUI.prefab");
        var art = ((RectTransform)preview.transform).sizeDelta;
        Cell("DeckCardCell", art, false);
        Cell("DeckPlacedCardCell", art, true);
        var row = PrefabUtility.LoadPrefabContents(Folder + "DeckCardRow.prefab");
        foreach (Transform child in row.transform.Cast<Transform>().Skip(2).ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        ((RectTransform)row.transform).sizeDelta = new Vector2(520, art.y + 56);
        var items = All<DeckCardCell>(row).OrderBy(x=>x.transform.GetSiblingIndex()).ToArray();
        for (int i = 0; i < items.Length; i++)
        {
            var rect = (RectTransform)items[i].transform;
            rect.anchoredPosition = new Vector2(42 + i * 229, 0);
            rect.sizeDelta = new Vector2(art.x + 4, art.y + 42);
        }
        var so = new SerializedObject(All<DeckCardRow>(row).Single());
        var array = so.FindProperty("m_Items");array.arraySize = 2;
        for (int i = 0; i < 2; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        so.ApplyModifiedPropertiesWithoutUndo();Save(row, "DeckCardRow");
        var root = PrefabUtility.LoadPrefabContents(Folder + "DeckEditWindow.prefab");
        var w = All<DeckEditWindow>(root).Single();
        Rect(root.transform, "MainHeader", 404, 176, 710, 34);
        Rect(root.transform, "MainTitle", 420, 176, 180, 34);
        Rect(root.transform, "Txt_MainCount", 830, 176, 262, 34);
        Rect(root.transform, "ExtraHeader", 404, 556, 710, 34);
        Rect(root.transform, "ExtraTitle", 420, 556, 180, 34);
        Rect(root.transform, "Txt_ExtraCount", 830, 556, 262, 34);
        Rect(root.transform, "Go_Empty", 451, 339, 610, 80);
        Rect(root.transform, "MainCards", 408, 210, 702, 338);
        Rect(root.transform, "ExtraCards", 408, 590, 702, 338);
        foreach (var scroll in new[] { Ref<ScrollRect>(w, "m_MainScroll"), Ref<ScrollRect>(w, "m_ExtraScroll") })
        {
            scroll.viewport.sizeDelta = scroll.content.sizeDelta = new Vector2(702, 338);
            var grid = All<GridLayoutGroup>(root).Single(x => x.transform == scroll.content);
            grid.cellSize = new Vector2(art.x + 4, art.y + 26);
            grid.constraintCount = 3;grid.spacing = new Vector2(12, 12);grid.padding = new RectOffset(28, 28, 8, 8);
        }
        Ref<RectTransform>(w, "m_DragRoot").sizeDelta = art;
        var drag = Ref<DeckCardView>(w, "m_DragCard");
        ((RectTransform)drag.transform).sizeDelta = art;
        Save(root, "DeckEditWindow");
        return "Matched preview card art " + art + "; deck 3 columns, pool 2 columns; kept existing details and bindings.";
    }
    public static string ApplyRowSizes()
    {
        var preview = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Shop/CardInUI.prefab");
        var art = ((RectTransform)preview.transform).sizeDelta;
        var size = new Vector2(art.x + 4, art.y + 42);
        var row = PrefabUtility.LoadPrefabContents(Folder + "DeckCardRow.prefab");
        foreach (Transform child in row.transform) ((RectTransform)child).sizeDelta = size;
        Save(row, "DeckCardRow");
        foreach (var cell in Resources.FindObjectsOfTypeAll<DeckCardCell>().Where(x => x.gameObject.scene.IsValid() && x.Data != null && !x.Data.InDeck))
            ((RectTransform)cell.transform).sizeDelta = size;
        return "Updated nested card rectangles and existing pool instances to " + size + ".";
    }
}
