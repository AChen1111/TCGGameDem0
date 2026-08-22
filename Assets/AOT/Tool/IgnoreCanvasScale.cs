using UnityEngine;

public class IgnoreCanvasScale : MonoBehaviour
{
    void OnEnable()
    {
        Canvas.willRenderCanvases += Apply;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= Apply;
    }

    void Apply()
    {
        float s = GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
        
        transform.localScale = new Vector3(1f / s, 1f / s, 1f);
    }
}
