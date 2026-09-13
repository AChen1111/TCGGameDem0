using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class SetupCardPickStage
{
    const string StagePath = "Assets/Prefab/Card/CardPickStage.prefab";
    const string WindowPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";
    const string CardPrefabPath = "Assets/Prefab/Card/CardPrefab.prefab";
    const string LayerName = "CardPick";

    public static string Run()
    {
        int layer = LayerMask.NameToLayer(LayerName);
        if (layer < 0)
        {
            throw new Exception("缺少 CardPick 层");
        }

        GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
        if (cardPrefab == null)
        {
            throw new Exception("找不到卡牌预制体: " + CardPrefabPath);
        }

        string stageResult = CreateStage(layer, cardPrefab);
        string windowResult = UpdateWindow();
        AssetDatabase.SaveAssets();
        return stageResult + ";" + windowResult;
    }

    static string CreateStage(int layer, GameObject cardPrefab)
    {
        var root = new GameObject("CardPickStage");
        SetLayerRecursively(root, layer);

        var cameraGo = new GameObject("CardPickCamera");
        cameraGo.transform.SetParent(root.transform, false);
        cameraGo.transform.localPosition = new Vector3(0f, 0f, -2f);
        Camera camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.orthographic = false;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 20f;
        camera.depth = -10f;
        camera.cullingMask = 1 << layer;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.useOcclusionCulling = false;
        camera.enabled = true;
        UniversalAdditionalCameraData cameraData = cameraGo.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
        }

        cameraData.renderType = CameraRenderType.Base;
        cameraData.renderPostProcessing = false;
        cameraData.renderShadows = false;
        SetLayerRecursively(cameraGo, layer);

        var lightGo = new GameObject("CardPickLight");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localRotation = Quaternion.Euler(35f, -30f, 0f);
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.cullingMask = 1 << layer;
        SetLayerRecursively(lightGo, layer);

        var cardRoot = new GameObject("CardPickRoot");
        cardRoot.transform.SetParent(root.transform, false);
        cardRoot.transform.localScale = Vector3.one;
        CardPickController controller = cardRoot.AddComponent<CardPickController>();
        GameObjectHorizontalLayout layout = cardRoot.AddComponent<GameObjectHorizontalLayout>();
        layout.enabled = false;
        SetLayerRecursively(cardRoot, layer);

        var controllerSo = new SerializedObject(controller);
        SetObject(controllerSo, "_gameObjectHorizontalLayout", layout);
        SetObject(controllerSo, "_cardPrefab", cardPrefab);
        SetObject(controllerSo, "_pickCamera", camera);
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        var layoutSo = new SerializedObject(layout);
        layoutSo.FindProperty("_spacing").floatValue = 0.1f;
        layoutSo.FindProperty("_childSize").floatValue = 1f;
        layoutSo.FindProperty("_childAlignment").enumValueIndex = 1;
        layoutSo.FindProperty("_ignoreInactive").boolValue = true;
        layoutSo.FindProperty("_controlChildScale").boolValue = true;
        layoutSo.FindProperty("_childScale").vector3Value = new Vector3(0.6f, 0.6f, 1f);
        layoutSo.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, StagePath);
        UnityEngine.Object.DestroyImmediate(root);
        return "stage-ok";
    }

    static string UpdateWindow()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(WindowPath);
        try
        {
            Transform root = contents.transform;
            Transform oldRoot = root.Find("CardPickRoot");
            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot.gameObject);
            }

            Transform rawTransform = root.Find("Raw_CardPick");
            GameObject rawGo;
            if (rawTransform == null)
            {
                rawGo = new GameObject("Raw_CardPick", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                rawGo.layer = LayerMask.NameToLayer("UI");
                rawGo.transform.SetParent(root, false);
            }
            else
            {
                rawGo = rawTransform.gameObject;
            }

            var rawRect = rawGo.GetComponent<RectTransform>();
            rawRect.anchorMin = Vector2.zero;
            rawRect.anchorMax = Vector2.one;
            rawRect.offsetMin = Vector2.zero;
            rawRect.offsetMax = Vector2.zero;
            rawRect.pivot = new Vector2(0.5f, 0.5f);
            rawRect.localScale = Vector3.one;
            rawRect.localRotation = Quaternion.identity;

            RawImage rawImage = rawGo.GetComponent<RawImage>();
            rawImage.color = Color.white;
            rawImage.raycastTarget = true;
            rawImage.texture = null;

            Transform next = root.Find("Btn_Next");
            rawGo.transform.SetSiblingIndex(next != null ? next.GetSiblingIndex() : root.childCount - 1);

            UiScreenGenerator generator = contents.GetComponent<UiScreenGenerator>();
            if (generator != null)
            {
                var generatorSo = new SerializedObject(generator);
                UiPrefixCollector.WriteBinds(generatorSo, "m_uiBinds", UiPrefixCollector.Collect(root));
                generatorSo.ApplyModifiedPropertiesWithoutUndo();
            }

            CardPickWindow window = contents.GetComponent<CardPickWindow>();
            if (window == null)
            {
                throw new Exception("CardPickWindow 组件缺失");
            }

            GameObject stagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StagePath);
            var windowSo = new SerializedObject(window);
            SerializedProperty rawProp = windowSo.FindProperty("m_RawCardPick");
            SerializedProperty stageProp = windowSo.FindProperty("_stagePrefab");
            if (rawProp == null || stageProp == null)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, WindowPath);
                return "window-hierarchy-ok;fields-pending-compile";
            }

            SetObject(windowSo, "m_RawCardPick", rawImage);
            SetObject(windowSo, "_stagePrefab", stagePrefab);
            SetObject(windowSo, "_cardPickController", null);

            UiPrefixCollector.ApplySerializedFields(windowSo, UiPrefixCollector.Collect(root));
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, WindowPath);
            return "window-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void SetObject(SerializedObject so, string field, UnityEngine.Object value)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        Transform transform = root.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }
}
