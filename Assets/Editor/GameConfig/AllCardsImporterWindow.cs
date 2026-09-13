using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>全部卡牌 CSV 预览并上传到后端, 导入即生效.</summary>
public sealed class AllCardsImporterWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.AllCards.BackendUrl";
    const string ConfigPath = "/api/gacha/admin/cards";

    string m_CsvPath;
    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待预览";
    string m_Preview = string.Empty;
    int m_CardRows;
    Vector2 m_Scroll;
    bool m_Busy;

    [MenuItem(EditorMenus.Window + "全部卡牌导入")]
    [MenuItem(EditorMenus.Config + "全部卡牌导入")]
    static void Open() => GetWindow<AllCardsImporterWindow>("全部卡牌导入");

    void OnEnable()
    {
        m_CsvPath = EditorPaths.FromProjectRoot("GameConfig", "all-cards.csv");
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, m_BackendUrl);
        minSize = new Vector2(640f, 420f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("全部卡牌 CSV → 后端", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("CardAll 抽卡池读取本表, 与 card-gacha.csv 的分池权重互不影响.", MessageType.None);
        using (new EditorGUI.DisabledScope(m_Busy))
        {
            EditorGUILayout.BeginHorizontal();
            m_CsvPath = EditorGUILayout.TextField("CSV 文件", m_CsvPath);
            if (GUILayout.Button("选择", GUILayout.Width(64f)))
            {
                string selected = EditorUtility.OpenFilePanel("选择全部卡牌 CSV", Path.GetDirectoryName(m_CsvPath), "csv");
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
        if (m_CardRows > 0)
            EditorGUILayout.LabelField($"卡牌行 {m_CardRows}");
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

                m_CardRows++;
                if (preview.Length < 4000) preview.AppendLine(line);
            }

            m_Preview = preview.ToString();
            m_Status = $"预览完成：卡牌 {m_CardRows}";
        }
        catch (Exception exception)
        {
            m_Preview = string.Empty;
            m_CardRows = 0;
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
            m_Status = "已读取后端全部卡牌";
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
            Debug.Log("[AllCards] 全部卡牌已导入后端");
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
            Debug.LogError("[AllCards] 操作失败. " + exception.Message);
        }
        finally
        {
            m_Busy = false;
            Repaint();
        }
    }
}
