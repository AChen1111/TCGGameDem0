using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperScrollView;

// Only executes in the stopped Editor. References are authored and serialized in real prefabs.
public static class BuildDeckPrefabs
{
    public static string Inspect() => string.Join(",", typeof(DeckEditWindow).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Select(x=>x.Name));
    public static string Cleanup()
    {
        foreach(var go in Resources.FindObjectsOfTypeAll<DeckEditWindow>().Where(x=>x.gameObject.scene.IsValid() && x.name=="DeckEditWindow").ToArray())UnityEngine.Object.DestroyImmediate(go.gameObject);
        return "Removed only unfinished deck authoring roots.";
    }
    const string Folder = "Assets/UI/Prefab/Hall/Deck/";
    const string Sprites = "Assets/UI/Sprite/DeckEditor/";
    const string Materials = "Assets/UI/Materials/Deck/";
    static TMP_FontAsset font;
    static Material[] versions;
    static Material background, panelMaterial;
    static readonly Color Green = new Color(.8f,1f,.03f);
    static readonly Color Navy = new Color(.035f,.085f,.125f,1);
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    static void Ref(UnityEngine.Object host, string name, UnityEngine.Object value)
    { var so=new SerializedObject(host);var field=so.FindProperty(name);
      if(field==null||field.propertyType!=SerializedPropertyType.ObjectReference)throw new InvalidOperationException(host.GetType().Name+" field is not a serialized reference: "+name);
      field.objectReferenceValue=value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static void Refs(UnityEngine.Object host,string name,UnityEngine.Object[] values)
    { var so=new SerializedObject(host);var p=so.FindProperty(name);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo(); }
    static RectTransform R(string name, Transform parent, float x,float y,float w,float h)
    { var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r; }
    static void Stretch(RectTransform r)
    { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero; }
    static Sprite S(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+".png");
    static Image Img(string name,Transform p,float x,float y,float w,float h,Sprite sprite,Color color)
    { var v=R(name,p,x,y,w,h).gameObject.AddComponent<Image>();v.sprite=sprite;v.color=color;v.type=sprite==null?Image.Type.Simple:Image.Type.Sliced;v.raycastTarget=false;return v; }
    static TMP_Text T(string name,Transform p,float x,float y,float w,float h,string value,float size=24)
    { var t=R(name,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=Color.white;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;return t; }
    static TMP_Text L(string name,Transform p,float x,float y,float w,float h,string key,float size=24)
    { var t=T(name,p,x,y,w,h,key,size);var local=t.gameObject.AddComponent<LocalizedText>();var so=new SerializedObject(local);so.FindProperty("key").stringValue=key;so.ApplyModifiedPropertiesWithoutUndo();return t; }
    static Button B(string name,Transform p,float x,float y,float w,float h,string label,string key="")
    {
        var im=Img(name,p,x,y,w,h,S("GUI_CommonButtonM"),Color.white);im.raycastTarget=true;
        var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.transition=Selectable.Transition.SpriteSwap;
        b.spriteState=new SpriteState{highlightedSprite=S("GUI_CommonButtonM_Over"),pressedSprite=S("GUI_CommonButtonM_Over")};
        var text=key.Length==0?T("Label",im.transform,6,0,w-12,h,label,25):L("Label",im.transform,6,0,w-12,h,key,25);
        text.alignment=TextAlignmentOptions.Center;
        if(name=="Btn_Back")
        { im.sprite=S("GUI_CommonButtonBack");im.type=Image.Type.Simple;im.preserveAspect=true;text.text="";
          b.spriteState=new SpriteState{highlightedSprite=S("GUI_CommonButtonBack_Over"),pressedSprite=S("GUI_CommonButtonBack_Over")}; }
        if(name=="Btn_Confirm"||name=="Btn_Cancel")text.color=Green;
        return b;
    }
    static TMP_InputField Input(string name,Transform p,float x,float y,float w,float h,string key)
    {
        var bg=Img(name,p,x,y,w,h,S("GUI_CommonInputField_Base"),Color.white);bg.raycastTarget=true;
        var view=R("TextArea",bg.transform,12,0,w-24,h);view.gameObject.AddComponent<RectMask2D>();
        var text=T("Text",view,0,0,w-24,h,"",25);var hint=L("Placeholder",view,0,0,w-24,h,key,25);hint.color=new Color(.7f,.73f,.75f);
        var input=bg.gameObject.AddComponent<TMP_InputField>();input.targetGraphic=bg;input.textViewport=view;input.textComponent=text;input.placeholder=hint;
        input.caretColor=Green;input.selectionColor=new Color(.65f,.85f,.15f,.3f);return input;
    }
    static void Panel(Transform p,float x,float y,float w,float h)
    {
        var bg=Img("PanelBase",p,x,y,w,h,null,Navy);bg.raycastTarget=true;
        bg.material=panelMaterial; bg.color=Color.white; Frame("PanelFrame",p,x,y,w,h,new Color(.63f,.68f,.7f));
    }
    static DeckPanelFrame Frame(string name,Transform p,float x,float y,float w,float h,Color color) { var frame=R(name,p,x,y,w,h).gameObject.AddComponent<DeckPanelFrame>();frame.color=color;frame.raycastTarget=false;return frame; }
    static GameObject Window(string name,Type type,bool popup)
    {
        var r=R(name,null,0,0,1706,960);Stretch(r);r.gameObject.AddComponent<CanvasGroup>();
        var controller=r.gameObject.AddComponent(type);var so=new SerializedObject(controller);so.FindProperty("isPopup").boolValue=popup;so.ApplyModifiedPropertiesWithoutUndo();
        if(!popup)
        { var bg=R("Background",r,0,0,1706,960).gameObject.AddComponent<RawImage>();Stretch(bg.rectTransform);bg.material=background;bg.raycastTarget=true;
          Img("Header",r,0,0,1706,80,null,new Color(0,0,0,.8f));Img("HeaderLine",r,0,79,1706,1,null,new Color(.4f,.45f,.5f)); }
        return r.gameObject;
    }
    static DeckCardView Card(string name,Transform p,float x,float y,float w,float h)
    {
        var root=R(name,p,x,y,w,h);var view=root.gameObject.AddComponent<DeckCardView>();var raw=root.gameObject.AddComponent<RawImage>();raw.raycastTarget=false;
        Ref(view,"m_Art",raw);Refs(view,"m_Versions",versions);return view;
    }
    static ScrollRect Scroll(string name,Transform p,float x,float y,float w,float h,bool grid=false,float cellW=62,float cellH=100,int columns=10)
    {
        var root=R(name,p,x,y,w,h);var scroll=root.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
        var viewport=R("Viewport",root,0,0,w,h);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=new Color(0,0,0,.01f);
        var content=R("Content",viewport,0,0,w,h);scroll.viewport=viewport;scroll.content=content;
        if(grid)
        {
            var layout=content.gameObject.AddComponent<GridLayoutGroup>();layout.cellSize=new Vector2(cellW,cellH);layout.spacing=new Vector2(8,12);layout.padding=new RectOffset(8,8,8,8);layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=columns;
            var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        }
        return scroll;
    }
    static void Save(GameObject root,string name,bool window=false)
    {
        if(window)
        {
            var generator=root.AddComponent<UiScreenGenerator>();var so=new SerializedObject(generator);
            so.FindProperty("m_kind").enumValueIndex=1;so.FindProperty("m_className").stringValue=name;so.FindProperty("m_folderPath").stringValue="Assets/Scripts/UI/PreGameUI/Deck";so.ApplyModifiedPropertiesWithoutUndo();
            generator.CollectUiBinds();generator.RebuildUiBinds();
        }
        PrefabUtility.SaveAsPrefabAsset(root,Folder+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
    }
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before authoring.");
        if(typeof(DeckListItem).GetField("m_DeckName",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)==null)
            throw new InvalidOperationException("Wait for compilation before rebuilding renamed serialized fields.");
        Directory.CreateDirectory(Folder);Directory.CreateDirectory(Sprites);Directory.CreateDirectory(Materials);
        foreach(var name in new[]{"GUI_Global_Window_Frame","GUI_CommonButtonM","GUI_CommonButtonM_Over","GUI_CommonButtonBack","GUI_CommonButtonBack_Over","GUI_CommonInputField_Base","GUI_CommonSubTitle_Base","GUI_DeckEdit_CardActionMenu_IconCardAdd","GUI_DeckEdit_CardActionMenu_IconCardRemove","GUI_DeckSelect_TrashIcon","GUI_CommonButtonAddDeck_Icon"})
            File.Copy(@"C:\Users\ldc20\OneDrive\Desktop\UI\Sprites\CommonUI\"+name+".png",Sprites+name+".png",true);
        File.Copy(@"C:\Users\ldc20\OneDrive\Desktop\UI\Sprites\Icon\DeckCase\HD\DeckCase2004_L.png",Sprites+"DeckCase2004_L.png",true);
        AssetDatabase.Refresh();
        foreach(var path in Directory.GetFiles(Sprites,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            if(Path.GetFileName(path).StartsWith("GUI_CommonButtonM"))importer.spriteBorder=new Vector4(22,22,22,22);
            if(Path.GetFileName(path)=="GUI_CommonInputField_Base.png")importer.spriteBorder=new Vector4(6,14,6,14);
            importer.SaveAndReimport();
        }
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        var old=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefab/Card/Materiel/Mat_Front.mat");
        versions=new Material[5];
        for(int i=0;i<5;i++)
        {
            string path=Materials+"DeckFoil"+i+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("TCG/UI/DeckCardFoil"));AssetDatabase.CreateAsset(material,path);}
            material.SetFloat("_Rarity",i);material.SetTexture("_GoldNoiseMap",old.GetTexture("_GoldNoiseMap"));EditorUtility.SetDirty(material);versions[i]=material;
        }
        string bgPath=Materials+"DeckBackground.mat";background=AssetDatabase.LoadAssetAtPath<Material>(bgPath);
        if(background==null){background=new Material(Shader.Find("TCG/UI/DeckBackground"));AssetDatabase.CreateAsset(background,bgPath);}
        background.SetFloat("_Panel",0); EditorUtility.SetDirty(background);
        string panelPath=Materials+"DeckPanel.mat";panelMaterial=AssetDatabase.LoadAssetAtPath<Material>(panelPath);
        if(panelMaterial==null){panelMaterial=new Material(background);AssetDatabase.CreateAsset(panelMaterial,panelPath);}
        panelMaterial.SetFloat("_Panel",1);EditorUtility.SetDirty(panelMaterial);
        Cell(false);Cell(true);Row();ListItem();List();Edit();Name();Unsaved();
        foreach(var path in Directory.GetFiles(Folder,"*.prefab"))AddressableCatalogMenu.AddPrefab(path.Replace('\\','/'));
        AddressableCatalogMenu.AddSprite(Sprites+"DeckCase2004_L.png");
        var settings=AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset");var settingsSo=new SerializedObject(settings);var screens=settingsSo.FindProperty("screensToRegister");
        foreach(var name in new[]{"DeckListWindow","DeckEditWindow","DeckNameWindow","DeckUnsavedWindow"})
        { var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab");bool exists=false;for(int i=0;i<screens.arraySize;i++)if(screens.GetArrayElementAtIndex(i).objectReferenceValue==prefab)exists=true;if(!exists){screens.arraySize++;screens.GetArrayElementAtIndex(screens.arraySize-1).objectReferenceValue=prefab;} }
        settingsSo.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();
        return "Built and registered eight deck prefabs, five foil materials, background and gold deck case.";
    }
    static void Cell(bool placed)
    {
        var artSize=placed?new Vector2(76,111):new Vector2(56,82);
        float w=artSize.x+4, artW=artSize.x, artH=artSize.y;
        var root=R(placed?"DeckPlacedCardCell":"DeckCardCell",null,0,0,w,artH+4);var cell=root.gameObject.AddComponent<DeckCardCell>();
        var hit=root.gameObject.AddComponent<Image>();hit.color=new Color(0,0,0,.01f);var button=root.gameObject.AddComponent<Button>();button.targetGraphic=hit;
        var art=Card("Art",root,2,2,artW,artH);var dim=Img("Unowned",root,2,2,artW,artH,null,new Color(.02f,.025f,.04f,.63f));
        var selected=Frame("Selected",root,0,0,w,artH+4,Green);selected.gameObject.SetActive(false);
        var name=T("Name",root,0,artH+4,w,22,"卡牌名称",18);name.alignment=TextAlignmentOptions.Center;name.color=new Color(.79f,.82f,.88f);
        name.textWrappingMode=TextWrappingModes.NoWrap;name.overflowMode=TextOverflowModes.Ellipsis;
        name.gameObject.SetActive(false);
        var badge=Img("QuantityBadge",root,artW-18,artH-18,20,20,null,Color.black);badge.gameObject.SetActive(!placed);
        var count=T("Quantity",badge.transform,0,0,20,20,"3",18);count.alignment=TextAlignmentOptions.Center;count.margin=Vector4.zero;count.gameObject.SetActive(!placed);
        Ref(cell,"m_View",art);Ref(cell,"m_Dim",dim);Ref(cell,"m_Selected",selected.gameObject);Ref(cell,"m_Button",button);Ref(cell,"m_Quantity",count);Ref(cell,"m_Rarity",name);
        Save(root.gameObject,placed?"DeckPlacedCardCell":"DeckCardCell");
    }
    static void Row()
    {
        var root=R("DeckCardRow",null,0,0,520,94);root.gameObject.AddComponent<LoopListViewItem2>();var row=root.gameObject.AddComponent<DeckCardRow>();var items=new DeckCardCell[8];
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"DeckCardCell.prefab");
        for(int i=0;i<8;i++){var child=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);((RectTransform)child.transform).anchoredPosition=new Vector2(i*64+6,-4);items[i]=All<DeckCardCell>(child).Single();}
        Refs(row,"m_Items",items);Save(root.gameObject,"DeckCardRow");
    }
    static void ListItem()
    {
        var root=R("DeckListItem",null,0,0,280,255);var view=root.gameObject.AddComponent<DeckListItem>();
        Panel(root,0,0,280,255);var button=root.gameObject.AddComponent<Button>();var hit=root.gameObject.AddComponent<Image>();hit.color=new Color(0,0,0,.01f);button.targetGraphic=hit;
        var box=Img("GoldDeckCase",root,86,22,110,142,S("DeckCase2004_L"),Color.white);box.type=Image.Type.Simple;box.preserveAspect=true;
        var name=T("Name",root,14,176,252,35,"卡组",23);name.alignment=TextAlignmentOptions.Center;name.overflowMode=TextOverflowModes.Ellipsis;
        var delete=B("Delete",root,235,218,32,28,"×");
        Ref(view,"m_Open",button);Ref(view,"m_Delete",delete);Ref(view,"m_DeckName",name);Save(root.gameObject,"DeckListItem");
    }
    static void List()
    {
        var root=Window("DeckListWindow",typeof(DeckListWindow),false);var controller=All<DeckListWindow>(root).Single();
        B("Btn_Back",root.transform,22,18,68,50,"‹");L("Title",root.transform,120,16,440,50,"ui.deck.list",38);
        T("Txt_Total",root.transform,1570,16,94,50,"0",33);
        var newButton=B("Btn_New",root.transform,68,142,280,255,"＋");All<TMP_Text>(newButton.gameObject).Single().fontSize=92;All<TMP_Text>(newButton.gameObject).Single().color=Green;
        var scroll=Scroll("Decks",root.transform,378,142,1256,780,true,280,255,4);
        var empty=L("Go_Empty",root.transform,378,432,1200,70,"ui.deck.empty",26);empty.alignment=TextAlignmentOptions.Center;
        Ref(controller,"m_Content",scroll.content);Ref(controller,"m_ItemPrefab",All<DeckListItem>(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"DeckListItem.prefab")).Single());
        Ref(controller,"m_Controls",All<CanvasGroup>(root).Single());Save(root,"DeckListWindow",true);
    }
    static void Edit()
    {
        var root=Window("DeckEditWindow",typeof(DeckEditWindow),false);var r=root.transform;var controller=All<DeckEditWindow>(root).Single();
        B("Btn_Back",r,22,18,68,50,"‹");L("Title",r,120,16,440,50,"ui.deck.edit",34);B("Btn_Save",r,1468,18,210,50,"保存","ui.deck.save");
        Panel(r,24,112,354,816);Panel(r,402,112,714,816);Panel(r,1140,112,542,816);
        var detail=R("Go_Detail",r,24,112,354,816);
        var header=Img("DetailHeader",detail,2,2,350,55,null,new Color(.5f,.4f,.18f));
        T("Txt_DetailName",detail,14,2,272,55,"青眼白龙",27).overflowMode=TextOverflowModes.Ellipsis;
        var attr=Img("Attribute",detail,296,8,43,43,null,Color.white);attr.type=Image.Type.Simple;attr.preserveAspect=true;
        var art=Card("DetailCard",detail,16,75,146,213);
        T("Txt_DetailStats",detail,181,78,164,180,"",28);
        T("Txt_Rarity",detail,16,300,320,34,"",21).color=Green;
        Img("TypeBar",detail,2,348,350,45,null,new Color(.32f,.27f,.13f));
        var type=T("Txt_DetailType",detail,12,348,330,45,"",19);type.enableAutoSizing=true;type.fontSizeMin=14;type.fontSizeMax=19;
        var descScroll=Scroll("Description",detail,14,406,328,309);
        var desc=T("Txt_DetailDesc",descScroll.content,0,0,324,308,"",23);desc.alignment=TextAlignmentOptions.TopLeft;desc.textWrappingMode=TextWrappingModes.Normal;
        var descFit=desc.gameObject.AddComponent<ContentSizeFitter>();descFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        var descLayout=descScroll.content.gameObject.AddComponent<VerticalLayoutGroup>();descLayout.childControlWidth=true;descLayout.childControlHeight=true;descLayout.childForceExpandHeight=false;
        var fit=descScroll.content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        B("Btn_Plus",detail,14,740,158,54,"+1");B("Btn_Minus",detail,182,740,158,54,"−1");
        var box=Img("DeckCase",r,416,122,36,44,S("DeckCase2004_L"),Color.white);box.type=Image.Type.Simple;box.preserveAspect=true;
        T("Txt_Name",r,467,119,530,51,"卡组",27).overflowMode=TextOverflowModes.Ellipsis;B("Btn_Rename",r,1015,125,85,38,"改名");
        Img("NameLine",r,416,172,684,2,null,new Color(.45f,.5f,.55f));
        Img("MainHeader",r,404,176,710,34,null,new Color(.1f,.14f,.17f));L("MainTitle",r,420,176,180,34,"ui.deck.main",25);T("Txt_MainCount",r,830,176,262,34,"0 / 60",25).alignment=TextAlignmentOptions.MidlineRight;
        var main=Scroll("MainCards",r,408,210,702,338,true,80,115,8);
        var empty=L("Go_Empty",r,451,339,610,80,"ui.deck.add_prompt",26);empty.alignment=TextAlignmentOptions.Center;
        Img("ExtraHeader",r,404,556,710,34,null,new Color(.1f,.14f,.17f));L("ExtraTitle",r,420,556,180,34,"ui.deck.extra",25);T("Txt_ExtraCount",r,830,556,262,34,"0 / 15",25).alignment=TextAlignmentOptions.MidlineRight;
        var extra=Scroll("ExtraCards",r,408,590,702,338,true,80,115,8);
        foreach(var scroll in new[]{main,extra}) { var layout=All<GridLayoutGroup>(scroll.gameObject).Single();layout.spacing=new Vector2(6,8);layout.padding=new RectOffset(10,10,8,8); }
        T("Txt_Status",r,640,119,353,51,"",18).alignment=TextAlignmentOptions.MidlineRight;
        Img("PoolHeader",r,1142,114,538,53,null,Green);L("PoolTitle",r,1160,114,440,53,"ui.deck.cards",30).color=Color.black;
        Input("Inp_Search",r,1151,179,408,49,"ui.deck.search");T("Txt_Results",r,1568,179,102,49,"0",25).alignment=TextAlignmentOptions.Center;
        var pool=Scroll("CardPool",r,1150,245,522,675);var loop=pool.gameObject.AddComponent<LoopListView2>();var grid=pool.gameObject.AddComponent<GridListController>();Ref(grid,"loopListView",loop);
        var poolEmpty=L("NoResults",r,1170,480,482,80,"ui.workshop.empty",25);poolEmpty.alignment=TextAlignmentOptions.Center;
        var dropMain=main.gameObject.AddComponent<DeckDropArea>();var dropExtra=extra.gameObject.AddComponent<DeckDropArea>();var dropPool=pool.gameObject.AddComponent<DeckDropArea>();
        foreach(var drop in new[]{dropMain,dropExtra,dropPool})Ref(drop,"m_Window",controller);
        foreach(var drop in new[]{dropMain,dropExtra}){var so=new SerializedObject(drop);so.FindProperty("m_IntoDeck").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();}
        var deckHighlight=Frame("DeckDropHighlight",r,402,182,714,746,Green);deckHighlight.enabled=false;
        var poolHighlight=Frame("PoolDropHighlight",r,1140,235,542,693,Green);poolHighlight.enabled=false;
        var drag=R("DragGhost",r,0,0,76,111);drag.pivot=new Vector2(.5f,.5f);var dragCard=Card("DragCard",drag,0,0,76,111);drag.gameObject.SetActive(false);
        Ref(controller,"m_Pool",grid);Ref(controller,"m_PoolScroll",pool);Ref(controller,"m_MainScroll",main);Ref(controller,"m_ExtraScroll",extra);
        Ref(controller,"m_DeckCellPrefab",All<DeckCardCell>(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"DeckPlacedCardCell.prefab")).Single());
        Ref(controller,"m_DetailCard",art);Ref(controller,"m_DragCard",dragCard);Ref(controller,"m_DragRoot",drag);Ref(controller,"m_CanvasRect",root.transform);Ref(controller,"m_Controls",All<CanvasGroup>(root).Single());
        Ref(controller,"m_Attribute",attr);Ref(controller,"m_DetailHeader",header);Refs(controller,"m_DropHighlights",new[]{deckHighlight,poolHighlight});Ref(controller,"m_PoolEmpty",poolEmpty.gameObject);
        var existing=PrefabUtility.LoadPrefabContents("Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab");var view=All<CardDetailView>(existing).Single();var viewSo=new SerializedObject(view);
        Refs(controller,"m_Attributes",new[]{"Light","Dark","Fire","Water","Wind","Earth","Divine"}.Select(x=>viewSo.FindProperty("m_SpriteAttr"+x).objectReferenceValue).ToArray());PrefabUtility.UnloadPrefabContents(existing);
        Save(root,"DeckEditWindow",true);
    }
    static void Name()
    {
        var root=Window("DeckNameWindow",typeof(DeckNameWindow),true);Panel(root.transform,340,235,1026,466);
        var title=T("Txt_Title",root.transform,375,279,956,110,"请输入卡组名",30);title.alignment=TextAlignmentOptions.Center;
        Img("Line",root.transform,342,414,1022,4,null,new Color(.02f,.55f,.72f));
        var input=Input("Inp_Name",root.transform,394,458,918,68,"ui.deck.name_placeholder");input.characterLimit=64;
        B("Btn_Cancel",root.transform,417,592,412,63,"取消","ui.deck.cancel");B("Btn_Confirm",root.transform,877,592,412,63,"确认","ui.deck.confirm");Save(root,"DeckNameWindow",true);
    }
    static void Unsaved()
    {
        var root=Window("DeckUnsavedWindow",typeof(DeckUnsavedWindow),true);Panel(root.transform,353,290,1000,374);
        var message=L("Txt_Message",root.transform,390,330,926,126,"ui.deck.unsaved",30);message.alignment=TextAlignmentOptions.Center;
        B("Btn_Save",root.transform,385,526,280,63,"保存并返回","ui.deck.save_back");B("Btn_Discard",root.transform,713,526,280,63,"放弃改动","ui.deck.discard");B("Btn_Continue",root.transform,1041,526,280,63,"继续编辑","ui.deck.continue");Save(root,"DeckUnsavedWindow",true);
    }
}
