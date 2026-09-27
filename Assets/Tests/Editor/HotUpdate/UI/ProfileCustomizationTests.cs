using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using AChen.Configuration;
using AChen.Networking;
using AChen.Player;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ProfileCustomizationTests
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    GameObject sessionObject, configObject, loaderObject, windowObject;
    ProfileEditWindow window;
    static void Set(object host, string field, object value) => host.GetType().GetField(field, Private).SetValue(host,value);
    static T Field<T>(object host,string field) => (T)host.GetType().GetField(field,Private).GetValue(host);
    static void Singleton<T>(T value) where T:MonoSingleton<T> => typeof(MonoSingleton<T>).GetField("s_instance",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,value);

    [Test]
    public void Published_catalog_has_exact_assets_prices_and_individual_masks()
    {
        var data=GameConfigTables.Assemble(Directory.GetFiles("Assets/GameConfiguration","*.bytes").ToDictionary(Path.GetFileNameWithoutExtension,File.ReadAllBytes));
        Assert.AreEqual(230,data.Catalog.Avatars.Length);
        Assert.AreEqual(60,data.Catalog.AvatarFrames.Length);
        Assert.IsTrue(data.Catalog.Avatars.All(x=>x.PriceGold==(x.Id==1010001?0:200)));
        Assert.IsTrue(data.Catalog.AvatarFrames.All(x=>x.PriceGold==(x.Id==1030001?0:500)));
        var catalog=AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>("Assets/AddressableCatalogs/SpriteCatalog.asset");
        foreach(var frame in data.Catalog.AvatarFrames)
        {
            Assert.NotNull(catalog.Get(frame.ResourceKey));Assert.NotNull(catalog.Get(frame.MaskResourceKey));
            Assert.IsTrue(File.Exists("Assets/UI/Sprite/ProfileCustomization/Masks/"+frame.MaskResourceKey+".png"));
        }
    }

    [UnityTest]
    public IEnumerator Window_keeps_separate_drafts_shows_only_owned_and_resets_on_reopen()
    {
        Assert.IsFalse(PlayerSession.HasInstance);Assert.IsFalse(GameConfigManager.HasInstance);Assert.IsFalse(AddressableLoader.HasInstance);
        sessionObject=new GameObject("TestSession");var session=sessionObject.AddComponent<PlayerSession>();Singleton(session);
        Set(session,"m_accessToken","test-session");
        configObject=new GameObject("TestConfig");var manager=configObject.AddComponent<GameConfigManager>();Singleton(manager);
        loaderObject=new GameObject("TestLoader");var loader=loaderObject.AddComponent<AddressableLoader>();Singleton(loader);
        Set(loader,"m_spriteCatalog",AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>("Assets/AddressableCatalogs/SpriteCatalog.asset"));
        Set(loader,"m_prefabCatalog",AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>("Assets/AddressableCatalogs/PrefabCatalog.asset"));
        var data=GameConfigTables.Assemble(Directory.GetFiles("Assets/GameConfiguration","*.bytes").ToDictionary(Path.GetFileNameWithoutExtension,File.ReadAllBytes)).Catalog;
        var now=DateTimeOffset.UtcNow;
        var snapshot=new GameConfigSnapshot(3,1,now,
            data.Avatars.Select(x=>new AvatarConfig(x.Id,x.Name,x.ResourceKey,x.PriceGold,x.SortOrder,x.IsEnabled,x.StartsAt,x.EndsAt)),
            Array.Empty<WallpaperConfig>(),Array.Empty<CardPackConfig>(),
            data.AvatarFrames.Select(x=>new AvatarFrameConfig(x.Id,x.Name,x.ResourceKey,x.PriceGold,x.SortOrder,x.IsEnabled,x.MaskResourceKey,x.StartsAt,x.EndsAt)));
        manager.Store.Replace(snapshot,"test",now,now,false);
        var player=(PlayerData)Activator.CreateInstance(typeof(PlayerData),Private,null,new object[]{Guid.NewGuid(),"原名字",(int?)1010001,new[]{1010001,1010002},(int?)1,new[]{1},Array.Empty<OwnedCardData>(),0L,0L,now,now,1030001,new[]{1030001,1031013}},null);
        typeof(PlayerSession).GetProperty("CurrentPlayer").SetValue(session,player);
        windowObject=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Profile/ProfileEditWindow.prefab"));
        window=Resources.FindObjectsOfTypeAll<ProfileEditWindow>().Single(x=>x.gameObject==windowObject);
        typeof(ProfileEditWindow).GetMethod("AddListeners",Private).Invoke(window,null);
        window.Show(new ProfileEditWindowProperties(ProfileEditTab.Name));
        Field<TMP_InputField>(window,"m_InpName").text="未保存的名字";
        Field<Button>(window,"m_BtnAvatar").onClick.Invoke();
        yield return null;yield return null;
        Assert.AreEqual(2,Field<System.Collections.Generic.List<ProfileCosmeticData>>(window,"m_items").Count);
        // 确认已装备头像为成功的无变更保存，不触发真实网络请求，窗口仍保持打开。
        Field<Button>(window,"m_BtnConfirm").onClick.Invoke();
        yield return null;
        Assert.IsTrue((bool)typeof(AUIScreenController).GetProperty("IsOpened",Private).GetValue(window));
        Assert.IsTrue(Field<Button>(window,"m_BtnConfirm").interactable);
        typeof(ProfileEditWindow).GetMethod("Select",Private).Invoke(window,new object[]{1});
        Field<Button>(window,"m_BtnFrame").onClick.Invoke();
        yield return null;
        var frames=Field<System.Collections.Generic.List<ProfileCosmeticData>>(window,"m_items");
        Assert.AreEqual(2,frames.Count);Assert.IsTrue(frames.All(x=>x.AvatarId==1010001));
        typeof(ProfileEditWindow).GetMethod("Select",Private).Invoke(window,new object[]{1});
        Field<Button>(window,"m_BtnAvatar").onClick.Invoke();
        Assert.AreEqual(1010002,Field<int>(window,"m_avatarDraft"));
        Assert.IsTrue(Field<System.Collections.Generic.List<ProfileCosmeticData>>(window,"m_items").All(x=>x.FrameId==1030001));
        Field<Button>(window,"m_BtnName").onClick.Invoke();
        Assert.AreEqual("未保存的名字",Field<TMP_InputField>(window,"m_InpName").text);
        window.Close();
        yield return new WaitForSecondsRealtime(.4f);
        window.Show(new ProfileEditWindowProperties(ProfileEditTab.Name));
        Assert.AreEqual("原名字",Field<TMP_InputField>(window,"m_InpName").text);
        Assert.AreEqual(1010001,Field<int>(window,"m_avatarDraft"));Assert.AreEqual(1030001,Field<int>(window,"m_frameDraft"));
        // 使用全拥有数据穿过滚动边界，检查最后一行绑定到正确的素材。
        var allOwned=(PlayerData)Activator.CreateInstance(typeof(PlayerData),Private,null,new object[]{player.Id,"原名字",(int?)1010001,data.Avatars.Select(x=>x.Id).ToArray(),(int?)1,new[]{1},Array.Empty<OwnedCardData>(),0L,0L,now,now,1030001,new[]{1030001,1031013}},null);
        typeof(PlayerSession).GetProperty("CurrentPlayer").SetValue(session,allOwned);
        Field<Button>(window,"m_BtnAvatar").onClick.Invoke();
        yield return null;yield return null;
        var grid=Field<GridListController>(window,"m_List");
        // EditMode 中直接提供已加载行资源，滚动验证不依赖 Addressables 的运行时 PlayerLoop。
        typeof(GridListController).GetMethod("BindList",Private).MakeGenericMethod(typeof(ProfileCosmeticData))
            .Invoke(grid,new object[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Profile/ProfileCosmeticRow.prefab"),Field<System.Collections.Generic.List<ProfileCosmeticData>>(window,"m_items")});
        var loop=Field<SuperScrollView.LoopListView2>(grid,"loopListView");
        loop.MovePanelToItemIndexImmediately(45,0);
        yield return null;
        Assert.NotNull(loop.GetShownItemByItemIndex(45));
        Assert.Less(loop.ShownItemCount,46);
        var last=loop.GetShownItemByItemIndex(45);
        var lastItems=Resources.FindObjectsOfTypeAll<ProfileCosmeticItem>().Where(x=>x.transform.IsChildOf(last.transform)).ToArray();
        CollectionAssert.AreEquivalent(new[]{225,226,227,228,229},lastItems.Select(x=>Field<int>(x,"m_index")));
        loop.MovePanelToItemIndexImmediately(0,0);
        yield return null;
        Assert.NotNull(loop.GetShownItemByItemIndex(0));
    }
    [TearDown]
    public void Cleanup()
    {
        if(windowObject!=null)Object.DestroyImmediate(windowObject);
        if(loaderObject!=null)Object.DestroyImmediate(loaderObject);
        if(configObject!=null)Object.DestroyImmediate(configObject);
        if(sessionObject!=null)Object.DestroyImmediate(sessionObject);
        Singleton<PlayerSession>(null);Singleton<GameConfigManager>(null);Singleton<AddressableLoader>(null);
    }
}
