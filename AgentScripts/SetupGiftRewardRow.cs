using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SetupGiftRewardRow
{
    const string Path = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftApplyRowPrefab.prefab";

    public static string Run()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(Path);
        try
        {
            var icon = Require(root.transform, "Img_icon").GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            Transform reward = root.transform.Find("Reward");
            if (reward == null)
            {
                var go = new GameObject("Reward", typeof(RectTransform));
                go.transform.SetParent(root.transform, false);
                reward = go.transform;
            }

            var rewardRt = reward.GetComponent<RectTransform>();
            rewardRt.anchorMin = new Vector2(0.5f, 0.5f);
            rewardRt.anchorMax = new Vector2(0.5f, 0.5f);
            rewardRt.pivot = new Vector2(0.5f, 0.5f);
            rewardRt.anchoredPosition = new Vector2(-480f, 0f);
            rewardRt.sizeDelta = new Vector2(160f, 160f);

            icon.transform.SetParent(reward, false);
            StretchFill(icon.rectTransform);

            RawImage card = EnsureCard(reward);
            TMP_Text count = EnsureCount(reward, root.transform);
            TMP_Text name = Require(root.transform, "Txt_name").GetComponent<TMP_Text>();
            var nameRt = name.rectTransform;
            nameRt.anchoredPosition = new Vector2(-40f, 0f);
            nameRt.sizeDelta = new Vector2(520f, 80f);
            name.alignment = TextAlignmentOptions.Left;
            name.fontSize = 40f;
            name.overflowMode = TextOverflowModes.Ellipsis;

            Transform rewards = root.transform.Find("Lay_Rewards");
            if (rewards != null)
            {
                rewards.gameObject.SetActive(false);
            }

            var row = root.GetComponent<GiftRewardRowItem>();
            SerializedObject so = new SerializedObject(row);
            SerializedProperty imgCard = so.FindProperty("m_ImgCard");
            SerializedProperty txtCount = so.FindProperty("m_TxtCount");
            if (imgCard == null || txtCount == null)
            {
                PrefabUtility.SaveAsPrefabAsset(root, Path);
                throw new System.Exception("GiftRewardRowItem 尚未编译出新字段,请稍后重跑");
            }

            so.FindProperty("m_ImgIcon").objectReferenceValue = icon;
            imgCard.objectReferenceValue = card;
            txtCount.objectReferenceValue = count;
            so.FindProperty("m_TxtName").objectReferenceValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, Path);
            return "gift-row-reward-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static RawImage EnsureCard(Transform reward)
    {
        Transform existing = reward.Find("Img_card");
        RawImage image = existing != null
            ? existing.GetComponent<RawImage>()
            : CreateChild(reward, "Img_card", typeof(RawImage)).GetComponent<RawImage>();
        image.raycastTarget = false;
        StretchFill(image.rectTransform);
        image.gameObject.SetActive(false);
        return image;
    }

    static TMP_Text EnsureCount(Transform reward, Transform sampleRoot)
    {
        Transform existing = reward.Find("Txt_count");
        TMP_Text tmp = existing != null
            ? existing.GetComponent<TMP_Text>()
            : CreateChild(reward, "Txt_count", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-4f, 4f);
        rt.sizeDelta = new Vector2(88f, 36f);
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.fontSize = 28f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        TMP_Text sample = sampleRoot.GetComponentInChildren<TMP_Text>(true);
        if (sample != null && sample != tmp)
        {
            tmp.font = sample.font;
        }

        return tmp;
    }

    static GameObject CreateChild(Transform parent, string name, params System.Type[] types)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        for (int i = 0; i < types.Length; i++)
        {
            if (go.GetComponent(types[i]) == null)
            {
                go.AddComponent(types[i]);
            }
        }

        return go;
    }

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    static Transform Require(Transform root, string name)
    {
        Transform child = root.Find(name);
        if (child != null) return child;
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
            {
                return all[i];
            }
        }

        throw new System.Exception("找不到 " + name);
    }
}
