using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AChen.Networking;
using AChen.Decks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

public sealed class DeckApiTests
{
    const string Id = "11111111-1111-1111-1111-111111111111";
    const string Json = "{\"id\":\"" + Id + "\",\"name\":\"Deck\",\"mainDeck\":[],\"extraDeck\":[{\"cardId\":\"01639384\",\"rarity\":1,\"count\":2}],\"revision\":7,\"createdAt\":\"2026-09-27T00:00:00Z\",\"updatedAt\":\"2026-09-27T00:00:00Z\"}";

    [TestCase("mainDeck")]
    [TestCase("extraDeck")]
    [TestCase("revision")]
    [TestCase("id")]
    public void Incomplete_response_is_not_treated_as_an_empty_valid_deck(string field)
    {
        string malformed = Json.Replace("\"" + field + "\":", "\"ignored\":");
        var error = Assert.Throws<BackendApiException>(() => DeckApi.ParseDeckJson(malformed));
        Assert.AreEqual("INVALID_RESPONSE", error.Code);
    }

    [UnityTest]
    public IEnumerator Network_roundtrip_preserves_ids_rarity_revision_and_authenticated_routes() => UniTask.ToCoroutine(async () =>
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        using var listener = new HttpListener();
        string root = "http://127.0.0.1:" + port;
        listener.Prefixes.Add(root + "/");
        listener.Start();
        var requests = new List<(string method, string path, string auth, string body)>();
        var server = Task.Run(async () =>
        {
            for (int i = 0; i < 5; i++)
            {
                var context = await listener.GetContextAsync();
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                requests.Add((context.Request.HttpMethod, context.Request.RawUrl, context.Request.Headers["Authorization"], await reader.ReadToEndAsync()));
                context.Response.StatusCode = i == 4 ? 204 : i == 0 ? 201 : 200;
                if (i != 4)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(i == 1 ? "[" + Json + "]" : Json);
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                }
                context.Response.Close();
            }
        });
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            var api = new DeckApi(new BackendHttpClient(new BackendConfig(root, 5)));
            var created = await api.CreateAsync("access", "Deck", cancellation.Token);
            Assert.AreEqual(new Guid(Id), created.Id);
            Assert.AreEqual("01639384", created.ExtraDeck[0].CardId);
            Assert.AreEqual(1, created.ExtraDeck[0].Rarity);
            Assert.AreEqual(2, created.ExtraDeck[0].Count);
            Assert.AreEqual(7, created.Revision);
            Assert.AreEqual(1, (await api.ListAsync("access", cancellation.Token)).Count);
            var loaded = await api.GetAsync("access", created.Id, cancellation.Token);
            await api.SaveAsync("access", loaded, cancellation.Token);
            await api.DeleteAsync("access", loaded.Id, loaded.Revision, cancellation.Token);
            await server;
            CollectionAssert.AreEqual(new[] { "POST", "GET", "GET", "PUT", "DELETE" }, requests.ConvertAll(x => x.method));
            Assert.IsTrue(requests.TrueForAll(x => x.auth == "Bearer access"));
            Assert.AreEqual("/api/player/decks/" + Id + "?expectedRevision=7", requests[4].path);
            StringAssert.Contains("\"expectedRevision\":7", requests[3].body);
            StringAssert.Contains("\"cardId\":\"01639384\"", requests[3].body);
            StringAssert.Contains("\"rarity\":1", requests[3].body);
        }
        finally { listener.Stop(); }
    });
}
