using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class PreviewGoldCard
{
    const string RootName = "__GoldPreview";
    const string CardPrefabPath = "Assets/Prefab/Card/CardPrefab.prefab";

    public static string Setup()
    {
        Cleanup();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
        if (prefab == null)
        {
            throw new System.Exception("找不到卡牌预制体");
        }

        var root = new GameObject(RootName);
        root.hideFlags = HideFlags.DontSave;
        root.transform.position = new Vector3(0f, -800f, 0f);

        var lightGo = new GameObject("Light");
        lightGo.hideFlags = HideFlags.DontSave;
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localRotation = Quaternion.Euler(25f, -20f, 0f);
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = Color.white;

        GameObject card = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        card.hideFlags = HideFlags.DontSave;
        card.name = "Card";
        card.transform.SetParent(root.transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;

        CardPickView view = card.GetComponent<CardPickView>();
        if (view == null)
        {
            throw new System.Exception("卡牌缺少 CardPickView");
        }

        var so = new SerializedObject(view);
        so.FindProperty("_cardShaderType").enumValueIndex = (int)CardShaderType.Gold;
        so.ApplyModifiedPropertiesWithoutUndo();
        typeof(CardPickView)
            .GetMethod("ApplyEffect", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(view, new object[] { CardShaderType.Gold });

        foreach (MeshRenderer renderer in card.GetComponentsInChildren<MeshRenderer>())
        {
            Material mat = renderer.sharedMaterial;
            if (mat == null || !mat.HasProperty("_GoldSparkle"))
            {
                continue;
            }

            mat.SetColor("_GoldColor", new Color(1.45f, 1.02f, 0.32f, 1f));
            mat.SetFloat("_GoldSparkle", 0.95f);
            mat.SetFloat("_GoldWidth", 0.028f);
        }

        var cameraGo = new GameObject("GoldPreviewCamera");
        cameraGo.hideFlags = HideFlags.DontSave;
        cameraGo.transform.SetParent(root.transform, false);
        cameraGo.transform.localPosition = new Vector3(0f, 0f, -2.35f);
        Camera camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
        camera.fieldOfView = 36f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 20f;
        camera.depth = -80f;
        camera.enabled = true;

        return "gold-preview-ready";
    }

    public static string Cleanup()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        return "gold-preview-cleared";
    }
}
