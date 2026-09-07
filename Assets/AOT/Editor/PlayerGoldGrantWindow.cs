#if UNITY_EDITOR
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>通过后端管理接口给指定账号增加金币.</summary>
public sealed class PlayerGoldGrantWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.AccountGold.BackendUrl";
    const string PublishKeyEnvironmentVariable = "ACHEN_CONTENT_PUBLISH_KEY";
    const string PublishKeySessionName = "AChen.BackendService.PublishKey";

    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_Username = "AChen1234";
    long m_Amount = 100000;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待查询";
    string m_CurrentGoldText = "—";
    bool m_Busy;

    [MenuItem("Window/AChen/账号金币")]
    static void Open() => GetWindow<PlayerGoldGrantWindow>("账号金币");

    void OnEnable()
    {
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, BackendServiceController.BaseUrl);
        minSize = new Vector2(420f, 260f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("给账号添加金币", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "需后端运行中，并使用与后端一致的内容发布密钥（ACHEN_CONTENT_PUBLISH_KEY 或后端服务窗口启动时生成的密钥）。",
            MessageType.Info);

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

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PublishKeyEnvironmentVariable)) &&
                string.IsNullOrEmpty(SessionState.GetString(PublishKeySessionName, string.Empty)))
            {
                m_MemoryPublishKey = EditorGUILayout.PasswordField("发布密钥（仅内存）", m_MemoryPublishKey);
            }
            else
            {
                EditorGUILayout.LabelField("发布密钥", DescribePublishKeySource());
            }

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
            ValidateConnection();
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

    async Task<string> SendAsync(string method, string path, string json)
    {
        using (var request = new UnityWebRequest(m_BackendUrl.TrimEnd('/') + path, method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            if (json != null)
            {
                byte[] body = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(body);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader("X-Content-Publish-Key", PublishKey());
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Delay(50);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                ProblemDetails problem = null;
                try
                {
                    problem = JsonUtility.FromJson<ProblemDetails>(request.downloadHandler.text);
                }
                catch
                {
                    // ignore parse failures
                }

                string detail = problem != null && !string.IsNullOrEmpty(problem.title)
                    ? problem.title
                    : request.error;
                string code = problem != null && !string.IsNullOrEmpty(problem.code)
                    ? " [" + problem.code + "]"
                    : string.Empty;
                throw new InvalidOperationException(detail + code + " (HTTP " + request.responseCode + ")");
            }

            return request.downloadHandler.text;
        }
    }

    void ValidateConnection()
    {
        if (!Uri.TryCreate(m_BackendUrl, UriKind.Absolute, out Uri uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("后端地址必须是绝对 HTTP/HTTPS 地址");
        }

        if (string.IsNullOrWhiteSpace(PublishKey()))
        {
            throw new InvalidOperationException("请设置发布密钥，或先通过「后端服务」窗口启动后端");
        }
    }

    string RequireUsername()
    {
        string username = m_Username?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(username))
        {
            throw new InvalidOperationException("账号不能为空");
        }

        return username;
    }

    string PublishKey()
    {
        string envKey = Environment.GetEnvironmentVariable(PublishKeyEnvironmentVariable);
        if (!string.IsNullOrEmpty(envKey))
        {
            return envKey;
        }

        string sessionKey = SessionState.GetString(PublishKeySessionName, string.Empty);
        if (!string.IsNullOrEmpty(sessionKey))
        {
            return sessionKey;
        }

        return m_MemoryPublishKey;
    }

    string DescribePublishKeySource()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PublishKeyEnvironmentVariable)))
        {
            return "来自 " + PublishKeyEnvironmentVariable;
        }

        if (!string.IsNullOrEmpty(SessionState.GetString(PublishKeySessionName, string.Empty)))
        {
            return "来自后端服务窗口会话";
        }

        return "未配置";
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

    [Serializable]
    sealed class ProblemDetails
    {
        public string title;
        public string code;
    }
}
#endif
