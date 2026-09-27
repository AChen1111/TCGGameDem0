using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;
// 本地 UI 验收：仅打开和取消确认窗，停止 Play 后丢弃内存状态。
public static class VerifyShopPurchase {
 static T Live<T>() where T:Component => Resources.FindObjectsOfTypeAll<T>().Single(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy);
 static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);
 static void Capture(string n) => ScreenCapture.CaptureScreenshot(Path.GetFullPath(".doc/verification/profile-visual-fix/"+n+".png"));
 public static async Task<string> Main(){
  if(!Application.isPlaying)throw new InvalidOperationException("Play mode required");
  // 禁止测试会话提交真实交易，不改动磁盘登录令牌或服务器数据。
  typeof(AChen.Player.PlayerSession).GetField("m_accessToken",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(AChen.Player.PlayerSession.Instance,null);
  EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
  var records=new List<string>();
  foreach(int category in new[]{1,2}) {
   Field<Button[]>(Live<ShopWindow>(),"m_ChooseButtons")[category].onClick.Invoke();
   await Task.Delay(1000);
   string prefix=category==1?"shop-avatars":"shop-frames";
   Capture(prefix); await Task.Delay(150);
   var item=Resources.FindObjectsOfTypeAll<ShopOwnedItem>().Where(x=>x.gameObject.scene.IsValid()&&x.gameObject.activeInHierarchy).Single(x=>Field<ShopOwnedItemData>(x,"m_Data").Index==1);
   var data=Field<ShopOwnedItemData>(item,"m_Data");
   var rect=(RectTransform)item.transform;
   var canvas=Resources.FindObjectsOfTypeAll<Canvas>().Single(x=>item.transform.IsChildOf(x.transform));
   var evt=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(evt,hits);
   if(hits.First().gameObject!=item.gameObject)throw new Exception("Hit blocked by "+hits.First().gameObject.name);
   ExecuteEvents.Execute(item.gameObject,evt,ExecuteEvents.pointerClickHandler);
   await Task.Delay(500);
   var popup=Live<ChooseWindow>();
   records.Add(data.CatalogType+"/"+data.Id+": "+Field<TextMeshProUGUI>(popup,"m_TxtMessage").text);
   Capture(prefix+"-purchase");await Task.Delay(150);
   Field<Button>(popup,"m_BtnNo").onClick.Invoke();await Task.Delay(400);
  }
  File.WriteAllLines(".doc/verification/profile-visual-fix/purchase-verification.txt",records);
  return string.Join("\n",records);
 }
}

