using System.Linq;
using AChen.Duel.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class BattleSceneAssetsTests
{
    [Test]
    public void StartupBattleSceneHasItsSavedDemoPreset()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity",OpenSceneMode.Additive);
        try
        {
            var controller=Object.FindObjectsByType<BattleSceneController>(FindObjectsSortMode.None).First(c=>c.gameObject.scene==scene);
            var serialized=new SerializedObject(controller);
            Assert.That(serialized.FindProperty("m_preset").objectReferenceValue,
                Is.EqualTo(AssetDatabase.LoadAssetAtPath<DuelDemoPreset>("Assets/Art/Battle/BattleDemoPreset.asset")));
            Assert.That(serialized.FindProperty("m_preset").objectReferenceValue,Is.Not.Null);
        }
        finally {EditorSceneManager.CloseScene(scene,true);}
    }
}
