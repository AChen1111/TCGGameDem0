using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PreviewCardDetailSample
{
    const string ScenePath = "Assets/Scenes/SceneUIRef.unity";
    const string SamplePath = "Assets/UI/Card/CardBag01_BlueEyes/71039903.jpg";

    public static string Run()
    {
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(SamplePath);
        CardDetailView view = null;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (scene.path != ScenePath)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                view = roots[r].GetComponentInChildren<CardDetailView>(true);
                if (view != null)
                {
                    break;
                }
            }
        }

        if (view == null)
        {
            return "preview-missing-view";
        }

        view.gameObject.SetActive(true);
        view.Show(new[] { new CardDetailEntry("71039903", "Card01", texture) }, 0);
        foreach (TextMeshProUGUI tmp in view.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.ForceMeshUpdate();
        }

        Camera sceneCamera = GameObject.Find("Camera")?.GetComponent<Camera>();
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && sceneCamera != null)
        {
            sceneView.AlignViewToObject(sceneCamera.transform);
            sceneView.orthographic = true;
            sceneView.Repaint();
        }

        return texture != null ? "preview-bound" : "preview-bound-no-texture";
    }
}
