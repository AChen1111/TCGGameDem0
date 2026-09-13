using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEditor;

public static class ImportGachaConfig
{
    public static string Run()
    {
        string key = PublishKeyProvider.Resolve(string.Empty);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new System.Exception("没有发布密钥。请先通过 Tools/后端服务 启动后端。");
        }

        using HttpClient http = new HttpClient
        {
            BaseAddress = new System.Uri("http://127.0.0.1:5080")
        };
        http.DefaultRequestHeaders.Add("X-Content-Publish-Key", key);

        string gacha = PutCsv(http, "/api/gacha/admin/config", "card-gacha.csv");
        string cards = PutCsv(http, "/api/gacha/admin/cards", "all-cards.csv");
        return "gacha=" + gacha + "; cards=" + cards;
    }

    static string PutCsv(HttpClient http, string path, string fileName)
    {
        string fullPath = EditorPaths.FromProjectRoot("GameConfig", fileName);
        byte[] body;
        using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (MemoryStream buffer = new MemoryStream())
        {
            stream.CopyTo(buffer);
            body = buffer.ToArray();
        }

        using ByteArrayContent content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        HttpResponseMessage response = http.PutAsync(path, content).GetAwaiter().GetResult();
        string text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            throw new System.Exception(path + " 导入失败: " + (int)response.StatusCode + " " + text);
        }

        return text;
    }
}
