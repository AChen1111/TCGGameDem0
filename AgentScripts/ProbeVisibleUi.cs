using UnityEngine;
using UnityEngine.UI;

public static class ProbeVisibleUi
{
    public static string Run()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        Camera cam = canvas != null ? canvas.worldCamera : Camera.main;
        var report = new System.Text.StringBuilder();
        report.Append("cam=").Append(cam != null ? cam.name : "null");
        if (cam != null)
        {
            report.Append(" size=").Append(cam.pixelWidth).Append('x').Append(cam.pixelHeight);
            report.Append(" pos=").Append(cam.transform.position.ToString("F1"));
        }

        report.Append(";canvas=").Append(canvas != null ? canvas.renderMode.ToString() : "null");
        GameObject preview = GameObject.Find("HeaderLabelPreview");
        if (preview == null)
        {
            return report.Append(";missing-preview").ToString();
        }

        string[] names = { "LeftLayout", "Btn_Play", "RigUp", "RightUpHud", "Btn_Gift", "Btn_Setting" };
        for (int i = 0; i < names.Length; i++)
        {
            Transform t = FindDeep(preview.transform, names[i]);
            if (t == null)
            {
                report.Append(';').Append(names[i]).Append("=missing");
                continue;
            }

            var rect = (RectTransform)t;
            Vector3 sp = cam != null ? cam.WorldToScreenPoint(rect.position) : Vector3.zero;
            report.Append(';').Append(names[i])
                .Append(" screen=").Append(sp.x.ToString("0")).Append(',').Append(sp.y.ToString("0"))
                .Append(" world=").Append(rect.position.ToString("F1"));
        }

        return report.ToString();
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
