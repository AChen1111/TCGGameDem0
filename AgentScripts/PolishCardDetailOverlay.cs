using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PolishCardDetailOverlay
{
    const string OverlayPath = "Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab";
    const string PreviewWindowPath = "Assets/UI/Prefab/BaseUI/CardPreviewWindow.prefab";
    const string PickWindowPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";
    const string ScenePath = "Assets/Scenes/SceneUIRef.unity";
    const string FontPath = "Assets/UI/Fonts/FZZYJW SDF.asset";
    const string TranslationCsv = "TableData/Localization/Translations.csv";
    const int UiLayer = 5;

    public static string Run()
    {
        string fontReport = BakeFont();
        GameObject contents = PrefabUtility.LoadPrefabContents(OverlayPath);
        try
        {
            Tune(contents.transform);
            WireView(contents.GetComponent<CardDetailView>());
            PrefabUtility.SaveAsPrefabAsset(contents, OverlayPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        WireHost(PreviewWindowPath);
        WireHost(PickWindowPath);
        PlaceInScene();
        AssetDatabase.SaveAssets();
        return "polish-ok;" + fontReport;
    }

    static string BakeFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            return "font-missing";
        }

        font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        font.isMultiAtlasTexturesEnabled = true;
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, TranslationCsv);
        var chars = new HashSet<char>();
        const string extra = "【】「」『』（）〔〕、。：；？！—…·0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/? ";
        foreach (char c in extra)
        {
            chars.Add(c);
        }

        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
            {
                foreach (char c in line)
                {
                    chars.Add(c);
                }
            }
        }

        var builder = new StringBuilder(chars.Count);
        foreach (char c in chars)
        {
            if (!char.IsControl(c))
            {
                builder.Append(c);
            }
        }

        bool added = font.TryAddCharacters(builder.ToString(), out string missing);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        int missingCount = string.IsNullOrEmpty(missing) ? 0 : missing.Length;
        return "font-added=" + added + ";missing=" + missingCount + ";chars=" + builder.Length;
    }

    static void Tune(Transform root)
    {
        Image dim = Find<Image>(root, "Img_Dim");
        if (dim != null)
        {
            dim.color = new Color(0f, 0f, 0f, 0.62f);
            dim.raycastTarget = true;
        }

        RawImage raw = Find<RawImage>(root, "Raw_Card");
        if (raw != null)
        {
            raw.raycastTarget = true;
            var rect = raw.rectTransform;
            rect.anchorMin = new Vector2(0.205f, 0.5f);
            rect.anchorMax = new Vector2(0.205f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 18f);
            rect.sizeDelta = new Vector2(448f, 654f);
        }

        Image info = Find<Image>(root, "Go_Info");
        if (info != null)
        {
            info.color = new Color(0.02f, 0.04f, 0.08f, 0.18f);
            info.raycastTarget = true;
            var rect = info.rectTransform;
            rect.anchorMin = new Vector2(0.42f, 0.16f);
            rect.anchorMax = new Vector2(0.97f, 0.84f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        PaintRow(root, "Go_LevelRow", new Color(0f, 0f, 0f, 0.5f));
        PaintRow(root, "Go_StatRow", new Color(0f, 0f, 0f, 0.5f));
        Image typeBar = Find<Image>(root, "Img_TypeBar");
        if (typeBar != null)
        {
            typeBar.color = new Color(0f, 0f, 0f, 0.72f);
        }

        EnsureAttr(root);
        EnsureAttrBank(root);
        FixLinkTypeSprite(root);

        foreach (TextMeshProUGUI tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.isOrthographic = true;
        }

        TextMeshProUGUI name = Find<TextMeshProUGUI>(root, "Txt_Name");
        if (name != null)
        {
            name.overflowMode = TextOverflowModes.Ellipsis;
            name.enableWordWrapping = false;
            var nameRect = name.rectTransform;
            nameRect.offsetMin = new Vector2(12f, 8f);
            nameRect.offsetMax = new Vector2(-52f, -8f);
        }

        TextMeshProUGUI desc = Find<TextMeshProUGUI>(root, "Txt_Desc");
        if (desc != null)
        {
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Overflow;
        }

        MoveArrow(root, "Btn_Prev", new Vector2(0.03f, 0.5f));
        MoveArrow(root, "Btn_Next", new Vector2(0.97f, 0.5f));
    }

    static void EnsureAttr(Transform root)
    {
        Transform nameBase = FindDeep(root, "Img_NameBase");
        if (nameBase == null)
        {
            return;
        }

        Transform attr = nameBase.Find("Img_Attr");
        Image image;
        if (attr == null)
        {
            var go = new GameObject("Img_Attr", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = UiLayer;
            go.transform.SetParent(nameBase, false);
            image = go.GetComponent<Image>();
        }
        else
        {
            image = attr.GetComponent<Image>();
        }

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-8f, 0f);
        rect.sizeDelta = new Vector2(40f, 40f);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        Sprite light = LoadSprite("Assets/UI/Sprite/Card/GUI_CardAttr_Light.png");
        if (light != null)
        {
            image.sprite = light;
        }
    }

    static void EnsureAttrBank(Transform root)
    {
        Transform bank = FindDeep(root, "Go_IconBank");
        if (bank == null)
        {
            return;
        }

        Bank(bank, "Img_BankAttrLight", "Assets/UI/Sprite/Card/GUI_CardAttr_Light.png");
        Bank(bank, "Img_BankAttrDark", "Assets/UI/Sprite/Card/GUI_CardAttr_Dark.png");
        Bank(bank, "Img_BankAttrFire", "Assets/UI/Sprite/Card/GUI_CardAttr_Fire.png");
        Bank(bank, "Img_BankAttrWater", "Assets/UI/Sprite/Card/GUI_CardAttr_Water.png");
        Bank(bank, "Img_BankAttrWind", "Assets/UI/Sprite/Card/GUI_CardAttr_Wind.png");
        Bank(bank, "Img_BankAttrEarth", "Assets/UI/Sprite/Card/GUI_CardAttr_Earth.png");
        Bank(bank, "Img_BankAttrDivine", "Assets/UI/Sprite/Card/GUI_CardAttr_Divine.png");
        Bank(bank, "Img_BankLinkType", "Assets/UI/Sprite/Card/GUI_CardType_Link.png");
    }

    static void FixLinkTypeSprite(Transform root)
    {
        Image linkType = Find<Image>(root, "Img_BankLinkType");
        Sprite sprite = LoadSprite("Assets/UI/Sprite/Card/GUI_CardType_Link.png");
        if (linkType != null && sprite != null)
        {
            linkType.sprite = sprite;
        }
    }

    static void Bank(Transform parent, string name, string spritePath)
    {
        Transform existing = parent.Find(name);
        Image image;
        if (existing == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = UiLayer;
            go.transform.SetParent(parent, false);
            image = go.GetComponent<Image>();
        }
        else
        {
            image = existing.GetComponent<Image>();
        }

        image.rectTransform.sizeDelta = new Vector2(32f, 32f);
        image.sprite = LoadSprite(spritePath);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    static void WireView(CardDetailView view)
    {
        if (view == null)
        {
            return;
        }

        Transform root = view.transform;
        var so = new SerializedObject(view);
        SetRef(so, "m_BtnDim", Find<Button>(root, "Img_Dim"));
        SetRef(so, "m_RawCard", Find<RawImage>(root, "Raw_Card"));
        SetRef(so, "m_ImgNameBase", Find<Image>(root, "Img_NameBase"));
        SetRef(so, "m_TxtName", Find<TextMeshProUGUI>(root, "Txt_Name"));
        SetRef(so, "m_ImgAttr", Find<Image>(root, "Img_Attr"));
        SetGo(so, "m_GoLevelRow", FindGo(root, "Go_LevelRow"));
        SetRef(so, "m_ImgLevel", Find<Image>(root, "Img_Level"));
        SetRef(so, "m_TxtLevel", Find<TextMeshProUGUI>(root, "Txt_Level"));
        SetGo(so, "m_GoStatRow", FindGo(root, "Go_StatRow"));
        SetGo(so, "m_GoAtk", FindGo(root, "Go_Atk"));
        SetRef(so, "m_TxtAtk", Find<TextMeshProUGUI>(root, "Txt_Atk"));
        SetGo(so, "m_GoDef", FindGo(root, "Go_Def"));
        SetRef(so, "m_ImgDef", Find<Image>(root, "Img_Def"));
        SetRef(so, "m_TxtDef", Find<TextMeshProUGUI>(root, "Txt_Def"));
        SetRef(so, "m_TxtType", Find<TextMeshProUGUI>(root, "Txt_Type"));
        SetRef(so, "m_TxtDesc", Find<TextMeshProUGUI>(root, "Txt_Desc"));
        SetRef(so, "m_BtnPrev", Find<Button>(root, "Btn_Prev"));
        SetRef(so, "m_BtnNext", Find<Button>(root, "Btn_Next"));
        SetSprite(so, "m_SpriteAttrLight", "Assets/UI/Sprite/Card/GUI_CardAttr_Light.png");
        SetSprite(so, "m_SpriteAttrDark", "Assets/UI/Sprite/Card/GUI_CardAttr_Dark.png");
        SetSprite(so, "m_SpriteAttrFire", "Assets/UI/Sprite/Card/GUI_CardAttr_Fire.png");
        SetSprite(so, "m_SpriteAttrWater", "Assets/UI/Sprite/Card/GUI_CardAttr_Water.png");
        SetSprite(so, "m_SpriteAttrWind", "Assets/UI/Sprite/Card/GUI_CardAttr_Wind.png");
        SetSprite(so, "m_SpriteAttrEarth", "Assets/UI/Sprite/Card/GUI_CardAttr_Earth.png");
        SetSprite(so, "m_SpriteAttrDivine", "Assets/UI/Sprite/Card/GUI_CardAttr_Divine.png");
        SetSprite(so, "m_SpriteLinkType", "Assets/UI/Sprite/Card/GUI_CardType_Link.png");
        SerializedProperty types = so.FindProperty("m_ImgTypes");
        if (types != null)
        {
            types.arraySize = 4;
            types.GetArrayElementAtIndex(0).objectReferenceValue = Find<Image>(root, "Img_Type0");
            types.GetArrayElementAtIndex(1).objectReferenceValue = Find<Image>(root, "Img_Type1");
            types.GetArrayElementAtIndex(2).objectReferenceValue = Find<Image>(root, "Img_Type2");
            types.GetArrayElementAtIndex(3).objectReferenceValue = Find<Image>(root, "Img_Type3");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireHost(string windowPath)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(windowPath);
        try
        {
            CardDetailView detail = contents.GetComponentInChildren<CardDetailView>(true);
            Component window = (Component)contents.GetComponent<CardPreviewWindow>()
                ?? contents.GetComponent<CardPickWindow>();
            if (detail != null && window != null)
            {
                var so = new SerializedObject(window);
                SerializedProperty prop = so.FindProperty("m_CardDetail");
                if (prop != null)
                {
                    prop.objectReferenceValue = detail;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(contents, windowPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void PlaceInScene()
    {
        var scene = default(UnityEngine.SceneManagement.Scene);
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var opened = EditorSceneManager.GetSceneAt(i);
            if (opened.path == ScenePath)
            {
                scene = opened;
                break;
            }
        }

        if (!scene.IsValid())
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        Canvas canvas = null;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Canvas found = roots[i].GetComponent<Canvas>();
            if (found != null && found.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas = found;
            }
        }

        if (canvas == null)
        {
            return;
        }

        CardDetailView[] views = canvas.GetComponentsInChildren<CardDetailView>(true);
        for (int i = 0; i < views.Length; i++)
        {
            if (IsHosted(views[i]))
            {
                continue;
            }

            UnityEngine.Object.DestroyImmediate(views[i].gameObject);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
        instance.name = "CardDetailOverlay";
        instance.SetActive(true);
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        instance.transform.SetAsLastSibling();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static bool IsHosted(CardDetailView view)
    {
        return view != null
            && (view.GetComponentInParent<CardPickWindow>(true) != null
                || view.GetComponentInParent<CardPreviewWindow>(true) != null);
    }

    static void PaintRow(Transform root, string name, Color color)
    {
        Transform row = FindDeep(root, name);
        if (row == null)
        {
            return;
        }

        Image image = row.GetComponent<Image>();
        if (image == null)
        {
            row.gameObject.AddComponent<CanvasRenderer>();
            image = row.gameObject.AddComponent<Image>();
        }

        image.color = color;
        image.raycastTarget = false;
    }

    static void MoveArrow(Transform root, string name, Vector2 anchor)
    {
        Transform arrow = FindDeep(root, name);
        if (arrow == null)
        {
            return;
        }

        var rect = arrow.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
    }

    static void SetRef(SerializedObject so, string field, UnityEngine.Object value)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    static void SetGo(SerializedObject so, string field, GameObject value)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    static void SetSprite(SerializedObject so, string field, string spritePath)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop != null)
        {
            prop.objectReferenceValue = LoadSprite(spritePath);
        }
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static T Find<T>(Transform root, string name) where T : Component
    {
        Transform child = FindDeep(root, name);
        return child != null ? child.GetComponent<T>() : null;
    }

    static GameObject FindGo(Transform root, string name)
    {
        Transform child = FindDeep(root, name);
        return child != null ? child.gameObject : null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
            {
                return all[i];
            }
        }

        return null;
    }
}
