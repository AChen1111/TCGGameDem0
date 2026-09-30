using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AChen.Activities;
using AChen.Configuration;
using AChen.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 使用演示快照渲染已保存的实际 Prefab，不访问玩家接口或发布活动。
public static class PreviewActivityPrefabs
{
    const string Folder="Assets/UI/Prefab/Hall/Activities/";
    static T[] All<T>(GameObject root) where T:Component => Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    public static string Main()
    {
        LocalizationService.Install(Table.TranslationRow.LoadBytes(File.ReadAllBytes("Assets/GameConfiguration/Translations.bytes")),AssetDatabase.LoadAssetAtPath<LocalizationSettings>("Assets/GameConfiguration/LocalizationSettings.asset"));
        var old=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try { Render(false); Render(true); }
        finally { SceneManager.SetActiveScene(old); EditorSceneManager.CloseScene(scene,true); }
        return "Tools/Activities/preview-window.png; Tools/Activities/preview-popup.png";
    }
    static ActivitySnapshot Sample(string id,string key,ActivityType type,int entries)
    {
        var d=new ActivityDefinition {Id=id,NameKey=key,Type=type,RequiredDays=3,Description="活动期间访问大厅即可累计登录天数，已解锁的奖励可以补领。",DefinitionVersion=1};
        var state=new ActivitySnapshot {Definition=d,Eligible=true}; state.PlayerState.Progress=2;
        for(int i=1;i<=entries;i++)
        {
            d.Entries.Add(new ActivityEntryDefinition {Id="day_"+i,NameKey="gift.gold_200.name",DayIndex=i,Rewards={new ActivityReward{RewardType=ActivityRewardType.Gold,Amount=i*200}}});
            state.PlayerState.EntryStates.Add(new ActivityEntryState {EntryId="day_"+i,CanClaim=i<=2,Status=i<=2?"claimable":"locked"});
        }
        return state;
    }
    static void Render(bool popup)
    {
        var cameraRoot=new GameObject("ActivityPreviewCamera"); var camera=cameraRoot.AddComponent<Camera>(); camera.orthographic=true;camera.orthographicSize=480;
        camera.transform.position=new Vector3(0,0,-100); camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.10f,.14f);camera.cullingMask=1<<31;
        var canvasRoot=new GameObject("ActivityPreviewCanvas",typeof(RectTransform)); var canvas=canvasRoot.AddComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
        ((RectTransform)canvasRoot.transform).sizeDelta=new Vector2(1706,960);
        var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+(popup?"ActivityPopupWindow":"ActivityWindow")+".prefab"),canvasRoot.transform);
        root.SetActive(true);
        foreach(var text in All<LocalizedText>(root)) text.Refresh();
        var sample=Sample("login","activity.login_three_days_2026.name",ActivityType.SignIn,3);
        var snapshot=new ActivityListResponse {ServerTime=DateTimeOffset.UtcNow,ServerDay="2026-10-01",NextResetAt=DateTimeOffset.UtcNow.AddHours(8),Activities={sample}};
        var manager=new ActivityManager(null); typeof(ActivityManager).GetMethod("Install",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(manager,new object[]{snapshot});
        var detail=All<ActivityDetailView>(root).Single();detail.Bind(sample,manager,_=>{},_=>{},CancellationToken.None);
        if(!popup)
        {
            var screen=All<ActivityWindow>(root).Single();var serialized=new SerializedObject(screen);
            var content=(RectTransform)serialized.FindProperty("m_ListContent").objectReferenceValue;
            var prefab=(ActivityListItem)serialized.FindProperty("m_ItemPrefab").objectReferenceValue;
            foreach(var state in new[]{sample,Sample("daily","activity.daily_gold.name",ActivityType.Gift,1),Sample("exchange","activity.daily_card_exchange.name",ActivityType.Exchange,1)})
            { var row=UnityEngine.Object.Instantiate(prefab,content);row.Bind(state,state==sample,()=>{}); }
            ((LocalizedText)serialized.FindProperty("m_Empty").objectReferenceValue).gameObject.SetActive(false);
        }
        foreach(var transform in All<Transform>(canvasRoot)) transform.gameObject.layer=31;
        Canvas.ForceUpdateCanvases(); var target=RenderTexture.GetTemporary(1706,960,24); camera.targetTexture=target;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1706,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1706,960),0,0);image.Apply();
        File.WriteAllBytes("Tools/Activities/preview-"+(popup?"popup":"window")+".png",image.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(canvasRoot);UnityEngine.Object.DestroyImmediate(cameraRoot);
    }
}
