using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>将大厅头像移出旧框的缩放层，保存实际 Prefab。</summary>
public static class FixLobbyPortrait
{
    public static string Main()
    {
        const string path = "Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var portrait = Resources.FindObjectsOfTypeAll<AvatarPortraitView>().Single(x => x.transform.IsChildOf(root.transform));
            var button = Resources.FindObjectsOfTypeAll<Button>().Single(x => x.transform.IsChildOf(root.transform) && x.name == "Btn_Avatar");
            var rect = (RectTransform)portrait.transform;
            rect.SetParent(button.transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(3, 2.85f);
            rect.sizeDelta = new Vector2(96, 96);
            rect.localScale = Vector3.one;
            var layers = Resources.FindObjectsOfTypeAll<Image>().Where(x => x.transform.IsChildOf(rect)).ToArray();
            foreach (var image in layers.Where(x => x.name == "ClipMask" || x.name == "FrameImage"))
            {
                image.rectTransform.anchorMin = Vector2.zero;
                image.rectTransform.anchorMax = Vector2.one;
                image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            }
            var frame = layers.Single(x => x.name == "FrameImage");
            frame.raycastTarget = true;
            button.targetGraphic = frame;
            foreach (var image in Resources.FindObjectsOfTypeAll<Image>().Where(x => x.transform.IsChildOf(root.transform)
                && (x.name == "AvatarBG" || x.name == "AvatarBG (1)")).ToArray())
                Object.DestroyImmediate(image.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            return "大厅头像直接绑定到 Btn_Avatar，尺寸 96×96；两层旧边框已移除。";
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
