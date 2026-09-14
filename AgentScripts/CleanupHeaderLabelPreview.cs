using UnityEditor;
using UnityEngine;

public static class CleanupHeaderLabelPreview
{
    const string PreviewName = "HeaderLabelPreview";

    public static string Run()
    {
        GameObject preview = GameObject.Find(PreviewName);
        if (preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        int restored = 0;
        if (canvas != null)
        {
            Transform canvasTf = canvas.transform;
            for (int i = 0; i < canvasTf.childCount; i++)
            {
                Transform child = canvasTf.GetChild(i);
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    restored++;
                }
            }
        }

        return "removed-preview;restored=" + restored;
    }
}
