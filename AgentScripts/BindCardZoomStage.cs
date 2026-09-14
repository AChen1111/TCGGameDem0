using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BindCardZoomStage
{
    const string PrefabPath = "Assets/UI/Prefab/Hall/Shop/CardZoomWindow.prefab";
    const string StagePath = "Assets/Prefab/Card/CardPickStage.prefab";

    public static string Run()
    {
        GameObject stagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StagePath);
        if (stagePrefab == null)
        {
            throw new System.Exception("找不到 CardPickStage");
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform raw = FindDeep(contents.transform, "Raw_Card");
            if (raw == null)
            {
                return "no-raw";
            }

            RectTransform rect = raw.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            RawImage rawImage = raw.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.raycastTarget = true;
                rawImage.color = Color.white;
            }

            Button button = raw.GetComponent<Button>();
            if (button != null)
            {
                Object.DestroyImmediate(button);
            }

            string bound = "stage-skipped";
            Component[] components = contents.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                SerializedObject so = new SerializedObject(components[i]);
                SerializedProperty stage = so.FindProperty("m_StagePrefab");
                if (stage == null)
                {
                    continue;
                }

                stage.objectReferenceValue = stagePrefab;
                SerializedProperty btn = so.FindProperty("m_BtnCard");
                if (btn != null)
                {
                    btn.objectReferenceValue = null;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                bound = "stage-bound";
                break;
            }

            PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
            if (bound == "stage-skipped")
            {
                string yaml = File.ReadAllText(PrefabPath);
                const string StageLine = "  m_StagePrefab: {fileID: 3403963794378004767, guid: 1fd3162e16e25d344b06f9c52f75f84a, type: 3}";
                if (!yaml.Contains("m_StagePrefab:"))
                {
                    yaml = yaml.Replace("  isPopup: 1\n", "  isPopup: 1\n" + StageLine + "\n");
                    yaml = yaml.Replace("  m_BtnCard: {fileID: 0}\n", "");
                    File.WriteAllText(PrefabPath, yaml);
                    AssetDatabase.ImportAsset(PrefabPath);
                    bound = "stage-yaml";
                }
            }

            AssetDatabase.SaveAssets();
            return "raw-stretch;" + bound;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
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
