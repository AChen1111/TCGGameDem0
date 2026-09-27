using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AChen.Configuration;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class UrSessionTests
{
    [UnityTest]
    public IEnumerator Workshop_refreshes_tokens_preserves_failed_assets_and_discards_late_account_responses()=>UniTask.ToCoroutine(async()=>
    {
        AChen.Events.EventCenter.ResetState();
        var probe=new TcpListener(IPAddress.Loopback,0);probe.Start();int port=((IPEndPoint)probe.LocalEndpoint).Port;probe.Stop();
        using var listener=new HttpListener();string root="http://127.0.0.1:"+port;listener.Prefixes.Add(root+"/");listener.Start();
        string previous=ContentSession.BackendUrl;ContentSession.BackendUrl=root;
        var received=new TaskCompletionSource<bool>();var release=new TaskCompletionSource<bool>();
        var server=Task.Run(async()=>
        {
            for(int i=0;i<7;i++)
            {
                var context=await listener.GetContextAsync();using var reader=new StreamReader(context.Request.InputStream,Encoding.UTF8);string body=await reader.ReadToEndAsync();
                if(i==1||i==3){Assert.AreEqual("/api/player/cards/craft",context.Request.RawUrl);StringAssert.Contains("01639384",body);StringAssert.Contains("expectedUrAmount\":30",body);}
                if(i==2)Assert.AreEqual("/api/auth/refresh",context.Request.RawUrl);
                if(i==3)Assert.AreEqual("Bearer renewed",context.Request.Headers["Authorization"]);
                if(i==4){Assert.AreEqual("/api/player/cards/dismantle",context.Request.RawUrl);StringAssert.Contains("expectedRevision\":1",body);StringAssert.Contains("expectedUrAmount\":10",body);}
                if(i==5)Assert.AreEqual("/api/gifts/22222222-2222-2222-2222-222222222222/claim",context.Request.RawUrl);
                if(i==6){received.TrySetResult(true);await release.Task;}
                string json=i==0?Auth("initial"):i==2?Auth("renewed"):i==1?"{\"code\":\"INVALID_ACCESS_TOKEN\",\"title\":\"Expired\"}":i==4?"{\"code\":\"CARD_IN_DECK\",\"title\":\"Blocked\",\"errors\":{\"decks\":[\"A\"]}}":i==5?"{\"player\":"+Player(140,2)+",\"urGained\":20}":"{\"player\":"+Player(i==6?110:120,i==6?3:1)+",\"urAmount\":30}";
                context.Response.StatusCode=i==1?401:i==4?422:200;context.Response.ContentType="application/json";byte[] bytes=Encoding.UTF8.GetBytes(json);context.Response.ContentLength64=bytes.Length;await context.Response.OutputStream.WriteAsync(bytes,0,bytes.Length);context.Response.Close();
            }
        });
        var go=new GameObject("UrSessionTest");go.SetActive(false);var session=go.AddComponent<PlayerSession>();using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            await session.LoginAsync("ur_test","test-password",timeout.Token);Assert.AreEqual(150,session.CurrentPlayer.Ur);
            var crafted=await session.CraftCardAsync("01639384",30,timeout.Token);Assert.AreEqual(120,crafted.Player.Ur);Assert.AreSame(crafted.Player,session.CurrentPlayer);
            var saved=session.CurrentPlayer;BackendApiException failure=null;
            try{await session.DismantleCardAsync("01639384",0,1,10,timeout.Token);}catch(BackendApiException ex){failure=ex;}
            Assert.AreEqual("CARD_IN_DECK",failure.Code);Assert.AreSame(saved,session.CurrentPlayer);
            var gift=await session.ClaimGiftAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"),timeout.Token);Assert.AreEqual(20,gift.UrGained);Assert.AreEqual(140,session.CurrentPlayer.Ur);
            var late=session.CraftCardAsync("00213326",30,timeout.Token).Preserve();await received.Task.AsUniTask().AttachExternalCancellation(timeout.Token);session.ClearSession();release.TrySetResult(true);failure=null;
            try{await late;}catch(BackendApiException ex){failure=ex;}
            Assert.AreEqual("SESSION_CHANGED",failure.Code);Assert.IsNull(session.CurrentPlayer);await server;
        }
        finally{release.TrySetResult(true);listener.Stop();session.ClearSession();UnityEngine.Object.DestroyImmediate(go);ContentSession.BackendUrl=previous;AChen.Events.EventCenter.ResetState();}
    });
    static string Player(long ur,int revision)=>"{\"id\":\"11111111-1111-1111-1111-111111111111\",\"nickname\":\"ur_test\",\"ur\":"+ur+",\"revision\":"+revision+",\"ownedCards\":[],\"createdAt\":\"2026-09-27T00:00:00Z\",\"updatedAt\":\"2026-09-27T00:00:00Z\"}";
    static string Auth(string access)=>"{\"accessToken\":\""+access+"\",\"refreshToken\":\"test-refresh\",\"user\":{\"id\":\"11111111-1111-1111-1111-111111111111\",\"username\":\"ur_test\",\"createdAt\":\"2026-09-27T00:00:00Z\"},\"player\":"+Player(150,0)+"}";
}
