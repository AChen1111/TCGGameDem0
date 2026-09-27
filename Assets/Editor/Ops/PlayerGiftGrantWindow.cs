using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>通过礼品箱接口给指定账号发放金币和/或卡牌.</summary>
public sealed class PlayerGiftGrantWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.AccountGift.BackendUrl";

    string m_BackendUrl = BackendServiceController.BaseUrl;
    string m_Username = "AChen1234";
    long m_Gold;
    string m_CardId = string.Empty;
    int m_CardCount = 1;
    int m_CardRarity;
    string m_TitleKey = string.Empty;
    string m_Status = "等待发放";
    bool m_Busy;

    [MenuItem(EditorMenus.Window + "账号礼品")]
    [MenuItem(EditorMenus.Ops + "账号礼品")]
    static void Open() => GetWindow<PlayerGiftGrantWindow>("账号礼品");

    void OnEnable()
    {
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, BackendServiceController.BaseUrl);
        minSize = new Vector2(420f, 360f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("发放礼品到玩家收件箱", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("金币与卡牌可同时填写，至少提供一种。领取后才会入账。本地 Development 后端无需发布密钥。", MessageType.Info);

        using (new EditorGUI.DisabledScope(m_Busy))
        {
            string backendUrl = EditorGUILayout.TextField("后端地址", m_BackendUrl);
            if (!string.Equals(backendUrl, m_BackendUrl, StringComparison.Ordinal))
            {
                m_BackendUrl = backendUrl;
                EditorPrefs.SetString(BackendUrlPreference, backendUrl);
            }

            m_Username = EditorGUILayout.TextField("账号", m_Username);
            m_Gold = EditorGUILayout.LongField("金币", m_Gold);
            m_CardId = EditorGUILayout.TextField("卡牌 Id", m_CardId);
            m_CardCount = EditorGUILayout.IntField("卡牌数量", m_CardCount);
            m_CardRarity = EditorGUILayout.IntField("卡牌稀有度", m_CardRarity);
            m_TitleKey = EditorGUILayout.TextField("文案 Key", m_TitleKey);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("金币礼包")) m_TitleKey = "ui.gifts.pack_gold";
                if (GUILayout.Button("卡牌礼包")) m_TitleKey = "ui.gifts.pack_card";
                if (GUILayout.Button("礼包")) m_TitleKey = "ui.gifts.pack_mixed";
            }

            EditorGUILayout.HelpBox("中间标题走本地化 key，空则按内容自动选金币/卡牌/礼包。", MessageType.None);

            if (GUILayout.Button("发放礼品", GUILayout.Height(30f)))
            {
                _ = GrantAsync();
            }
        }

        EditorGUILayout.HelpBox(
            m_Status,
            m_Status.StartsWith("失败", StringComparison.Ordinal) ? MessageType.Error : MessageType.Info);
    }

    async Task GrantAsync()
    {
        await RunAsync(async () =>
        {
            string username = RequireUsername();
            string cardId = m_CardId?.Trim() ?? string.Empty;
            bool hasCard = cardId.Length > 0 && m_CardCount > 0;
            if (m_Gold <= 0 && !hasCard)
            {
                throw new InvalidOperationException("金币或卡牌至少填写一种，且数量须大于 0");
            }

            if (m_Gold < 0)
            {
                throw new InvalidOperationException("金币不能为负数");
            }

            string summary = username + "：";
            if (m_Gold > 0)
            {
                summary += m_Gold.ToString("N0") + " 金币";
            }

            if (hasCard)
            {
                if (m_Gold > 0) summary += " + ";
                summary += cardId + " ×" + m_CardCount;
            }

            if (!EditorUtility.DisplayDialog("发放礼品", "确认发放到礼品箱？\n" + summary, "发放", "取消"))
            {
                m_Status = "已取消";
                return;
            }

            var body = new GrantRequest
            {
                username = username,
                gold = Math.Max(0, m_Gold),
                titleKey = string.IsNullOrWhiteSpace(m_TitleKey) ? string.Empty : m_TitleKey.Trim(),
                cards = hasCard
                    ? new[] { new GrantCard { cardId = cardId, count = m_CardCount, rarity = m_CardRarity } }
                    : Array.Empty<GrantCard>()
            };
            string response = await SendAsync(
                "POST",
                "/api/accounts/admin/gifts",
                JsonUtility.ToJson(body));
            GrantResult result = JsonUtility.FromJson<GrantResult>(response);
            m_Status = $"发放成功：GiftId={result.giftId} Target={result.targetPlayerId}";
            Debug.Log("[AccountGift] 发放礼品成功. " + m_Status);
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
            m_Busy = true;
            m_Status = "处理中…";
            Repaint();
            await action();
        }
        catch (Exception exception)
        {
            m_Status = "失败：" + exception.Message;
            Debug.LogError("[AccountGift] 操作失败. " + exception.Message);
        }
        finally
        {
            m_Busy = false;
            Repaint();
        }
    }

    Task<string> SendAsync(string method, string path, string json) =>
        EditorBackendHttp.SendJsonAsync(m_BackendUrl, method, path, json, PublishKeyProvider.Resolve(string.Empty));

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
        public long gold;
        public string titleKey;
        public GrantCard[] cards;
    }

    [Serializable]
    sealed class GrantCard
    {
        public string cardId;
        public int count;
        public int rarity;
    }

    [Serializable]
    sealed class GrantResult
    {
        public string giftId;
        public string targetPlayerId;
    }
}
