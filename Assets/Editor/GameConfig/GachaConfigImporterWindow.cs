using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>抽卡卡池 CSV 预览并上传到后端, 导入即生效.</summary>
public sealed class GachaConfigImporterWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.Gacha.BackendUrl";
    const string ConfigPath = "/api/gacha/admin/config";

    string m_CsvPath;
    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待预览";
    string m_Preview = string.Empty;
    int m_CardRows;
    int m_RarityRows;
    Vector2 m_Scroll;
    bool m_Busy;

    [MenuItem(EditorMenus.Window + "抽卡卡池导入")]
    [MenuItem(EditorMenus.Config + "抽卡卡池导入")]
    static void Open() => GetWindow<GachaConfigImporterWindow>("抽卡卡池导入");

    void OnEnable()
    {
        m_CsvPath = EditorPaths.FromProjectRoot("GameConfig", "card-gacha.csv");
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, m_BackendUrl);
        minSize = new Vector2(640f, 420f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("抽卡卡池 CSV → 后端", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(m_Busy))
        {
            EditorGUILayout.BeginHorizontal();
            m_CsvPath = EditorGUILayout.TextField("CSV 文件", m_CsvPath);
            if (GUILayout.Button("选择", GUILayout.Width(64f)))
            {
                string selected = EditorUtility.OpenFilePanel("选择抽卡卡池 CSV", Path.GetDirectoryName(m_CsvPath), "csv");
                if (!string.IsNullOrEmpty(selected)) m_CsvPath = selected;
            }
            EditorGUILayout.EndHorizontal();
            string backendUrl = EditorGUILayout.TextField("后端地址", m_BackendUrl);
            if (backendUrl != m_BackendUrl)
            {
                m_BackendUrl = backendUrl;
                EditorPrefs.SetString(BackendUrlPreference, backendUrl);
            }

            m_MemoryPublishKey = PublishKeyProvider.DrawField(m_MemoryPublishKey);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("预览", GUILayout.Height(30f))) PreviewCsv();
            if (GUILayout.Button("查看后端配置", GUILayout.Height(30f))) _ = RefreshAsync();
            if (GUILayout.Button("上传并生效", GUILayout.Height(30f))) _ = UploadAsync();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox(m_Status, m_Status.StartsWith("失败") ? MessageType.Error : MessageType.Info);
        if (m_CardRows > 0 || m_RarityRows > 0)
            EditorGUILayout.LabelField($"Card 行 {m_CardRows} / Rarity 行 {m_RarityRows}");
        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        EditorGUILayout.TextArea(m_Preview, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    void PreviewCsv()
    {
        try
        {
            string[] lines = File.ReadAllLines(m_CsvPath, Encoding.UTF8);
            m_CardRows = 0;
            m_RarityRows = 0;
            var preview = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0) continue;
                if (i == 0)
                {
                    preview.AppendLine(line);
                    continue;
                }

                if (line.StartsWith("Card,", StringComparison.OrdinalIgnoreCase)) m_CardRows++;
                else if (line.StartsWith("Rarity,", StringComparison.OrdinalIgnoreCase)) m_RarityRows++;
                if (preview.Length < 4000) preview.AppendLine(line);
            }

            m_Preview = preview.ToString();
            m_Status = $"预览完成：Card {m_CardRows}，Rarity {m_RarityRows}";
        }
        catch (Exception exception)
        {
            m_Preview = string.Empty;
            m_CardRows = 0;
            m_RarityRows = 0;
            m_Status = "失败：" + exception.Message;
        }
    }

    async Task RefreshAsync()
    {
        await RunAsync(async () =>
        {
            string response = await EditorBackendHttp.GetAsync(
                m_BackendUrl, ConfigPath, PublishKeyProvider.Resolve(m_MemoryPublishKey));
            m_Preview = response;
            m_Status = "已读取后端抽卡配置";
        });
    }

    async Task UploadAsync()
    {
        await RunAsync(async () =>
        {
            byte[] body = File.ReadAllBytes(m_CsvPath);
            string response = await EditorBackendHttp.SendRawAsync(
                m_BackendUrl,
                "PUT",
                ConfigPath,
                body,
                "text/csv",
                PublishKeyProvider.Resolve(m_MemoryPublishKey));
            m_Preview = response;
            m_Status = "上传成功，配置已生效";
            Debug.Log("[Gacha] 抽卡卡池已导入后端");
        });
    }

    async Task RunAsync(Func<Task> action)
    {
        if (m_Busy) return;
        try
        {
            EditorBackendHttp.ValidateBaseUrl(m_BackendUrl);
            PublishKeyProvider.RequireKey(m_MemoryPublishKey);
            m_Busy = true;
            m_Status = "处理中…";
            Repaint();
            await action();
        }
        catch (Exception exception)
        {
            m_Status = "失败：" + exception.Message;
            Debug.LogError("[Gacha] 操作失败. " + exception.Message);
        }
        finally
        {
            m_Busy = false;
            Repaint();
        }
    }
}
