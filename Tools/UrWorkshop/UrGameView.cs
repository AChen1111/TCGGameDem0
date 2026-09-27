using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
public static class UrGameView
{
    const string Saved="UR.Verification.GameViewIndex";
    public static string Inspect()
    {
        var t=typeof(Editor).Assembly.GetType("UnityEditor.GameView");var v=EditorWindow.GetWindow(t);var lines=new System.Collections.Generic.List<string>();
        foreach(var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public))if(p.Name.ToLowerInvariant().Contains("size")||p.Name.ToLowerInvariant().Contains("group"))
        {try{lines.Add(p.Name+"="+p.GetValue(p.GetMethod.IsStatic?null:v));}catch{}}
        var st=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");var si=st.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);
        foreach(var p in st.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))if(p.Name.ToLowerInvariant().Contains("group")){try{lines.Add(p.Name+"="+p.GetValue(si));}catch{}}
        return string.Join("\n",lines)+"\nScreen="+Screen.width+"x"+Screen.height;
    }
    public static string Baseline()=>Resize(1706,960);
    public static string Wide()=>Resize(2160,1080);
    static string Resize(int w,int h)
    {
        var assembly=typeof(Editor).Assembly;var type=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);view.Focus();
        var selected=type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if(SessionState.GetInt(Saved,-1)<0)SessionState.SetInt(Saved,(int)selected.GetValue(view));
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");var sizes=sizesType.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);
        var group=sizesType.GetProperty("currentGroup",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(sizes);
        var sizeType=assembly.GetType("UnityEditor.GameViewSize");var sizeKind=assembly.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(sizeType,new[]{Enum.Parse(sizeKind,"FixedResolution"),(object)w,h,"UR acceptance "+w+"x"+h});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);selected.SetValue(view,count-1);view.Repaint();return w+"x"+h;
    }
    public static string Restore()
    {var type=typeof(Editor).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,SessionState.GetInt(Saved,0));SessionState.EraseInt(Saved);return "Original Game View selection restored.";}
    public static async Task<string> Capture(string name)
    {
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();await Task.Delay(250);
        string path=Path.GetFullPath(".doc/verification/ur-workshop/"+name+".png");ScreenCapture.CaptureScreenshot(path);await Task.Delay(350);return path+" "+Screen.width+"x"+Screen.height;
    }
}
