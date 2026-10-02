using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>按原 Shader 类别适配解包资源，保留网格、贴图、材质参数、动画和 GUID。</summary>
public static class MDPro3BattleImport
{
    public const string Folder = "Assets/Art/MDPro3Battle";
    [Serializable] sealed class MaterialFamilies { public MaterialFamily[] materials; }
    [Serializable] sealed class MaterialFamily { public string path; public string originalShader; public string[] properties; public string[] textures; }
    [Serializable] sealed class AnimationBindings { public OriginalTimeline[] timelines; }
    [Serializable] sealed class OriginalTimeline { public string prefabName; public OriginalTrack[] tracks; public double duration; }
    [Serializable] sealed class OriginalTrack { public string path; public int occurrence; public OriginalClip[] clips; }
    [Serializable] sealed class OriginalClip
    {
        public string clipPath;
        public double start, duration, clipIn, timeScale;
        public Vector3 position, eulerAngles;
        public bool removeStartOffset;
        public OriginalTextCurve[] textCurves;
    }
    [Serializable] sealed class OriginalTextCurve { public string path, property; public OriginalKey[] keys; }
    [Serializable] sealed class OriginalKey
    {
        public float time, value, inWeight, outWeight;
        public string inSlope, outSlope;
        public int weightedMode;
    }

