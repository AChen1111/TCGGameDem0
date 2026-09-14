using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FrameCardDetailPreview
{
    public static string Run()
    {
        RestoreHostOverlay<CardPickWindow>();
        RestoreHostOverlay<CardPreviewWindow>();

        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        CardDetailView[] views = Object.FindObjectsByType<CardDetailView>(FindObjectsInactive.Include);
        CardDetailView standalone = null;
        int extra = 0;
        for (int i = 0; i < views.Length; i++)
        {
            CardDetailView view = views[i];
            if (view == null)
            {
                continue;
            }

            if (IsHosted(view))
            {
                continue;
            }

            if (standalone == null)
            {
                standalone = view;
                continue;
            }

            extra++;
            Object.DestroyImmediate(view.gameObject);
        }

        if (standalone == null)
        {
            return "frame-missing";
        }

        standalone.gameObject.SetActive(true);
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/UI/Card/CardBag01_BlueEyes/71039903.jpg");
        standalone.Show(new[]
        {
            new CardDetailEntry("71039903", "Card01", texture),
            new CardDetailEntry("89631139", "Card01", texture)
        }, 0);
        foreach (TextMeshProUGUI tmp in standalone.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.ForceMeshUpdate();
        }

        Camera cam = canvas != null ? canvas.worldCamera : Camera.main;
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && cam != null)
        {
            Vector3 look = cam.transform.position + cam.transform.forward * (canvas != null ? canvas.planeDistance : 100f);
            sceneView.LookAt(look, cam.transform.rotation, 10f, true, true);
            sceneView.Repaint();
        }

        Canvas.ForceUpdateCanvases();
        if (standalone.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(standalone.gameObject.scene);
            EditorSceneManager.SaveScene(standalone.gameObject.scene);
        }

        int hosted = 0;
        CardDetailView[] after = Object.FindObjectsByType<CardDetailView>(FindObjectsInactive.Include);
        for (int i = 0; i < after.Length; i++)
        {
            if (after[i] != null && IsHosted(after[i]))
            {
                hosted++;
            }
        }

        return "framed;extraRemoved=" + extra + ";hosted=" + hosted + ";cam=" + (cam != null ? cam.name : "null");
    }

    static void RestoreHostOverlay<T>() where T : Component
    {
        T[] hosts = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
        GameObject overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab");
        for (int i = 0; i < hosts.Length; i++)
        {
            if (hosts[i].GetComponentInChildren<CardDetailView>(true) != null || overlayPrefab == null)
            {
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(overlayPrefab, hosts[i].transform);
            instance.name = "CardDetailOverlay";
            instance.SetActive(false);
            var rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            instance.transform.SetAsLastSibling();
        }
    }

    static bool IsHosted(CardDetailView view)
    {
        return view.GetComponentInParent<CardPickWindow>(true) != null
            || view.GetComponentInParent<CardPreviewWindow>(true) != null;
    }
}
