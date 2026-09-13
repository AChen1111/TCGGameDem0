using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;

public static class PublishGameConfigCsv
{
    public static string Run()
    {
        string key = PublishKeyProvider.Resolve(string.Empty);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new System.Exception("没有发布密钥。请先通过 Tools/后端服务 启动后端。");
        }

        string csvPath = EditorPaths.FromProjectRoot("GameConfig", "game-config.csv");
        string csv;
        using (System.IO.FileStream stream = new System.IO.FileStream(
            csvPath,
            System.IO.FileMode.Open,
            System.IO.FileAccess.Read,
            System.IO.FileShare.ReadWrite))
        using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
        {
            csv = reader.ReadToEnd();
        }

        EditorGameConfigDocument document = GameConfigCsvEditorParser.Parse(csv);

        using HttpClient http = new HttpClient
        {
            BaseAddress = new System.Uri("http://127.0.0.1:5080")
        };
        http.DefaultRequestHeaders.Add("X-Content-Publish-Key", key);

        string draftJson = http.GetStringAsync("/api/game-config/admin/draft").GetAwaiter().GetResult();
        AdminState state = JsonConvert.DeserializeObject<AdminState>(draftJson);
        var replace = new ReplaceRequest
        {
            expectedEditRevision = state.editRevision,
            avatars = document.avatars,
            wallpapers = document.wallpapers,
            cardPacks = document.cardPacks
        };

        StringContent putContent = new StringContent(
            JsonConvert.SerializeObject(replace),
            Encoding.UTF8,
            "application/json");
        HttpResponseMessage put = http.PutAsync("/api/game-config/admin/draft", putContent).GetAwaiter().GetResult();
        string putBody = put.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!put.IsSuccessStatusCode)
        {
            throw new System.Exception("上传草稿失败: " + (int)put.StatusCode + " " + putBody);
        }

        state = JsonConvert.DeserializeObject<AdminState>(putBody);
        StringContent publishContent = new StringContent(
            JsonConvert.SerializeObject(new PublishRequest { expectedEditRevision = state.editRevision }),
            Encoding.UTF8,
            "application/json");
        HttpResponseMessage published = http.PostAsync("/api/game-config/admin/publish", publishContent)
            .GetAwaiter()
            .GetResult();
        string publishedBody = published.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!published.IsSuccessStatusCode)
        {
            throw new System.Exception("发布失败: " + (int)published.StatusCode + " " + publishedBody);
        }

        return
            $"avatars={document.avatars.Length}; wallpapers={document.wallpapers.Length}; cardPacks={document.cardPacks.Length}; {publishedBody}";
    }

    sealed class AdminState
    {
        public long editRevision { get; set; }
        public long draftRevision { get; set; }
    }

    sealed class ReplaceRequest
    {
        public long expectedEditRevision;
        public EditorAvatarConfig[] avatars;
        public EditorWallpaperConfig[] wallpapers;
        public EditorCardPackConfig[] cardPacks;
    }

    sealed class PublishRequest
    {
        public long expectedEditRevision;
    }
}
