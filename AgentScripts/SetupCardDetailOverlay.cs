using System;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupCardDetailOverlay
{
    const string OverlayPath = "Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab";
    const string PreviewWindowPath = "Assets/UI/Prefab/BaseUI/CardPreviewWindow.prefab";
    const string PickWindowPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";
    const string ScenePath = "Assets/Scenes/SceneUIRef.unity";
    const string FontPath = "Assets/UI/Fonts/FZZYJW SDF.asset";
    const string SampleCardPath = "Assets/UI/Card/CardBag01_BlueEyes/71039903.jpg";
    const int UiLayer = 5;
    const string ScriptGuid = "3a7c9e2f14b84c6d9e8f0a1b2c3d4e5f";

    public static string Run()
    {
        GameObject overlay = CreateOverlay();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(overlay, OverlayPath);
        UnityEngine.Object.DestroyImmediate(overlay);
        if (prefab == null)
        {
            throw new Exception("保存 CardDetailOverlay 失败");
        }

        Type viewType = FindType("CardDetailView");
        if (viewType != null)
        {
            AttachView(OverlayPath, viewType);
        }
        else
        {
            InjectView(OverlayPath);
        }

        Embed(PreviewWindowPath);
        Embed(PickWindowPath);
        PlaceInScene(OverlayPath);
        AssetDatabase.SaveAssets();
        return viewType != null ? "card-detail-ok" : "card-detail-ok;script-injected";
    }

    static GameObject CreateOverlay()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var root = new GameObject("CardDetailOverlay", typeof(RectTransform));
        root.layer = UiLayer;
        Stretch(root.GetComponent<RectTransform>());
        root.SetActive(false);

        GameObject dim = Child(root.transform, "Img_Dim", typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Stretch(dim.GetComponent<RectTransform>());
        Image dimImage = dim.GetComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.72f);
        dimImage.raycastTarget = true;
        dim.GetComponent<Button>().targetGraphic = dimImage;

        GameObject card = Child(root.transform, "Raw_Card", typeof(CanvasRenderer), typeof(RawImage));
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.22f, 0.5f);
        cardRect.anchorMax = new Vector2(0.22f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(0f, 24f);
        cardRect.sizeDelta = new Vector2(420f, 612f);
        RawImage raw = card.GetComponent<RawImage>();
        raw.color = Color.white;
        raw.raycastTarget = false;

        GameObject info = Child(root.transform, "Go_Info", typeof(CanvasRenderer), typeof(Image));
        var infoRect = info.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.4f, 0.14f);
        infoRect.anchorMax = new Vector2(0.965f, 0.86f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        info.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.82f);
        info.GetComponent<Image>().raycastTarget = true;

        GameObject nameBase = Child(info.transform, "Img_NameBase", typeof(CanvasRenderer), typeof(Image));
        TopBar(nameBase.GetComponent<RectTransform>(), 56f);
        Image nameImage = nameBase.GetComponent<Image>();
        nameImage.sprite = LoadSprite("Assets/UI/Sprite/Card/GUI_CardInfo_NameBase.png");
        nameImage.type = Image.Type.Sliced;
        nameImage.color = new Color(0.76f, 0.48f, 0.22f, 1f);
        nameImage.raycastTarget = false;

        GameObject nameText = Child(nameBase.transform, "Txt_Name", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Stretch(nameText.GetComponent<RectTransform>(), 20f, 12f, 8f, 8f);
        SetupTmp(nameText.GetComponent<TextMeshProUGUI>(), font, 32f, TextAlignmentOptions.MidlineLeft);
        AddDynamicLoc(nameText);

        GameObject levelRow = Child(info.transform, "Go_LevelRow");
        Below(levelRow.GetComponent<RectTransform>(), -56f, 48f);
        Horizontal(levelRow, 16f, 8f);
        Image levelIcon = Icon(levelRow.transform, "Img_Level", LoadSprite("Assets/UI/Sprite/Duel/GUI_T_Icon1_Other_Level.png"), 36f);
        GameObject levelText = Child(levelRow.transform, "Txt_Level", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Size(levelText.GetComponent<RectTransform>(), 48f, 36f);
        SetupTmp(levelText.GetComponent<TextMeshProUGUI>(), font, 28f, TextAlignmentOptions.MidlineLeft);
        for (int i = 0; i < 4; i++)
        {
            Image typeIcon = Icon(levelRow.transform, "Img_Type" + i, i == 0 ? LoadSprite("Assets/UI/Sprite/Duel/GUI_Icon_Tuner.png") : LoadSprite("Assets/UI/Sprite/Card/GUI_CardType_Effect.png"), 36f);
            typeIcon.gameObject.SetActive(i < 2);
        }

        GameObject statRow = Child(info.transform, "Go_StatRow");
        Below(statRow.GetComponent<RectTransform>(), -104f, 48f);
        Horizontal(statRow, 16f, 20f);
        GameObject atk = Child(statRow.transform, "Go_Atk");
        Size(atk.GetComponent<RectTransform>(), 180f, 40f);
        Horizontal(atk, 0f, 8f);
        Icon(atk.transform, "Img_Atk", LoadSprite("Assets/UI/Sprite/Duel/GUI_Icon_Atk.png"), 36f);
        GameObject atkText = Child(atk.transform, "Txt_Atk", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Size(atkText.GetComponent<RectTransform>(), 120f, 36f);
        SetupTmp(atkText.GetComponent<TextMeshProUGUI>(), font, 28f, TextAlignmentOptions.MidlineLeft, new Color(1f, 0.82f, 0.35f));

        GameObject def = Child(statRow.transform, "Go_Def");
        Size(def.GetComponent<RectTransform>(), 180f, 40f);
        Horizontal(def, 0f, 8f);
        Icon(def.transform, "Img_Def", LoadSprite("Assets/UI/Sprite/Duel/GUI_Icon_Def.png"), 36f);
        GameObject defText = Child(def.transform, "Txt_Def", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Size(defText.GetComponent<RectTransform>(), 120f, 36f);
        SetupTmp(defText.GetComponent<TextMeshProUGUI>(), font, 28f, TextAlignmentOptions.MidlineLeft, new Color(0.7f, 0.85f, 1f));

        GameObject typeBar = Child(info.transform, "Img_TypeBar", typeof(CanvasRenderer), typeof(Image));
        Below(typeBar.GetComponent<RectTransform>(), -152f, 36f);
        typeBar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        typeBar.GetComponent<Image>().raycastTarget = false;
        GameObject typeText = Child(typeBar.transform, "Txt_Type", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Stretch(typeText.GetComponent<RectTransform>(), 16f, 8f, 4f, 4f);
        SetupTmp(typeText.GetComponent<TextMeshProUGUI>(), font, 20f, TextAlignmentOptions.MidlineLeft);

        GameObject scroll = Child(info.transform, "Scr_Desc", typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        var scrollRect = scroll.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(0f, 12f);
        scrollRect.offsetMax = new Vector2(0f, -196f);
        scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        scroll.GetComponent<Image>().raycastTarget = true;
        GameObject viewport = Child(scroll.transform, "Viewport", typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.GetComponent<Image>().raycastTarget = true;
        GameObject desc = Child(viewport.transform, "Txt_Desc", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var descRect = desc.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 1f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.pivot = new Vector2(0.5f, 1f);
        descRect.anchoredPosition = Vector2.zero;
        descRect.sizeDelta = new Vector2(-24f, 0f);
        TextMeshProUGUI descTmp = desc.GetComponent<TextMeshProUGUI>();
        SetupTmp(descTmp, font, 22f, TextAlignmentOptions.TopLeft);
        descTmp.enableWordWrapping = true;
        var fitter = desc.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddDynamicLoc(desc);
        ScrollRect sr = scroll.GetComponent<ScrollRect>();
        sr.content = descRect;
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        Sprite arrow = LoadSprite("Assets/UI/Sprite/Common/GUI_alpha_PagingWindow_Arrow.png");
        CreateArrow(root.transform, "Btn_Prev", arrow, new Vector2(0.04f, 0.5f), false);
        CreateArrow(root.transform, "Btn_Next", arrow, new Vector2(0.96f, 0.5f), true);

        GameObject bank = Child(root.transform, "Go_IconBank");
        bank.SetActive(false);
        Bank(bank.transform, "Img_BankLevel", "Assets/UI/Sprite/Duel/GUI_T_Icon1_Other_Level.png");
        Bank(bank.transform, "Img_BankRank", "Assets/UI/Sprite/Duel/GUI_T_Icon1_Other_Rank.png");
        Bank(bank.transform, "Img_BankLink", "Assets/UI/Sprite/Duel/GUI_T_Icon1_Other_link.png");
        Bank(bank.transform, "Img_BankDef", "Assets/UI/Sprite/Duel/GUI_Icon_Def.png");
        Bank(bank.transform, "Img_BankTuner", "Assets/UI/Sprite/Duel/GUI_Icon_Tuner.png");
        Bank(bank.transform, "Img_BankEffect", "Assets/UI/Sprite/Card/GUI_CardType_Effect.png");
        Bank(bank.transform, "Img_BankFusion", "Assets/UI/Sprite/Card/GUI_CardType_Fusion.png");
        Bank(bank.transform, "Img_BankSynchro", "Assets/UI/Sprite/Card/GUI_CardType_Synchro.png");
        Bank(bank.transform, "Img_BankXyz", "Assets/UI/Sprite/Card/GUI_CardType_Xyz.png");
        Bank(bank.transform, "Img_BankLinkType", "Assets/UI/Sprite/Duel/GUI_Icon_Link.png");
        Bank(bank.transform, "Img_BankSpell", "Assets/UI/Sprite/Card/GUI_CardType_Spell.png");
        Bank(bank.transform, "Img_BankTrap", "Assets/UI/Sprite/Card/GUI_CardType_Trap.png");
        Bank(bank.transform, "Img_BankQuick", "Assets/UI/Sprite/Card/GUI_CardType_Quick.png");
        Bank(bank.transform, "Img_BankContinuous", "Assets/UI/Sprite/Card/GUI_CardType_Continuous.png");
        Bank(bank.transform, "Img_BankEquip", "Assets/UI/Sprite/Card/GUI_CardType_Equip.png");
        Bank(bank.transform, "Img_BankField", "Assets/UI/Sprite/Card/GUI_CardType_Field.png");
        Bank(bank.transform, "Img_BankRitual", "Assets/UI/Sprite/Card/GUI_CardType_Ritual.png");
        Bank(bank.transform, "Img_BankCounter", "Assets/UI/Sprite/Card/GUI_CardType_Counter.png");

        _ = levelIcon;
        return root;
    }

    static void AttachView(string path, Type viewType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Component view = contents.GetComponent(viewType) ?? contents.AddComponent(viewType);
            var so = new SerializedObject(view);
            so.FindProperty("m_BtnDim").objectReferenceValue = Find<Button>(contents.transform, "Img_Dim");
            so.FindProperty("m_RawCard").objectReferenceValue = Find<RawImage>(contents.transform, "Raw_Card");
            so.FindProperty("m_ImgNameBase").objectReferenceValue = Find<Image>(contents.transform, "Img_NameBase");
            so.FindProperty("m_TxtName").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Name");
            so.FindProperty("m_GoLevelRow").objectReferenceValue = FindGo(contents.transform, "Go_LevelRow");
            so.FindProperty("m_ImgLevel").objectReferenceValue = Find<Image>(contents.transform, "Img_Level");
            so.FindProperty("m_TxtLevel").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Level");
            SerializedProperty types = so.FindProperty("m_ImgTypes");
            types.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                types.GetArrayElementAtIndex(i).objectReferenceValue = Find<Image>(contents.transform, "Img_Type" + i);
            }

            so.FindProperty("m_GoStatRow").objectReferenceValue = FindGo(contents.transform, "Go_StatRow");
            so.FindProperty("m_GoAtk").objectReferenceValue = FindGo(contents.transform, "Go_Atk");
            so.FindProperty("m_TxtAtk").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Atk");
            so.FindProperty("m_GoDef").objectReferenceValue = FindGo(contents.transform, "Go_Def");
            so.FindProperty("m_ImgDef").objectReferenceValue = Find<Image>(contents.transform, "Img_Def");
            so.FindProperty("m_TxtDef").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Def");
            so.FindProperty("m_TxtType").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Type");
            so.FindProperty("m_TxtDesc").objectReferenceValue = Find<TextMeshProUGUI>(contents.transform, "Txt_Desc");
            so.FindProperty("m_BtnPrev").objectReferenceValue = Find<Button>(contents.transform, "Btn_Prev");
            so.FindProperty("m_BtnNext").objectReferenceValue = Find<Button>(contents.transform, "Btn_Next");
            so.FindProperty("m_SpriteLevel").objectReferenceValue = Find<Image>(contents.transform, "Img_BankLevel").sprite;
            so.FindProperty("m_SpriteRank").objectReferenceValue = Find<Image>(contents.transform, "Img_BankRank").sprite;
            so.FindProperty("m_SpriteLink").objectReferenceValue = Find<Image>(contents.transform, "Img_BankLink").sprite;
            so.FindProperty("m_SpriteDef").objectReferenceValue = Find<Image>(contents.transform, "Img_BankDef").sprite;
            so.FindProperty("m_SpriteTuner").objectReferenceValue = Find<Image>(contents.transform, "Img_BankTuner").sprite;
            so.FindProperty("m_SpriteEffect").objectReferenceValue = Find<Image>(contents.transform, "Img_BankEffect").sprite;
            so.FindProperty("m_SpriteFusion").objectReferenceValue = Find<Image>(contents.transform, "Img_BankFusion").sprite;
            so.FindProperty("m_SpriteSynchro").objectReferenceValue = Find<Image>(contents.transform, "Img_BankSynchro").sprite;
            so.FindProperty("m_SpriteXyz").objectReferenceValue = Find<Image>(contents.transform, "Img_BankXyz").sprite;
            so.FindProperty("m_SpriteLinkType").objectReferenceValue = Find<Image>(contents.transform, "Img_BankLinkType").sprite;
            so.FindProperty("m_SpriteSpell").objectReferenceValue = Find<Image>(contents.transform, "Img_BankSpell").sprite;
            so.FindProperty("m_SpriteTrap").objectReferenceValue = Find<Image>(contents.transform, "Img_BankTrap").sprite;
            so.FindProperty("m_SpriteQuick").objectReferenceValue = Find<Image>(contents.transform, "Img_BankQuick").sprite;
            so.FindProperty("m_SpriteContinuous").objectReferenceValue = Find<Image>(contents.transform, "Img_BankContinuous").sprite;
            so.FindProperty("m_SpriteEquip").objectReferenceValue = Find<Image>(contents.transform, "Img_BankEquip").sprite;
            so.FindProperty("m_SpriteField").objectReferenceValue = Find<Image>(contents.transform, "Img_BankField").sprite;
            so.FindProperty("m_SpriteRitual").objectReferenceValue = Find<Image>(contents.transform, "Img_BankRitual").sprite;
            so.FindProperty("m_SpriteCounter").objectReferenceValue = Find<Image>(contents.transform, "Img_BankCounter").sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void InjectView(string path)
    {
        string yaml = System.IO.File.ReadAllText(path);
        if (yaml.Contains("HotUpdate::CardDetailView"))
        {
            return;
        }

        long rootId = FindYamlFileId(yaml, "m_Name: CardDetailOverlay");
        if (rootId == 0)
        {
            throw new Exception("注入详情脚本失败: 找不到根物体");
        }

        long componentId = 8123456789012345678;
        yaml = AddComponentToGameObject(yaml, rootId, componentId);
        yaml +=
            "\n--- !u!114 &" + componentId + "\n" +
            "MonoBehaviour:\n" +
            "  m_ObjectHideFlags: 0\n" +
            "  m_CorrespondingSourceObject: {fileID: 0}\n" +
            "  m_PrefabInstance: {fileID: 0}\n" +
            "  m_PrefabAsset: {fileID: 0}\n" +
            "  m_GameObject: {fileID: " + rootId + "}\n" +
            "  m_Enabled: 1\n" +
            "  m_EditorHideFlags: 0\n" +
            "  m_Script: {fileID: 11500000, guid: " + ScriptGuid + ", type: 3}\n" +
            "  m_Name: \n" +
            "  m_EditorClassIdentifier: HotUpdate::CardDetailView\n";
        System.IO.File.WriteAllText(path, yaml);
        AssetDatabase.ImportAsset(path);
    }

    static void Embed(string windowPath)
    {
        GameObject overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPath);
        GameObject contents = PrefabUtility.LoadPrefabContents(windowPath);
        try
        {
            Transform existing = contents.transform.Find("CardDetailOverlay");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(overlayPrefab, contents.transform);
            instance.name = "CardDetailOverlay";
            instance.SetActive(false);
            Stretch(instance.GetComponent<RectTransform>());
            instance.transform.SetAsLastSibling();
            PrefabUtility.SaveAsPrefabAsset(contents, windowPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void PlaceInScene(string overlayPath)
    {
        var scene = default(UnityEngine.SceneManagement.Scene);
        bool opened = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var openedScene = EditorSceneManager.GetSceneAt(i);
            if (openedScene.path == ScenePath)
            {
                scene = openedScene;
                break;
            }
        }

        if (!scene.IsValid())
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            opened = true;
        }

        try
        {
            Canvas canvas = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                canvas = roots[i].GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                {
                    break;
                }
            }

            if (canvas == null)
            {
                return;
            }

            Transform old = canvas.transform.Find("CardDetailOverlay");
            if (old != null)
            {
                UnityEngine.Object.DestroyImmediate(old.gameObject);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(overlayPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            instance.name = "CardDetailOverlay";
            instance.SetActive(true);
            Stretch(instance.GetComponent<RectTransform>());
            Texture sample = AssetDatabase.LoadAssetAtPath<Texture>(SampleCardPath);
            RawImage raw = Find<RawImage>(instance.transform, "Raw_Card");
            if (raw != null && sample != null)
            {
                raw.texture = sample;
                raw.enabled = true;
            }

            TextMeshProUGUI name = Find<TextMeshProUGUI>(instance.transform, "Txt_Name");
            if (name != null)
            {
                name.text = "太古的白石";
            }

            TextMeshProUGUI level = Find<TextMeshProUGUI>(instance.transform, "Txt_Level");
            if (level != null)
            {
                level.text = "1";
            }

            TextMeshProUGUI atk = Find<TextMeshProUGUI>(instance.transform, "Txt_Atk");
            if (atk != null)
            {
                atk.text = "600";
            }

            TextMeshProUGUI def = Find<TextMeshProUGUI>(instance.transform, "Txt_Def");
            if (def != null)
            {
                def.text = "500";
            }

            TextMeshProUGUI type = Find<TextMeshProUGUI>(instance.transform, "Txt_Type");
            if (type != null)
            {
                type.text = "【龙族/调整/效果】【71039903】";
            }

            TextMeshProUGUI desc = Find<TextMeshProUGUI>(instance.transform, "Txt_Desc");
            if (desc != null)
            {
                desc.text = "这个卡名的②的效果1回合只能使用1次。\n①：这张卡被送去墓地的回合的结束阶段才能发动。从卡组把1只「青眼」怪兽特殊召唤。\n②：把墓地的这张卡除外，以自己墓地1只「青眼」怪兽为对象才能发动。那只怪兽加入手卡。";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        finally
        {
            if (opened)
            {
                EditorSceneManager.CloseScene(scene, false);
            }
        }
    }

    static void CreateArrow(Transform parent, string name, Sprite sprite, Vector2 anchor, bool flip)
    {
        GameObject go = Child(parent, name, typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(72f, 120f);
        if (flip)
        {
            rect.localScale = new Vector3(-1f, 1f, 1f);
        }

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = true;
        go.GetComponent<Button>().targetGraphic = image;
    }

    static void Bank(Transform parent, string name, string spritePath)
    {
        Icon(parent, name, LoadSprite(spritePath), 32f);
    }

    static Image Icon(Transform parent, string name, Sprite sprite, float size)
    {
        GameObject go = Child(parent, name, typeof(CanvasRenderer), typeof(Image));
        Size(go.GetComponent<RectTransform>(), size, size);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    static void AddDynamicLoc(GameObject go)
    {
        Type type = FindType("LocalizedText");
        if (type == null || go.GetComponent(type) != null)
        {
            return;
        }

        Component loc = go.AddComponent(type);
        var so = new SerializedObject(loc);
        so.FindProperty("dynamicContent").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetupTmp(TextMeshProUGUI tmp, TMP_FontAsset font, float size, TextAlignmentOptions align)
    {
        SetupTmp(tmp, font, size, align, Color.white);
    }

    static void SetupTmp(TextMeshProUGUI tmp, TMP_FontAsset font, float size, TextAlignmentOptions align, Color color)
    {
        if (font != null)
        {
            tmp.font = font;
            tmp.fontSharedMaterial = font.material;
        }

        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    static GameObject Child(Transform parent, string name, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = UiLayer;
        go.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++)
        {
            if (go.GetComponent(components[i]) == null)
            {
                go.AddComponent(components[i]);
            }
        }

        return go;
    }

    static void Stretch(RectTransform rect)
    {
        Stretch(rect, 0f, 0f, 0f, 0f);
    }

    static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    static void TopBar(RectTransform rect, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
    }

    static void Below(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(0f, height);
    }

    static void Size(RectTransform rect, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
    }

    static void Horizontal(GameObject go, float pad, float spacing)
    {
        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset((int)pad, (int)pad, 4, 4);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
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

    static Type FindType(string name)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
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

    static long FindYamlFileId(string yaml, string marker)
    {
        int markerIndex = yaml.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return 0;
        }

        int start = yaml.LastIndexOf("--- !u!", markerIndex, StringComparison.Ordinal);
        int amp = yaml.IndexOf('&', start);
        int end = yaml.IndexOf('\n', amp);
        return long.Parse(yaml.Substring(amp + 1, end - amp - 1).Trim());
    }

    static string AddComponentToGameObject(string yaml, long rootId, long componentId)
    {
        string idLine = "&" + rootId;
        int start = yaml.IndexOf(idLine, StringComparison.Ordinal);
        int components = yaml.IndexOf("m_Component:", start, StringComparison.Ordinal);
        int layer = yaml.IndexOf("m_Layer:", components, StringComparison.Ordinal);
        return yaml.Insert(layer, "  - component: {fileID: " + componentId + "}\n");
    }
}
