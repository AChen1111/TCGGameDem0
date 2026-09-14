using TMPro;
using UnityEngine;

public static class ProbeLiveHeaderLabels
{
    static readonly string[] Names = { "Btn_Gift", "Btn_Friend", "Btn_Mail", "Btn_Setting" };

    public static string Run()
    {
        var report = new System.Text.StringBuilder();
        report.Append("play=").Append(Application.isPlaying);
        for (int i = 0; i < Names.Length; i++)
        {
            GameObject button = Find(Names[i]);
            if (button == null)
            {
                report.Append(';').Append(Names[i]).Append("=missing");
                continue;
            }

            Transform desc = button.transform.Find("desc");
            var text = desc != null ? desc.GetComponent<TextMeshProUGUI>() : null;
            if (text == null)
            {
                report.Append(';').Append(Names[i]).Append("=no-desc");
                continue;
            }

            var btnRect = ((RectTransform)button.transform).rect;
            report.Append(';').Append(Names[i])
                .Append(" active=").Append(button.activeInHierarchy)
                .Append(" btn=").Append(btnRect.width.ToString("0.0")).Append('x').Append(btnRect.height.ToString("0.0"))
                .Append(" size=").Append(text.fontSize.ToString("0.00"))
                .Append(" min=").Append(text.fontSizeMin.ToString("0.00"))
                .Append(" max=").Append(text.fontSizeMax.ToString("0.00"))
                .Append(" auto=").Append(text.enableAutoSizing)
                .Append(" '").Append(text.text).Append('\'');
        }

        return report.ToString();
    }

    static GameObject Find(string name)
    {
        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
            {
                return all[i].gameObject;
            }
        }

        return null;
    }
}
