using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class SeedAChenSocial
{
    const string Password = "correct-horse-42";
    static readonly string[] CandidateUsers = { "AChen", "AChen1234" };
    static readonly string[] Bots = { "DemoAlice", "DemoBob", "DemoCarol" };

    public static async Task<string> Run()
    {
        string url = "http://127.0.0.1:5080";
        PublishKeyProvider.RequireKey(string.Empty);
        string key = PublishKeyProvider.Resolve(string.Empty);

        await GetAsync(url, "/health", null);
        string username = null;
        PlayerLookup player = null;
        for (int i = 0; i < CandidateUsers.Length; i++)
        {
            string body = await EditorBackendHttp.GetAsync(
                url,
                "/api/accounts/admin/player?username=" + Uri.EscapeDataString(CandidateUsers[i]),
                key,
                true);
            if (string.IsNullOrEmpty(body))
            {
                continue;
            }

            player = JsonUtility.FromJson<PlayerLookup>(body);
            if (player != null && !string.IsNullOrEmpty(player.id))
            {
                username = CandidateUsers[i];
                break;
            }
        }

        if (player == null || string.IsNullOrEmpty(username))
        {
            throw new InvalidOperationException("未找到账号 AChen / AChen1234");
        }

        var sb = new StringBuilder();
        sb.Append("账号=").Append(username)
            .Append(" 昵称=").Append(player.nickname)
            .Append(" Id=").Append(player.id)
            .AppendLine();

        sb.AppendLine(await GrantGift(url, key, username, 500, Array.Empty<GrantCard>()));
        sb.AppendLine(await GrantGift(url, key, username, 0, new[]
        {
            new GrantCard { cardId = "14558127", count = 2, rarity = 0 }
        }));
        sb.AppendLine(await GrantGift(url, key, username, 300, new[]
        {
            new GrantCard { cardId = "08491308", count = 1, rarity = 1 },
            new GrantCard { cardId = "01639384", count = 1, rarity = 0 }
        }));

        for (int i = 0; i < Bots.Length; i++)
        {
            sb.AppendLine(await SendFriendRequest(url, Bots[i], player.id));
        }

        return sb.ToString().Trim();
    }

    static async Task<string> GrantGift(string url, string key, string username, long gold, GrantCard[] cards)
    {
        var body = new GrantRequest
        {
            username = username,
            gold = gold,
            titleKey = gold > 0 && cards != null && cards.Length > 0
                ? "ui.gifts.pack_mixed"
                : cards != null && cards.Length > 0
                    ? "ui.gifts.pack_card"
                    : "ui.gifts.pack_gold",
            cards = cards ?? Array.Empty<GrantCard>()
        };
        string response = await EditorBackendHttp.SendJsonAsync(
            url,
            "POST",
            "/api/accounts/admin/gifts",
            JsonUtility.ToJson(body),
            key);
        GrantResult result = JsonUtility.FromJson<GrantResult>(response);
        return "礼包 giftId=" + result.giftId + " gold=" + gold + " cards=" + cards.Length;
    }

    static async Task<string> SendFriendRequest(string url, string botName, string targetPlayerId)
    {
        AuthResponse auth = await LoginOrRegister(url, botName);
        string response;
        try
        {
            response = await SendAuthJson(
                url,
                "POST",
                "/api/friends/requests",
                "{\"targetPlayerId\":\"" + targetPlayerId + "\"}",
                auth.accessToken);
        }
        catch (Exception exception)
        {
            return "申请 " + botName + " 失败: " + exception.Message;
        }

        RequestCreated created = JsonUtility.FromJson<RequestCreated>(response);
        return "申请 from=" + botName + " nickname=" + auth.player.nickname + " requestId=" + created.id;
    }

    static async Task<AuthResponse> LoginOrRegister(string url, string username)
    {
        string json = "{\"username\":\"" + username + "\",\"password\":\"" + Password + "\"}";
        try
        {
            string body = await SendAuthJson(url, "POST", "/api/auth/login", json, null);
            return RequireAuth(body, username);
        }
        catch (Exception)
        {
            string body = await SendAuthJson(url, "POST", "/api/auth/register", json, null);
            return RequireAuth(body, username);
        }
    }

    static AuthResponse RequireAuth(string body, string username)
    {
        AuthResponse auth = JsonUtility.FromJson<AuthResponse>(body);
        if (auth == null || string.IsNullOrEmpty(auth.accessToken) || auth.player == null)
        {
            throw new InvalidOperationException(username + " 登录数据不完整");
        }

        return auth;
    }

    static async Task<string> GetAsync(string baseUrl, string path, string accessToken)
    {
        using (var request = UnityWebRequest.Get(EditorBackendHttp.BuildUrl(baseUrl, path)))
        {
            request.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            }

            await EditorBackendHttp.AwaitAsync(request);
            EditorBackendHttp.ThrowIfFailed(request);
            return request.downloadHandler.text;
        }
    }

    static async Task<string> SendAuthJson(string baseUrl, string method, string path, string json, string accessToken)
    {
        using (var request = new UnityWebRequest(EditorBackendHttp.BuildUrl(baseUrl, path), method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            if (json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            }

            await EditorBackendHttp.AwaitAsync(request);
            EditorBackendHttp.ThrowIfFailed(request);
            return request.downloadHandler.text;
        }
    }

    [Serializable]
    sealed class PlayerLookup
    {
        public string id;
        public string username;
        public string nickname;
    }

    [Serializable]
    sealed class AuthResponse
    {
        public string accessToken;
        public PlayerLookup player;
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
    }

    [Serializable]
    sealed class RequestCreated
    {
        public string id;
    }
}
