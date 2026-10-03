using System;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>复用已有恢复资源导入器，按清单固定选择 HD 或唯一版本。</summary>
public static class BattleCutinImporter
{
    public static readonly string[] CardIds = {
        "01948619", "19324993", "22908820", "23204029", "32828466", "40044918", "50954680", "55063751", "55171412", "56733747",
        "58004362", "58481572", "60461804", "63288574", "63767246", "75147529", "89631139", "89943723", "90673289", "93347961"
    };
    public static readonly string[] RelativeFolders = {
        "1948619/MonsterCutin2", "19324993/MonsterCutin2", "22908820/MonsterCutin2", "23204029/MonsterCutin2", "32828466/MonsterCutin2",
        "40044918/MonsterCutin2", "50954680/MonsterCutin2", "55063751/MonsterCutin/highend_hd/1", "55171412/MonsterCutin2",
        "56733747/MonsterCutin/highend_hd/0.88", "58004362/MonsterCutin2", "58481572/MonsterCutin/highend_hd/0.611",
        "60461804/MonsterCutin/highend_hd/0.91", "63288574/MonsterCutin/p3899/highend_hd/0.87", "63767246/MonsterCutin/highend_hd/0.88",
        "75147529/MonsterCutin/highend_hd/1", "89631139/MonsterCutin/p3801/highend_hd/0.87", "89943723/MonsterCutin2",
        "90673289/MonsterCutin/p3433/highend_hd/0.545", "93347961/MonsterCutin2"
    };
    public static string Import()
    {
        const string root = "C:/Users/ldc20/OneDrive/Desktop/UI/Spine/";
        for (int i = 0; i < CardIds.Length; i++)
        {
            string destination = "Assets/Art/Spine/BattleCutins/" + CardIds[i];
            if (Directory.Exists(destination) && AssetDatabase.FindAssets("t:SkeletonDataAsset", new[] { destination }).Length != 0) continue;
            SpineRecoveryImporter.ImportOne(root + RelativeFolders[i], destination);
        }
        AssetDatabase.SaveAssets();
        return "已导入 20 张召唤演出资源";
    }
}
