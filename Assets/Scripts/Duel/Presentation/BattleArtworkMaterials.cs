using System.Collections.Generic;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    /// <summary>相同模板与图集页使用同一运行时材质，最后一个 Renderer 释放时销毁。</summary>
    static class BattleArtworkMaterials
    {
        sealed class Entry { public Material Material; public int Users; }
        static readonly Dictionary<(Material Template, Texture Page), Entry> s_materials = new();
        public static Material Acquire(Material template, Texture page)
        {
            var key = (template, page);
            if (!s_materials.TryGetValue(key, out var entry))
            {
                var material = new Material(template) { name = template.name + " · " + page.name, enableInstancing = true };
                material.SetTexture("_BaseMap", page);
                s_materials.Add(key, entry = new Entry { Material = material });
            }
            entry.Users++;
            return entry.Material;
        }
        public static void Release(Material template, Texture page)
        {
            var key = (template, page); var entry = s_materials[key];
            if (--entry.Users != 0) return;
            s_materials.Remove(key);
            Object.Destroy(entry.Material);
        }
    }
}
