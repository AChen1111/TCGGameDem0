using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PreviewHeaderIconLabels
{
    const string ScenePath = "Assets/Scenes/SceneUIRef.unity";
    const string PanelPath = "Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";
    const string PreviewName = "HeaderLabelPreview";
    const float FontSize = 20f;

    static readonly string[] ButtonNames = { "Btn_Gift", "Btn_Friend", "Btn_Mail", "Btn_Setting" };
    static readonly string[] Chinese = { "礼包", "好友", "邮件", "设置" };
    static readonly string[] English = { "Gift Pack", "Friends", "Mail", "Settings" };

    public static string Run(string language)
    {
        EnsureScene();
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            return "missing-canvas";
        }

        Transform canvasTf = canvas.transform;
        for (int i = 0; i < canvasTf.childCount; i++)
        {
            Transform child = canvasTf.GetChild(i);
            child.gameObject.SetActive(child.name == PreviewName);
        }

        GameObject preview = GameObject.Find(PreviewName);
        if (preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath);
        preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasTf);
        preview.name = PreviewName;

        var rect = preview.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        preview.SetActive(true);
        FlattenZ(preview.transform);

        Transform hud = FindDeep(preview.transform, "RightUpHud");
        if (hud != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)hud);
        }

        Transform rigUp = FindDeep(preview.transform, "RigUp");
        if (rigUp != null)
        {
            var rigRect = (RectTransform)rigUp;
            Vector2 pos = rigRect.anchoredPosition;
            pos.x -= 150f;
            rigRect.anchoredPosition = pos;
        }

        bool english = language == "en";
        string[] labels = english ? English : Chinese;
        var report = new System.Text.StringBuilder();
        report.Append(english ? "en" : "zh");
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        for (int i = 0; i < ButtonNames.Length; i++)
        {
            Transform button = FindDeep(preview.transform, ButtonNames[i]);
            if (button == null)
            {
                report.Append(";missing=").Append(ButtonNames[i]);
                continue;
            }

            Transform desc = button.Find("desc");
            var text = desc.GetComponent<TextMeshProUGUI>();
            var localized = desc.GetComponent<LocalizedText>();
            if (localized != null)
            {
                localized.enabled = false;
            }

            var textRectTf = text.rectTransform;
            textRectTf.sizeDelta = new Vector2(0f, 36f);
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSize;
            text.fontSizeMax = FontSize;
            text.fontSize = FontSize;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.text = labels[i];
            text.ForceMeshUpdate();
            var btnRect = button.GetComponent<RectTransform>().rect;
            var textRect = text.rectTransform.rect;
            report.Append(';').Append(ButtonNames[i])
                .Append(" btn=").Append(btnRect.width.ToString("0.0")).Append('x').Append(btnRect.height.ToString("0.0"))
                .Append(" txt=").Append(textRect.width.ToString("0.0")).Append('x').Append(textRect.height.ToString("0.0"))
                .Append(" size=").Append(text.fontSize.ToString("0.00"))
                .Append(" pref=").Append(text.preferredWidth.ToString("0.0"))
                .Append(" '").Append(text.text).Append('\'');
        }

        Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && cam != null)
        {
            Vector3 look = cam.transform.position + cam.transform.forward * canvas.planeDistance;
            sceneView.LookAt(look, cam.transform.rotation, 10f, true, true);
            sceneView.Repaint();
        }

        Canvas.ForceUpdateCanvases();
        return report.ToString();
    }

    static void FlattenZ(Transform root)
    {
        Vector3 pos = root.localPosition;
        if (Mathf.Abs(pos.z) > 0.01f)
        {
            pos.z = 0f;
            root.localPosition = pos;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            FlattenZ(root.GetChild(i));
        }
    }

    static void EnsureScene()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            if (EditorSceneManager.GetSceneAt(i).path == ScenePath)
            {
                return;
            }
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
