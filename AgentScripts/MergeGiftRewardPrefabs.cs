using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MergeGiftRewardPrefabs
{
    const string RewardPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftRewardItem.prefab";
    const string GoldPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftGoldItem.prefab";
    const string CardPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftCardItem.prefab";
    const string RowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftApplyRowPrefab.prefab";
    const string GoldSpriteGuid = "30aa2c10b1f9b0a4298b64274880e844";
    const string FontGuid = "e08f61ff18e195345941c2dfab7459a0";
    const string ScriptGuid = "9c4e8a21b7d64f3e9a15c0d82f6b4e71";

    public static string Run()
    {
        string created = CreateRewardPrefab();
        string nested = NestOnRow();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(GoldPath) != null)
        {
            AssetDatabase.DeleteAsset(GoldPath);
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(CardPath) != null)
        {
            AssetDatabase.DeleteAsset(CardPath);
        }

        RemoveCatalogNames("GiftGoldItem", "GiftCardItem");
        AddressableCatalogSetup.SyncAddressKeys();
        AssetDatabase.SaveAssets();
        return created + ";" + nested + ";catalog-ok";
    }

    static string CreateRewardPrefab()
    {
        Type rewardType = FindType("GiftRewardItem");
        var root = new GameObject("GiftRewardItem", typeof(RectTransform));
        try
        {
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 160f);

            Image gold = CreateGold(root.transform);
            RawImage card = CreateCard(root.transform);
            TMP_Text count = CreateCount(root.transform);

            if (rewardType != null)
            {
                var item = root.AddComponent(rewardType);
                SerializedObject so = new SerializedObject(item);
                so.FindProperty("m_ImgGold").objectReferenceValue = gold;
                so.FindProperty("m_ImgCard").objectReferenceValue = card;
                so.FindProperty("m_TxtCount").objectReferenceValue = count;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, RewardPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        if (rewardType == null)
        {
            InjectScriptYaml(RewardPath);
        }

        return "reward-prefab-ok";
    }

    static string NestOnRow()
    {
        GameObject row = PrefabUtility.LoadPrefabContents(RowPath);
        try
        {
            Transform oldReward = row.transform.Find("Reward");
            Vector2 pos = new Vector2(-480f, 0f);
            Vector2 size = new Vector2(160f, 160f);
            if (oldReward is RectTransform oldRt)
            {
                pos = oldRt.anchoredPosition;
                size = oldRt.sizeDelta;
                UnityEngine.Object.DestroyImmediate(oldReward.gameObject);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RewardPath);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, row.transform);
            inst.name = "Reward";
            var instRt = inst.GetComponent<RectTransform>();
            instRt.anchorMin = instRt.anchorMax = new Vector2(0.5f, 0.5f);
            instRt.pivot = new Vector2(0.5f, 0.5f);
            instRt.anchoredPosition = pos;
            instRt.sizeDelta = size;
            instRt.SetSiblingIndex(1);

            MonoBehaviour rowItem = FindNamed(row, "GiftRewardRowItem");
            if (rowItem != null)
            {
                SerializedObject so = new SerializedObject(rowItem);
                SerializedProperty rewardProp = so.FindProperty("m_Reward");
                if (rewardProp != null)
                {
                    rewardProp.objectReferenceValue = inst.GetComponent("GiftRewardItem");
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(row, RowPath);
            return "row-nested";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(row);
        }
    }

    static Image CreateGold(Transform parent)
    {
        var go = new GameObject("Img_gold", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        var image = go.GetComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(GoldSpriteGuid));
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    static RawImage CreateCard(Transform parent)
    {
        var go = new GameObject("Img_card", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(parent, false);
        go.SetActive(false);
        Stretch(go.GetComponent<RectTransform>());
        var image = go.GetComponent<RawImage>();
        image.raycastTarget = false;
        return image;
    }

    static TMP_Text CreateCount(Transform parent)
    {
        var go = new GameObject("Txt_count", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-4f, 4f);
        rt.sizeDelta = new Vector2(88f, 36f);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(FontGuid));
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.raycastTarget = false;
        tmp.text = string.Empty;
        return tmp;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void InjectScriptYaml(string prefabPath)
    {
        string text = File.ReadAllText(prefabPath);
        if (text.Contains(ScriptGuid))
        {
            return;
        }

        const string fileId = "6128493750182947163";
        text = text.Replace(
            "  m_Component:\n  - component: {fileID: ",
            "  m_Component:\n  - component: {fileID: " + fileId + "}\n  - component: {fileID: ");

        int rootEnd = text.IndexOf("m_Name: GiftRewardItem", StringComparison.Ordinal);
        if (rootEnd < 0)
        {
            throw new InvalidOperationException("找不到 GiftRewardItem 根节点");
        }

        string goldId = ExtractFileId(text, "m_Name: Img_gold", "UnityEngine.UI.Image");
        string cardId = ExtractFileId(text, "m_Name: Img_card", "UnityEngine.UI.RawImage");
        string countId = ExtractFileId(text, "m_Name: Txt_count", "TMPro.TextMeshProUGUI");
        string block =
            "\n--- !u!114 &" + fileId + "\n" +
            "MonoBehaviour:\n" +
            "  m_ObjectHideFlags: 0\n" +
            "  m_CorrespondingSourceObject: {fileID: 0}\n" +
            "  m_PrefabInstance: {fileID: 0}\n" +
            "  m_PrefabAsset: {fileID: 0}\n" +
            "  m_GameObject: {fileID: " + ExtractRootGoId(text) + "}\n" +
            "  m_Enabled: 1\n" +
            "  m_EditorHideFlags: 0\n" +
            "  m_Script: {fileID: 11500000, guid: " + ScriptGuid + ", type: 3}\n" +
            "  m_Name: \n" +
            "  m_EditorClassIdentifier: HotUpdate::GiftRewardItem\n" +
            "  m_ImgGold: {fileID: " + goldId + "}\n" +
            "  m_ImgCard: {fileID: " + cardId + "}\n" +
            "  m_TxtCount: {fileID: " + countId + "}\n";
        File.WriteAllText(prefabPath, text + block);
        AssetDatabase.ImportAsset(prefabPath);
    }

    static string ExtractRootGoId(string text)
    {
        int name = text.IndexOf("m_Name: GiftRewardItem", StringComparison.Ordinal);
        int go = text.LastIndexOf("--- !u!1 &", name, StringComparison.Ordinal);
        int start = go + "--- !u!1 &".Length;
        int end = text.IndexOf('\n', start);
        return text.Substring(start, end - start).Trim();
    }

    static string ExtractFileId(string text, string nameMarker, string classMarker)
    {
        int name = text.IndexOf(nameMarker, StringComparison.Ordinal);
        int classAt = text.IndexOf(classMarker, name, StringComparison.Ordinal);
        int header = text.LastIndexOf("--- !u!114 &", classAt, StringComparison.Ordinal);
        int start = header + "--- !u!114 &".Length;
        int end = text.IndexOf('\n', start);
        return text.Substring(start, end - start).Trim();
    }

    static void RemoveCatalogNames(params string[] names)
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>(AddressableCatalogSetup.PrefabPath);
        var kept = new List<AddressableEntry<UnityEngine.AddressableAssets.AssetReferenceGameObject>>();
        for (int i = 0; i < catalog.Entries.Count; i++)
        {
            bool drop = false;
            for (int n = 0; n < names.Length; n++)
            {
                if (catalog.Entries[i].assetName == names[n])
                {
                    drop = true;
                    break;
                }
            }

            if (!drop)
            {
                kept.Add(catalog.Entries[i]);
            }
        }

        catalog.EditorSetEntries(kept);
        EditorUtility.SetDirty(catalog);
    }

    static MonoBehaviour FindNamed(GameObject root, string typeName)
    {
        MonoBehaviour[] items = root.GetComponents<MonoBehaviour>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].GetType().Name == typeName)
            {
                return items[i];
            }
        }

        return null;
    }

    static Type FindType(string name)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(name);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}
