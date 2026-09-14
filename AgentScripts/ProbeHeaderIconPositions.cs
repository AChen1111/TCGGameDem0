using UnityEngine;

public static class ProbeHeaderIconPositions
{
    public static string Run()
    {
        GameObject preview = GameObject.Find("HeaderLabelPreview");
        if (preview == null)
        {
            return "missing-preview";
        }

        var report = new System.Text.StringBuilder();
        string[] names = { "RigUp", "RightUpHud", "Btn_Gift", "Btn_Friend", "Btn_Mail", "Btn_Setting" };
        Camera cam = GameObject.Find("Canvas")?.GetComponent<Canvas>()?.worldCamera;
        for (int i = 0; i < names.Length; i++)
        {
            Transform t = FindDeep(preview.transform, names[i]);
            if (t == null)
            {
                report.Append(names[i]).Append("=missing;");
                continue;
            }

            var rect = t as RectTransform;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 sp = cam != null ? cam.WorldToScreenPoint(rect.position) : rect.position;
            report.Append(names[i])
                .Append(" active=").Append(t.gameObject.activeInHierarchy)
                .Append(" size=").Append(rect.rect.width.ToString("0.0")).Append('x').Append(rect.rect.height.ToString("0.0"))
                .Append(" pos=").Append(rect.anchoredPosition.ToString())
                .Append(" screen=").Append(sp.x.ToString("0")).Append(',').Append(sp.y.ToString("0"))
                .Append(" world=").Append(rect.position.ToString("F1"))
                .Append(';');
        }

        return report.ToString();
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
