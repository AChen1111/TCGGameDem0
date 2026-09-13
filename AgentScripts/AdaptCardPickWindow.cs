using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AdaptCardPickWindow
{
    const string WindowPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";

    public static string Run()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(WindowPath);
        try
        {
            Transform root = contents.transform;
            EnsureDimmer(root);
            AdaptBackground(root);
            AdaptNextButton(root);

            UiScreenGenerator generator = contents.GetComponent<UiScreenGenerator>();
            if (generator != null)
            {
                var generatorSo = new SerializedObject(generator);
                UiPrefixCollector.WriteBinds(generatorSo, "m_uiBinds", UiPrefixCollector.Collect(root));
                generatorSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(contents, WindowPath);
            AssetDatabase.SaveAssets();
            return "adapt-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void EnsureDimmer(Transform root)
    {
        Transform existing = root.Find("Img_Dim");
        GameObject dimGo = existing != null
            ? existing.gameObject
            : new GameObject("Img_Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dimGo.layer = LayerMask.NameToLayer("UI");
        dimGo.transform.SetParent(root, false);
        dimGo.transform.SetAsFirstSibling();

        var rect = dimGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Image image = dimGo.GetComponent<Image>();
        image.color = new Color(0.04f, 0.04f, 0.06f, 1f);
        image.raycastTarget = true;
    }

    static void AdaptBackground(Transform root)
    {
        Transform bg = root.Find("bg");
        if (bg == null)
        {
            throw new System.Exception("找不到 bg");
        }

        var rect = bg.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(950f, 628f);
        rect.localScale = Vector3.one;

        AspectRatioFitter fitter = bg.GetComponent<AspectRatioFitter>();
        if (fitter == null)
        {
            fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
        }

        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = 950f / 628f;
        bg.SetSiblingIndex(1);
    }

    static void AdaptNextButton(Transform root)
    {
        Transform next = root.Find("Btn_Next");
        if (next == null)
        {
            return;
        }

        var rect = next.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 72f);
        rect.sizeDelta = new Vector2(160f, 30f);
        next.SetAsLastSibling();
    }
}
