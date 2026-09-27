using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AChen.Configuration;
using AChen.Decks;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class DeckSessionTests
{
    [UnityTest]
    public IEnumerator Deck_reads_refresh_expired_tokens_and_discard_results_after_logout() => UniTask.ToCoroutine(async () =>
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        using var listener = new HttpListener();
        string root = "http://127.0.0.1:" + port;
        listener.Prefixes.Add(root + "/");
        listener.Start();
        string previousUrl = ContentSession.BackendUrl;
        ContentSession.BackendUrl = root;
        var receivedLateRequest = new TaskCompletionSource<bool>();
        var releaseLateResponse = new TaskCompletionSource<bool>();
        var server = Task.Run(async () =>
        {
            for (int i = 0; i < 5; i++)
            {
                var context = await listener.GetContextAsync();
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string body = await reader.ReadToEndAsync();
                if (i == 2)
                {
                    Assert.AreEqual("/api/auth/refresh", context.Request.RawUrl);
                    StringAssert.Contains("test-refresh", body);
                }
                if (i == 3) Assert.AreEqual("Bearer renewed-access", context.Request.Headers["Authorization"]);
                if (i == 4)
                {
                    receivedLateRequest.TrySetResult(true);
                    await releaseLateResponse.Task;
                }
                string json = i == 0 ? AuthJson("first-access") : i == 2 ? AuthJson("renewed-access")
                    : i == 1 ? "{\"code\":\"INVALID_ACCESS_TOKEN\",\"title\":\"Expired\"}" : "[]";
                context.Response.StatusCode = i == 1 ? 401 : 200;
                context.Response.ContentType = "application/json";
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                context.Response.Close();
            }
        });
        var host = new GameObject("DeckSessionNetworkTest");
        host.SetActive(false);
        var session = host.AddComponent<PlayerSession>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            await session.LoginAsync("deck_test", "test-password", timeout.Token);
            Assert.IsEmpty(await session.GetDecksAsync(timeout.Token));
            var pending = session.GetDecksAsync(timeout.Token).Preserve();
            await receivedLateRequest.Task.AsUniTask().AttachExternalCancellation(timeout.Token);
            session.ClearSession();
            releaseLateResponse.TrySetResult(true);
            BackendApiException failure = null;
            try { await pending; }
            catch (BackendApiException ex) { failure = ex; }
            Assert.NotNull(failure);
            Assert.AreEqual("SESSION_CHANGED", failure.Code);
            Assert.IsFalse(session.IsAuthenticated);
            await server;
        }
        finally
        {
            releaseLateResponse.TrySetResult(true);
            listener.Stop();
            session.ClearSession(); // only the isolated loopback URL's token file is touched
            UnityEngine.Object.DestroyImmediate(host);
            ContentSession.BackendUrl = previousUrl;
        }
    });

    static string AuthJson(string accessToken) => "{\"accessToken\":\"" + accessToken
        + "\",\"refreshToken\":\"test-refresh\",\"user\":{\"id\":\"11111111-1111-1111-1111-111111111111\",\"username\":\"deck_test\",\"createdAt\":\"2026-09-27T00:00:00Z\"},"
        + "\"player\":{\"id\":\"11111111-1111-1111-1111-111111111111\",\"nickname\":\"deck_test\",\"ownedCards\":[],\"revision\":0,\"createdAt\":\"2026-09-27T00:00:00Z\",\"updatedAt\":\"2026-09-27T00:00:00Z\"}}";

    [UnityTest]
    public IEnumerator Deck_operations_require_login_even_when_configuration_is_not_ready() => UniTask.ToCoroutine(async () =>
    {
        var host = new GameObject("DeckSessionTest");
        host.SetActive(false);
        try
        {
            var session = host.AddComponent<PlayerSession>();
            var id = Guid.NewGuid();
            var draft = new DeckDraft(new DeckData(id, "Draft", Array.Empty<DeckCardEntry>(), Array.Empty<DeckCardEntry>()));
            var operations = new Func<UniTask>[]
            {
                async () => { await session.GetDecksAsync(); },
                async () => { await session.GetDeckAsync(id); },
                async () => { await session.CreateDeckAsync("Draft"); },
                async () => { await session.SaveDeckAsync(draft); },
                () => session.DeleteDeckAsync(id, 0)
            };
            foreach (var operation in operations)
            {
                BackendApiException failure = null;
                try { await operation(); }
                catch (BackendApiException ex) { failure = ex; }
                Assert.NotNull(failure);
                Assert.AreEqual("INVALID_ACCESS_TOKEN", failure.Code);
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(host); }
    });
}
