using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕适配器
/// </summary>
public class ScreenFitter : MonoBehaviour
{
   private int height = 960; //逻辑宽度
   public CanvasScaler[] m_CanvasScaler;
   public Camera m_Camera;

   private void Awake() 
   {
     //计算比例
     float ratio = (float)Screen.width/Screen.height;
     DoFit(ratio);
   }

   private void DoFit(float ratio) {
      foreach (var cs in m_CanvasScaler)
      {
        cs.referenceResolution = new Vector2(height*ratio, height);
      }
      if(m_Camera != null)
      {
        //2f是因为算的半轴
        //100 是因为 ppu = 100
        m_Camera.orthographicSize = height/(2f * 100);
      }
   }
}
