using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ScaffoldDeckWindows
{
    public static string RegisterRow()
    {
        const string path = "Assets/UI/Prefab/Hall/Deck/DeckCardRow.prefab";
        if (File.Exists(path)) throw new System.InvalidOperationException("Row already exists; use BuildDeckPrefabs for layout updates.");
        var go = new GameObject("DeckCardRow", typeof(RectTransform));
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        AddressableCatalogMenu.AddPrefab(path);
        return "Registered row catalog key.";
    }
    public static string Main()
    {
        const string folder = "Assets/UI/Prefab/Hall/Deck/";
        if (File.Exists("Assets/Scripts/UI/PreGameUI/Deck/DeckEditWindow.cs"))
            throw new System.InvalidOperationException("Controllers already exist; use BuildDeckPrefabs to preserve business code.");
        Directory.CreateDirectory(folder);
        Directory.CreateDirectory("Assets/Scripts/UI/PreGameUI/Deck");
        foreach (var definition in new[] {
            "DeckListWindow|Btn_Back,Btn_New,Txt_Total,Go_Empty",
            "DeckEditWindow|Btn_Back,Btn_Save,Btn_Rename,Btn_Plus,Btn_Minus,Inp_Search,Txt_Name,Txt_MainCount,Txt_ExtraCount,Txt_Status,Txt_Results,Txt_DetailName,Txt_DetailStats,Txt_DetailType,Txt_DetailDesc,Txt_Rarity,Go_Empty,Go_Detail",
            "DeckNameWindow|Btn_Confirm,Btn_Cancel,Inp_Name,Txt_Title",
            "DeckUnsavedWindow|Btn_Save,Btn_Discard,Btn_Continue,Txt_Message" })
        {
            var parts = definition.Split('|');
            var root = new GameObject(parts[0], typeof(RectTransform), typeof(CanvasGroup));
            foreach (var name in parts[1].Split(','))
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(root.transform, false);
                if (name.StartsWith("Btn_")) go.AddComponent<Button>();
                if (name.StartsWith("Txt_")) go.AddComponent<TextMeshProUGUI>();
                if (name.StartsWith("Inp_")) go.AddComponent<TMP_InputField>();
            }
            var generator = root.AddComponent<UiScreenGenerator>();
            var so = new SerializedObject(generator);
            so.FindProperty("m_kind").enumValueIndex = 1;
            so.FindProperty("m_className").stringValue = parts[0];
            so.FindProperty("m_folderPath").stringValue = "Assets/Scripts/UI/PreGameUI/Deck";
            so.ApplyModifiedPropertiesWithoutUndo();
            generator.CollectUiBinds();
            generator.CreateUiScript();
            PrefabUtility.SaveAsPrefabAsset(root, folder + parts[0] + ".prefab");
            Object.DestroyImmediate(root);
            AddressableCatalogMenu.AddPrefab(folder + parts[0] + ".prefab");
        }
        AssetDatabase.SaveAssets();
        return "Generated four Window controllers and registered their prefab catalog entries.";
    }
}
