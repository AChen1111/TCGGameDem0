using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperScrollView;

public static class BuildProfilePrefabs
{
    const string Folder = "Assets/UI/Prefab/Hall/Profile/";
    const string Sprites = "Assets/UI/Sprite/ProfileCustomization/";
    static TMP_FontAsset font;
    static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + "Common/" + name + ".png");
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>().Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    static void Ref(UnityEngine.Object host, string name, UnityEngine.Object value)
    { var so = new SerializedObject(host); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static void Refs(UnityEngine.Object host, string name, UnityEngine.Object[] values)
    { var so = new SerializedObject(host); var p = so.FindProperty(name); p.arraySize=values.Length; for(int i=0;i<values.Length;i++) p.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
    static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); rect.anchorMin=rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1);
        rect.anchoredPosition=new Vector2(x,-y); rect.sizeDelta=new Vector2(width,height); return rect;
    }
    static void Stretch(RectTransform rect)
    { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    static Image Img(string name, Transform parent, float x,float y,float w,float h, Sprite sprite, Color color, bool sliced=true)
    {
        var rect=Rect(name,parent,x,y,w,h); var im=rect.gameObject.AddComponent<Image>(); im.sprite=sprite; im.color=color; im.raycastTarget=false;
        im.type=sliced?Image.Type.Sliced:Image.Type.Simple; return im;
    }
    static TextMeshProUGUI Text(string name,Transform parent,float x,float y,float w,float h,string value,float size)
    {
        var label=Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>(); label.font=font; label.text=value; label.fontSize=size;
        label.color=Color.white; label.raycastTarget=false; label.alignment=TextAlignmentOptions.MidlineLeft; return label;
    }
    static Button Button(string name,Transform parent,float x,float y,float w,float h,string label,Sprite normal,Sprite hover)
    {
        var image=Img(name,parent,x,y,w,h,normal,Color.white); image.raycastTarget=true;
        var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.transition=Selectable.Transition.SpriteSwap;
        button.spriteState=new SpriteState{highlightedSprite=hover,pressedSprite=hover,selectedSprite=normal,disabledSprite=normal};
        if(label.Length>0) { var t=Text("Label",image.transform,24,0,w-48,h,label,25);t.alignment=TextAlignmentOptions.Center; }
        return button;
    }
    static AvatarPortraitView Portrait(Transform parent,float x,float y,float size)
    {
        var root=Rect("Portrait",parent,x,y,size,size); var view=root.gameObject.AddComponent<AvatarPortraitView>();
        var mask=Img("ClipMask",root,0,0,size,size,AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"Masks/af_1030001_Mask.png"),Color.white,false);
        mask.preserveAspect=true; var stencil=mask.gameObject.AddComponent<Mask>();stencil.showMaskGraphic=false;
        var avatar=Img("AvatarImage",mask.transform,0,0,size,size,AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"Avatars/ProfileIcon1010001_L.png"),Color.white,false);
        avatar.rectTransform.anchorMin=avatar.rectTransform.anchorMax=avatar.rectTransform.pivot=new Vector2(.5f,.5f); avatar.rectTransform.anchoredPosition=Vector2.zero;
        var aspect=avatar.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=1;
        var frame=Img("FrameImage",root,0,0,size,size,AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"Frames/ProfileFrame1030001_L.png"),Color.white,false);frame.preserveAspect=true;
        Stretch(mask.rectTransform);Stretch(frame.rectTransform);
        Ref(view,"m_ImgAvatar",avatar);Ref(view,"m_ImgMask",mask);Ref(view,"m_ImgFrame",frame);Ref(view,"m_AvatarAspect",aspect);
        return view;
    }
    static void Save(GameObject root,string path)
    { PrefabUtility.SaveAsPrefabAsset(root,path); UnityEngine.Object.DestroyImmediate(root); }
    public static string Main()
    {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        BuildPortrait();BuildRow();BuildWindow();UpdateExisting();AssetDatabase.SaveAssets();
        return "Profile window, five-column row, shared portrait and existing consumers bound";
    }
    static void BuildPortrait()
    { var p=Portrait(null,0,0,176);p.name="AvatarPortrait";Save(p.gameObject,Folder+"AvatarPortrait.prefab"); }
    static void BuildRow()
    {
        var root=Rect("ProfileCosmeticRow",null,0,0,1000,194); root.gameObject.AddComponent<LoopListViewItem2>(); var row=root.gameObject.AddComponent<ProfileCosmeticRow>();
        var items=new ProfileCosmeticItem[5];
        for(int i=0;i<5;i++)
        {
            var item=Rect("Item"+i,root,10+i*198,0,176,176);items[i]=item.gameObject.AddComponent<ProfileCosmeticItem>();
            Img("Base",item,0,0,176,176,S("GUI_CommonItemFrameS_Base"),Color.black);
            var portrait=Portrait(item,8,8,160);
            var border=Img("Border",item,0,0,176,176,S("GUI_CommonItemFrameS_Frame"),Color.white);border.raycastTarget=true;
            var button=border.gameObject.AddComponent<Button>();button.targetGraphic=border; button.transition=Selectable.Transition.None;
            var selected=Img("Selected",item,-4,-4,184,184,S("GUI_CommonItemFrameS_Over"),Color.white);selected.gameObject.SetActive(false);
            var equipped=Img("Equipped",item,-8,-8,44,44,S("GUI_CommonButtonPlof_Check_On"),Color.white,false);
            Ref(items[i],"m_BtnSelect",button);Ref(items[i],"m_Portrait",portrait);Ref(items[i],"m_GoSelected",selected.gameObject);Ref(items[i],"m_GoEquipped",equipped.gameObject);
        }
        Refs(row,"m_Items",items);Save(root.gameObject,Folder+"ProfileCosmeticRow.prefab");
    }
    static void BuildWindow()
    {
        var root=Rect("ProfileEditWindow",null,0,0,1706,960);Stretch(root);root.gameObject.SetActive(false);
        var group=root.gameObject.AddComponent<CanvasGroup>(); var window=root.gameObject.AddComponent<ProfileEditWindow>();
        var panel=Img("Window",root,0,0,1480,820,S("GUI_CommonWindowS"),new Color(.025f,.055f,.085f,.98f));panel.raycastTarget=true;
        panel.rectTransform.anchorMin=panel.rectTransform.anchorMax=panel.rectTransform.pivot=new Vector2(.5f,.5f);panel.rectTransform.anchoredPosition=Vector2.zero;
        Text("Title",panel.transform,36,22,800,48,"个人资料",32);
        Button("Btn_Close",panel.transform,1394,22,48,48,"",S("GUI_ButtonClose"),S("GUI_ButtonClose"));
        string[] names={"Btn_Name","Btn_Avatar","Btn_Frame"};string[] labels={"玩家名","头像","头像边框"};var tabImages=new Image[3];var tabLabels=new TMP_Text[3];
        for(int i=0;i<3;i++)
        {
            var b=Button(names[i],panel.transform,32,120+i*100,260,84,"",S("GUI_CommonButtonSetting"),S("GUI_CommonButtonSetting_Over"));tabImages[i]=(Image)b.targetGraphic;
            tabLabels[i]=Text("TabLabel"+i,b.transform,28,0,220,84,labels[i],28);
            Img("Accent",b.transform,0,6,4,72,S("GUI_CommonWindowS"),new Color(.72f,1,0));
        }
        var preview=Portrait(panel.transform,338,100,88);Ref(window,"m_Preview",preview);
        Text("Txt_Selected",panel.transform,448,113,820,58,"头像 1010001",27);
        var namePage=Rect("Go_NamePage",panel.transform,338,230,1058,430);
        Text("NameCaption",namePage,0,0,900,44,"输入新的玩家名",28);
        var inputBg=Img("Inp_Name",namePage,0,80,920,66,S("GUI_CommonInputField_Base"),Color.white);inputBg.raycastTarget=true;
        var viewport=Rect("Viewport",inputBg.transform,18,4,884,58);viewport.gameObject.AddComponent<RectMask2D>();
        var inputText=Text("InputText",viewport,0,0,884,58,"",27);var placeholder=Text("Placeholder",viewport,0,0,884,58,"2–24 个字符",27);placeholder.color=new Color(.6f,.65f,.7f);
        var input=inputBg.gameObject.AddComponent<TMP_InputField>();input.textViewport=viewport;input.textComponent=inputText;input.placeholder=placeholder;input.targetGraphic=inputBg;input.characterLimit=24;
        input.transition=Selectable.Transition.SpriteSwap;input.spriteState=new SpriteState{highlightedSprite=S("GUI_CommonInputField_Edit_Over"),selectedSprite=S("GUI_CommonInputField_Edit_Over")};
        Text("NameHint",namePage,0,170,960,42,"确认后保存当前页，切换页签会保留未保存内容。",20).color=new Color(.6f,.7f,.78f);
        var cosmetics=Rect("Go_Cosmetics",panel.transform,336,218,1066,498);
        var scroll=cosmetics.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
        var view=Rect("Viewport",cosmetics,0,0,1032,498);view.gameObject.AddComponent<RectMask2D>();var viewBg=view.gameObject.AddComponent<Image>();viewBg.color=new Color(0,0,0,.01f);
        var content=Rect("Content",view,0,0,1000,498);scroll.viewport=view;scroll.content=content;
        var track=Img("Scrollbar",cosmetics,1048,0,12,498,S("GUI_CommonScrollBar"),new Color(.12f,.17f,.23f));
        var slide=Rect("SlidingArea",track.transform,0,0,12,498);Stretch(slide);
        var handle=Img("Handle",slide,0,0,12,80,S("GUI_CommonScrollBar"),new Color(.65f,.7f,.74f));
        Stretch(handle.rectTransform);
        var bar=track.gameObject.AddComponent<Scrollbar>();bar.targetGraphic=handle;bar.handleRect=handle.rectTransform;bar.direction=Scrollbar.Direction.BottomToTop;bar.value=1;
        scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
        var loop=cosmetics.gameObject.AddComponent<LoopListView2>();var list=cosmetics.gameObject.AddComponent<GridListController>();Ref(list,"loopListView",loop);Ref(window,"m_List",list);
        Button("Btn_Confirm",panel.transform,1144,740,260,56,"确认",S("GUI_CommonButtonM"),S("GUI_CommonButtonM_Over"));
        Refs(window,"m_TabImages",tabImages);Refs(window,"m_TabLabels",tabLabels);Ref(window,"m_TabNormal",S("GUI_CommonButtonSetting"));Ref(window,"m_TabSelected",S("GUI_CommonButtonSetting_On"));
        var generator=root.gameObject.AddComponent<UiScreenGenerator>();var so=new SerializedObject(generator);so.FindProperty("m_kind").enumValueIndex=1;so.FindProperty("m_className").stringValue="ProfileEditWindow";so.FindProperty("m_folderPath").stringValue="Assets/Scripts/UI/PreGameUI/Avatar";so.ApplyModifiedPropertiesWithoutUndo();generator.CollectUiBinds();
        cosmetics.gameObject.SetActive(true);namePage.gameObject.SetActive(false);
        Save(root.gameObject,Folder+"ProfileEditWindow.prefab");
    }
    static void ReplacePortrait(GameObject root,Component controller,string oldImageName)
    {
        var old=All<Image>(root).Single(x=>x.name==oldImageName);var rect=old.rectTransform;
        var portrait=Portrait(rect.parent,0,0,rect.sizeDelta.x);
        var pr=(RectTransform)portrait.transform; pr.anchorMin=rect.anchorMin;pr.anchorMax=rect.anchorMax;pr.pivot=rect.pivot;pr.anchoredPosition=rect.anchoredPosition;pr.sizeDelta=rect.sizeDelta;
        pr.SetSiblingIndex(rect.GetSiblingIndex());Ref(controller,"m_Portrait",portrait);UnityEngine.Object.DestroyImmediate(old.gameObject);
    }
    static void UpdateExisting()
    {
        string hall="Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";
        var root=PrefabUtility.LoadPrefabContents(hall);ReplacePortrait(root,All<PlayerProfileView>(root).Single(),"AvatarImg");
        var hallPortrait=All<AvatarPortraitView>(root).Single();
        var avatarButton=All<Button>(root).Single(x=>x.name=="Btn_Avatar");
        var hallRect=(RectTransform)hallPortrait.transform;hallRect.SetParent(avatarButton.transform,false);
        hallRect.anchorMin=hallRect.anchorMax=hallRect.pivot=new Vector2(.5f,.5f);hallRect.anchoredPosition=new Vector2(3,2.85f);hallRect.sizeDelta=new Vector2(96,96);hallRect.localScale=Vector3.one;
        foreach(var image in All<Image>(hallPortrait.gameObject).Where(x=>x.name=="ClipMask"||x.name=="FrameImage"))Stretch(image.rectTransform);
        foreach(var image in All<Image>(root).Where(x=>x.name=="AvatarBG"||x.name=="AvatarBG (1)"))UnityEngine.Object.DestroyImmediate(image.gameObject);
        var hallFrame=All<Image>(hallPortrait.gameObject).Single(x=>x.name=="FrameImage");hallFrame.raycastTarget=true;avatarButton.targetGraphic=hallFrame;
        PrefabUtility.SaveAsPrefabAsset(root,hall);PrefabUtility.UnloadPrefabContents(root);
        foreach(string path in new[]{"Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendRowPrefab.prefab","Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/FriendApplyRowPrefab.prefab"})
        {
            root=PrefabUtility.LoadPrefabContents(path); Component ctl=path.Contains("FriendRowPrefab")?(Component)All<FriendRowItem>(root).Single():All<GiftRequestRowItem>(root).Single();
            ReplacePortrait(root,ctl,"Img_icon");PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
        }
        string itemPath="Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab";root=PrefabUtility.LoadPrefabContents(itemPath);
        var item=All<ShopOwnedItem>(root).Single();var itemSo=new SerializedObject(item);var old=(Image)itemSo.FindProperty("m_ImgMain").objectReferenceValue;var r=old.rectTransform;
        var portrait=Portrait(r.parent,0,0,160);var p=(RectTransform)portrait.transform;p.anchorMin=r.anchorMin;p.anchorMax=r.anchorMax;p.pivot=r.pivot;p.anchoredPosition=r.anchoredPosition;p.sizeDelta=r.sizeDelta;Ref(item,"m_Portrait",portrait);
        PrefabUtility.SaveAsPrefabAsset(root,itemPath);PrefabUtility.UnloadPrefabContents(root);
        string shopPath="Assets/UI/Prefab/Hall/Shop/ShopWindows.prefab";root=PrefabUtility.LoadPrefabContents(shopPath);
        var shop=All<ShopWindow>(root).Single();var ss=new SerializedObject(shop);var buttons=ss.FindProperty("m_ChooseButtons");var oldButtons=Enumerable.Range(0,buttons.arraySize).Select(i=>(Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        var frameButton=UnityEngine.Object.Instantiate(oldButtons[1],oldButtons[1].transform.parent);frameButton.name="Btn_AvatarFrame";frameButton.transform.SetSiblingIndex(oldButtons[1].transform.GetSiblingIndex()+1);
        foreach(var t in All<TMP_Text>(frameButton.gameObject))t.text="头像框";
        foreach(var t in All<LocalizedText>(frameButton.gameObject)) {var ts=new SerializedObject(t);var key=ts.FindProperty("key");if(key!=null){key.stringValue="ui.shop.tab_avatar_frame";ts.ApplyModifiedPropertiesWithoutUndo();}}
        Refs(shop,"m_ChooseButtons",new UnityEngine.Object[]{oldButtons[0],oldButtons[1],frameButton,oldButtons[2]});PrefabUtility.SaveAsPrefabAsset(root,shopPath);PrefabUtility.UnloadPrefabContents(root);
        var settings=AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset");var set=new SerializedObject(settings);var screens=set.FindProperty("screensToRegister");
        for(int i=screens.arraySize-1;i>=0;i--) {var screen=screens.GetArrayElementAtIndex(i).objectReferenceValue;if(screen.name=="SelfChooseWindow"||screen.name=="ChangeNameWindow"){screens.DeleteArrayElementAtIndex(i);if(i<screens.arraySize&&screens.GetArrayElementAtIndex(i).objectReferenceValue==null)screens.DeleteArrayElementAtIndex(i);}}
        int index=screens.arraySize;screens.InsertArrayElementAtIndex(index);screens.GetArrayElementAtIndex(index).objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"ProfileEditWindow.prefab");set.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(settings);
    }
}
