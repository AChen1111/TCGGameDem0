using UnityEditor;
using UnityEngine;

public static class LuaComponetBinder
{
    public static LuaComponet[] FindInTree(GameObject source)
    {
        Transform root = source.transform;
        while (root.parent != null)
        {
            root = root.parent;
        }
        return root.GetComponentsInChildren<LuaComponet>(true);
    }

    public static string DefaultName(Object obj)
    {
        if (obj is GameObject go)
        {
            return "m_" + go.name;
        }

        var component = (Component)obj;
        return "m_" + component.gameObject.name + "_" + component.GetType().Name;
    }

    public static string AddReference(LuaComponet target, string name, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty("m_objectReferences");
        string unique = UniqueName(prop, name);
        int index = prop.arraySize;
        prop.arraySize = index + 1;
        SerializedProperty elem = prop.GetArrayElementAtIndex(index);
        elem.FindPropertyRelative("name").stringValue = unique;
        elem.FindPropertyRelative("value").objectReferenceValue = value;
        so.ApplyModifiedProperties();
        return unique;
    }

    private static string UniqueName(SerializedProperty array, string name)
    {
        if (!ContainsName(array, name))
        {
            return name;
        }

        int i = 1;
        string candidate;
        do
        {
            candidate = name + "_" + i;
            i++;
        }
        while (ContainsName(array, candidate));
        return candidate;
    }

    private static bool ContainsName(SerializedProperty array, string name)
    {
        for (int i = 0; i < array.arraySize; i++)
        {
            if (array.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
            {
                return true;
            }
        }
        return false;
    }
}
