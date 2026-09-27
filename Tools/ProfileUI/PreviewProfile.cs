using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class PreviewProfile
{
    const string Folder="Assets/UI/Prefab/Hall/Profile/";
    const string Art="Assets/UI/Sprite/ProfileCustomization/";
    static T[] All<T>(GameObject root) where T:Component => Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==root.transform||x.transform.IsChildOf(root.transform)).ToArray();
    static UnityEngine.Object Ref(UnityEngine.Object obj,string field) => new SerializedObject(obj).FindProperty(field).objectReferenceValue;
    static Sprite Sprite(string relative) => AssetDatabase.LoadAssetAtPath<Sprite>(Art+relative+".png");
    static void Apply(AvatarPortraitView portrait,int avatar,int frame) => portrait.Apply(Sprite("Avatars/ProfileIcon"+avatar+"_L"),Sprite("Frames/ProfileFrame"+frame+"_L"),Sprite("Masks/af_"+frame+"_Mask"));
    public static string Main()
    {
        Directory.CreateDirectory("Temp/ProfileWork/Screenshots");
        foreach(var size in new[]{new Vector2Int(1706,960),new Vector2Int(1920,1080),new Vector2Int(1280,720)})
            Render(size.x,size.y,1,false);
        Render(1706,960,0,false);Render(1706,960,2,false);Render(1600,1080,2,true);
        return "Preview screenshots saved to Temp/ProfileWork/Screenshots";
    }
    static void Render(int width,int height,int tab,bool allFrames)
    {
        var scene=EditorSceneManager.NewPreviewScene();var camObject=new GameObject("PreviewCamera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObject,scene);
        var camera=camObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.07f,.11f);camera.orthographic=true;camera.scene=scene;
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;
        var canvasObject=new GameObject("PreviewCanvas",typeof(RectTransform));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasObject,scene);
        var canvas=canvasObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        var scaler=canvasObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(allFrames?1600:1706,allFrames?1080:960);scaler.matchWidthOrHeight=.5f;
        var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"ProfileEditWindow.prefab"),canvasObject.transform);root.SetActive(true);
        var controller=All<ProfileEditWindow>(root).Single();controller.enabled=false;
        ((GameObject)Ref(controller,"m_GoNamePage")).SetActive(tab==0);var cosmetics=(GameObject)Ref(controller,"m_GoCosmetics");cosmetics.SetActive(tab!=0);
        var images=new SerializedObject(controller).FindProperty("m_TabImages");var labels=new SerializedObject(controller).FindProperty("m_TabLabels");
        for(int i=0;i<3;i++){var image=(Image)images.GetArrayElementAtIndex(i).objectReferenceValue;image.sprite=(Sprite)Ref(controller,i==tab?"m_TabSelected":"m_TabNormal");image.color=i==tab?Color.white:new Color(.12f,.23f,.29f);((TMP_Text)labels.GetArrayElementAtIndex(i).objectReferenceValue).color=i==tab?Color.black:Color.white;}
        ((TMP_InputField)Ref(controller,"m_InpName")).SetTextWithoutNotify("决斗者");((TMP_Text)Ref(controller,"m_TxtSelected")).text=tab==0?"玩家名":tab==1?"头像 1010001":"头像框 1030001";
        Apply((AvatarPortraitView)Ref(controller,"m_Preview"),1010001,1030001);
        var scroll=All<ScrollRect>(root).Single();scroll.enabled=false;scroll.verticalScrollbar.size=.3f;scroll.verticalScrollbar.value=1;var content=scroll.content;
        int[] ids=Directory.GetFiles(Art+(tab==1?"Avatars":"Frames"),"*.png").Select(x=>int.Parse(System.Text.RegularExpressions.Regex.Match(Path.GetFileName(x),@"\d+").Value)).OrderBy(x=>x).ToArray();
        if(allFrames)
        {
            root.SetActive(false);content=(RectTransform)new GameObject("AllFrameGrid",typeof(RectTransform)).transform;content.SetParent(canvasObject.transform,false);content.anchorMin=Vector2.zero;content.anchorMax=Vector2.one;content.offsetMin=content.offsetMax=Vector2.zero;
            for(int i=0;i<ids.Length;i++)
            {
                var item=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"AvatarPortrait.prefab"),content);var r=(RectTransform)item.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(20+(i%10)*158,-20-(i/10)*170);r.sizeDelta=new Vector2(140,140);Apply(All<AvatarPortraitView>(item).Single(),1010001,ids[i]);
                var labelRoot=new GameObject("FrameId",typeof(RectTransform));labelRoot.transform.SetParent(item.transform,false);var lr=(RectTransform)labelRoot.transform;lr.anchorMin=lr.anchorMax=lr.pivot=new Vector2(0,1);lr.anchoredPosition=new Vector2(0,-140);lr.sizeDelta=new Vector2(140,26);var label=labelRoot.AddComponent<TextMeshProUGUI>();label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");label.text=ids[i].ToString();label.fontSize=18;label.alignment=TextAlignmentOptions.Center;
            }
        }
        else if(tab!=0)
        {
            for(int row=0;row<3;row++)
            {
                var rowObject=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"ProfileCosmeticRow.prefab"),content);var r=(RectTransform)rowObject.transform;r.anchoredPosition=new Vector2(0,-row*194);
                var rowController=All<ProfileCosmeticRow>(rowObject).Single();var items=new SerializedObject(rowController).FindProperty("m_Items");
                for(int col=0;col<5;col++)
                {
                    var item=items.GetArrayElementAtIndex(col).objectReferenceValue;int index=row*5+col;
                    Apply((AvatarPortraitView)Ref(item,"m_Portrait"),tab==1?ids[index]:1010001,tab==1?1030001:ids[index]);((GameObject)Ref(item,"m_GoEquipped")).SetActive(index==0);((GameObject)Ref(item,"m_GoSelected")).SetActive(index==0);
                }
            }
        }
        Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();RenderTexture.active=previous;
        File.WriteAllBytes("Temp/ProfileWork/Screenshots/"+(allFrames?"all-60-frames":$"profile-tab{tab}-{width}x{height}")+".png",texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);EditorSceneManager.ClosePreviewScene(scene);
    }
}
