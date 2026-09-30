using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 使用真实 Prefab 和序列化字段绑定加载状态、进度与重试按钮。
public static class ConfigureLoadingPrefab
{
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.layer = 5;
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }
    static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, Vector2 size, Vector2 position, string value)
    {
        var rect = Rect(name, parent, size, position); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.fontSize = 30; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        text.text = value; return text;
    }
    public static string Main()
    {
        const string path = "Assets/UI/Prefab/Common/LoadIN.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var rect in All<RectTransform>(root).Where(x => x.name is "ActivityStatus" or "ActivityProgress" or "ActivityRetry").ToArray())
                UnityEngine.Object.DestroyImmediate(rect.gameObject);
            var font = All<TMP_Text>(root).First().font;
            var status = Text("ActivityStatus", root.transform, font, new Vector2(1200, 160), new Vector2(0, -180), "正在准备活动配置 / Preparing activities");
            var bar = Rect("ActivityProgress", root.transform, new Vector2(640, 16), new Vector2(0, -300));
            var track = bar.gameObject.AddComponent<Image>(); track.color = new Color(.12f, .18f, .22f); track.raycastTarget = false;
            var fill = Rect("Fill", bar, Vector2.zero, Vector2.zero); fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            var image = fill.gameObject.AddComponent<Image>(); image.color = new Color(.32f, .85f, .78f); image.raycastTarget = false;
            var progress = bar.gameObject.AddComponent<Slider>(); progress.fillRect = fill; progress.targetGraphic = image; progress.interactable = false; progress.value = 0;
            var retryRect = Rect("ActivityRetry", root.transform, new Vector2(260, 64), new Vector2(0, -390));
            var background = retryRect.gameObject.AddComponent<Image>(); background.color = new Color(.1f, .42f, .4f);
            var retry = retryRect.gameObject.AddComponent<Button>(); retry.targetGraphic = background;
            var retryText = Text("Label", retryRect, font, new Vector2(260, 64), Vector2.zero, "重试 / Retry");
            var existing = All<SceneTransitionOverlayView>(root); var view = existing.Length == 0 ? root.AddComponent<SceneTransitionOverlayView>() : existing.Single();
            var serialized = new SerializedObject(view);
            void Bind(string field, UnityEngine.Object value) => serialized.FindProperty(field).objectReferenceValue = value;
            Bind("m_canvas", All<Canvas>(root).Single()); Bind("m_group", All<CanvasGroup>(root).Single());
            Bind("m_background", All<Image>(root).Single(x => x.name == "Image").rectTransform);
            Bind("m_status", status); Bind("m_progress", progress); Bind("m_retry", retry); Bind("m_retryLabel", retryText);
            serialized.ApplyModifiedPropertiesWithoutUndo(); retry.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return path;
    }
}
