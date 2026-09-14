using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class AdjustCardDetailOverlay
{
    const string OverlayPath = "Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab";
    const string ScenePath = "Assets/Scenes/SceneUIRef.unity";

    public static string Run()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(OverlayPath);
        try
        {
            Tune(contents.transform);
            PrefabUtility.SaveAsPrefabAsset(contents, OverlayPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        TuneScene();
        AssetDatabase.SaveAssets();
        return "adjust-ok";
    }

    static void Tune(Transform root)
    {
        RawImage raw = Find<RawImage>(root, "Raw_Card");
        if (raw != null)
        {
            raw.raycastTarget = true;
            var rect = raw.rectTransform;
            rect.anchorMin = new Vector2(0.205f, 0.5f);
            rect.anchorMax = new Vector2(0.205f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 18f);
            rect.sizeDelta = new Vector2(448f, 654f);
        }

        Image info = Find<Image>(root, "Go_Info");
        if (info != null)
        {
            info.color = new Color(0.02f, 0.04f, 0.08f, 0.55f);
            info.raycastTarget = true;
            var rect = info.rectTransform;
            rect.anchorMin = new Vector2(0.42f, 0.16f);
            rect.anchorMax = new Vector2(0.97f, 0.84f);
        }

        Image nameBase = Find<Image>(root, "Img_NameBase");
        if (nameBase != null)
        {
            nameBase.type = Image.Type.Simple;
        }

        PaintRow(root, "Go_LevelRow", new Color(0f, 0f, 0f, 0.45f));
        PaintRow(root, "Go_StatRow", new Color(0f, 0f, 0f, 0.45f));

        TextMeshProUGUI name = Find<TextMeshProUGUI>(root, "Txt_Name");
        if (name != null)
        {
            name.overflowMode = TextOverflowModes.Ellipsis;
            name.enableWordWrapping = false;
        }

        TextMeshProUGUI desc = Find<TextMeshProUGUI>(root, "Txt_Desc");
        if (desc != null)
        {
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Overflow;
        }

        MoveArrow(root, "Btn_Prev", new Vector2(0.02f, 0.5f));
        MoveArrow(root, "Btn_Next", new Vector2(0.98f, 0.5f));
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

    static void TuneScene()
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
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform overlay = FindDeep(roots[i].transform, "CardDetailOverlay");
            if (overlay != null)
            {
                Tune(overlay);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return;
            }
        }
    }

    static T Find<T>(Transform root, string name) where T : Component
    {
        Transform child = FindDeep(root, name);
        return child != null ? child.GetComponent<T>() : null;
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
