using System;
using UnityEngine;

[Serializable]
public sealed class WallpaperDisplayOffset
{
    public int id;
    public Vector3 spriteOffset;
    public Vector3 downOffset;
}

[CreateAssetMenu(fileName = "WallpaperDisplayConfig", menuName = "AChen/Wallpaper Display Config")]
public sealed class WallpaperDisplayConfig : ScriptableObject
{
    [SerializeField] WallpaperDisplayOffset[] m_Items = Array.Empty<WallpaperDisplayOffset>();

    public void GetOffsets(int id, out Vector3 spriteOffset, out Vector3 downOffset)
    {
        for (int i = 0; i < m_Items.Length; i++)
        {
            WallpaperDisplayOffset item = m_Items[i];
            if (item != null && item.id == id)
            {
                spriteOffset = item.spriteOffset;
                downOffset = item.downOffset;
                return;
            }
        }

        spriteOffset = Vector3.zero;
        downOffset = Vector3.zero;
    }
}
