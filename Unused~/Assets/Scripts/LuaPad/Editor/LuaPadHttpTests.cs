using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class LuaPadHttpTests
{
    [Test]
    public void Rpc_Post_ReturnsHandlerJson()
    {
        using (LuaPadHttpServer http = LuaPadHttpServer.Start(msg => new JObject { ["pong"] = (string)msg["ping"] }))
        {
            JObject obj = PostRpc(http.Origin, "{\"ping\":\"hi\"}");
            Assert.AreEqual("hi", (string)obj["pong"]);
        }
    }

    [Test]
    public void Rpc_PostCompletionShape_RoundTripsItems()
    {
        using (LuaPadHttpServer http = LuaPadHttpServer.Start(msg => new JObject
        {
            ["id"] = msg["id"],
            ["items"] = new JArray
            {
                new JObject
                {
                    ["label"] = "Error",
                    ["detail"] = "(strMessage, strCategory) -> nil",
                    ["insertText"] = "Error",
                    ["kind"] = 1,
                },
            },
        }))
        {
            JObject obj = PostRpc(http.Origin, "{\"method\":\"completion\",\"id\":\"1\"}");
            Assert.AreEqual("1", (string)obj["id"]);
            Assert.AreEqual("Error", (string)obj["items"][0]["label"]);
            Assert.AreEqual("(strMessage, strCategory) -> nil", (string)obj["items"][0]["detail"]);
        }
    }

    static JObject PostRpc(string origin, string body)
    {
        var req = (HttpWebRequest)WebRequest.Create(origin + "/rpc");
        req.Method = "POST";
        req.Proxy = null;
        req.ContentType = "application/json";
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        req.ContentLength = bytes.Length;
        using (Stream s = req.GetRequestStream())
        {
            s.Write(bytes, 0, bytes.Length);
        }
        using (var resp = (HttpWebResponse)req.GetResponse())
        using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
        {
            return JObject.Parse(reader.ReadToEnd());
        }
    }
}
