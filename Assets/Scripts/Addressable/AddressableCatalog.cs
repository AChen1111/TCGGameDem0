using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AddressableEntry<TRef> where TRef : AssetReference
{
    public string assetName;
    public TRef reference;
}

[Serializable]
public class AssetReferenceScene : AssetReference
{
    public AssetReferenceScene() { }

    public AssetReferenceScene(string guid) : base(guid) { }

#if UNITY_EDITOR
    public override bool ValidateAsset(string path)
    {
        return path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
    }

    public override bool ValidateAsset(UnityEngine.Object obj)
    {
        return obj is SceneAsset;
    }
#endif
}

[Serializable]
public class AssetReferenceUISettings : AssetReferenceT<UISettings>
{
    public AssetReferenceUISettings() : base("") { }

    public AssetReferenceUISettings(string guid) : base(guid) { }
}

public abstract class AddressableCatalog<TRef> : ScriptableObject where TRef : AssetReference
{
    [SerializeField] List<AddressableEntry<TRef>> m_entries = new List<AddressableEntry<TRef>>();

    Dictionary<string, TRef> m_map;

    void OnEnable()
    {
        BuildMap();
    }

    public void BuildMap()
    {
        m_map = new Dictionary<string, TRef>(m_entries.Count);
        for (int i = 0; i < m_entries.Count; i++)
        {
            m_map.Add(m_entries[i].assetName, m_entries[i].reference);
        }
    }

    public TRef Get(string assetName)
    {
        return m_map[assetName];
    }

#if UNITY_EDITOR
    public List<AddressableEntry<TRef>> Entries => m_entries;

    public void EditorSetEntries(List<AddressableEntry<TRef>> entries)
    {
        m_entries = entries;
    }

    public void EditorAdd(string assetName, TRef reference)
    {
        for (int i = 0; i < m_entries.Count; i++)
        {
            if (m_entries[i].assetName == assetName)
            {
                m_entries[i].reference = reference;
                return;
            }
        }

        m_entries.Add(new AddressableEntry<TRef> { assetName = assetName, reference = reference });
    }
#endif
}