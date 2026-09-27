using UnityEngine;

/// <summary>日志系统运行时设置。出包时若 EnableInPlayer=false,ALog 不再写日志。</summary>
public class ALogSettings : ScriptableObject
{
    public const string ResourceName = "ALogSettings";
    public const string AssetPath = "Assets/GameConfiguration/ALogSettings.asset";
    public const string Address = "GameConfig/ALogSettings";

    [Tooltip("Enable ALog output in release players.")]
    public bool EnableInPlayer = true;

    private static ALogSettings s_instance;

    public static void ResetState() => s_instance = null;

    public static ALogSettings Instance {
        get {
            return s_instance;
        }
    }

    public static void SetEditorInstance(ALogSettings settings) {
        s_instance = settings;
    }
}
