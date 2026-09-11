using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>通过后端管理接口给指定账号增加金币.</summary>
public sealed class PlayerGoldGrantWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.AccountGold.BackendUrl";

    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_Username = "AChen1234";
    long m_Amount = 100000;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待查询";
    string m_CurrentGoldText = "—";
    bool m_Busy;

    [MenuItem(EditorMenus.Window + "账号金币")]
    [MenuItem(EditorMenus.Ops + "账号金币")]
    static void Open() => GetWindow<PlayerGoldGrantWindow>("账号金币");

    void OnEnable()
    {
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, BackendServiceController.BaseUrl);
        minSize = new Vector2(420f, 260f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("给账号添加金币", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("需后端运行中，并使用与后端一致的内容发布密钥。", MessageType.Info);

        using (new EditorGUI.DisabledScope(m_Busy))
        {
            string backendUrl = EditorGUILayout.TextField("后端地址", m_BackendUrl);
            if (!string.Equals(backendUrl, m_BackendUrl, StringComparison.Ordinal))
            {
                m_BackendUrl = backendUrl;
                EditorPrefs.SetString(BackendUrlPreference, backendUrl);
            }

            m_Username = EditorGUILayout.TextField("账号", m_Username);
            m_Amount = EditorGUILayout.LongField("添加数量", m_Amount);
            EditorGUILayout.LabelField("当前金币", m_CurrentGoldText);
            m_MemoryPublishKey = PublishKeyProvider.DrawField(m_MemoryPublishKey);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查询", GUILayout.Height(30f)))
            {
                _ = QueryAsync();
            }

            if (GUILayout.Button("添加金币", GUILayout.Height(30f)))
            {
                _ = GrantAsync();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox(
            m_Status,
            m_Status.StartsWith("失败", StringComparison.Ordinal) ? MessageType.Error : MessageType.Info);
    }

    async Task QueryAsync()
    {
        await RunAsync(async () =>
        {
            string username = RequireUsername();
            string response = await SendAsync(
                "GET",
                "/api/accounts/admin/gold?username=" + Uri.EscapeDataString(username),
                null);
            GoldSummary summary = JsonUtility.FromJson<GoldSummary>(response);
            m_CurrentGoldText = summary.gold.ToString("N0");
            m_Status = $"查询成功：{summary.username} 当前金币 {summary.gold:N0}（Revision {summary.revision}）";
        });
    }

    async Task GrantAsync()
    {
        await RunAsync(async () =>
        {
            string username = RequireUsername();
            if (m_Amount <= 0)
            {
                throw new InvalidOperationException("添加数量必须大于 0");
            }

            if (!EditorUtility.DisplayDialog(
                    "添加金币",
                    $"确认给账号 {username} 添加 {m_Amount:N0} 金币？",
                    "添加",
                    "取消"))
            {
                m_Status = "已取消";
                return;
            }

            var body = new GrantRequest { username = username, amount = m_Amount };
            string response = await SendAsync(
                "POST",
                "/api/accounts/admin/gold",
                JsonUtility.ToJson(body));
            GoldGrantResult result = JsonUtility.FromJson<GoldGrantResult>(response);
            m_CurrentGoldText = result.gold.ToString("N0");
            m_Status =
                $"添加成功：{result.username} {result.previousGold:N0} + {result.addedAmount:N0} = {result.gold:N0}（Revision {result.revision}）";
            Debug.Log(
                $"[AccountGold] 添加金币成功. Username={result.username}; Previous={result.previousGold}; Added={result.addedAmount}; Gold={result.gold}; Revision={result.revision}");
        });
    }

    async Task RunAsync(Func<Task> action)
    {
        if (m_Busy)
        {
            return;
        }

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
            Debug.LogError("[AccountGold] 操作失败. " + exception.Message);
        }
        finally
        {
            m_Busy = false;
            Repaint();
        }
    }

    Task<string> SendAsync(string method, string path, string json) =>
        EditorBackendHttp.SendJsonAsync(m_BackendUrl, method, path, json, PublishKeyProvider.Resolve(m_MemoryPublishKey));

    string RequireUsername()
    {
        string username = m_Username?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(username))
        {
            throw new InvalidOperationException("账号不能为空");
        }

        return username;
    }

    [Serializable]
    sealed class GrantRequest
    {
        public string username;
        public long amount;
    }

    [Serializable]
    sealed class GoldSummary
    {
        public string id;
        public string username;
        public string nickname;
        public long gold;
        public long revision;
    }

    [Serializable]
    sealed class GoldGrantResult
    {
        public string id;
        public string username;
        public long previousGold;
        public long addedAmount;
        public long gold;
        public long revision;
    }
}