    /// <summary>迁移原 Timeline 的动画轨道，不导入旧自定义脚本或声音插件。</summary>
    public static void PrepareOriginalTimeline(GameObject root, string originalPrefabName, PlayableDirector director)
    {
        var sources = JsonUtility.FromJson<AnimationBindings>(File.ReadAllText(Folder + "/animation-bindings.json"));
        var source = sources.timelines.Single(x => x.prefabName == originalPrefabName);
        string output = Folder + "/AdaptedTimeline";
        if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(Folder, "AdaptedTimeline");
        string path = output + "/" + originalPrefabName + ".playable";
        TimelineAsset timeline;
        if (File.Exists(path))
        {
            timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            foreach (var track in timeline.GetRootTracks().ToArray()) timeline.DeleteTrack(track);
        }
        else
        {
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, path);
        }
        timeline.editorSettings.frameRate = 60;
        timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
        timeline.fixedDuration = source.duration;
        director.playOnAwake = false;
        director.enabled = true;
        director.extrapolationMode = DirectorWrapMode.None;
        director.playableAsset = timeline;
        var animators = EditorComponents<Animator>(root.transform).ToArray();
        foreach (var originalTrack in source.tracks)
        {
            var animator = animators.Where(x => AnimationUtility.CalculateTransformPath(x.transform, root.transform) == originalTrack.path).ElementAt(originalTrack.occurrence);
            animator.runtimeAnimatorController = null;
            var track = timeline.CreateTrack<AnimationTrack>(null, originalTrack.path);
            track.trackOffset = TrackOffset.ApplyTransformOffsets;
            director.SetGenericBinding(track, animator);
            foreach (var originalClip in originalTrack.clips)
            {
                var clip = track.CreateClip<AnimationPlayableAsset>();
                var asset = (AnimationPlayableAsset)clip.asset;
                asset.clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(originalClip.clipPath);
                RestoreTextCurves(asset.clip, originalClip.textCurves);
                asset.position = originalClip.position;
                asset.eulerAngles = originalClip.eulerAngles;
                asset.removeStartOffset = originalClip.removeStartOffset;
                asset.applyFootIK = false;
                clip.start = originalClip.start;
                clip.duration = originalClip.duration;
                clip.clipIn = originalClip.clipIn;
                clip.timeScale = originalClip.timeScale;
            }
        }
        EditorUtility.SetDirty(timeline);
        EditorUtility.SetDirty(director);
        AssetDatabase.SaveAssets();
    }

    static void RestoreTextCurves(AnimationClip clip, OriginalTextCurve[] originalCurves)
    {
        if (originalCurves.Length == 0) return;
        foreach (var binding in AnimationUtility.GetCurveBindings(clip).Where(x => x.propertyName.StartsWith("script_0x")))
            AnimationUtility.SetEditorCurve(clip, binding, null);
        foreach (var original in originalCurves)
        {
            var keys = original.keys.Select(x => new Keyframe(x.time, x.value,
                float.Parse(x.inSlope, CultureInfo.InvariantCulture), float.Parse(x.outSlope, CultureInfo.InvariantCulture), x.inWeight, x.outWeight)
                { weightedMode = (WeightedMode)x.weightedMode }).ToArray();
            var binding = EditorCurveBinding.FloatCurve(original.path, typeof(TMPro.TextMeshPro), original.property);
            AnimationUtility.SetEditorCurve(clip, binding, new AnimationCurve(keys));
        }
        EditorUtility.SetDirty(clip);
    }

    public static void PrepareMaterials()
    {
        var families = JsonUtility.FromJson<MaterialFamilies>(File.ReadAllText(Folder + "/material-families.json"));
        foreach (var family in families.materials)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(family.path);
            PrepareMaterial(material, family);
            EditorUtility.SetDirty(material);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            Clean(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets();
    }

    static void PrepareMaterial(Material material, MaterialFamily family)
    {
        var saved = new SerializedObject(material);
        var textureEnvs = saved.FindProperty("m_SavedProperties.m_TexEnvs");
        var textures = new Dictionary<string, (Texture Texture, Vector2 Scale, Vector2 Offset)>();
        for (int i = 0; i < textureEnvs.arraySize; i++)
        {
            var element = textureEnvs.GetArrayElementAtIndex(i);
            textures[element.FindPropertyRelative("first").stringValue] = (
                (Texture)element.FindPropertyRelative("second.m_Texture").objectReferenceValue,
                element.FindPropertyRelative("second.m_Scale").vector2Value,
                element.FindPropertyRelative("second.m_Offset").vector2Value);
        }
        var colors = ReadColors(saved.FindProperty("m_SavedProperties.m_Colors"));
        string source = family.originalShader;
        material.shaderKeywords = Array.Empty<string>();
        material.renderQueue = -1;
        if (source.Contains("leafShadow")) material.shader = Shader.Find("TCG/Battle/OriginalLeafShadow");
        else if (source == "Shader Graphs/GraveCommon01") material.shader = Shader.Find("TCG/Battle/OriginalGrave");
        else if (source == "Shader Graphs/Timer_c001_02") material.shader = Shader.Find("TCG/Battle/OriginalTimerProgress");
        else if (source.StartsWith("Shader Graphs/Timer_c001_")) material.shader = Shader.Find("TCG/Battle/OriginalTimerBody");
        else if (source.StartsWith("Shader Graphs/PlayableGuide_c001_"))
        {
            material.shader = Shader.Find("TCG/Battle/OriginalPlayableGuide");
            material.SetFloat("_Luminous", source.EndsWith("Luminous") ? 1f : 0f);
            material.SetFloat("_ChangeLayer", source.EndsWith("change") ? 1f : 0f);
            material.renderQueue = source.EndsWith("Luminous") ? 4000 : source.EndsWith("chousei") ? 2500 : 3000;
            if (source.EndsWith("change"))
            {
                var texture = textures["_SampleTexture2D_72db3480b1bdea8583e6b7743c79a377_Texture_1_Texture2D"];
                material.SetTexture("_Texture2D", texture.Texture);
                material.SetTextureScale("_Texture2D", texture.Scale);
                material.SetTextureOffset("_Texture2D", texture.Offset);
            }
        }
        else if (source is "Shader Graphs/UnlitRecieveShadow" or "Shader Graphs/UnlitRecieveShadow_vc")
        {
            material.shader = Shader.Find("TCG/Battle/OriginalFieldOpaque");
            material.SetFloat("_UseVertexColor", source.EndsWith("_vc") ? 1f : 0f);
        }
        else if (source is "Shader Graphs/UnlitRecieveShadow_alpha" or "Shader Graphs/Unlit_simpleAlpha" or "Shader Graphs/fxs_ASu008_04_alpha")
            material.shader = Shader.Find("TCG/Battle/OriginalFieldAlpha");
        else if (source.StartsWith("Universal Render Pipeline/"))
        {
            material.shader = Shader.Find(source);
            bool cutout = material.GetFloat("_AlphaClip") > .5f;
            if (cutout)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = 2450;
            }
            if (family.textures.Contains("_BumpMap")) material.EnableKeyword("_NORMALMAP");
            if (source == "Universal Render Pipeline/Particles/Unlit")
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                if (material.GetFloat("_SoftParticlesEnabled") > .5f) material.EnableKeyword("_SOFTPARTICLES_ON");
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_ZWrite", 0f);
                material.renderQueue = 3000;
            }
        }
        else if (source == "TextMeshPro/Distance Field") material.shader = Shader.Find(source);
        else if (source.StartsWith("VFX_Shader/") || source.StartsWith("Ygom/Effect/") || source.StartsWith("TimelineShader/"))
        {
            material.shader = Shader.Find("TCG/Battle/OriginalVfx");
            var textureNames = new[] { "_Main_Texture", "_Texture", "_Texture2D", "_MainTex", "_BaseMap" }.Where(family.textures.Contains).ToArray();
            if (textureNames.Length > 0)
            {
                var texture = textures[textureNames[0]];
                material.SetTexture("_SourceTexture", texture.Texture);
                material.SetTextureScale("_SourceTexture", texture.Scale);
                material.SetTextureOffset("_SourceTexture", texture.Offset);
            }
            var tintNames = new[] { "_Emissive", "_Emission", "_TintColor", "_Color" }.Where(colors.ContainsKey).ToArray();
            var tint = tintNames.Length == 0 ? Color.white : colors[tintNames[0]];
            tint.a = 1f;
            material.SetColor("_SourceTint", tint);
            material.SetFloat("_SourceHasTexture", textureNames.Length == 0 ? 0f : 1f);
            material.SetFloat("_Family", source.Contains("ParticleRing") || source.Contains("FlareRing") ? 1f : source.Contains("GraveSoul") || source.Contains("SinWaveTrail") ? 2f : source.Contains("Noise") || source.Contains("Twist") ? 3f : 0f);
            bool additive = source.EndsWith("_k") || source.Contains("ParticleAdd") || source.Contains("SimpleHDR");
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", additive ? 1f : 10f);
        }
        else
        {
            // 原辅助卡面材质仍保留数据；卡牌实例显式绑定本项目的正反面材质。
            material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
    }

    static Dictionary<string, Color> ReadColors(SerializedProperty properties)
    {
        var colors = new Dictionary<string, Color>();
        for (int i = 0; i < properties.arraySize; i++)
        {
            var element = properties.GetArrayElementAtIndex(i);
            colors[element.FindPropertyRelative("first").stringValue] = element.FindPropertyRelative("second").colorValue;
        }
        return colors;
    }

    static void Clean(Transform transform)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
        transform.gameObject.layer = 0;
        foreach (var child in Enumerable.Range(0, transform.childCount).Select(transform.GetChild).ToArray()) Clean(child);
        // 旧 Timeline 导出的脚本不可用，原 AnimationClip 的新绑定接管演出。
        foreach (var director in DirectEditorComponents<PlayableDirector>(transform.gameObject)) director.enabled = false;
    }

    static IEnumerable<T> EditorComponents<T>(Transform root) where T : Component
    {
        foreach (var component in DirectEditorComponents<T>(root.gameObject)) yield return component;
        for (int i = 0; i < root.childCount; i++)
            foreach (var component in EditorComponents<T>(root.GetChild(i))) yield return component;
    }

    static IEnumerable<T> DirectEditorComponents<T>(GameObject root) where T : Component
    {
        var components = new SerializedObject(root).FindProperty("m_Component");
        for (int i = 0; i < components.arraySize; i++)
            if (components.GetArrayElementAtIndex(i).FindPropertyRelative("component").objectReferenceValue is T component) yield return component;
    }
}
