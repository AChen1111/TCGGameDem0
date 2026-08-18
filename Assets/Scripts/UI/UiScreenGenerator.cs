using System;
using System.IO;
using System.Text;
using deVoid.UIFramework;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu("UI/UI Screen Generator")]
public class UiScreenGenerator : MonoBehaviour
{
    public enum Kind
    {
        Panel,
        Window
    }

    [SerializeField] Kind m_kind = Kind.Panel;
    [SerializeField] string m_className;
    [SerializeField] [FolderPath] string m_folderPath = "Assets/Scripts/UI";
    [SerializeField] UiPrefixBind[] m_uiBinds;

    [Button("收集UI引用")]
    [ContextMenu("收集UI引用")]
    public void CollectUiBinds()
    {
        var binds = UiPrefixCollector.Collect(transform);
#if UNITY_EDITOR
        var so = new UnityEditor.SerializedObject(this);
        UiPrefixCollector.WriteBinds(so, "m_uiBinds", binds);
        so.ApplyModifiedProperties();
        var behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IUIScreenController && behaviours[i] != this)
            {
                var screenSo = new UnityEditor.SerializedObject(behaviours[i]);
                UiPrefixCollector.ApplySerializedFields(screenSo, binds);
                screenSo.ApplyModifiedProperties();
            }
        }
#else
        m_uiBinds = binds;
#endif
    }

    [Button("创建UI脚本")]
    [ContextMenu("创建UI脚本")]
    public void CreateUiScript()
    {
        string folder = m_folderPath.Replace('\\', '/').TrimEnd('/');
        Directory.CreateDirectory(folder);
        string path = folder + "/" + m_className + ".cs";
        File.WriteAllText(path, BuildCsSource());
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
    }

    public string BuildCsSource()
    {
        bool useUi = false;
        bool useTmp = false;
        var binds = m_uiBinds;
#if UNITY_EDITOR
        binds = ReadBindsFromSerialized();
#endif
        if (binds != null)
        {
            for (int i = 0; i < binds.Length; i++)
            {
                Type type = FieldType(binds[i]);
                if (type != null && type.Namespace == "UnityEngine.UI")
                {
                    useUi = true;
                }
                if (type != null && type.Namespace == "TMPro")
                {
                    useTmp = true;
                }
            }
        }

        var sb = new StringBuilder();
        sb.Append("using UnityEngine;\n");
        if (useUi)
        {
            sb.Append("using UnityEngine.UI;\n");
        }
        if (useTmp)
        {
            sb.Append("using TMPro;\n");
        }
        sb.Append("using deVoid.UIFramework;\n\n");
        string baseType = m_kind == Kind.Window ? "AWindowController" : "APanelController";
        sb.Append("public class ").Append(m_className).Append(" : ").Append(baseType).Append("\n{\n");
        if (binds != null)
        {
            for (int i = 0; i < binds.Length; i++)
            {
                Type type = FieldType(binds[i]);
                sb.Append("    [SerializeField] ").Append(CsTypeName(type)).Append(' ').Append(binds[i].fieldName).Append(";\n");
            }
        }
        sb.Append("}\n");
        return sb.ToString();
    }

    static Type FieldType(UiPrefixBind bind)
    {
        return bind.component != null ? bind.component.GetType() : typeof(GameObject);
    }

    static string CsTypeName(Type type)
    {
        return type.Name;
    }

#if UNITY_EDITOR
    UiPrefixBind[] ReadBindsFromSerialized()
    {
        var so = new UnityEditor.SerializedObject(this);
        var prop = so.FindProperty("m_uiBinds");
        var binds = new UiPrefixBind[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            binds[i] = new UiPrefixBind
            {
                fieldName = elem.FindPropertyRelative("fieldName").stringValue,
                target = (GameObject)elem.FindPropertyRelative("target").objectReferenceValue,
                component = (Component)elem.FindPropertyRelative("component").objectReferenceValue
            };
        }
        return binds;
    }
#endif
}
