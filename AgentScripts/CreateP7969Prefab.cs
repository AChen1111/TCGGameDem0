using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateP7969Prefab
{
    const string DataPath = "Assets/Art/Spine/P7969JS/P7969JS_SkeletonData.asset";
    const string PrefabPath = "Assets/Art/Spine/P7969JS/P7969JS.prefab";
    const string ScenePath = "Assets/Art/Spine/P7969JS/P7969JSPreview.unity";
    const string AnimationName = "animation_kde";

    public static string Run()
    {
        SkeletonDataAsset data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DataPath);
        if (data == null)
            throw new System.Exception("找不到 SkeletonDataAsset: " + DataPath);

        Scene scene = default;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene opened = SceneManager.GetSceneAt(i);
            if (opened.path == ScenePath || opened.name == "P7969JSPreview")
            {
                scene = opened;
                break;
            }
        }

        if (!scene.IsValid())
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        EditorSceneManager.SetActiveScene(scene);

        foreach (GameObject root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        SkeletonAnimation anim = EditorInstantiation.InstantiateSkeletonAnimation(data, "default");
        if (anim == null)
            throw new System.Exception("InstantiateSkeletonAnimation 失败");

        anim.gameObject.name = "P7969JS";
        anim.loop = true;
        anim.AnimationName = AnimationName;
        if (anim.state != null)
        {
            anim.state.SetAnimation(0, AnimationName, true);
            anim.state.Update(0);
            anim.state.Apply(anim.skeleton);
            anim.skeleton.UpdateWorldTransform(Spine.Skeleton.Physics.Update);
        }

        EditorSceneManager.MoveGameObjectToScene(anim.gameObject, scene);

        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            anim.gameObject, PrefabPath, InteractionMode.AutomatedAction);
        if (prefab == null)
            throw new System.Exception("保存预制体失败: " + PrefabPath);

        GameObject camGo = new GameObject("PreviewCamera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 12f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        EditorSceneManager.MoveGameObjectToScene(camGo, scene);

        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = anim.gameObject;
        EditorGUIUtility.PingObject(prefab);
        return PrefabPath;
    }
}
