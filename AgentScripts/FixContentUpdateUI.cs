using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixContentUpdateUI
{
    static RectTransform Rect(Transform t, Vector2 size, Vector2 pos)
    {
        var r = (RectTransform)t;
        r.localScale = Vector3.one;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, .5f);
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        return r;
    }
    static GameObject Child(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Rect(go.transform, size, pos);
        return go;
    }
    static void Style(TMP_Text text, TMP_FontAsset font, float size)
    {
        text.font = font;
        text.fontSharedMaterial = font.material;
        text.fontStyle = FontStyles.Normal;
        text.enableAutoSizing = false;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero;
    }
    static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, string value, Vector2 size, Vector2 pos, float pointSize)
    {
        var text = Child(name, parent, size, pos).AddComponent<TextMeshProUGUI>();
        Style(text, font, pointSize);
        text.text = value;
        return text;
    }
    static Button Button(string name, Transform parent, TMP_FontAsset font, string label, Vector2 pos)
    {
        var go = Child(name, parent, new Vector2(220, 54), pos);
        var image = go.AddComponent<Image>();
        image.color = new Color(.12f, .29f, .49f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        Label("Label", go.transform, font, label, new Vector2(200, 48), Vector2.zero, 24);
        return button;
    }
    static void Scaler(GameObject root)
    {
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.matchWidthOrHeight = .5f;
    }
    public static void Run()
    {
        var pre = EditorSceneManager.GetActiveScene();
        if (pre.path != "Assets/Scenes/PreInit.unity") throw new InvalidOperationException("Open PreInit first.");
        var canvas = pre.GetRootGameObjects().Single(g => g.name == "Canvas");
        Scaler(canvas);
        var panel = canvas.transform.Find("Panel");
        foreach (var c in panel.GetComponents<MonoBehaviour>())
            if (c != null && c.GetType().Name == "AspectScaler") UnityEngine.Object.DestroyImmediate(c);
        var panelRect = (RectTransform)panel;
        panelRect.localScale = Vector3.one;
        panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        var title = panel.Find("Title").GetComponent<TMP_Text>();
        var font = title.font;
        foreach (var text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            foreach (var c in text.GetComponents<MonoBehaviour>())
                if (c != null && c.GetType().Name == "LocalizedText") UnityEngine.Object.DestroyImmediate(c);
            Style(text, font, 24);
        }
        Rect(title.transform, new Vector2(800,64), new Vector2(0,110));
        title.fontSize = 36; title.text = "游戏内容更新";
        var slider = panel.Find("Slider").GetComponent<Slider>();
        Rect(slider.transform, new Vector2(720,32), new Vector2(0,36));
        slider.interactable = false;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.value = 0;
        var status = panel.Find("Percent").GetComponent<TMP_Text>();
        Rect(status.transform, new Vector2(820,80), new Vector2(0,-35));
        status.text = "正在更新游戏内容 0%";
        var old = panel.Find("RetryButton");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        Button("RetryButton", panel, font, "重试加载", new Vector2(0,-126)).gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(pre);
        EditorSceneManager.SaveScene(pre);

        var init = EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Additive);
        var previous = init.GetRootGameObjects().FirstOrDefault(g => g.name == "ContentUpdatePrompt");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
        var root = new GameObject("ContentUpdatePrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, init);
        Scaler(root);
        root.GetComponent<Canvas>().sortingOrder = 32767;
        var blocker = Child("Blocker", root.transform, Vector2.zero, Vector2.zero);
        var br = (RectTransform)blocker.transform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.offsetMin = br.offsetMax = Vector2.zero;
        blocker.AddComponent<Image>().color = new Color(0,0,0,.8f);
        var modal = Child("Panel", root.transform, new Vector2(680,260), Vector2.zero);
        modal.AddComponent<Image>().color = new Color(.06f,.09f,.15f);
        var msg = Label("Message", modal.transform, font, "游戏内容检查失败，请检查网络后重试。", new Vector2(616,120), new Vector2(0,38), 26);
        msg.enableAutoSizing = true; msg.fontSizeMin = 20; msg.fontSizeMax = 26;
        Button("Action", modal.transform, font, "重试", new Vector2(0,-78));
        var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Network/GameConfig/ContentUpdatePrompt.cs");
        root.AddComponent(script.GetClass());
        root.GetComponent<Canvas>().enabled = false;
        EditorSceneManager.MarkSceneDirty(init);
        EditorSceneManager.SaveScene(init);
        EditorSceneManager.CloseScene(init, true);
        Debug.Log("Authored startup UI and persistent Init update dialog.");
    }
    public static void PreviewError()
    {
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GameObject.Find("Camera").GetComponent<Camera>();
        canvas.planeDistance = 1;
        var panel = GameObject.Find("Canvas").transform.Find("Panel");
        panel.Find("Percent").GetComponent<TMP_Text>().text = "游戏内容加载失败，请检查网络后重试。";
        panel.Find("RetryButton").gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
    }
    public static void Restore()
    {
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        canvas.planeDistance = 100;
        var panel = GameObject.Find("Canvas").transform.Find("Panel");
        panel.Find("Percent").GetComponent<TMP_Text>().text = "正在更新游戏内容 0%";
        panel.Find("RetryButton").gameObject.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
