using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AChen.Player;
using AChen.Networking;

// Visual acceptance fixture only. No API calls, saved token writes or production account mutations.
// Start a normal local-assets Play session before running. Stop Play to discard fixture state.
public static class VerifyUrUI
{
    static T Active<T>() where T:Component=>Resources.FindObjectsOfTypeAll<T>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy);
    static UnityEngine.Object Ref(UnityEngine.Object o,string field)=>new SerializedObject(o).FindProperty(field).objectReferenceValue;
    static void Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
    static UIFrame Frame()=>Resources.FindObjectsOfTypeAll<UIFrame>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy&&x.name.Contains("UIFrame")&&!x.name.Contains("Login"));
    static PlayerData Fixture(long ur,int normal=0)=>AuthApi.ParsePlayerJson("{\"id\":\"11111111-1111-1111-1111-111111111111\",\"nickname\":\"工坊验收样本\",\"avatarId\":1010001,\"avatarFrameId\":1030001,\"ownedAvatarIds\":[1010001],\"ownedAvatarFrameIds\":[1030001],\"backgroundId\":1,\"ownedBackgroundIds\":[1],\"gold\":12000,\"ur\":"+ur+",\"revision\":1,\"createdAt\":\"2026-09-27T00:00:00Z\",\"updatedAt\":\"2026-09-27T00:00:00Z\",\"ownedCards\":[{\"cardId\":\"26077389\",\"rarity\":0,\"count\":3},{\"cardId\":\"26077389\",\"rarity\":1,\"count\":2},{\"cardId\":\"26077389\",\"rarity\":2,\"count\":1},{\"cardId\":\"26077389\",\"rarity\":3,\"count\":3},{\"cardId\":\"26077389\",\"rarity\":4,\"count\":2},{\"cardId\":\"01639384\",\"rarity\":0,\"count\":"+normal+"}]}");
    static void Player(long ur,int normal=0)
    {
        var player=Fixture(ur,normal);
        typeof(PlayerSession).GetProperty("CurrentPlayer").SetValue(PlayerSession.Instance,player);
        AChen.Events.EventCenter.Dispatch(AChen.Events.GameEvent.PlayerUrChanged,(long?)ur);
        AChen.Events.EventCenter.Dispatch(AChen.Events.GameEvent.PlayerOwnedCardsChanged,player);
    }
    public static string Hall()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Play mode required.");
        // Block accidental fixture transactions while preserving the user's on-disk refresh token.
        typeof(PlayerSession).GetField("m_accessToken",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(PlayerSession.Instance,null);
        Player(150);return "Visual fixture: UR 150, gold 12000. No server or persisted account data changed.";
    }
    public static async Task<string> LoadFixtureHall()
    {
        Hall();
        await GameConfigManager.Instance.InitializeAsync();
        await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
        await Task.Delay(1000);
        return "Loaded the actual lobby with memory-only sample assets; production API calls disabled.";
    }
    public static async Task<string> Workshop()
    {
        Frame().OpenWindow(AddressKeys.Prefab.ShopWindows);await Task.Delay(700);
        var shop=Active<ShopWindow>();var so=new SerializedObject(shop);var p=so.FindProperty("m_ChooseButtons");((Button)p.GetArrayElementAtIndex(4).objectReferenceValue).onClick.Invoke();await Task.Delay(1000);
        return State();
    }
    public static string State()
    {
        var panel=Active<CardWorkshopPanel>();var search=(TMP_InputField)Ref(panel,"m_Search");
        return "Search="+search.text+"; visibleCards="+Resources.FindObjectsOfTypeAll<CardWorkshopItem>().Count(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy)+"; texts="+string.Join(" | ",Resources.FindObjectsOfTypeAll<TMP_Text>().Where(x=>x.gameObject.activeInHierarchy&&x.transform.IsChildOf(panel.transform)).Select(x=>x.text));
    }
    public static async Task<string> Search()
    {var panel=Active<CardWorkshopPanel>();((TMP_InputField)Ref(panel,"m_Search")).text="01639384";await Task.Delay(500);return State();}
    public static string Confirm()
    {var detail=Active<CardWorkshopDetail>();((Button)Ref(detail,"m_Action")).onClick.Invoke();return "Opened actual craft confirmation; cancel instead of submitting sample data.";}
    public static string Cancel()
    {var choose=Active<ChooseWindow>();((Button)Ref(choose,"m_BtnNo")).onClick.Invoke();return "Cancelled.";}
    public static async Task<string> RefreshAndFilter()
    {Player(120,1);var panel=Active<CardWorkshopPanel>();((Toggle)Ref(panel,"m_OnlyCraftable")).isOn=true;await Task.Delay(500);return State();}
    public static async Task<string> Dismantle()
    {var panel=Active<CardWorkshopPanel>();((TMP_InputField)Ref(panel,"m_Search")).text="";((Button)Ref(panel,"m_DismantleTab")).onClick.Invoke();await Task.Delay(600);((TMP_InputField)Ref(panel,"m_Search")).text="26077389";await Task.Delay(500);var detail=Active<CardWorkshopDetail>();var p=new SerializedObject(detail).FindProperty("m_Versions");((Button)p.GetArrayElementAtIndex(4).objectReferenceValue).onClick.Invoke();((Button)Ref(detail,"m_Plus")).onClick.Invoke();return State();}
    public static string DeckNotice()
    {Frame().OpenWindow(AddressKeys.Prefab.UrNoticeWindow,new UrNoticeProperties(new LocalizedMessage("ui.workshop.deck_blocked",new Dictionary<string,object>{{"decks","测试卡组 A、测试草稿 B"}})));return "Deck-protection rejection presentation.";}
    public static string GiftNotice()
    {Player(140,1);Frame().OpenWindow(AddressKeys.Prefab.UrNoticeWindow,new UrNoticeProperties(new LocalizedMessage("ui.workshop.gift_overflow",new Dictionary<string,object>{{"amount",20}})));return "Gift-overflow presentation and balance refreshed.";}
    public static string CloseNotice(){((Button)Ref(Active<UrNoticeWindow>(),"m_Ok")).onClick.Invoke();return "Closed OK notice.";}
    public static async Task<string> Reopen()
    {Active<ShopWindow>().UI_Close();await Task.Delay(500);await Workshop();return "Shop closed and reopened. "+State();}
    public static async Task<string> Draw()
    {
        var cards=LocalGameConfiguration.Data.AllCards.Take(5).ToArray();var data=new List<CardPickViewData>();
        for(int i=0;i<cards.Length;i++)data.Add(new CardPickViewData{cardId=cards[i].CardId,cardShaderType=(CardShaderType)i,cardTexture=await CardPoolAddress.LoadCardTextureAsync(cards[i].SourcePool,cards[i].CardId),overflowUr=i%2==0?10+5*i:0});
        Frame().OpenWindow(AddressKeys.Prefab.CardPickWindow,new CardPickWindowProperty(data));await Task.Delay(800);return "Draw reveal opened. Advance follows the production reveal sequence.";
    }
    public static async Task<string> RevealAll()
    {var controller=Active<CardPickController>();for(int i=0;i<10;i++){controller.Advance();await Task.Delay(950);}await Task.Delay(1000);return "Final reveal: "+string.Join(" | ",Resources.FindObjectsOfTypeAll<CardOverflowBadge>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy).Select(x=>((GameObject)Ref(x,"m_Visual")).activeSelf.ToString()));}
    public static string InspectDraw(){Active<CardPickController>().NotifyInspect(0);return "Requested details on the overflow card.";}
}
