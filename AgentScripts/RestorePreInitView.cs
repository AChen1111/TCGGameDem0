using System.Text;
using UnityEditor;
using UnityEngine;

public static class RestorePreInitView
{
    public static string Run()
    {
        var leftover = GameObject.Find("__GoldPreview");
        if (leftover != null)
        {
            Object.DestroyImmediate(leftover);
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (camera != null && camera.name == "GoldPreviewCamera")
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        Camera sceneCamera = GameObject.Find("Camera")?.GetComponent<Camera>();
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        Vector3 cameraBefore = sceneCamera != null ? sceneCamera.transform.position : Vector3.zero;
        if (sceneCamera != null)
        {
            Undo.RecordObject(sceneCamera.transform, "Restore PreInit Camera");
            sceneCamera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            sceneCamera.enabled = true;
        }

        if (canvas != null && sceneCamera != null)
        {
            Undo.RecordObject(canvas, "Restore PreInit Canvas Camera");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = sceneCamera;
            canvas.planeDistance = 100f;
        }

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && sceneCamera != null)
        {
            sceneView.AlignViewToObject(sceneCamera.transform);
            sceneView.Repaint();
        }

        var report = new StringBuilder();
        report.Append("cameraBefore=");
        report.Append(cameraBefore.ToString());
        report.Append("; camera=");
        report.Append(sceneCamera == null ? "missing" : sceneCamera.transform.position.ToString());
        report.Append("; canvas=");
        report.Append(canvas == null ? "missing" : canvas.renderMode.ToString());
        report.Append("; worldCamera=");
        report.Append(canvas != null && canvas.worldCamera != null ? canvas.worldCamera.name : "null");
        report.Append("; leftoverCleared=");
        report.Append(leftover != null);
        return report.ToString();
    }
}
