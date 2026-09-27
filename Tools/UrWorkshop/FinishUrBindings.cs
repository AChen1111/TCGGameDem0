using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;
public static class FinishUrBindings
{
    static T[] All<T>(GameObject r)where T:Component=>Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==r.transform||x.transform.IsChildOf(r.transform)).ToArray();
    public static string Main()
    {
        int bound=0;
        foreach(var name in new[]{"CardWorkshopItem","CardWorkshopDetail","UrNoticeWindow"})
        {
            string path="Assets/UI/Prefab/Hall/Workshop/"+name+".prefab";var r=PrefabUtility.LoadPrefabContents(path);
            var controller=All<MonoBehaviour>(r).Single(x=>x.GetType().Name==name);
            var pairs=name=="CardWorkshopItem"?new[]{"m_CardName:Name","m_Owned:Owned"}:name=="CardWorkshopDetail"?new[]{"m_Title:Title","m_Owned:Owned","m_Price:Price","m_ActionLabel:Label"}:new[]{"m_Message:Txt_Message"};
            foreach(var pair in pairs)
            {
                var parts=pair.Split(':');var text=All<TMP_Text>(r).Single(x=>x.name==parts[1]&&(parts[0]!="m_ActionLabel"||x.transform.parent.name=="Action"));
                var local=All<LocalizedText>(text.gameObject).Single();var so=new SerializedObject(controller);so.FindProperty(parts[0]).objectReferenceValue=local;so.ApplyModifiedPropertiesWithoutUndo();bound++;
            }
            if(name=="UrNoticeWindow"){var so=new SerializedObject(controller);so.FindProperty("isPopup").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();}
            PrefabUtility.SaveAsPrefabAsset(r,path);PrefabUtility.UnloadPrefabContents(r);
        }
        AssetDatabase.SaveAssets();return "Serialized localization bindings: "+bound;
    }
}
