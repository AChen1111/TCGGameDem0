using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class FixDeckWindowLayout
{
 const string Folder="Assets/UI/Prefab/Hall/Deck/";
 static T[] All<T>(GameObject root) where T:Component=>Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==root.transform||x.transform.IsChildOf(root.transform)).ToArray();
 public static string Main()
 {
  foreach(var name in new[]{"DeckListWindow","DeckEditWindow"})
  {
   var root=PrefabUtility.LoadPrefabContents(Folder+name+".prefab");
   foreach(var child in new[]{"Header","HeaderLine"})
   {
    var rect=(RectTransform)root.transform.Find(child);
    rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);
    rect.anchoredPosition=new Vector2(0,rect.anchoredPosition.y);
    rect.sizeDelta=new Vector2(0,rect.sizeDelta.y);
   }
   if(name=="DeckListWindow")
   {
    var so=new SerializedObject(All<DeckListWindow>(root).Single());
    so.FindProperty("hideOnForegroundLost").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();
   }
   PrefabUtility.SaveAsPrefabAsset(root,Folder+name+".prefab");PrefabUtility.UnloadPrefabContents(root);
  }
  return "Stretched deck headers to full width; retained deck list underneath editor to prevent lobby flash.";
 }
}
