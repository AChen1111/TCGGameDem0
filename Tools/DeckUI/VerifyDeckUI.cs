using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using AChen.Decks;
using AChen.Player;
using AChen.Networking;
using AChen.Configuration;

// Acceptance tooling only: temporary test decks are deleted after persistence checks.
public static class VerifyDeckUI
{
    static T Active<T>() where T:Component=>Resources.FindObjectsOfTypeAll<T>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy);
    static UnityEngine.Object Ref(UnityEngine.Object o,string field)=>new SerializedObject(o).FindProperty(field).objectReferenceValue;
    static object Value(object o,string field)=>o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static void Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
    public static string Assets()
    {
        var missing=new List<string>();int checkedFields=0;
        foreach(var path in Directory.GetFiles("Assets/UI/Prefab/Hall/Deck","*.prefab"))
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            foreach(var c in Resources.FindObjectsOfTypeAll<MonoBehaviour>().Where(x=>x.transform==root.transform||x.transform.IsChildOf(root.transform)))
            {
                if(!c.GetType().Name.StartsWith("Deck"))continue;
                var so=new SerializedObject(c);
                foreach(var field in c.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Where(x=>x.IsDefined(typeof(SerializeField))))
                {
                    var prop=so.FindProperty(field.Name);
                    if(prop.propertyType==SerializedPropertyType.ObjectReference){checkedFields++;if(prop.objectReferenceValue==null)missing.Add(path+":"+field.Name);}
                    if(prop.isArray){for(int i=0;i<prop.arraySize;i++){checkedFields++;if(prop.GetArrayElementAtIndex(i).objectReferenceValue==null)missing.Add(path+":"+field.Name+"["+i+"]");}}
                }
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        foreach(var name in new[]{"TCG/UI/DeckCardFoil","TCG/UI/DeckBackground"})if(ShaderUtil.ShaderHasError(Shader.Find(name)))missing.Add(name+":Shader error");
        if(missing.Count>0)throw new Exception(string.Join("\n",missing));
        var table=BinaryTable.Decode(File.ReadAllBytes("Assets/GameConfiguration/Translations.bytes"));
        int key=table.Column("Key","string");int keys=table.Rows.Count(x=>((string)x[key]).StartsWith("ui.deck."));
        return "Verified "+checkedFields+" serialized references; no shader errors; compiled deck translation keys="+keys;
    }
    public static string Boot()
    {EditorSceneManager.OpenScene("Assets/Scenes/PreInit.unity");return "Opened existing PreInit startup scene.";}
    public static string State()
    {
        bool session=PlayerSession.Instance.IsAuthenticated;
        return "authenticated="+session+" config="+LocalGameConfiguration.IsReady+" frames="+Resources.FindObjectsOfTypeAll<UIFrame>().Count(x=>x.gameObject.scene.IsValid())
            +" windows="+string.Join(",",Resources.FindObjectsOfTypeAll<AWindowController>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy).Select(x=>x.name));
    }
    public static string List()
    {Active<UIFrame>().OpenWindow(AddressKeys.Prefab.DeckListWindow);return State();}
    public static string Name()
    {((Button)Ref(Active<DeckListWindow>(),"m_BtnNew")).onClick.Invoke();return "Opened real name window.";}
    public static async Task<string> Create()
    {
        var name=Active<DeckNameWindow>();((TMP_InputField)Ref(name,"m_InpName")).text="金色卡盒 · 界面验收";
        ((Button)Ref(name,"m_BtnConfirm")).onClick.Invoke();await Task.Delay(1400);return EditorState();
    }
    public static string EditorState()
    {
        var window=Active<DeckEditWindow>();var state=(DeckEditorState)Value(window,"m_state");
        if(!(bool)Value(window,"m_ready"))return "Editor loading.";
        var data=state.Draft.ToData();return "id="+data.Id+" name="+data.Name+" main="+data.MainDeck.Sum(x=>x.Count)+" extra="+data.ExtraDeck.Sum(x=>x.Count)
            +" revision="+data.Revision+" dirty="+state.IsDirty+" results="+((TMP_Text)Ref(window,"m_TxtResults")).text;
    }
    public static async Task<string> Search(string term)
    {((TMP_InputField)Ref(Active<DeckEditWindow>(),"m_InpSearch")).text=term;await Task.Delay(450);return EditorState();}
    public static async Task<string> AddOwned()
    {
        var window=Active<DeckEditWindow>();var cards=(List<DeckCardData>)Value(window,"m_pool");var card=cards.First(x=>x.Owned>0&&LocalGameConfiguration.DeckRules.GetMaxCopies(x.CardId)>0);
        window.SelectCard(card);Call(window,"AddSelected");await Task.Delay(400);return EditorState();
    }
    public static async Task<string> SearchCheck()
    {
        var window=Active<DeckEditWindow>();var input=(TMP_InputField)Ref(window,"m_InpSearch");
        input.text="青眼";await Task.Delay(450);var pool=(List<DeckCardData>)Value(window,"m_pool");
        if(pool.Count==0||pool.Any(x=>!CardNameSearch.Matches(x.CardId,"青眼")))throw new Exception("Name search mismatch");
        int found=pool.Count;input.text="zzzz__no_cards";await Task.Delay(450);
        if(((List<DeckCardData>)Value(window,"m_pool")).Count!=0||!((GameObject)Ref(window,"m_PoolEmpty")).activeSelf)throw new Exception("Empty search failed");
        input.text="";await Task.Delay(450);return "Fuzzy name search="+found+"; empty result state passed; clear restored="+((List<DeckCardData>)Value(window,"m_pool")).Count;
    }
    public static async Task<string> Save()
    {((Button)Ref(Active<DeckEditWindow>(),"m_BtnSave")).onClick.Invoke();await Task.Delay(900);return EditorState();}
    public static async Task<string> Server()
    {
        var window=Active<DeckEditWindow>();var state=(DeckEditorState)Value(window,"m_state");var draft=state.Draft.ToData();
        var saved=await PlayerSession.Instance.GetDeckAsync(draft.Id);
        string Entries(IReadOnlyList<DeckCardEntry> cards)=>string.Join(";",cards.OrderBy(x=>x.CardId).ThenBy(x=>x.Rarity).Select(x=>x.CardId+":"+x.Rarity+":"+x.Count));
        if(saved.Revision!=draft.Revision||saved.Name!=draft.Name||Entries(saved.MainDeck)!=Entries(draft.MainDeck)||Entries(saved.ExtraDeck)!=Entries(draft.ExtraDeck))throw new Exception("Server round trip mismatch");
        return "Server confirmed deck="+saved.Id+" revision="+saved.Revision+" main="+saved.MainDeck.Sum(x=>x.Count)+" extra="+saved.ExtraDeck.Sum(x=>x.Count);
    }
    static async Task Capture(string name)
    {
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        await Task.Delay(300);Directory.CreateDirectory(".doc/verification/deck-ui");
        ScreenCapture.CaptureScreenshot(Path.GetFullPath(".doc/verification/deck-ui/"+name+".png"));await Task.Delay(350);
    }
    static void Click(UnityEngine.Object target,string field)=>((Button)Ref(target,field)).onClick.Invoke();
    static DeckEditorState EditorModel(DeckEditWindow w)=>(DeckEditorState)Value(w,"m_state");
    static void DismissMessage()
    { foreach(var m in Resources.FindObjectsOfTypeAll<MessageWindow>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy))m.UI_Close(); }
    static DeckCardCell Cell(DeckEditWindow w,DeckCardData data)=>Resources.FindObjectsOfTypeAll<DeckCardCell>()
        .First(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy&&Value(x,"m_data")!=null&&((DeckCardData)Value(x,"m_data")).InDeck==data.InDeck
            &&((DeckCardData)Value(x,"m_data")).CardId==data.CardId&&((DeckCardData)Value(x,"m_data")).Rarity==data.Rarity);
    static Vector2 Point(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(Active<UIFrame>().UICamera,rect.TransformPoint(rect.rect.center));
    static PointerEventData Event(Vector2 position)=>new PointerEventData(EventSystem.current){position=position,delta=new Vector2(40,0),button=PointerEventData.InputButton.Left};
    static GameObject Hit(Vector2 position)
    {
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(Event(position),hits);
        if(hits.Count==0)throw new Exception("No UI raycast hit");return hits[0].gameObject;
    }
    static async Task Drag(DeckCardCell cell,ScrollRect destination,bool screenshot=false)
    {
        Vector2 start=Point((RectTransform)cell.transform),end=Point(destination.viewport);
        if(Hit(start)!=cell.gameObject)throw new Exception("Card raycast blocked by "+Hit(start).name);
        var e=Event(start);ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.beginDragHandler);
        if(((DeckCardView)Ref(Active<DeckEditWindow>(),"m_DragCard")).Texture==null)throw new Exception("Drag art was not preserved");
        e.position=end;ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.dragHandler);
        if(screenshot)await Capture("05-drag");
        var target=ExecuteEvents.ExecuteHierarchy(Hit(end),e,ExecuteEvents.dropHandler);
        if(target!=destination.gameObject)throw new Exception("Drop raycast mismatch");
        ExecuteEvents.Execute(cell.gameObject,e,ExecuteEvents.endDragHandler);await Task.Delay(350);
    }
    public static async Task<string> Acceptance()
    {
        var records=new List<string>();FullHD();await Task.Delay(300);
        var w=Active<DeckEditWindow>();await Capture("03-empty");records.Add(await SearchCheck());
        var pool=(List<DeckCardData>)Value(w,"m_pool");
        var data=pool.First(x=>x.Owned>=2&&LocalGameConfiguration.DeckRules.GetMaxCopies(x.CardId)>=2
            &&LocalGameConfiguration.DeckRules.TryGetSection(x.CardId,out var section)&&section==DeckSection.Main);
        // Filter to the chosen name so the actual draggable cell is visible at the top.
        ((TMP_InputField)Ref(w,"m_InpSearch")).text=LocalizationService.GetText("card."+data.CardId+".name");await Task.Delay(500);
        var cell=Cell(w,data);ExecuteEvents.Execute(cell.gameObject,Event(Point((RectTransform)cell.transform)),ExecuteEvents.pointerClickHandler);
        await Task.Delay(350);
        await Drag(cell,(ScrollRect)Ref(w,"m_MainScroll"),true);await Drag(Cell(w,data),(ScrollRect)Ref(w,"m_MainScroll"));
        if(EditorModel(w).Count(data.CardId,data.Rarity)!=2)throw new Exception("Drag additions failed");
        var placed=new DeckCardData(data.CardId,data.SourcePool,data.Rarity,0,true,w);
        var main=(ScrollRect)Ref(w,"m_MainScroll");
        if(main.content.childCount!=2)throw new Exception("Duplicate cards not expanded into two cells");
        foreach(Transform placedItem in main.content)
        {var c=Resources.FindObjectsOfTypeAll<DeckCardCell>().Single(x=>x.transform==placedItem);if(((TMP_Text)Ref(c,"m_Quantity")).gameObject.activeSelf)throw new Exception("Deck showed owned quantity");}
        await Drag(Cell(w,placed),(ScrollRect)Ref(w,"m_PoolScroll"));
        if(EditorModel(w).Count(data.CardId,data.Rarity)!=1)throw new Exception("Drag removal failed");
        Click(w,"m_BtnPlus");await Task.Delay(350);if(EditorModel(w).Count(data.CardId,data.Rarity)!=2)throw new Exception("Detail +1 failed");
        Click(w,"m_BtnMinus");await Task.Delay(350);if(EditorModel(w).Count(data.CardId,data.Rarity)!=1)throw new Exception("Detail -1 failed");
        records.Add("Raycast click; pool→deck drag twice; two expanded cells without quantity; deck→pool drag; detail +1/-1 passed");
        ((TMP_InputField)Ref(w,"m_InpSearch")).text="";await Task.Delay(500);
        pool=(List<DeckCardData>)Value(w,"m_pool");var extra=pool.First(x=>x.Owned>0&&LocalGameConfiguration.DeckRules.GetMaxCopies(x.CardId)>0
            &&LocalGameConfiguration.DeckRules.TryGetSection(x.CardId,out var section)&&section==DeckSection.Extra);
        w.SelectCard(extra);Click(w,"m_BtnPlus");await Task.Delay(350);
        if(EditorModel(w).Draft.ToData().ExtraDeck.Sum(x=>x.Count)!=1)throw new Exception("Extra routing failed");
        records.Add("Extra monster routed to extra deck");
        // Open/continue the unsaved modal, then rename through the real name popup.
        Click(w,"m_BtnBack");await Task.Delay(350);await Capture("06-unsaved");Click(Active<DeckUnsavedWindow>(),"m_BtnContinue");await Task.Delay(350);
        if(!EditorModel(w).IsDirty)throw new Exception("Continue discarded edits");
        Click(w,"m_BtnRename");await Task.Delay(350);var naming=Active<DeckNameWindow>();
        ((TMP_InputField)Ref(naming,"m_InpName")).text="金色卡盒 · 界面验收改名";Click(naming,"m_BtnConfirm");await Task.Delay(350);
        Click(w,"m_BtnSave");await Task.Delay(1000);DismissMessage();
        if(EditorModel(w).IsDirty||EditorModel(w).Draft.ToData().Revision!=1)throw new Exception("Save did not reset revision/dirty state");records.Add(await Server());
        var id=EditorModel(w).Draft.ToData().Id;await Capture("04-edit");Small();await Task.Delay(350);await Capture("07-edit-720p");FullHD();
        Click(w,"m_BtnBack");await Task.Delay(450);await Capture("01-list");
        Active<DeckListWindow>().Edit(id);await Task.Delay(800);w=Active<DeckEditWindow>();records.Add("Reopened: "+await Server());
        // Dirty back/discard must restore the server version, while save/back persists.
        w.SelectCard(data);Click(w,"m_BtnPlus");await Task.Delay(350);Click(w,"m_BtnBack");await Task.Delay(350);
        Click(Active<DeckUnsavedWindow>(),"m_BtnDiscard");await Task.Delay(900);Active<DeckListWindow>().Edit(id);await Task.Delay(800);w=Active<DeckEditWindow>();
        if(EditorModel(w).Count(data.CardId,data.Rarity)!=1||EditorModel(w).IsDirty)throw new Exception("Discard failed");
        w.SelectCard(data);Click(w,"m_BtnPlus");await Task.Delay(350);Click(w,"m_BtnBack");await Task.Delay(350);
        Click(Active<DeckUnsavedWindow>(),"m_BtnSave");await Task.Delay(900);
        var list=Active<DeckListWindow>();var saved=await PlayerSession.Instance.GetDeckAsync(id);
        if(saved.MainDeck.Sum(x=>x.Count)!=2||saved.Revision!=2)throw new Exception("Save/back failed");
        records.Add("Rename, continue, discard, save/back and reopen passed; final revision="+saved.Revision);
        var item=Resources.FindObjectsOfTypeAll<DeckListItem>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy&&((DeckData)Value(x,"m_data")).Id==id);
        Click(item,"m_Delete");await Task.Delay(350);Click(Active<ChooseWindow>(),"m_BtnNo");await Task.Delay(350);
        if(!(await PlayerSession.Instance.GetDecksAsync()).Any(x=>x.Id==id))throw new Exception("Cancel deleted deck");
        list.Delete(saved);await Task.Delay(350);Click(Active<ChooseWindow>(),"m_BtnOk");await Task.Delay(800);
        if((await PlayerSession.Instance.GetDecksAsync()).Any(x=>x.Id==id))throw new Exception("Delete failed");
        records.Add("Delete cancel/confirm passed; temporary test deck removed");RestoreSize();
        File.WriteAllLines(".doc/verification/deck-ui/acceptance.txt",records);return string.Join("\n",records);
    }
    public static async Task<string> RemoveTestDeck()
    {
        var window=Active<DeckEditWindow>();var data=((DeckEditorState)Value(window,"m_state")).Draft.ToData();
        if(data.Name!="金色卡盒 · 界面验收"&&data.Name!="金色卡盒 · 界面验收改名")throw new Exception("Refusing to delete a non-test deck");
        window.UI_Close();await PlayerSession.Instance.DeleteDeckAsync(data.Id,data.Revision);
        return "Deleted only the acceptance test deck.";
    }
    public static string FullHD()=>Resize(1920,1080);
    public static string Small()=>Resize(1280,720);
    static string Resize(int width,int height)
    {
        var assembly=typeof(Editor).Assembly;var type=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);view.Focus();
        var selected=type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if(SessionState.GetInt("DeckUI.GameViewIndex",-1)<0)SessionState.SetInt("DeckUI.GameViewIndex",(int)selected.GetValue(view));
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");var sizes=sizesType.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);
        var group=sizesType.GetProperty("currentGroup",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(sizes);
        var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,new[]{Enum.Parse(kind,"FixedResolution"),(object)width,height,"Deck acceptance "+width+"x"+height});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);selected.SetValue(view,count-1);view.Repaint();return width+"x"+height;
    }
    public static string RestoreSize()
    {
        var type=typeof(Editor).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);
        type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,SessionState.GetInt("DeckUI.GameViewIndex",0));SessionState.EraseInt("DeckUI.GameViewIndex");return "Restored original Game View size.";
    }
}
