using System;
using System.Linq;
using AChen.Networking;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class UrPrefabTests
{
    [Test]
    public void New_prefabs_and_consumers_have_required_serialized_references()
    {
        var paths=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/UI/Prefab/Hall/Workshop"}).Select(AssetDatabase.GUIDToAssetPath)
            .Concat(new[]{"Assets/UI/Prefab/Hall/Shop/ShopWindows.prefab","Assets/Prefab/Card/CardPrefab.prefab","Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab"});
        var types=new[]{typeof(CardWorkshopPanel),typeof(CardWorkshopDetail),typeof(CardWorkshopItem),typeof(CardWorkshopRow),typeof(CardOverflowBadge),typeof(PlayerUrView),typeof(UrNoticeWindow)};
        int found=0;
        foreach(var path in paths)
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(var component in Resources.FindObjectsOfTypeAll<MonoBehaviour>().Where(x=>x.transform==root.transform||x.transform.IsChildOf(root.transform)))
                {
                    if(!types.Contains(component.GetType()))continue;found++;
                    Assert.AreEqual("HotUpdate",component.GetType().Assembly.GetName().Name);
                    var p=new SerializedObject(component).GetIterator();
                    while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference)
                        Assert.IsNotNull(p.objectReferenceValue,path+" / "+component.name+" / "+p.propertyPath);
                }
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        Assert.Greater(found,10);
    }

    [Test]
    public void Overflow_badge_is_non_interactive_and_hides_on_non_overflow_cards()
    {
        var root=PrefabUtility.LoadPrefabContents("Assets/UI/Prefab/Hall/Workshop/CardOverflowBadge.prefab");
        try
        {
            var view=Resources.FindObjectsOfTypeAll<CardOverflowBadge>().Single(x=>x.gameObject==root);
            var so=new SerializedObject(view);var visual=(GameObject)so.FindProperty("m_Visual").objectReferenceValue;
            view.Show(20);Assert.IsTrue(visual.activeSelf);view.Show(0);Assert.IsFalse(visual.activeSelf);
            foreach(var image in Resources.FindObjectsOfTypeAll<Graphic>().Where(x=>x.transform.IsChildOf(root.transform)))Assert.IsFalse(image.raycastTarget);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }

    [Test]
    public void Draw_json_retains_per_card_overflow_and_ur_balance()
    {
        const string player="{\"id\":\"11111111-1111-1111-1111-111111111111\",\"ur\":55,\"revision\":3,\"ownedCards\":[],\"createdAt\":\"2026-09-27T00:00:00Z\",\"updatedAt\":\"2026-09-27T00:00:00Z\"}";
        var response=AuthApi.ParseDrawJson("{\"player\":"+player+",\"results\":[{\"cardId\":\"01639384\",\"rarity\":4,\"sourcePool\":\"Card01\",\"isOverflow\":true,\"urGained\":30}]}");
        Assert.AreEqual(55,response.Player.Ur);Assert.AreEqual("01639384",response.Results[0].CardId);Assert.IsTrue(response.Results[0].IsOverflow);Assert.AreEqual(30,response.Results[0].UrGained);
    }
}
