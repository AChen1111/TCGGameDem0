using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public static class AuthorResponsiveDeckPool
{
 const string Folder="Assets/UI/Prefab/Hall/Deck/";
 static T[] All<T>(GameObject root) where T:Component=>Resources.FindObjectsOfTypeAll<T>().Where(x=>x.transform==root.transform||x.transform.IsChildOf(root.transform)).ToArray();
 public static void Layout(GameObject root)
 {
  foreach(RectTransform rect in root.transform)
  {
   if(rect.anchorMax.x==1||rect.anchoredPosition.x<1140||rect.anchoredPosition.y>-100)continue;
   if(rect.name=="Txt_Results")
   {
    rect.anchorMin=rect.anchorMax=new Vector2(1,1);
    rect.anchoredPosition+=new Vector2(-1706,0);
   }
   else
   {
    rect.anchorMax=new Vector2(1,1);
    rect.sizeDelta+=new Vector2(-1706,0);
   }
  }
  var window=All<DeckEditWindow>(root).Single();var so=new SerializedObject(window);
  var search=(TMP_InputField)so.FindProperty("m_InpSearch").objectReferenceValue;
  search.textViewport.anchorMax=new Vector2(1,1);search.textViewport.sizeDelta=new Vector2(-24,search.textViewport.sizeDelta.y);
  foreach(var text in new[]{search.textComponent,(TMP_Text)search.placeholder})
  {text.rectTransform.anchorMax=new Vector2(1,1);text.rectTransform.sizeDelta=new Vector2(0,text.rectTransform.sizeDelta.y);}
  var scroll=(ScrollRect)so.FindProperty("m_PoolScroll").objectReferenceValue;
  scroll.viewport.anchorMax=new Vector2(1,1);scroll.viewport.sizeDelta=new Vector2(0,scroll.viewport.sizeDelta.y);
  scroll.content.anchorMax=new Vector2(1,1);scroll.content.sizeDelta=new Vector2(0,scroll.content.sizeDelta.y);
  var grid=new SerializedObject(All<GridListController>(root).Single());
  grid.FindProperty("stretchRowToViewport").boolValue=true;grid.ApplyModifiedPropertiesWithoutUndo();
 }
 public static string Main()
 {
  var row=PrefabUtility.LoadPrefabContents(Folder+"DeckCardRow.prefab");
  var so=new SerializedObject(All<DeckCardRow>(row).Single());so.FindProperty("m_Rect").objectReferenceValue=row.transform;so.ApplyModifiedPropertiesWithoutUndo();
  PrefabUtility.SaveAsPrefabAsset(row,Folder+"DeckCardRow.prefab");PrefabUtility.UnloadPrefabContents(row);
  var root=PrefabUtility.LoadPrefabContents(Folder+"DeckEditWindow.prefab");Layout(root);
  PrefabUtility.SaveAsPrefabAsset(root,Folder+"DeckEditWindow.prefab");PrefabUtility.UnloadPrefabContents(root);
  return "Stretched right deck pool to right edge and bound adaptive eight-card row.";
 }
}
