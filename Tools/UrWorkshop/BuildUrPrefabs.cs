using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperScrollView;

// Run in the stopped Unity Editor with unity command run_script. All references are serialized here.
public static class BuildUrPrefabs
{
    const string Folder="Assets/UI/Prefab/Hall/Workshop/";
    const string Icon="Assets/UI/Sprite/CardWorkshop/Icon_Rarity_UR.png";
    static TMP_FontAsset font;
    static Sprite S(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprite/ProfileCustomization/Common/"+name+".png");
    static T[] All<T>(GameObject r) where T:Component=>Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==r.transform||x.transform.IsChildOf(r.transform)).ToArray();
    static void Ref(UnityEngine.Object h,string n,UnityEngine.Object v){var so=new SerializedObject(h);so.FindProperty(n).objectReferenceValue=v;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Refs(UnityEngine.Object h,string n,UnityEngine.Object[] a){var so=new SerializedObject(h);var p=so.FindProperty(n);p.arraySize=a.Length;for(int i=0;i<a.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=a[i];so.ApplyModifiedPropertiesWithoutUndo();}
    static RectTransform R(string n,Transform parent,float x,float y,float w,float h){var r=(RectTransform)new GameObject(n,typeof(RectTransform)).transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    static Image Img(string n,Transform p,float x,float y,float w,float h,Sprite s,Color c){var im=R(n,p,x,y,w,h).gameObject.AddComponent<Image>();im.sprite=s;im.color=c;im.type=s==null?Image.Type.Simple:Image.Type.Sliced;im.raycastTarget=false;return im;}
    static TMP_Text T(string n,Transform p,float x,float y,float w,float h,string value,float size=24){var t=R(n,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=Color.white;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;return t;}
    static void Loc(TMP_Text t,string key){var c=t.gameObject.AddComponent<LocalizedText>();var so=new SerializedObject(c);so.FindProperty("key").stringValue=key;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Dynamic(TMP_Text t){var c=t.gameObject.AddComponent<LocalizedText>();var so=new SerializedObject(c);so.FindProperty("dynamicContent").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();}
    static Button B(string n,Transform p,float x,float y,float w,float h,string text,string key=""){var im=Img(n,p,x,y,w,h,S("GUI_CommonButtonM"),Color.white);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.transition=Selectable.Transition.SpriteSwap;b.spriteState=new SpriteState{highlightedSprite=S("GUI_CommonButtonM_Over"),pressedSprite=S("GUI_CommonButtonM_Over")};var t=T("Label",b.transform,6,0,w-12,h,text,24);t.alignment=TextAlignmentOptions.Center;if(key.Length>0)Loc(t,key);return b;}
    static RawImage Card(Transform p,float x,float y,float w,float h){var im=R("CardArt",p,x,y,w,h).gameObject.AddComponent<RawImage>();im.raycastTarget=false;return im;}
    static GameObject Instance(string name,Transform parent){return (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab"),parent);}
    static void Save(GameObject r,string name){PrefabUtility.SaveAsPrefabAsset(r,Folder+name+".prefab");UnityEngine.Object.DestroyImmediate(r);}
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before building assets.");
        Directory.CreateDirectory(Folder);Directory.CreateDirectory(Path.GetDirectoryName(Icon));
        File.Copy(@"C:\Users\ldc20\OneDrive\Desktop\UI\Sprites\CommonUI\Icon_Rarity_UR.png",Icon,true);
        AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath(Icon);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        Balance();Item();Row();Detail();Panel();Notice();Badge();Integrate();
        foreach(var path in Directory.GetFiles(Folder,"*.prefab"))AddressableCatalogMenu.AddPrefab(path.Replace('\\','/'));
        AddressableCatalogMenu.AddSprite(Icon);AssetDatabase.SaveAssets();
        return "Saved and bound UR balance, workshop item/row/detail/panel, notice window and overflow badge; registered catalogs.";
    }
    static void Balance()
    {
        var r=R("UrBalance",null,0,0,230,58);var view=r.gameObject.AddComponent<PlayerUrView>();
        var icon=Img("Icon",r,0,2,54,54,AssetDatabase.LoadAssetAtPath<Sprite>(Icon),Color.white);icon.type=Image.Type.Simple;icon.preserveAspect=true;
        var amount=T("Amount",r,64,0,160,58,"0",28);amount.enableAutoSizing=true;amount.fontSizeMin=15;amount.fontSizeMax=28;Ref(view,"m_Amount",amount);Save(r.gameObject,"UrBalance");
    }
    static void Item()
    {
        var r=R("CardWorkshopItem",null,0,0,210,342);var v=r.gameObject.AddComponent<CardWorkshopItem>();
        var bg=Img("Frame",r,0,0,210,342,S("GUI_CommonWindowS"),new Color(.055f,.08f,.12f));bg.raycastTarget=true;
        var b=bg.gameObject.AddComponent<Button>();b.targetGraphic=bg;var art=Card(r,12,10,186,271.6f);
        var dim=Img("OwnedDim",r,12,10,186,271.6f,null,new Color(.1f,.1f,.1f,.62f));
        var name=T("Name",r,12,285,186,28,"卡名",18);name.overflowMode=TextOverflowModes.Ellipsis;
        var count=T("Owned",r,12,313,186,24,"普通 ×0",18);count.color=new Color(.66f,.72f,.8f);
        var selected=Img("Selected",r,0,0,4,342,null,new Color(.78f,1,.3f));
        Dynamic(name);Dynamic(count);Ref(v,"m_Card",art);Ref(v,"m_CardName",All<LocalizedText>(name.gameObject).Single());Ref(v,"m_Owned",All<LocalizedText>(count.gameObject).Single());Ref(v,"m_Dim",dim);Ref(v,"m_Select",b);Ref(v,"m_Selected",selected.gameObject);
        Save(r.gameObject,"CardWorkshopItem");
    }
    static void Row()
    {
        var r=R("CardWorkshopRow",null,0,0,920,358);r.gameObject.AddComponent<LoopListViewItem2>();var row=r.gameObject.AddComponent<CardWorkshopRow>();var items=new CardWorkshopItem[4];
        for(int i=0;i<4;i++){var go=Instance("CardWorkshopItem",r);((RectTransform)go.transform).anchoredPosition=new Vector2(6+i*228,0);items[i]=All<CardWorkshopItem>(go).Single();}
        Refs(row,"m_Items",items);Save(r.gameObject,"CardWorkshopRow");
    }
    static void Detail()
    {
        var r=R("CardWorkshopDetail",null,0,0,470,652);var detail=r.gameObject.AddComponent<CardWorkshopDetail>();
        var bg=Img("Background",r,0,0,470,652,S("GUI_CommonWindowS"),new Color(.018f,.035f,.06f,.95f));
        T("Title",r,22,14,426,46,"卡牌详情",25);Ref(detail,"m_Card",Card(r,147,72,176,257));
        T("Owned",r,24,338,422,32,"普通 · 持有 0",23);
        var versions=R("Versions",r,20,383,430,48);var buttons=new Button[5];var labels=new TMP_Text[5];
        for(int i=0;i<5;i++){buttons[i]=B("Version"+i,versions,i*86,0,82,48,"版本","");labels[i]=All<TMP_Text>(buttons[i].gameObject).Single();labels[i].fontSize=17;}
        Refs(detail,"m_Versions",buttons);Refs(detail,"m_VersionLabels",labels);Ref(detail,"m_VersionControls",versions.gameObject);
        var quantity=R("QuantityControls",r,110,447,250,42);Ref(detail,"m_Minus",B("Minus",quantity,0,0,60,42,"−"));Ref(detail,"m_Plus",B("Plus",quantity,190,0,60,42,"+"));var count=T("Quantity",quantity,66,0,118,42,"1",25);count.alignment=TextAlignmentOptions.Center;Ref(detail,"m_Quantity",count);Ref(detail,"m_QuantityControls",quantity.gameObject);
        var price=T("Price",r,24,506,422,40,"30 UR",27);price.color=new Color(.87f,.7f,1);price.alignment=TextAlignmentOptions.Center;
        var action=B("Action",r,75,568,320,60,"合成");Ref(detail,"m_Action",action);
        foreach(var label in All<TMP_Text>(r.gameObject).Where(x=>new[]{"Title","Owned","Price"}.Contains(x.name)||x.transform.parent==action.transform))Dynamic(label);
        foreach(var pair in new[]{"m_Title:Title","m_Owned:Owned","m_Price:Price","m_ActionLabel:Label"}){var a=pair.Split(':');var text=All<TMP_Text>(r.gameObject).Single(x=>x.name==a[1]&&(a[0]!="m_ActionLabel"||x.transform.parent==action.transform));Ref(detail,a[0],All<LocalizedText>(text.gameObject).Single());}
        Save(r.gameObject,"CardWorkshopDetail");
    }
    static void Panel()
    {
        var r=R("CardWorkshopPanel",null,0,0,1530,750);var panel=r.gameObject.AddComponent<CardWorkshopPanel>();
        var bg=Img("Background",r,0,0,1530,750,S("GUI_CommonWindowS"),new Color(.025f,.043f,.067f,.98f));bg.raycastTarget=true;
        var craft=B("CraftTab",r,24,14,150,52,"合成","ui.workshop.craft");var dismantle=B("DismantleTab",r,186,14,150,52,"分解","ui.workshop.dismantle");
        Ref(panel,"m_CraftTab",craft);Ref(panel,"m_DismantleTab",dismantle);Ref(panel,"m_CraftLabel",All<TMP_Text>(craft.gameObject).Single());Ref(panel,"m_DismantleLabel",All<TMP_Text>(dismantle.gameObject).Single());
        var balance=Instance("UrBalance",r);((RectTransform)balance.transform).anchoredPosition=new Vector2(1250,-10);
        var inputBg=Img("Search",r,358,17,360,46,S("GUI_CommonInputField_Base"),Color.white);inputBg.raycastTarget=true;
        var viewport=R("Viewport",inputBg.transform,12,0,336,46);viewport.gameObject.AddComponent<RectMask2D>();var text=T("Text",viewport,0,0,336,46,"",22);var hint=T("Placeholder",viewport,0,0,336,46,"搜索卡名或 ID",21);hint.color=new Color(.6f,.65f,.72f);Loc(hint,"ui.workshop.search");
        var input=inputBg.gameObject.AddComponent<TMP_InputField>();input.textViewport=viewport;input.textComponent=text;input.placeholder=hint;input.targetGraphic=inputBg;Ref(panel,"m_Search",input);
        var toggleRect=R("OnlyCraftable",r,748,21,245,40);var toggle=toggleRect.gameObject.AddComponent<Toggle>();var box=Img("Box",toggleRect,0,4,30,30,S("GUI_CommonWindowS"),new Color(.35f,.4f,.5f));box.raycastTarget=true;var check=Img("Check",box.transform,5,5,20,20,null,new Color(.78f,1,.3f));toggle.targetGraphic=box;toggle.graphic=check;toggle.isOn=false;Loc(T("Label",toggleRect,42,0,200,40,"仅可合成",22),"ui.workshop.only_craftable");Ref(panel,"m_OnlyCraftable",toggle);
        var area=R("CardGrid",r,24,88,970,638);var scroll=area.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
        var view=R("Viewport",area,0,0,946,638);view.gameObject.AddComponent<RectMask2D>();var viewBg=view.gameObject.AddComponent<Image>();viewBg.color=new Color(0,0,0,.01f);
        var content=R("Content",view,0,0,920,638);scroll.viewport=view;scroll.content=content;
        var track=Img("Scrollbar",area,954,0,10,638,S("GUI_CommonScrollBar"),new Color(.12f,.17f,.23f));var handle=Img("Handle",track.transform,0,0,10,100,S("GUI_CommonScrollBar"),new Color(.65f,.7f,.74f));Stretch(handle.rectTransform);var bar=track.gameObject.AddComponent<Scrollbar>();bar.targetGraphic=handle;bar.handleRect=handle.rectTransform;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
        var loop=area.gameObject.AddComponent<LoopListView2>();var list=area.gameObject.AddComponent<GridListController>();Ref(list,"loopListView",loop);Ref(panel,"m_List",list);
        var empty=T("Empty",r,100,330,800,60,"没有符合条件的卡牌",28);empty.alignment=TextAlignmentOptions.Center;Loc(empty,"ui.workshop.empty");Ref(panel,"m_Empty",empty);
        var detail=Instance("CardWorkshopDetail",r);((RectTransform)detail.transform).anchoredPosition=new Vector2(1034,-84);Ref(panel,"m_Detail",All<CardWorkshopDetail>(detail).Single());
        Save(r.gameObject,"CardWorkshopPanel");
    }
    static void Notice()
    {
        var r=PrefabUtility.LoadPrefabContents("Assets/UI/Prefab/BaseUI/ChooseWindow.prefab");r.name="UrNoticeWindow";var old=All<ChooseWindow>(r).Single();var so=new SerializedObject(old);var message=so.FindProperty("m_TxtMessage").objectReferenceValue;var ok=so.FindProperty("m_BtnOk").objectReferenceValue;var no=(Button)so.FindProperty("m_BtnNo").objectReferenceValue;
        UnityEngine.Object.DestroyImmediate(no.gameObject);foreach(var generator in All<UiScreenGenerator>(r))UnityEngine.Object.DestroyImmediate(generator);UnityEngine.Object.DestroyImmediate(old);
        var notice=r.AddComponent<UrNoticeWindow>();Ref(notice,"m_Message",All<LocalizedText>(((Component)message).gameObject).Single());Ref(notice,"m_Ok",ok);var ns=new SerializedObject(notice);ns.FindProperty("isPopup").boolValue=true;ns.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(r,Folder+"UrNoticeWindow.prefab");PrefabUtility.UnloadPrefabContents(r);
        var settings=AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset");var ss=new SerializedObject(settings);var p=ss.FindProperty("screensToRegister");var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"UrNoticeWindow.prefab");
        if(!Enumerable.Range(0,p.arraySize).Any(i=>p.GetArrayElementAtIndex(i).objectReferenceValue==prefab)){p.arraySize++;p.GetArrayElementAtIndex(p.arraySize-1).objectReferenceValue=prefab;ss.ApplyModifiedPropertiesWithoutUndo();}
    }
    static void Badge()
    {
        var r=R("CardOverflowBadge",null,0,0,186,272);r.pivot=new Vector2(.5f,.5f);var canvas=r.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=5;var badge=r.gameObject.AddComponent<CardOverflowBadge>();
        var visual=R("Visual",r,0,0,186,272);Stretch(visual);Img("Dim",visual,0,0,186,272,null,new Color(.2f,.2f,.2f,.64f));var icon=Img("UrIcon",visual,41,75,104,104,AssetDatabase.LoadAssetAtPath<Sprite>(Icon),Color.white);icon.type=Image.Type.Simple;icon.preserveAspect=true;
        var amount=T("Amount",visual,93,151,76,48,"10",36);amount.alignment=TextAlignmentOptions.BottomRight;amount.fontStyle=FontStyles.Bold;amount.outlineWidth=.2f;Ref(badge,"m_Visual",visual.gameObject);Ref(badge,"m_Amount",amount);visual.gameObject.SetActive(false);Save(r.gameObject,"CardOverflowBadge");
    }
    static void Integrate()
    {
        string shopPath="Assets/UI/Prefab/Hall/Shop/ShopWindows.prefab";var r=PrefabUtility.LoadPrefabContents(shopPath);var shop=All<ShopWindow>(r).Single();var so=new SerializedObject(shop);var p=so.FindProperty("m_ChooseButtons");var buttons=Enumerable.Range(0,p.arraySize).Select(i=>(Button)p.GetArrayElementAtIndex(i).objectReferenceValue).ToList();
        if(buttons.Count==4){var b=UnityEngine.Object.Instantiate(buttons[0],buttons[0].transform.parent);b.name="Btn_Workshop";foreach(var t in All<TMP_Text>(b.gameObject))t.text="卡牌工坊";foreach(var l in All<LocalizedText>(b.gameObject)){var ls=new SerializedObject(l);ls.FindProperty("key").stringValue="ui.shop.tab_workshop";ls.ApplyModifiedPropertiesWithoutUndo();}buttons.Add(b);Refs(shop,"m_ChooseButtons",buttons.ToArray());}
        foreach(var b in buttons)((RectTransform)b.transform).sizeDelta=new Vector2(238,80);
        foreach(var old in All<CardWorkshopPanel>(r))UnityEngine.Object.DestroyImmediate(old.gameObject);
        var regular=(GridListController)so.FindProperty("m_ListController").objectReferenceValue;var panel=Instance("CardWorkshopPanel",regular.transform.parent);var rect=(RectTransform)panel.transform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(0,-50);panel.SetActive(false);Ref(shop,"m_Workshop",All<CardWorkshopPanel>(panel).Single());Ref(shop,"m_RegularContent",regular.gameObject);
        PrefabUtility.SaveAsPrefabAsset(r,shopPath);PrefabUtility.UnloadPrefabContents(r);
        string hall="Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";r=PrefabUtility.LoadPrefabContents(hall);foreach(var old in All<PlayerUrView>(r))UnityEngine.Object.DestroyImmediate(old.gameObject);
        var gold=All<PlayerGoldView>(r).Single();var host=gold.transform.parent;var balance=Instance("UrBalance",host);var br=(RectTransform)balance.transform;br.anchorMin=br.anchorMax=new Vector2(0,1);br.pivot=new Vector2(0,1);br.anchoredPosition=new Vector2(-350,-22);var layout=balance.AddComponent<LayoutElement>();layout.ignoreLayout=true;
        PrefabUtility.SaveAsPrefabAsset(r,hall);PrefabUtility.UnloadPrefabContents(r);
        string card="Assets/Prefab/Card/CardPrefab.prefab";r=PrefabUtility.LoadPrefabContents(card);foreach(var old in All<CardOverflowBadge>(r))UnityEngine.Object.DestroyImmediate(old.gameObject);var view=All<CardPickView>(r).Single();var vs=new SerializedObject(view);var front=(MeshRenderer)vs.FindProperty("_meshFrontRenderer").objectReferenceValue;var badge=Instance("CardOverflowBadge",front.transform);badge.transform.localPosition=new Vector3(0,0,-.001f);badge.transform.localRotation=Quaternion.identity;badge.transform.localScale=new Vector3(1f/186,1f/272,1);foreach(var t in All<Transform>(badge))t.gameObject.layer=front.gameObject.layer;Ref(view,"_overflowBadge",All<CardOverflowBadge>(badge).Single());PrefabUtility.SaveAsPrefabAsset(r,card);PrefabUtility.UnloadPrefabContents(r);
    }
}
