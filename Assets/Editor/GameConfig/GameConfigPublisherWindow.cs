using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>游戏配置 CSV 解析预览, 上传到后端草稿并发布.</summary>
public sealed class GameConfigPublisherWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.GameConfig.BackendUrl";
    const string DraftPath = "/api/game-config/admin/draft";

    string m_CsvPath;
    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待解析";
    string m_JsonPreview = string.Empty;
    Vector2 m_Scroll;
    EditorGameConfigDocument m_Document;
    AdminState m_State;
    bool m_Busy;

    [MenuItem(EditorMenus.Window + "游戏配置发布")]
    [MenuItem(EditorMenus.Config + "发布窗口")]
    static void Open() => GetWindow<GameConfigPublisherWindow>("游戏配置发布");

    void OnEnable()
    {
        m_CsvPath = EditorPaths.FromProjectRoot("GameConfig", "game-config.csv");
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, m_BackendUrl);
        minSize = new Vector2(640f, 520f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("游戏配置 CSV → JSON → 后端草稿", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(m_Busy))
        {
            EditorGUILayout.BeginHorizontal();
            m_CsvPath = EditorGUILayout.TextField("CSV 文件", m_CsvPath);
            if (GUILayout.Button("选择", GUILayout.Width(64f)))
            {
                string selected = EditorUtility.OpenFilePanel("选择游戏配置 CSV", Path.GetDirectoryName(m_CsvPath), "csv");
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
            if (GUILayout.Button("解析并预览", GUILayout.Height(30f))) ParseCsv();
            if (GUILayout.Button("刷新后端草稿", GUILayout.Height(30f))) _ = RefreshStateAsync();
            if (GUILayout.Button("上传到草稿", GUILayout.Height(30f))) _ = UploadAsync();
            if (GUILayout.Button("发布当前草稿", GUILayout.Height(30f))) _ = PublishAsync();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox(m_Status, m_Status.StartsWith("失败") ? MessageType.Error : MessageType.Info);
        if (m_Document != null)
            EditorGUILayout.LabelField($"头像 {m_Document.avatars.Length} / 壁纸 {m_Document.wallpapers.Length} / 卡包 {m_Document.cardPacks.Length}");
        if (m_State != null)
            EditorGUILayout.LabelField($"后端 Draft {m_State.draftRevision} / EditRevision {m_State.editRevision}");
        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        EditorGUILayout.TextArea(m_JsonPreview, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    void ParseCsv()
    {
        try
        {
            m_Document = GameConfigCsvEditorParser.ParseFile(m_CsvPath);
            m_JsonPreview = JsonConvert.SerializeObject(m_Document, Formatting.Indented);
            string tempDirectory = EditorPaths.FromProjectRoot("Temp", "GameConfig");
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllText(Path.Combine(tempDirectory, "game-config.json"), m_JsonPreview, new UTF8Encoding(false));
            m_Status = "解析成功，JSON 预览已写入 Temp/GameConfig/game-config.json";
        }
        catch (Exception exception)
        {
            m_Document = null;
            m_JsonPreview = string.Empty;
            m_Status = "失败：" + exception.Message;
        }
    }

    async Task RefreshStateAsync()
    {
        await RunAsync(async () =>
        {
            string response = await SendAsync("GET", DraftPath, null);
            m_State = JsonUtility.FromJson<AdminState>(response);
            m_Status = $"草稿状态已刷新：Draft {m_State.draftRevision} / EditRevision {m_State.editRevision}";
        });
    }

    async Task UploadAsync()
    {
        if (m_Document == null) ParseCsv();
        if (m_Document == null) return;
        await RunAsync(async () =>
        {
            string stateJson = await SendAsync("GET", DraftPath, null);
            m_State = JsonUtility.FromJson<AdminState>(stateJson);
            var request = new ReplaceRequest
            {
                expectedEditRevision = m_State.editRevision,
                avatars = m_Document.avatars,
                wallpapers = m_Document.wallpapers,
                cardPacks = m_Document.cardPacks
            };
            string response = await SendAsync("PUT", DraftPath, JsonConvert.SerializeObject(request));
            m_State = JsonUtility.FromJson<AdminState>(response);
            m_Status = $"上传成功：Draft {m_State.draftRevision} / EditRevision {m_State.editRevision}";
            Debug.Log($"[GameConfig] 草稿上传成功. Avatars={m_Document.avatars.Length}; Wallpapers={m_Document.wallpapers.Length}; CardPacks={m_Document.cardPacks.Length}");
        });
    }

    async Task PublishAsync()
    {
        if (m_State == null) await RefreshStateAsync();
        if (m_State == null) return;
        if (!EditorUtility.DisplayDialog(
            "发布游戏配置",
            $"确认发布 Draft {m_State.draftRevision}？\n头像 {m_State.avatars?.Length ?? 0}，壁纸 {m_State.wallpapers?.Length ?? 0}，卡包 {m_State.cardPacks?.Length ?? 0}",
            "发布",
            "取消")) return;
        await RunAsync(async () =>
        {
            var request = new PublishRequest { expectedEditRevision = m_State.editRevision };
            string response = await SendAsync("POST", "/api/game-config/admin/publish", JsonConvert.SerializeObject(request));
            Publication publication = JsonUtility.FromJson<Publication>(response);
            m_Status = $"发布成功：Revision {publication.publishedRevision}，新草稿 {publication.draftRevision}";
            m_State = null;
            Debug.Log($"[GameConfig] 配置发布成功. Revision={publication.publishedRevision}");
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
            Debug.LogError("[GameConfig] 操作失败. " + exception.Message);
        }
        finally
        {
            m_Busy = false;
            Repaint();
        }
    }

    Task<string> SendAsync(string method, string path, string json) =>
        EditorBackendHttp.SendJsonAsync(m_BackendUrl, method, path, json, PublishKeyProvider.Resolve(m_MemoryPublishKey));

    [Serializable] sealed class ReplaceRequest { public long expectedEditRevision; public EditorAvatarConfig[] avatars; public EditorWallpaperConfig[] wallpapers; public EditorCardPackConfig[] cardPacks; }
    [Serializable] sealed class PublishRequest { public long expectedEditRevision; }
    [Serializable] sealed class AdminState { public long draftRevision; public long editRevision; public EditorAvatarConfig[] avatars; public EditorWallpaperConfig[] wallpapers; public EditorCardPackConfig[] cardPacks; }
    [Serializable] sealed class Publication { public long publishedRevision; public long draftRevision; }
}
