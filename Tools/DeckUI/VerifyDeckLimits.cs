using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AChen.Configuration;
using AChen.Decks;
using AChen.Networking;
using AChen.Player;

// Local UI/validation acceptance. Never creates, saves or deletes a backend deck.
public static class VerifyDeckLimits
{
    const string Output = ".doc/verification/deck-limits/";
    static readonly string[] Ids = { "23434538", "14558127", "21143940" };
    static T Active<T>() where T:Component => Resources.FindObjectsOfTypeAll<T>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy);
    static object Field(object o,string name)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static void Set(object o,string name,object value)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    static void Call(object o,string name)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,null);
    static T Ref<T>(UnityEngine.Object o,string name) where T:UnityEngine.Object=>(T)new SerializedObject(o).FindProperty(name).objectReferenceValue;
    static void Check(bool b,string message){if(!b)throw new Exception(message);}
    static DeckEditorState Model(DeckEditWindow w)=>(DeckEditorState)Field(w,"m_state");
    static DeckCardCell PoolCell(string id)=>Resources.FindObjectsOfTypeAll<DeckCardCell>().First(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy&&x.Data!=null&&!x.Data.InDeck&&x.Data.CardId==id);
    static void Click(DeckCardCell cell)
    {
        var p=RectTransformUtility.WorldToScreenPoint(Active<UIFrame>().UICamera,cell.View.ArtRect.TransformPoint(cell.View.ArtRect.rect.center));
        var e=new PointerEventData(EventSystem.current){position=p,button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&hits[0].gameObject==cell.gameObject,"Card click raycast blocked");
        ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.pointerUpHandler);
    }
    static async Task Find(DeckEditWindow w,string id)
    {Ref<TMP_InputField>(w,"m_InpSearch").text=LocalizationService.GetText("card."+id+".name");await Task.Delay(400);}
    static void Badge(DeckCardCell cell)
    {
        int limit=LocalGameConfiguration.DeckRules.GetMaxCopies(cell.Data.CardId);
        var icon=Ref<Image>(cell,"m_LimitIcon");
        Check(icon.gameObject.activeSelf==(limit<3),"Limit visibility mismatch: "+cell.Data.CardId);
        if(limit<3)Check(icon.sprite.name=="GUI_T_Icon1_Limit0"+limit,"Wrong limit sprite");
        Check(!icon.raycastTarget,"Limit icon intercepts clicks");
        Check(!Ref<TMP_Text>(cell,"m_Rarity").gameObject.activeSelf,"Thumbnail name still visible");
        if(!cell.Data.InDeck)Check(Ref<TMP_Text>(cell,"m_Quantity").text==cell.Data.Owned.ToString(),"Quantity format mismatch");
    }
    static async Task Capture(string name)
    {
        Directory.CreateDirectory(Output);
        ScreenCapture.CaptureScreenshot(Path.GetFullPath(Output+name+".png"));await Task.Delay(450);
    }
    static DeckData Empty()=>new DeckData(Guid.Empty,"禁限验收临时草稿",Array.Empty<DeckCardEntry>(),Array.Empty<DeckCardEntry>());
    public static async Task<string> Main()
    {
        var records=new List<string>();var rules=LocalGameConfiguration.DeckRules;
        foreach(var id in Ids)
        {
            int limit=Array.IndexOf(Ids,id);Check(rules.GetMaxCopies(id)==limit,"Config limit mismatch");
            var state=new DeckEditorState(Empty());var inventory=new[]{new DeckCardEntry(id,0,3),new DeckCardEntry(id,1,3)};
            for(int i=0;i<limit;i++)Check(state.TryChange(id,0,1,rules,inventory).IsValid,"Allowed copy rejected");
            var result=state.TryChange(id,1,1,rules,inventory);
            Check(!result.IsValid&&result.Issues.Any(x=>x.Code==DeckIssueCode.CopyLimitExceeded&&x.Allowed==limit),"Excess cross-rarity copy accepted");
            Check(state.Count(id,0)==limit&&state.Count(id,1)==0,"Rejected addition changed draft");
            records.Add(id+": max="+limit+"; allowed copies pass, next copy at another rarity rejected without mutating draft.");
        }
        var decks=await PlayerSession.Instance.GetDecksAsync();Check(decks.Count>0,"No existing deck to open");
        Active<UIFrame>().OpenWindow(AddressKeys.Prefab.DeckListWindow);await Task.Delay(600);
        Active<UIFrame>().OpenWindow(AddressKeys.Prefab.DeckEditWindow,new DeckEditWindowProperties(decks[0].Id));await Task.Delay(1200);
        var w=Active<DeckEditWindow>();var original=Model(w);var selected=Field(w,"m_selected");var input=Ref<TMP_InputField>(w,"m_InpSearch");string search=input.text;
        try
        {
            Set(w,"m_state",new DeckEditorState(Empty()));input.SetTextWithoutNotify("");Call(w,"Refresh");await Task.Delay(450);
            foreach(var id in Ids)
            {
                await Find(w,id);Badge(PoolCell(id));await Capture("0"+(Array.IndexOf(Ids,id)+1)+"-limit-"+rules.GetMaxCopies(id));
                if(rules.GetMaxCopies(id)==0)
                {
                    Click(PoolCell(id));await Task.Delay(300);
                    Check(Model(w).Count(id,PoolCell(id).Data.Rarity)==0&&((IList)Field(w,"m_cardMoves")).Count==0,"Forbidden click added or animated a card");
                    foreach(var message in Resources.FindObjectsOfTypeAll<MessageWindow>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy))message.UI_Close();
                    await Task.Delay(450);records.Add("Owned forbidden card: real raycast click rejected, no draft change or flight.");
                }
                if(rules.GetMaxCopies(id)==1)
                {
                    var cell=PoolCell(id);Check(cell.Data.Owned>0,"Limited card unavailable for UI click test");
                    Click(cell);await Task.Delay(450);Check(Model(w).Count(id,cell.Data.Rarity)==1,"Limited card first click failed");
                    Click(PoolCell(id));await Task.Delay(300);
                    Check(Model(w).Count(id,cell.Data.Rarity)==1,"Repeated UI addition exceeded limit one");
                    foreach(var message in Resources.FindObjectsOfTypeAll<MessageWindow>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy))message.UI_Close();
                    await Task.Delay(450);Set(w,"m_state",new DeckEditorState(Empty()));Call(w,"Refresh");await Task.Delay(350);
                    records.Add("Limit-one card: first real click adds; repeated click is rejected. This account owns only one of this ID, so rule-specific cross-rarity cap is covered by the isolated state checks.");
                }
            }
            // UI click/flight with the account's real inventory; no synthetic ownership.
            input.text="";await Task.Delay(400);
            var pool=(List<DeckCardData>)Field(w,"m_pool");
            var normal=pool.First(x=>x.Owned>=2&&rules.GetMaxCopies(x.CardId)==3);
            await Find(w,normal.CardId);Click(PoolCell(normal.CardId));await Task.Delay(70);
            Check(Model(w).Count(normal.CardId,normal.Rarity)==1,"Click failed to add card");
            Check(((IList)Field(w,"m_cardMoves")).Count==1,"Click flight missing");
            await Task.Delay(450);Check(((IList)Field(w,"m_cardMoves")).Count==0,"Flight did not complete");
            Click(PoolCell(normal.CardId));await Task.Delay(450);Check(Model(w).Count(normal.CardId,normal.Rarity)==2,"Second click failed");
            records.Add("Real inventory: two raycast clicks add two copies; flight starts and completes. Normal-card limit icon hidden.");
            // Fixture models an existing deck with a now-forbidden card for visual inspection only.
            var fixture=Ids.Concat(LocalGameConfiguration.Data.AllCards.Where(x=>rules.GetMaxCopies(x.CardId)==3&&rules.TryGetSection(x.CardId,out var section)&&section==DeckSection.Main).Select(x=>x.CardId).Take(5))
                .Select(x=>new DeckCardEntry(x,0,1)).ToArray();
            Set(w,"m_state",new DeckEditorState(new DeckData(Guid.Empty,"禁限角标 · 本地展示",fixture,Array.Empty<DeckCardEntry>())));
            input.SetTextWithoutNotify("");Call(w,"Refresh");await Task.Delay(800);Canvas.ForceUpdateCanvases();
            var placed=(List<DeckCardCell>)Field(w,"m_mainCells");Check(placed.Count==8,"Expected eight fixture cards");
            foreach(var cell in placed)Badge(cell);
            Check(placed.All(x=>Mathf.Abs(((RectTransform)x.transform).anchoredPosition.y-((RectTransform)placed[0].transform).anchoredPosition.y)<1),"Eight deck cards do not fit one row");
            var rows=Resources.FindObjectsOfTypeAll<DeckCardRow>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy).ToArray();
            Check(rows.All(x=>x.RowCardCount==8),"Pool row is not eight columns");
            foreach(var scroll in new[]{Ref<ScrollRect>(w,"m_MainScroll"),Ref<ScrollRect>(w,"m_ExtraScroll")})
            {
                var grid=Resources.FindObjectsOfTypeAll<GridLayoutGroup>().Single(x=>x.transform==scroll.content);
                Check(grid.constraintCount==8&&grid.cellSize.x*8+grid.spacing.x*7+grid.padding.horizontal<=scroll.viewport.rect.width+.1f,"Grid overflows viewport");
            }
            await Capture("04-eight-columns-overview");
            records.Add("Pool and deck show all three limit badges correctly; unrestricted icons hidden; names hidden; numeric quantity format; main/extra grids and pool use eight columns.");
            records.Add("Overview uses a local display fixture containing a forbidden card to verify an old deck's icon. Not saved or sent to backend.");
        }
        finally
        {
            Call(w,"StopCardMoves");Set(w,"m_state",original);Set(w,"m_selected",selected);input.SetTextWithoutNotify(search);Call(w,"Refresh");
        }
        Check(ReferenceEquals(Model(w),original),"Original draft not restored");
        records.Add("Original editor state restored. No create/save/delete requests or inventory writes.");
        File.WriteAllLines(Output+"verification.txt",records);
        return string.Join("\n",records);
    }
}
